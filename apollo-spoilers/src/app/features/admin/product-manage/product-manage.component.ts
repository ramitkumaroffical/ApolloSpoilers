import { Component, inject, signal, OnInit, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { Meta } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';

import { ProductAdminService } from '../../../core/services/product-admin.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import type { ProductListItem } from '../../../core/models/models';

@Component({
  selector: 'app-product-manage',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterModule,
    MatCardModule, MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatTableModule,
    MatChipsModule, MatTooltipModule, MatProgressSpinnerModule, MatDialogModule,
  ],
  templateUrl: './product-manage.component.html',
  styleUrl: './product-manage.component.css',
})
export class ProductManageComponent implements OnInit {
  private router = inject(Router);
  private notify = inject(NotificationService);
  private dialog = inject(MatDialog);
  public auth = inject(AuthService);
  private meta = inject(Meta);
  private adminSvc = inject(ProductAdminService);

  readonly products = signal<ProductListItem[]>([]);
  readonly loading = signal(true);
  readonly searchQuery = signal('');

  readonly columns = ['name', 'price', 'stock', 'status', 'actions'];
  readonly displayedProducts = signal<ProductListItem[]>([]);

  ngOnInit(): void {
    this.meta.updateTag({ name: 'robots', content: 'noindex, nofollow' });
    this.loadProducts();
  }

  loadProducts(): void {
    this.loading.set(true);
    this.adminSvc.list({ page: 1, pageSize: 200 }).subscribe({
      next: r => {
        this.products.set(r.items ?? []);
        this.filterProducts();
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  filterProducts(): void {
    const q = this.searchQuery().toLowerCase();
    const all = this.products();
    this.displayedProducts.set(q ? all.filter(p => p.name.toLowerCase().includes(q)) : all);
  }

  onSearchInput(event: Event): void {
    this.searchQuery.set((event.target as HTMLInputElement).value);
    this.filterProducts();
  }

  editProduct(id: string): void {
    this.router.navigateByUrl(`/admin/products/${id}/edit`);
  }

  deleteProduct(id: string): void {
    const dialogRef = this.dialog.open(ConfirmDeleteDialog, {
      data: { message: 'Are you sure you want to deactivate this product? It will no longer be visible to customers.' },
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.adminSvc.delete(id).subscribe({
          next: () => {
            this.notify.success('Product deactivated.');
            this.loadProducts();
          },
          error: () => this.notify.error('Could not deactivate product.'),
        });
      }
    });
  }
}

/** Simple confirm dialog for deletion. */
@Component({
  selector: 'confirm-delete-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, CommonModule],
  template: `
    <h2 mat-dialog-title>Deactivate Product</h2>
    <mat-dialog-content>
      <p>{{ data.message }}</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-flat-button color="warn" [mat-dialog-close]="true">Deactivate</button>
    </mat-dialog-actions>
  `,
})
export class ConfirmDeleteDialog {
  constructor(@Inject(MAT_DIALOG_DATA) public data: { message: string }) {}
}
