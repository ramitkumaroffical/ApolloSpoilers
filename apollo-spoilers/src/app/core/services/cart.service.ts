import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Cart } from '../models/models';
import { environment } from '../../../environments/environment.prod';

@Injectable({ providedIn: 'root' })
export class CartService {
  private http = inject(HttpClient);

  // Signal for cart count badge in the toolbar
  private _cart = signal<Cart | null>(null);
  readonly cart = this._cart.asReadonly();
  readonly itemCount = signal<number>(0);

  get(): Observable<Cart> {
    return this.http.get<Cart>(`${environment.apiUrl}/cart`).pipe(tap(c => this.updateCart(c)));
  }

  add(productId: string, quantity = 1): Observable<Cart> {
    return this.http.post<Cart>(`${environment.apiUrl}/cart/items`, { productId, quantity })
      .pipe(tap(c => this.updateCart(c)));
  }

  update(itemId: string, quantity: number): Observable<Cart> {
    return this.http.put<Cart>(`${environment.apiUrl}/cart/items/${itemId}`, { quantity })
      .pipe(tap(c => this.updateCart(c)));
  }

  remove(itemId: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/cart/items/${itemId}`)
      .pipe(tap(() => this.get().subscribe()));
  }

  clear(): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/cart`)
      .pipe(tap(() => this.updateCart({ id: '', items: [], subtotal: 0, totalItems: 0 })));
  }

  private updateCart(cart: Cart): void {
    this._cart.set(cart);
    this.itemCount.set(cart.totalItems ?? 0);
  }
}
