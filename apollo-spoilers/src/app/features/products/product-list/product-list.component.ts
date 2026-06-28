import { Component, OnInit, inject, signal, computed, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Meta, Title } from '@angular/platform-browser';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSliderModule } from '@angular/material/slider';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { FormsModule } from '@angular/forms';
import { CatalogService } from '../../../core/services/catalog.service';
import { CartService } from '../../../core/services/cart.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ProductListItem, Category, ProductQuery, PagedResult } from '../../../core/models/models';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    CommonModule, RouterModule, FormsModule,
    MatCardModule, MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatSliderModule,
    MatPaginatorModule
  ],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.css',
})
export class ProductListComponent implements OnInit, OnDestroy {
  readonly environment = environment;
  private catalog = inject(CatalogService);
  private cart = inject(CartService);
  private notify = inject(NotificationService);
  private meta = inject(Meta);
  private title = inject(Title);

  readonly categories = signal<Category[]>([]);
  readonly carBrands = signal<string[]>([]);
  readonly carModels = signal<string[]>([]);
  readonly result = signal<PagedResult<ProductListItem> | null>(null);
  readonly loading = signal(false);
  readonly page = signal(1);
  readonly pageSize = signal(12);

  filters: ProductQuery = {
    search: '', categoryId: '', carBrand: '', carModel: '',
    maxPrice: undefined, sortBy: 'Newest', page: 1, pageSize: 12
  };

  ngOnInit(): void {
    this.catalog.categories().subscribe(c => this.categories.set(c));
    this.catalog.carBrands().subscribe(b => this.carBrands.set(b));
    this.setupSeo();
    this.load();
  }

  ngOnDestroy(): void {
    this.removeStructuredData();
    this.removeCanonical();
  }

  private setupSeo(): void {
    const titleStr = 'Buy Premium Car Spoilers, GT Wings & Aero Parts Online India | Apollo Spoilers';
    const desc = 'Shop India\'s #1 range of premium car spoilers, GT wings, rear diffusers, lip spoilers & aero styling kits. Carbon fiber, OEM-level fitment for 150+ car models. Free insured shipping, 1-Year warranty. Pay by Card, UPI, Paytm or COD.';
    const url = 'https://apollospoilers.com/';

    this.title.setTitle(titleStr);
    this.meta.updateTag({ name: 'description', content: desc });
    this.meta.updateTag({ name: 'keywords', content: 'car spoiler India, GT wing, rear spoiler, carbon fiber spoiler, lip spoiler, car aero parts, Apollo Spoilers, buy car spoiler online, Supra spoiler, Mustang spoiler' });

    this.meta.updateTag({ property: 'og:title', content: titleStr });
    this.meta.updateTag({ property: 'og:description', content: desc });
    this.meta.updateTag({ property: 'og:url', content: url });
    this.meta.updateTag({ property: 'og:type', content: 'website' });

    this.meta.updateTag({ name: 'twitter:title', content: titleStr });
    this.meta.updateTag({ name: 'twitter:description', content: desc });

    this.meta.updateTag({ name: 'robots', content: 'index, follow' });

    this.setCanonical(url);
    this.injectItemListSchema();
  }

  private setCanonical(url: string): void {
    this.removeCanonical();
    const link = document.createElement('link');
    link.setAttribute('rel', 'canonical');
    link.setAttribute('href', url);
    link.id = 'page-canonical';
    document.head.appendChild(link);
  }

  private removeCanonical(): void {
    document.getElementById('page-canonical')?.remove();
  }

  private injectItemListSchema(): void {
    this.removeStructuredData();
    const schema = {
      '@context': 'https://schema.org',
      '@type': 'ItemList',
      name: 'Apollo Spoilers — Car Spoilers & Aero Parts',
      itemListElement: (this.result()?.items ?? []).slice(0, 12).map((item, i) => ({
        '@type': 'ListItem',
        position: i + 1,
        name: item.name,
        url: `https://apollospoilers.com/products/${item.slug}`,
      })),
    };
    const script = document.createElement('script');
    script.type = 'application/ld+json';
    script.id = 'productlist-jsonld';
    script.textContent = JSON.stringify(schema);
    document.head.appendChild(script);
  }

  private removeStructuredData(): void {
    document.getElementById('productlist-jsonld')?.remove();
  }

  load(): void {
    this.loading.set(true);
    const query: ProductQuery = { ...this.filters, page: this.page(), pageSize: this.pageSize() };
    // Strip empty strings
    Object.keys(query).forEach(k => {
      if (query[k as keyof ProductQuery] === '') (query as any)[k] = undefined;
    });
    this.catalog.search(query).subscribe({
      next: r => { this.result.set(r); this.loading.set(false); this.injectItemListSchema(); },
      error: () => this.loading.set(false)
    });
  }

  applyFilters(): void {
    this.page.set(1);
    this.load();
  }

  onBrandChange(): void {
    this.filters.carModel = '';
    if (this.filters.carBrand) {
      this.catalog.carModels(this.filters.carBrand).subscribe(m => this.carModels.set(m));
    } else {
      this.carModels.set([]);
    }
    this.applyFilters();
  }

  resetFilters(): void {
    this.filters = { search: '', categoryId: '', carBrand: '', carModel: '', maxPrice: undefined, sortBy: 'Newest' };
    this.carModels.set([]);
    this.page.set(1);
    this.load();
  }

  onPageChange(e: PageEvent): void {
    this.page.set(e.pageIndex + 1);
    this.pageSize.set(e.pageSize);
    this.load();
  }

  /** Discount % helper exposed to the template. */
  discountPercent(p: ProductListItem): number {
    if (!p.compareAtPrice || p.compareAtPrice <= p.price) return 0;
    return Math.round(((p.compareAtPrice - p.price) / p.compareAtPrice) * 100);
  }

  addToCart(p: ProductListItem): void {
    if (p.stockQuantity === 0) return;
    this.cart.add(p.id).subscribe({
      next: () => this.notify.addedToCart(p.name),
      error: () => this.notify.error('Could not add to cart. Please try again.'),
    });
  }
}
