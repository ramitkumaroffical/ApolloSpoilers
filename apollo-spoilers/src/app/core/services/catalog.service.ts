import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Category, ProductDetail, ProductListItem, PagedResult, ProductQuery, Review
} from '../models/models';
import { environment } from '../../../environments/environment.prod';

@Injectable({ providedIn: 'root' })
export class CatalogService {
  private http = inject(HttpClient);

  search(query: ProductQuery): Observable<PagedResult<ProductListItem>> {
    let params = new HttpParams();
    for (const [k, v] of Object.entries(query)) {
      if (v !== undefined && v !== null && v !== '') {
        params = params.set(k, String(v));
      }
    }
    return this.http.get<PagedResult<ProductListItem>>(`${environment.apiUrl}/products`, { params });
  }

  getBySlug(slug: string): Observable<ProductDetail> {
    return this.http.get<ProductDetail>(`${environment.apiUrl}/products/${slug}`);
  }

  categories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${environment.apiUrl}/products/categories`);
  }

  carBrands(): Observable<string[]> {
    return this.http.get<string[]>(`${environment.apiUrl}/products/car-brands`);
  }

  carModels(brand: string): Observable<string[]> {
    return this.http.get<string[]>(`${environment.apiUrl}/products/car-models/${encodeURIComponent(brand)}`);
  }

  reviews(productId: string): Observable<Review[]> {
    return this.http.get<Review[]>(`${environment.apiUrl}/products/${productId}/reviews`);
  }

  addReview(productId: string, rating: number, comment?: string): Observable<Review> {
    return this.http.post<Review>(`${environment.apiUrl}/products/${productId}/reviews`, { rating, comment });
  }
}
