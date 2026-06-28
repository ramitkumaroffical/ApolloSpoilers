import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { OrderListComponent } from './order-list.component';
import { OrderService } from '../../../core/services/order.service';

describe('OrderListComponent', () => {
  let mockOrderService: { myOrders: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    mockOrderService = { myOrders: vi.fn().mockReturnValue(of([])) };

    await TestBed.configureTestingModule({
      imports: [OrderListComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        { provide: OrderService, useValue: mockOrderService },
      ],
    }).compileComponents();
  });

  function create() {
    const fixture = TestBed.createComponent(OrderListComponent);
    fixture.detectChanges();
    return fixture;
  }

  it('creates and fetches orders on init', () => {
    const fixture = create();
    expect(fixture.componentInstance).toBeTruthy();
    expect(mockOrderService.myOrders).toHaveBeenCalled();
  });

  it('returns correct chip colors per status', () => {
    const fixture = create();
    const cmp = fixture.componentInstance;
    expect(cmp.statusColor('Pending')).toBe('warn');
    expect(cmp.statusColor('Confirmed')).toBe('primary');
    expect(cmp.statusColor('Shipped')).toBe('primary');
    expect(cmp.statusColor('Delivered')).toBe('accent');
    expect(cmp.statusColor('Unknown')).toBeUndefined();
  });
});
