import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, of, throwError, switchMap, BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, UserProfile } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  private readonly TOKEN_KEY = 'apollo_access_token';
  private readonly REFRESH_KEY = 'apollo_refresh_token';

  // Signal-based state
  private _currentUser = signal<UserProfile | null>(null);
  readonly currentUser = this._currentUser.asReadonly();
  readonly isAuthenticated = computed(() => this._currentUser() !== null);
  readonly isAdmin = computed(() =>
    this._currentUser()?.roles?.includes('Admin') ?? false
  );

  private refreshing = false;
  private refreshSubject = new BehaviorSubject<string | null>(null);

  constructor() {
    this.tryRestoreSession();
  }

  private tryRestoreSession(): void {
    const token = this.getAccessToken();
    if (!token) return;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      if (payload.exp * 1000 < Date.now()) {
        // Access token expired — refresh handled by interceptor
        return;
      }
      this._currentUser.set({
        id: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload.sub || payload.jti,
        email: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || payload.email || '',
        firstName: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || '',
        lastName: '',
        fullName: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || '',
        roles: this.extractRoles(payload),
      });
    } catch {
      this.logout();
    }
  }

  private extractRoles(payload: any): string[] {
    const roleClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
      || payload.role;
    return Array.isArray(roleClaim) ? roleClaim : (roleClaim ? [roleClaim] : []);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_KEY);
  }

  register(payload: { firstName: string; lastName: string; email: string; password: string; phoneNumber?: string }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/register`, payload).pipe(
      tap(res => this.setSession(res))
    );
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, { email, password }).pipe(
      tap(res => this.setSession(res))
    );
  }

  logout(): void {
    const token = this.getAccessToken();
    if (token) {
      this.http.post(`${environment.apiUrl}/auth/logout`, {}).subscribe({
        complete: () => {},
        error: () => {}
      });
    }
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
    this._currentUser.set(null);
    this.router.navigate(['/']);
  }

  refresh(): Observable<AuthResponse> {
    const accessToken = this.getAccessToken();
    const refreshToken = this.getRefreshToken();
    if (!accessToken || !refreshToken) {
      this.logout();
      return throwError(() => new Error('No tokens'));
    }
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/refresh`, {
      accessToken,
      refreshToken
    }).pipe(tap(res => this.setSession(res)));
  }

  updateProfile(payload: { firstName: string; lastName: string; phoneNumber?: string }): Observable<UserProfile> {
    return this.http.put<UserProfile>(`${environment.apiUrl}/auth/profile`, payload).pipe(
      tap(profile => this._currentUser.update(u => u ? { ...u, ...profile } : null))
    );
  }

  forgotPassword(email: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/auth/forgot-password`, { email });
  }

  resetPassword(payload: { email: string; token: string; newPassword: string }): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/auth/reset-password`, payload);
  }

  private setSession(res: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, res.accessToken);
    localStorage.setItem(this.REFRESH_KEY, res.refreshToken);
    this._currentUser.set(res.user);
  }
}
