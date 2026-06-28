import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { ProductDetailComponent } from './product-detail.component';
import { CatalogService } from '../../../core/services/catalog.service';
import { CartService } from '../../../core/services/cart.service';
import { AuthService } from '../../../core/services/auth.service';
import { ProductDetail } from '../../../core/models/models';

describe('ProductDetailComponent', () => {
  const mockProduct: ProductDetail = {
    id: 'p1',
    name: 'Test Spoiler',
    slug: 'test-spoiler',
    price: 299,
    description: 'Great spoiler',
    stockQuantity: 5,
    categoryName: 'Spoilers',
    averageRating: 4.5,
    reviewCount: 10,
    images: [{ id: 'img1', imageUrl: '/img.jpg', altText: 'Test', isPrimary: true }],
  };
  let mockCatalog: {
    getBySlug: ReturnType<typeof vi.fn>;
    reviews: ReturnType<typeof vi.fn>;
  };
  let mockCart: { add: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    mockCatalog = {
      getBySlug: vi.fn().mockReturnValue(of(mockProduct)),
      reviews: vi.fn().mockReturnValue(of([])),
    };
    mockCart = { add: vi.fn().mockReturnValue(of({})) };

    await TestBed.configureTestingModule({
      imports: [ProductDetailComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        { provide: CatalogService, useValue: mockCatalog },
        { provide: CartService, useValue: mockCart },
        { provide: AuthService, useValue: { currentUser: vi.fn() } },
      ],
    }).compileComponents();
  });

  function create(slug = 'test-spoiler') {
    const fixture = TestBed.createComponent(ProductDetailComponent);
    fixture.componentRef.setInput('slug', slug);
    fixture.detectChanges();
    return fixture;
  }

  it('creates and loads product by slug', () => {
    const fixture = create();
    expect(fixture.componentInstance).toBeTruthy();
    expect(mockCatalog.getBySlug).toHaveBeenCalledWith('test-spoiler');
    expect(fixture.componentInstance.product()).toEqual(mockProduct);
  });

  it('sets selected image to primary', () => {
    const fixture = create();
    expect(fixture.componentInstance.selectedImage()).toBe('/img.jpg');
  });

  it('delegates addToCart to CartService', () => {
    const fixture = create();
    fixture.componentInstance.addToCart();
    expect(mockCart.add).toHaveBeenCalledWith('p1');
  });

  it('does not add to cart when product is null', () => {
    const fixture = create();
    fixture.componentInstance.product.set(null);
    fixture.componentInstance.addToCart();
    expect(mockCart.add).not.toHaveBeenCalled();
  });
});
