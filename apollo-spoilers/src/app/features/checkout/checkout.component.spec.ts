import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { CheckoutComponent } from './checkout.component';
import { CartService } from '../../core/services/cart.service';
import { OrderService } from '../../core/services/order.service';
import { Cart } from '../../core/models/models';

describe('CheckoutComponent', () => {
  const emptyCart: Cart = { id: 'c1', items: [], subtotal: 0, totalItems: 0 };
  let mockCartService: { cart: ReturnType<typeof vi.fn>; get: ReturnType<typeof vi.fn> };
  let mockOrderService: { placeOrder: ReturnType<typeof vi.fn> };
  let router: Router;

  beforeEach(async () => {
    mockCartService = {
      cart: vi.fn().mockReturnValue(emptyCart),
      get: vi.fn().mockReturnValue(of(emptyCart)),
    };
    mockOrderService = { placeOrder: vi.fn() };
    vi.useFakeTimers();

    await TestBed.configureTestingModule({
      imports: [CheckoutComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        { provide: CartService, useValue: mockCartService },
        { provide: OrderService, useValue: mockOrderService },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate');
  });

  afterEach(() => { vi.useRealTimers(); });

  function create() {
    const fixture = TestBed.createComponent(CheckoutComponent);
    fixture.detectChanges();
    return fixture;
  }

  it('creates and builds the form', () => {
    const fixture = create();
    expect(fixture.componentInstance).toBeTruthy();
    expect(fixture.componentInstance.form.value.shippingFullName).toBe('');
  });

  it('does not submit when form is invalid', () => {
    const fixture = create();
    fixture.componentInstance.submit();
    expect(mockOrderService.placeOrder).not.toHaveBeenCalled();
  });

  it('places order and navigates on success', () => {
    mockOrderService.placeOrder.mockReturnValue(of({ id: 'o1' }));
    const fixture = create();
    const cmp = fixture.componentInstance;
    cmp.form.patchValue({
      shippingFullName: 'Test User',
      shippingAddressLine: '123 St',
      shippingCity: 'City',
      shippingState: 'ST',
      shippingPostalCode: '12345',
      shippingCountry: 'US',
    });
    cmp.submit();
    expect(mockOrderService.placeOrder).toHaveBeenCalled();
    expect(cmp.success()).toBe(true);
    vi.advanceTimersByTime(1500);
    expect(router.navigate).toHaveBeenCalledWith(['/orders']);
  });

  it('shows error on failure', () => {
    mockOrderService.placeOrder.mockReturnValue(throwError(() => new Error('fail')));
    const fixture = create();
    fixture.componentInstance.form.patchValue({
      shippingFullName: 'Test',
      shippingAddressLine: '123',
      shippingCity: 'C',
      shippingState: 'S',
      shippingPostalCode: '00000',
      shippingCountry: 'U',
    });
    fixture.componentInstance.submit();
    expect(fixture.componentInstance.error()).toBeTruthy();
  });
});
