import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  ProductDetail, ProductListItem, CreateProductRequest, UpdateProductRequest,
  ProductImage, ImageUploadResponse, PagedResult
} from '../models/models';
import { environment } from '../../../environments/environment.prod';

@Injectable({ providedIn: 'root' })
export class ProductAdminService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/admin/products`;

  /** Get a single product by id (admin detail view). */
  getById(id: string): Observable<ProductDetail> {
    return this.http.get<ProductDetail>(`${this.base}/${id}`);
  }

  /** List products with optional search/filter. */
  list(query?: Record<string, string | number | boolean | undefined>): Observable<PagedResult<ProductListItem>> {
    let params: URLSearchParams | undefined;
    if (query) {
      params = new URLSearchParams();
      for (const [k, v] of Object.entries(query)) {
        if (v !== undefined && v !== null && v !== '') {
          params.set(k, String(v));
        }
      }
    }
    const qs = params?.toString();
    const url = qs ? `${this.base}?${qs}` : `${environment.apiUrl}/products`;
    return this.http.get<PagedResult<ProductListItem>>(url);
  }

  /** Create a new product. */
  create(dto: CreateProductRequest): Observable<ProductDetail> {
    return this.http.post<ProductDetail>(this.base, dto);
  }

  /** Update an existing product. */
  update(id: string, dto: UpdateProductRequest): Observable<ProductDetail> {
    return this.http.put<ProductDetail>(`${this.base}/${id}`, dto);
  }

  /** Soft-delete (deactivate) a product. */
  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  /** Add an image by URL. */
  addImage(productId: string, imageUrl: string, isPrimary: boolean): Observable<ProductImage> {
    return this.http.post<ProductImage>(`${this.base}/${productId}/images`, { imageUrl, isPrimary });
  }

  /** Upload an image file for a specific product. */
  uploadImageForProduct(productId: string, file: File, isPrimary = false): Observable<ProductImage> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ProductImage>(`${this.base}/${productId}/images/upload?isPrimary=${isPrimary}`, formData);
  }

  /** Upload a standalone image (not yet attached to a product). */
  uploadImage(file: File): Observable<ImageUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ImageUploadResponse>(`${this.base}/images/upload`, formData);
  }

  /** Update stock for a product. */
  updateStock(productId: string, quantity: number, lowStockThreshold = 5): Observable<void> {
    return this.http.put<void>(`${this.base}/${productId}/stock`, { quantity, lowStockThreshold });
  }
}
