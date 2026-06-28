import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { ProductListComponent } from './product-list.component';
import { CatalogService } from '../../../core/services/catalog.service';
import { CartService } from '../../../core/services/cart.service';
import { PagedResult } from '../../../core/models/models';

describe('ProductListComponent', () => {
  const emptyResult: PagedResult<any> = { items: [], totalCount: 0, page: 1, pageSize: 12 };
  let mockCatalog: {
    search: ReturnType<typeof vi.fn>;
    categories: ReturnType<typeof vi.fn>;
    carBrands: ReturnType<typeof vi.fn>;
    carModels: ReturnType<typeof vi.fn>;
  };
  let mockCart: { add: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    mockCatalog = {
      search: vi.fn().mockReturnValue(of(emptyResult)),
      categories: vi.fn().mockReturnValue(of([])),
      carBrands: vi.fn().mockReturnValue(of([])),
      carModels: vi.fn().mockReturnValue(of([])),
    };
    mockCart = { add: vi.fn().mockReturnValue(of({})) };

    await TestBed.configureTestingModule({
      imports: [ProductListComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        { provide: CatalogService, useValue: mockCatalog },
        { provide: CartService, useValue: mockCart },
      ],
    }).compileComponents();
  });

  function create() {
    const fixture = TestBed.createComponent(ProductListComponent);
    fixture.detectChanges();
    return fixture;
  }

  it('creates and loads categories, brands, and products on init', () => {
    const fixture = create();
    expect(fixture.componentInstance).toBeTruthy();
    expect(mockCatalog.categories).toHaveBeenCalled();
    expect(mockCatalog.carBrands).toHaveBeenCalled();
    expect(mockCatalog.search).toHaveBeenCalled();
  });

  it('delegates addToCart to CartService', () => {
    const fixture = create();
    fixture.componentInstance.addToCart({ id: 'p1' } as any);
    expect(mockCart.add).toHaveBeenCalledWith('p1');
  });

  it('resets filters and reloads', () => {
    const fixture = create();
    fixture.componentInstance.resetFilters();
    expect(fixture.componentInstance.filters.search).toBe('');
    expect(mockCatalog.search).toHaveBeenCalledTimes(2); // init + reset
  });
});
