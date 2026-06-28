import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { CartComponent } from './cart.component';
import { CartService } from '../../core/services/cart.service';
import { Cart } from '../../core/models/models';

describe('CartComponent', () => {
  const emptyCart: Cart = { id: 'c1', items: [], subtotal: 0, totalItems: 0 };
  let mockCartService: {
    cart: ReturnType<typeof vi.fn>;
    get: ReturnType<typeof vi.fn>;
    update: ReturnType<typeof vi.fn>;
    remove: ReturnType<typeof vi.fn>;
    clear: ReturnType<typeof vi.fn>;
  };
  let router: Router;

  beforeEach(async () => {
    mockCartService = {
      cart: vi.fn().mockReturnValue(emptyCart),
      get: vi.fn().mockReturnValue(of(emptyCart)),
      update: vi.fn().mockReturnValue(of(emptyCart)),
      remove: vi.fn().mockReturnValue(of(void 0)),
      clear: vi.fn().mockReturnValue(of(void 0)),
    };

    await TestBed.configureTestingModule({
      imports: [CartComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        { provide: CartService, useValue: mockCartService },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate');
  });

  function create() {
    const fixture = TestBed.createComponent(CartComponent);
    fixture.detectChanges();
    return fixture;
  }

  it('creates and loads the cart on init', () => {
    const fixture = create();
    expect(fixture.componentInstance).toBeTruthy();
    expect(mockCartService.get).toHaveBeenCalled();
  });

  it('delegates update/remove/clear to CartService', () => {
    const fixture = create();
    const cmp = fixture.componentInstance;

    cmp.update('i1', 3);
    expect(mockCartService.update).toHaveBeenCalledWith('i1', 3);

    cmp.remove('i1');
    expect(mockCartService.remove).toHaveBeenCalledWith('i1');

    cmp.clear();
    expect(mockCartService.clear).toHaveBeenCalled();
  });

  it('navigates to checkout', () => {
    const fixture = create();
    fixture.componentInstance.checkout();
    expect(router.navigate).toHaveBeenCalledWith(['/checkout']);
  });
});
