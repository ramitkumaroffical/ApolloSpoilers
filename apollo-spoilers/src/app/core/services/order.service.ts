import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Order, OrderStatus } from '../models/models';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private http = inject(HttpClient);

  placeOrder(payload: any): Observable<Order> {
    return this.http.post<Order>(`${environment.apiUrl}/orders`, payload);
  }

  myOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${environment.apiUrl}/orders/my`);
  }

  getById(id: string): Observable<Order> {
    return this.http.get<Order>(`${environment.apiUrl}/orders/${id}`);
  }

  adminListAll(page = 1, pageSize = 20): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/admin/orders`, {
      params: { page: String(page), pageSize: String(pageSize) }
    });
  }

  adminUpdateStatus(orderId: string, status: OrderStatus): Observable<Order> {
    return this.http.put<Order>(`${environment.apiUrl}/admin/orders/${orderId}/status`, { status });
  }
}
