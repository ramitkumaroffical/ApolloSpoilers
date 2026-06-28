import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Meta, Title } from '@angular/platform-browser';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { CatalogService } from '../../core/services/catalog.service';
import { ProductListItem } from '../../core/models/models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule, RouterModule, MatCardModule, MatButtonModule, MatIconModule, MatChipsModule],
  templateUrl: './about.component.html',
  styleUrl: './about.component.css',
})
export class AboutComponent implements OnInit, OnDestroy {
  readonly environment = environment;
  private catalog = inject(CatalogService);
  private meta = inject(Meta);
  private title = inject(Title);

  readonly featured = signal<ProductListItem[]>([]);
  readonly loading = signal(false);

  readonly spoilerTypes = [
    {
      icon: 'speed',
      title: 'Lip Spoilers',
      desc: 'Subtle yet effective — lip spoilers mount flush to your trunk lid or hatch, reducing lift at highway speeds while maintaining a clean OEM silhouette.',
    },
    {
      icon: 'flight',
      title: 'GT Wings',
      desc: 'Track-proven downforce. Our GT wings feature adjustable angle-of-attack, swan-neck or center mounts, and CFRP airfoil profiles engineered for real aero balance.',
    },
    {
      icon: 'architecture',
      title: 'Diffusers & Splitters',
      desc: 'Underbody aero matters. Front splitters manage airflow while rear diffusers accelerate exit air, reducing drag and adding stability at speed.',
    },
    {
      icon: 'layers',
      title: 'Roof Spoilers',
      desc: 'Roof-line spoilers channel air over the rear glass, improving rear grip without the visual weight of a full wing — perfect for sedans and SUVs.',
    },
    {
      icon: 'build',
      title: 'Custom Aero Kits',
      desc: 'Full front fascia + side skirts + rear bumper packages designed as integrated systems. Every element works together for maximum visual and aerodynamic impact.',
    },
  ];

  ngOnInit(): void {
    this.setupSeo();
    this.loadFeatured();
  }

  ngOnDestroy(): void {
    this.removeStructuredData();
    this.removeCanonical();
  }

  private setupSeo(): void {
    const titleStr = 'About Apollo Spoilers — India\'s #1 Premium Car Spoiler Brand | Our Story';
    const desc = 'Discover why 10,000+ car enthusiasts trust Apollo Spoilers for premium GT wings, lip spoilers, carbon fiber aero kits & exterior styling. CFD-designed, track-proven, Made in India with a 1-Year warranty.';
    const url = 'https://apollospoilers.com/about';

    this.title.setTitle(titleStr);
    this.meta.updateTag({ name: 'description', content: desc });
    this.meta.updateTag({ name: 'keywords', content: 'about Apollo Spoilers, car spoiler brand India, premium aero manufacturer, GT wing company, carbon fiber spoiler India, Apollo Spoilers story' });
    this.meta.updateTag({ property: 'og:title', content: titleStr });
    this.meta.updateTag({ property: 'og:description', content: desc });
    this.meta.updateTag({ property: 'og:url', content: url });
    this.meta.updateTag({ property: 'og:type', content: 'website' });
    this.meta.updateTag({ name: 'twitter:title', content: titleStr });
    this.meta.updateTag({ name: 'twitter:description', content: desc });
    this.meta.updateTag({ name: 'robots', content: 'index, follow' });

    this.setCanonical(url);
    this.injectAboutSchema();
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

  private injectAboutSchema(): void {
    this.removeStructuredData();
    const schema = {
      '@context': 'https://schema.org',
      '@type': 'AboutPage',
      name: 'About Apollo Spoilers',
      description: 'India\'s premier brand for premium car spoilers, GT wings, and exterior aero styling. Track-proven, OEM-level fitment, 1-Year warranty.',
      url: 'https://apollospoilers.com/about',
      mainEntity: {
        '@type': 'Organization',
        name: 'Apollo Spoilers',
        url: 'https://apollospoilers.com',
        logo: 'https://apollospoilers.com/assets/logo.svg',
        description: 'Apollo Spoilers engineers premium exterior aero components — from lip spoilers to full aero kits — for 150+ car models across India.',
        foundingDate: '2020',
        numberOfEmployees: { '@type': 'QuantitativeValue', minValue: 10, maxValue: 50 },
      },
    };
    const script = document.createElement('script');
    script.type = 'application/ld+json';
    script.id = 'about-jsonld';
    script.textContent = JSON.stringify(schema);
    document.head.appendChild(script);
  }

  private removeStructuredData(): void {
    document.getElementById('about-jsonld')?.remove();
  }

  private loadFeatured(): void {
    this.loading.set(true);
    this.catalog.search({ isFeatured: true, pageSize: 6, sortBy: 'Newest' } as any).subscribe({
      next: r => {
        this.featured.set(r.items ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
