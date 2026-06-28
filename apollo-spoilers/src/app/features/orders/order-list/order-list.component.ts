import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Meta } from '@angular/platform-browser';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { RouterModule } from '@angular/router';
import { OrderService } from '../../../core/services/order.service';
import { Order } from '../../../core/models/models';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatChipsModule, MatProgressSpinnerModule, MatIconModule, MatButtonModule, RouterModule],
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.css',
})
export class OrderListComponent implements OnInit {
  private orderSvc = inject(OrderService);
  private meta = inject(Meta);
  readonly orders = signal<Order[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.meta.updateTag({ name: 'robots', content: 'noindex, nofollow' });
    this.orderSvc.myOrders().subscribe({
      next: o => { this.orders.set(o); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  statusColor(status: string): 'primary' | 'accent' | 'warn' | undefined {
    switch (status) {
      case 'Pending': return 'warn';
      case 'Confirmed': case 'Shipped': return 'primary';
      case 'Delivered': return 'accent';
      default: return undefined;
    }
  }
}
