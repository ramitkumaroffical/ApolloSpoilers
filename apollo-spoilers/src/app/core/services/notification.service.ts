import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);

  private readonly panelClass = {
    success: 'apollo-snack-success',
    error: 'apollo-snack-error',
    info: 'apollo-snack-info',
    warn: 'apollo-snack-warn',
  };

  success(message: string, action?: string, duration = 3500): void {
    this.open(message, action, duration, 'success');
  }

  error(message: string, action?: string, duration = 5000): void {
    this.open(message, action, duration, 'error');
  }

  info(message: string, action?: string, duration = 3500): void {
    this.open(message, action, duration, 'info');
  }

  warn(message: string, action?: string, duration = 4000): void {
    this.open(message, action, duration, 'warn');
  }

  /** "Added to cart" toast with a "View Cart" action button */
  addedToCart(productName?: string): void {
    const msg = productName
      ? `${productName} added to cart`
      : 'Item added to cart';
    this.snackBar.open(msg, 'View Cart', {
      duration: 4000,
      horizontalPosition: 'right',
      verticalPosition: 'bottom',
      panelClass: ['apollo-snack-success', 'apollo-snack-action'],
    }).onAction().subscribe(() => this.router.navigate(['/cart']));
  }

  private open(message: string, action: string | undefined, duration: number, type: 'success' | 'error' | 'info' | 'warn'): void {
    this.snackBar.open(message, action ?? 'Dismiss', {
      duration,
      horizontalPosition: 'right',
      verticalPosition: 'bottom',
      panelClass: [this.panelClass[type]],
    });
  }
}
