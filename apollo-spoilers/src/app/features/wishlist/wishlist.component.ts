import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Meta } from '@angular/platform-browser';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { WishlistService } from '../../core/services/wishlist.service';
import { CartService } from '../../core/services/cart.service';
import { NotificationService } from '../../core/services/notification.service';
import { Wishlist } from '../../core/models/models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [CommonModule, RouterModule, MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './wishlist.component.html',
  styleUrl: './wishlist.component.css',
})
export class WishlistComponent implements OnInit {
  private wishlistSvc = inject(WishlistService);
  private cartSvc = inject(CartService);
  private notify = inject(NotificationService);
  private meta = inject(Meta);

  readonly environment = environment;
  readonly items = signal<Wishlist | null>(null);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.meta.updateTag({ name: 'robots', content: 'noindex, nofollow' });
    this.wishlistSvc.get().subscribe({
      next: w => { this.items.set(w); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  add(productId: string, productName: string): void {
    this.cartSvc.add(productId).subscribe({
      next: () => this.notify.addedToCart(productName),
      error: () => this.notify.error('Could not add to cart.'),
    });
  }

  remove(productId: string): void {
    this.wishlistSvc.remove(productId).subscribe({
      next: () => {
        this.notify.info('Removed from wishlist');
        this.wishlistSvc.get().subscribe(w => this.items.set(w));
      },
      error: () => this.notify.error('Could not remove item'),
    });
  }
}
