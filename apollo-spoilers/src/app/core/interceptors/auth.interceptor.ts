import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError, from, of } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';

/**
 * Attaches the JWT bearer token to outgoing API requests, and transparently
 * refreshes the access token once on 401 responses.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  // Only attach token to API requests
  if (!req.url.startsWith(environment.apiUrl)) {
    return next(req);
  }

  const token = auth.getAccessToken();
  let authReq = req;
  if (token) {
    authReq = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !req.url.includes('/auth/refresh') && !req.url.includes('/auth/login')) {
        // Attempt a single refresh
        return auth.refresh().pipe(
          switchMap(res => {
            const refreshed = req.clone({
              setHeaders: { Authorization: `Bearer ${res.accessToken}` }
            });
            return next(refreshed);
          }),
          catchError(() => {
            auth.logout();
            return throwError(() => error);
          })
        );
      }
      return throwError(() => error);
    })
  );
};

/** Normalizes backend error responses into a readable message. */
export function extractErrorMessage(error: HttpErrorResponse): string {
  if (error.error) {
    if (typeof error.error === 'string') return error.error;
    if (error.error.message) return error.error.message;
    if (error.error.error) return error.error.error;
    if (error.error.errors) {
      const parts: string[] = [];
      for (const key in error.error.errors) {
        const vals = error.error.errors[key];
        if (Array.isArray(vals)) parts.push(...vals);
        else parts.push(String(vals));
      }
      return parts.join(' ');
    }
  }
  return error.message || 'An unexpected error occurred.';
}
