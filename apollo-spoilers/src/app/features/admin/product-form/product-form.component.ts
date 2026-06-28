import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { CatalogService } from '../../../core/services/catalog.service';
import { ProductAdminService } from '../../../core/services/product-admin.service';
import { NotificationService } from '../../../core/services/notification.service';
import { extractErrorMessage } from '../../../core/interceptors/auth.interceptor';
import type { Category } from '../../../core/models/models';

/** Tracks an image that has been uploaded or is being uploaded. */
interface ImageEntry {
  file?: File;
  imageUrl: string;
  isPrimary: boolean;
  uploading: boolean;
}

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterModule,
    MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule,
    MatSelectModule, MatCheckboxModule, MatIconModule,
    MatProgressSpinnerModule, MatDividerModule
  ],
  templateUrl: './product-form.component.html',
  styleUrl: './product-form.component.css',
})
export class ProductFormComponent implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private catalog = inject(CatalogService);
  private adminSvc = inject(ProductAdminService);
  private notify = inject(NotificationService);

  /** If a productId is passed via route param, we are in edit mode. */
  readonly productId = signal<string | null>(null);
  readonly isEdit = signal(false);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly categories = signal<Category[]>([]);
  readonly images = signal<ImageEntry[]>([]);
  readonly carBrands = signal<string[]>([]);
  readonly carModels = signal<string[]>([]);
  

  public dragCounter = 0;

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.required, Validators.minLength(10)]],
    price: [0, [Validators.required, Validators.min(0)]],
    compareAtPrice: [null as number | null],
    material: [''],
    color: [''],
    carBrand: [''],
    carModel: [''],
    fitYearFrom: [null as number | null],
    fitYearTo: [null as number | null],
    categoryId: ['', Validators.required],
    isActive: [true],
    isFeatured: [false],
    initialStock: [0, [Validators.required, Validators.min(0)]],
    lowStockThreshold: [5],
  });

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.productId.set(id);
        this.isEdit.set(true);
        this.loadProduct(id);
      } else {
        this.loading.set(false);
      }
    });

    this.catalog.categories().subscribe(cats => this.categories.set(cats));
    this.catalog.carBrands().subscribe(brands => this.carBrands.set(brands));

    // When carBrand changes, load models
    this.form.get('carBrand')?.valueChanges.subscribe(brand => {
      this.form.patchValue({ carModel: '' });
      if (brand) {
        this.catalog.carModels(brand).subscribe(models => this.carModels.set(models));
      } else {
        this.carModels.set([]);
      }
    });
  }

  ngOnDestroy(): void {
    // cleanup if needed
  }

  /** Load product data for edit mode. */
  private loadProduct(id: string): void {
    this.loading.set(true);
    this.adminSvc.getById(id).subscribe({
      next: p => {
        this.form.patchValue({
          name: p.name,
          description: p.description,
          price: p.price,
          compareAtPrice: p.compareAtPrice ?? null,
          material: p.material ?? '',
          color: p.color ?? '',
          carBrand: p.carBrand ?? '',
          carModel: p.carModel ?? '',
          fitYearFrom: p.fitYearFrom ?? null,
          fitYearTo: p.fitYearTo ?? null,
          categoryId: p.categoryId,
          isActive: p.isActive,
          isFeatured: p.isFeatured,
          initialStock: p.stockQuantity,
          lowStockThreshold: p.lowStockThreshold,
        });
        // Load existing images
        this.images.set(p.images.map(img => ({
          imageUrl: img.imageUrl,
          isPrimary: img.isPrimary,
          uploading: false,
        })));

        // Load car models if brand is set
        if (p.carBrand) {
          this.catalog.carModels(p.carBrand).subscribe(models => this.carModels.set(models));
        }
        this.loading.set(false);
      },
      error: e => {
        this.error.set(extractErrorMessage(e));
        this.loading.set(false);
      },
    });
  }

  /** Submit the form (create or update). */
  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    const raw = this.form.getRawValue();
    const dto = {
      ...raw,
      compareAtPrice: raw.compareAtPrice || undefined,
      material: raw.material || undefined,
      color: raw.color || undefined,
      carBrand: raw.carBrand || undefined,
      carModel: raw.carModel || undefined,
      fitYearFrom: raw.fitYearFrom || undefined,
      fitYearTo: raw.fitYearTo || undefined,
    };

    const save$ = this.isEdit()
      ? this.adminSvc.update(this.productId()!, dto)
      : this.adminSvc.create(dto);

    save$.subscribe({
      next: savedProduct => {
        // Upload any pending new images
        this.uploadPendingImages(savedProduct.id).then(() => {
          this.notify.success(this.isEdit() ? 'Product updated successfully.' : 'Product created successfully.');
          this.router.navigateByUrl('/admin/products');
        });
      },
      error: e => {
        this.error.set(extractErrorMessage(e));
        this.saving.set(false);
      },
    });
  }

  /** Upload any new file-based images after product is saved. */
  private async uploadPendingImages(productId: string): Promise<void> {
    const pending = this.images().filter(i => i.file && i.uploading);
    for (const entry of pending) {
      await new Promise<void>((resolve) => {
        this.adminSvc.uploadImageForProduct(productId, entry.file!, entry.isPrimary).subscribe({
          next: () => resolve(),
          error: () => resolve(), // best effort
        });
      });
    }
  }

  // ====== Image Handling ======

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.addFiles(Array.from(input.files));
      input.value = ''; // reset so same file can be selected again
    }
  }

  onDragEnter(event: DragEvent): void {
    event.preventDefault();
    this.dragCounter++;
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.dragCounter--;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragCounter = 0;
    if (event.dataTransfer?.files) {
      this.addFiles(Array.from(event.dataTransfer.files));
    }
  }

  private addFiles(files: File[]): void {
    const allowed = new Set(['image/jpeg', 'image/png', 'image/gif', 'image/webp']);
    for (const file of files) {
      if (!allowed.has(file.type)) continue;
      if (file.size > 5 * 1024 * 1024) continue;

      // Read file as data URL for preview
      const reader = new FileReader();
      reader.onload = () => {
        const existing = this.images();
        const isPrimary = existing.length === 0; // first image is primary
        this.images.update(list => [
          ...list,
          {
            file,
            imageUrl: reader.result as string,
            isPrimary,
            uploading: true,
          },
        ]);
      };
      reader.readAsDataURL(file);
    }
  }

  removeImage(index: number): void {
    this.images.update(list => {
      const removed = list.splice(index, 1)[0];
      // If removed was primary, make first remaining primary
      if (removed?.isPrimary && list.length > 0) {
        list[0].isPrimary = true;
      }
      return [...list];
    });
  }

  setPrimary(index: number): void {
    this.images.update(list => list.map((img, i) => ({ ...img, isPrimary: i === index })));
  }

  cancel(): void {
    this.router.navigateByUrl('/admin/products');
  }
}
