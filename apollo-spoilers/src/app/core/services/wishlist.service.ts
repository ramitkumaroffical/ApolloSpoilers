import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Wishlist } from '../models/models';
import { environment } from '../../../environments/environment.prod';

@Injectable({ providedIn: 'root' })
export class WishlistService {
  private http = inject(HttpClient);

  get(): Observable<Wishlist> {
    return this.http.get<Wishlist>(`${environment.apiUrl}/wishlist`);
  }

  add(productId: string): Observable<Wishlist> {
    return this.http.post<Wishlist>(`${environment.apiUrl}/wishlist/${productId}`, {});
  }

  remove(productId: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/wishlist/${productId}`);
  }
}
