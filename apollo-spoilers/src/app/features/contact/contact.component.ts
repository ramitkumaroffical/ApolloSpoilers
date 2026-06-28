import { Component, OnInit, OnDestroy, inject, signal, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { Title, Meta } from '@angular/platform-browser';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule, RouterModule,
    MatCardModule, MatButtonModule, MatIconModule,
    MatInputModule, MatFormFieldModule, MatSelectModule,
  ],
  templateUrl: './contact.component.html',
  styleUrl: './contact.component.css',
})
export class ContactComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly title = inject(Title);
  private readonly meta = inject(Meta);
  private readonly platformId = inject(PLATFORM_ID);

  readonly submitted = signal(false);
  readonly sending = signal(false);

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: ['', [Validators.required, Validators.pattern('^[6-9]\\d{9}$')]],
    subject: ['general', Validators.required],
    carBrand: [''],
    carModel: [''],
    message: ['', [Validators.required, Validators.minLength(10)]],
  });

  readonly contactInfo = {
    email: 'ApolloSpoilers@gmail.com',
    phone1: '8076642594',
    phone2: '9717714997',
    address: 'India',
    supportHours: 'Mon – Sat, 9:00 AM – 7:00 PM IST',
  };

  readonly achievements = [
    { icon: 'verified', value: '10K+', label: 'Spoilers Delivered', desc: 'Trusted by car enthusiasts across India' },
    { icon: 'star', value: '98%', label: 'Fit Rating', desc: 'OEM-level precision on every product' },
    { icon: 'speed', value: '150+', label: 'Car Models Covered', desc: 'Spoilers for every make and model' },
    { icon: 'workspace_premium', value: '5 Year', label: 'Quality Warranty', desc: 'Backed by our ironclad guarantee' },
  ];

  readonly trustPoints = [
    { icon: 'shield', title: 'Secure Payments', desc: '256-bit SSL encryption on every transaction. Your data never leaves our protected servers.' },
    { icon: 'verified_user', title: 'Authentic Products', desc: 'Every spoiler is genuine, inspected, and ships with a certificate of authenticity.' },
    { icon: 'local_shipping', title: 'Insured Shipping', desc: 'Free shipping on all orders with full damage protection during transit.' },
    { icon: 'support_agent', title: 'Expert Support', desc: 'Our team of automotive specialists helps you pick the right spoiler for your car.' },
    { icon: 'autorenew', title: 'Easy Returns', desc: '30-day hassle-free returns. If it doesn\'t fit, we make it right — no questions asked.' },
    { icon: 'engineering', title: 'Track-Proven Design', desc: 'Every profile is CFD-validated and wind-tunnel tested for real-world performance.' },
  ];

  readonly faqs = [
    {
      q: 'How do I know which spoiler fits my car?',
      a: 'Use our website filters to select your car brand and model, or chat with AasRa AI — our styling assistant will find the perfect match based on your vehicle\'s year, make, and model.',
    },
    {
      q: 'What materials are your spoilers made from?',
      a: 'We use autoclave-cured carbon fiber, aerospace-grade aluminum, and UV-stable ABS plastic. Each material is chosen for its durability, weight savings, and finish quality.',
    },
    {
      q: 'Do you offer installation support?',
      a: 'Yes! Every product ships with detailed installation instructions. Our support team is also available via phone and email to guide you through the process.',
    },
    {
      q: 'What is your return and warranty policy?',
      a: 'We offer a 30-day hassle-free return policy and a 1-Year warranty on all products. If your spoiler arrives damaged or doesn\'t fit, we\'ll replace it at no cost.',
    },
    {
      q: 'How long does shipping take?',
      a: 'Standard delivery takes 5–7 business days across India. Express shipping (2–3 days) is available at checkout. All orders include tracking and damage insurance.',
    },
    {
      q: 'Can I get a custom spoiler made for my car?',
      a: 'Absolutely. We offer custom aero solutions — reach out via our contact form or call our support team to discuss your project requirements.',
    },
  ];

  readonly openFaq = signal<number | null>(null);

  ngOnInit(): void {
    this.setupSeo();
    this.injectStructuredData();
  }

  ngOnDestroy(): void {
    // Cleanup to prevent duplicate tags/schemas on re-navigation
    this.removeCanonical();
    this.removeStructuredData();
  }

  toggleFaq(index: number): void {
    this.openFaq.update(v => v === index ? null : index);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.sending.set(true);
    setTimeout(() => {
      this.sending.set(false);
      this.submitted.set(true);
    }, 1200);
  }

  getErrorMessage(controlName: string): string {
    const control = this.form.get(controlName);
    if (control?.errors && control.touched) {
      if (control.errors['required']) return 'This field is required.';
      if (control.errors['email']) return 'Please enter a valid email.';
      if (control.errors['minLength']) return `Minimum ${control.errors['minLength'].requiredLength} characters.`;
    }
    return '';
  }

  private removeCanonical(): void {
    document.getElementById('page-canonical')?.remove();
  }

  private setupSeo(): void {
    const pageTitle = 'Contact Apollo Spoilers — Support, Custom Orders & Enquiries';
    const pageDesc = 'Reach Apollo Spoilers for product support, fitment help, custom spoiler orders & returns. Call 8076642594 / 9717714997 or email ApolloSpoilers@gmail.com. Fast response guaranteed.';

    this.title.setTitle(pageTitle);
    this.meta.updateTag({ name: 'description', content: pageDesc });
    this.meta.updateTag({ name: 'keywords', content: 'contact Apollo Spoilers, spoiler support, car spoiler help, custom spoiler order, spoiler warranty, Apollo Spoilers phone, car spoiler returns, aero support India' });
    this.meta.updateTag({ property: 'og:title', content: pageTitle });
    this.meta.updateTag({ property: 'og:description', content: pageDesc });
    this.meta.updateTag({ property: 'og:type', content: 'website' });
    this.meta.updateTag({ property: 'og:url', content: 'https://apollospoilers.com/contact' });
    this.meta.updateTag({ name: 'twitter:card', content: 'summary' });
    this.meta.updateTag({ name: 'twitter:title', content: pageTitle });
    this.meta.updateTag({ name: 'twitter:description', content: pageDesc });
    this.meta.updateTag({ name: 'robots', content: 'index, follow' });
    this.meta.updateTag({ name: 'author', content: 'Apollo Spoilers' });

    // Canonical (id-based for cleanup)
    this.removeCanonical();
    const link: HTMLLinkElement = document.createElement('link');
    link.setAttribute('rel', 'canonical');
    link.setAttribute('href', 'https://apollospoilers.com/contact');
    link.id = 'page-canonical';
    document.head.appendChild(link);
  }

  private removeStructuredData(): void {
    document.querySelectorAll('script[data-contact-jsonld]').forEach(el => el.remove());
  }

  private injectStructuredData(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    // Contact Page — WebPage schema
    const webPageSchema = {
      '@context': 'https://schema.org',
      '@type': 'ContactPage',
      name: 'Contact Apollo Spoilers',
      description: 'Get in touch with Apollo Spoilers for product support, custom orders, warranty claims, and general inquiries.',
      url: 'https://apollospoilers.com/contact',
      mainEntity: {
        '@type': 'Organization',
        name: 'Apollo Spoilers',
        url: 'https://apollospoilers.com',
        logo: 'https://apollospoilers.com/assets/logo.svg',
        email: 'ApolloSpoilers@gmail.com',
        telephone: '+91-8076642594',
        contactPoint: [
          {
            '@type': 'ContactPoint',
            telephone: '+91-8076642594',
            contactType: 'customer support',
            availableLanguage: ['English', 'Hindi'],
          },
          {
            '@type': 'ContactPoint',
            telephone: '+91-9717714997',
            contactType: 'sales',
            availableLanguage: ['English', 'Hindi'],
          },
        ],
        address: {
          '@type': 'PostalAddress',
          addressCountry: 'IN',
        },
        sameAs: [
          'https://instagram.com/apollospoilers',
          'https://youtube.com/@apollospoilers',
          'https://facebook.com/apollospoilers',
        ],
      },
    };

    // FAQ Schema — rich results eligible
    const faqSchema = {
      '@context': 'https://schema.org',
      '@type': 'FAQPage',
      mainEntity: this.faqs.map(f => ({
        '@type': 'Question',
        name: f.q,
        acceptedAnswer: {
          '@type': 'Answer',
          text: f.a,
        },
      })),
    };

    // BreadcrumbList Schema
    const breadcrumbSchema = {
      '@context': 'https://schema.org',
      '@type': 'BreadcrumbList',
      itemListElement: [
        { '@type': 'ListItem', position: 1, name: 'Home', item: 'https://apollospoilers.com' },
        { '@type': 'ListItem', position: 2, name: 'Contact', item: 'https://apollospoilers.com/contact' },
      ],
    };

    [webPageSchema, faqSchema, breadcrumbSchema].forEach(schema => {
      const script = document.createElement('script');
      script.type = 'application/ld+json';
      script.setAttribute('data-contact-jsonld', '');
      script.textContent = JSON.stringify(schema);
      document.head.appendChild(script);
    });
  }
}
