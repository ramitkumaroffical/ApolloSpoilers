import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { WishlistComponent } from './wishlist.component';
import { WishlistService } from '../../core/services/wishlist.service';
import { CartService } from '../../core/services/cart.service';
import { Wishlist } from '../../core/models/models';

describe('WishlistComponent', () => {
  const emptyWishlist: Wishlist = { id: 'w1', items: [], totalItems: 0 };
  let mockWishlistService: {
    get: ReturnType<typeof vi.fn>;
    remove: ReturnType<typeof vi.fn>;
  };
  let mockCartService: { add: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    mockWishlistService = {
      get: vi.fn().mockReturnValue(of(emptyWishlist)),
      remove: vi.fn().mockReturnValue(of(void 0)),
    };
    mockCartService = { add: vi.fn().mockReturnValue(of(void 0)) };

    await TestBed.configureTestingModule({
      imports: [WishlistComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        { provide: WishlistService, useValue: mockWishlistService },
        { provide: CartService, useValue: mockCartService },
      ],
    }).compileComponents();
  });

  function create() {
    const fixture = TestBed.createComponent(WishlistComponent);
    fixture.detectChanges();
    return fixture;
  }

  it('creates and fetches wishlist on init', () => {
    const fixture = create();
    expect(fixture.componentInstance).toBeTruthy();
    expect(mockWishlistService.get).toHaveBeenCalled();
  });

  it('delegates add to CartService', () => {
    const fixture = create();
    fixture.componentInstance.add('p1');
    expect(mockCartService.add).toHaveBeenCalledWith('p1');
  });

  it('delegates remove and refreshes wishlist', () => {
    mockWishlistService.get.mockReturnValue(of(emptyWishlist));
    const fixture = create();
    fixture.componentInstance.remove('p1');
    expect(mockWishlistService.remove).toHaveBeenCalledWith('p1');
    expect(mockWishlistService.get).toHaveBeenCalledTimes(2); // init + refresh
  });
});
