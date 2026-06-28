import { Component, OnInit, OnDestroy, inject, input, signal, computed, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Meta, Title } from '@angular/platform-browser';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CatalogService } from '../../../core/services/catalog.service';
import { CartService } from '../../../core/services/cart.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { ProductDetail, Review } from '../../../core/models/models';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, MatCardModule, MatButtonModule, MatIconModule, MatChipsModule, MatProgressSpinnerModule],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.css',
})
export class ProductDetailComponent implements OnInit, OnDestroy {
  private catalog = inject(CatalogService);
  private cart = inject(CartService);
  private notify = inject(NotificationService);
  public auth = inject(AuthService);
  private meta = inject(Meta);
  private title = inject(Title);
  private el = inject(ElementRef);

  readonly environment = environment;
  readonly slug = input.required<string>();
  readonly product = signal<ProductDetail | null>(null);
  readonly reviews = signal<Review[]>([]);
  readonly selectedImage = signal<string | null>(null);
  readonly loading = signal(true);
  readonly addingToCart = signal(false);

  /** Discount percentage when compareAtPrice > price. */
  readonly discountPercent = computed(() => {
    const p = this.product();
    if (!p?.compareAtPrice || p.compareAtPrice <= p.price) return 0;
    return Math.round(((p.compareAtPrice - p.price) / p.compareAtPrice) * 100);
  });

  /** Dollar amount saved vs the compare-at price. */
  readonly savings = computed(() => {
    const p = this.product();
    if (!p?.compareAtPrice || p.compareAtPrice <= p.price) return 0;
    return p.compareAtPrice - p.price;
  });

  ngOnInit(): void {
    const s = this.slug();
    this.catalog.getBySlug(s).subscribe({
      next: p => {
        this.product.set(p);
        this.selectedImage.set(p.images.find(i => i.isPrimary)?.imageUrl || p.images[0]?.imageUrl || null);
        this.loading.set(false);
        this.catalog.reviews(p.id).subscribe(r => this.reviews.set(r));
        this.setupSeo(p);
      },
      error: () => this.loading.set(false),
    });
  }

  ngOnDestroy(): void {
    this.removeStructuredData();
  }

  private setupSeo(p: ProductDetail): void {
    const url = `https://apollospoilers.com/products/${p.slug}`;
    const img = p.images.find(i => i.isPrimary)?.imageUrl;
    const imgUrl = img ? `${environment.imageUrl}${img}` : `https://apollospoilers.com/assets/og-banner.svg`;

    // Title
    const titleStr = `${p.name} — Apollo Spoilers | Premium Car Aero Parts India`;
    this.title.setTitle(titleStr);

    // Meta description
    const desc = `Buy ${p.name} at Apollo Spoilers. ${p.carBrand ? `Fits ${p.carBrand} ${p.carModel || ''}.` : ''} ${p.material ? `${p.material} construction.` : ''} Free shipping, 1-Year warranty, OEM-level fitment across India.`;
    this.meta.updateTag({ name: 'description', content: desc });

    // Keywords
    this.meta.updateTag({ name: 'keywords', content: `${p.name}, ${p.carBrand || ''} ${p.carModel || ''} spoiler, car spoiler India, ${p.categoryName || ''}, buy ${p.name}, Apollo Spoilers` });

    // Open Graph
    this.meta.updateTag({ property: 'og:title', content: titleStr });
    this.meta.updateTag({ property: 'og:description', content: desc });
    this.meta.updateTag({ property: 'og:url', content: url });
    this.meta.updateTag({ property: 'og:image', content: imgUrl });
    this.meta.updateTag({ property: 'og:type', content: 'product' });

    // Twitter
    this.meta.updateTag({ name: 'twitter:title', content: titleStr });
    this.meta.updateTag({ name: 'twitter:description', content: desc });
    this.meta.updateTag({ name: 'twitter:image', content: imgUrl });

    // Canonical
    this.setCanonical(url);

    // Robots
    this.meta.updateTag({ name: 'robots', content: 'index, follow' });

    // JSON-LD Product schema
    this.injectProductSchema(p, imgUrl, url);
  }

  private setCanonical(url: string): void {
    const head = document.head;
    // Remove existing canonical
    const existing = head.querySelector('link[rel="canonical"]');
    if (existing) existing.remove();
    const link = document.createElement('link');
    link.setAttribute('rel', 'canonical');
    link.setAttribute('href', url);
    head.appendChild(link);
  }

  private injectProductSchema(p: ProductDetail, imgUrl: string, url: string): void {
    this.removeStructuredData();
    const schema = {
      '@context': 'https://schema.org',
      '@type': 'Product',
      name: p.name,
      description: p.description,
      image: imgUrl,
      url,
      sku: p.id,
      brand: { '@type': 'Brand', name: 'Apollo Spoilers' },
      category: p.categoryName,
      offers: {
        '@type': 'Offer',
        price: p.price.toFixed(2),
        priceCurrency: 'INR',
        availability: p.stockQuantity > 0
          ? 'https://schema.org/InStock'
          : 'https://schema.org/OutOfStock',
        seller: { '@type': 'Organization', name: 'Apollo Spoilers' },
        url,
      },
      ...(p.averageRating > 0 ? {
        aggregateRating: {
          '@type': 'AggregateRating',
          ratingValue: p.averageRating.toFixed(1),
          reviewCount: p.reviewCount,
          bestRating: 5,
        },
      } : {}),
    };
    const script = document.createElement('script');
    script.type = 'application/ld+json';
    script.id = 'product-jsonld';
    script.textContent = JSON.stringify(schema);
    document.head.appendChild(script);
  }

  private removeStructuredData(): void {
    document.getElementById('product-jsonld')?.remove();
  }

  addToCart(): void {
    const p = this.product();
    if (!p) return;
    this.addingToCart.set(true);
    this.cart.add(p.id).subscribe({
      next: () => {
        this.addingToCart.set(false);
        this.notify.addedToCart(p.name);
      },
      error: () => {
        this.addingToCart.set(false);
        this.notify.error('Could not add to cart. Please try again.');
      },
    });
  }
}
