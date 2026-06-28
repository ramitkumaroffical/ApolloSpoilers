import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { Meta } from '@angular/platform-browser';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../core/services/cart.service';
import { NotificationService } from '../../core/services/notification.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, MatCardModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css',
})
export class CartComponent implements OnInit {
  private cartSvc = inject(CartService);
  private router = inject(Router);
  private notify = inject(NotificationService);
  private meta = inject(Meta);

  readonly cart = this.cartSvc.cart;
  readonly loading = signal(false);
  readonly environment = environment;

  ngOnInit(): void {
    this.meta.updateTag({ name: 'robots', content: 'noindex, nofollow' });
    this.loading.set(true);
    this.cartSvc.get().subscribe({
      next: () => this.loading.set(false),
      error: () => this.loading.set(false),
    });
  }

  update(id: string, qty: number): void {
    this.cartSvc.update(id, qty).subscribe({
      next: () => this.notify.info('Cart updated'),
      error: () => this.notify.error('Could not update quantity'),
    });
  }

  remove(id: string): void {
    this.cartSvc.remove(id).subscribe({
      next: () => this.notify.info('Item removed from cart'),
      error: () => this.notify.error('Could not remove item'),
    });
  }

  clear(): void {
    this.cartSvc.clear().subscribe({
      next: () => this.notify.info('Cart cleared'),
      error: () => this.notify.error('Could not clear cart'),
    });
  }

  checkout(): void {
    this.router.navigate(['/checkout']);
  }
}
