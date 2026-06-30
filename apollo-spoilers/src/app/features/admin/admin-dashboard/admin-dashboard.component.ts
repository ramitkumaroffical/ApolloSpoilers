import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Meta } from '@angular/platform-browser';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Order, OrderStatus } from '../../../core/models/models';
import { environment } from '../../../../environments/environment.prod';

interface AdminProduct {
  id: string; name: string; price: number; stockQuantity: number; isActive: boolean; slug: string;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterModule,
    MatCardModule, MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatSelectModule,
    MatTableModule, MatChipsModule, MatProgressSpinnerModule
  ],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css',
})
export class AdminDashboardComponent implements OnInit {
  private http = inject(HttpClient);
  private notify = inject(NotificationService);
  public auth = inject(AuthService);
  private meta = inject(Meta);

  readonly tab = signal<'orders' | 'products'>('orders');
  readonly orders = signal<Order[]>([]);
  readonly products = signal<AdminProduct[]>([]);
  readonly loadingOrders = signal(true);
  readonly loadingProducts = signal(true);

  readonly columns = ['number', 'date', 'total', 'payment', 'status', 'actions'];
  readonly productColumns = ['name', 'price', 'stock', 'status'];

  ngOnInit(): void {
    // Admin must never be indexed
    this.meta.updateTag({ name: 'robots', content: 'noindex, nofollow' });
    this.loadOrders();
    this.loadProducts();
  }

  loadOrders(): void {
    this.loadingOrders.set(true);
    this.http.get<any>(`${environment.apiUrl}/admin/orders`, { params: { page: '1', pageSize: '50' } })
      .subscribe({
        next: r => { this.orders.set(r.items ?? []); this.loadingOrders.set(false); },
        error: () => this.loadingOrders.set(false),
      });
  }

  loadProducts(): void {
    this.loadingProducts.set(true);
    this.http.get<any>(`${environment.apiUrl}/products`, { params: { page: '1', pageSize: '100' } })
      .subscribe({
        next: r => {
          this.products.set((r.items ?? []).map((p: any) => ({
            id: p.id, name: p.name, price: p.price,
            stockQuantity: p.stockQuantity, isActive: true, slug: p.slug
          })));
          this.loadingProducts.set(false);
        },
        error: () => this.loadingProducts.set(false),
      });
  }

  updateStatus(order: Order, status: OrderStatus): void {
    this.http.put<Order>(`${environment.apiUrl}/admin/orders/${order.id}/status`, { status }).subscribe({
      next: updated => {
        this.orders.update(list => list.map(o => o.id === updated.id ? updated : o));
        this.notify.success(`Order ${order.orderNumber} marked as ${status}`);
      },
      error: () => this.notify.error('Could not update order status'),
    });
  }

  color(status: string): 'primary' | 'accent' | 'warn' | undefined {
    switch (status) {
      case 'Pending': return 'warn';
      case 'Confirmed': case 'Shipped': return 'primary';
      case 'Delivered': return 'accent';
      default: return undefined;
    }
  }
}
