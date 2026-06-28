import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { HttpClient } from '@angular/common/http';
import { of } from 'rxjs';
import { AdminDashboardComponent } from './admin-dashboard.component';
import { AuthService } from '../../../core/services/auth.service';

describe('AdminDashboardComponent', () => {
  let mockHttp: { get: ReturnType<typeof vi.fn>; put: ReturnType<typeof vi.fn> };
  let mockAuth: { currentUser: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    mockHttp = {
      get: vi.fn().mockReturnValue(of({ items: [] })),
      put: vi.fn().mockReturnValue(of({})),
    };
    mockAuth = { currentUser: vi.fn().mockReturnValue({ fullName: 'Admin' }) };

    await TestBed.configureTestingModule({
      imports: [AdminDashboardComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        { provide: HttpClient, useValue: mockHttp },
        { provide: AuthService, useValue: mockAuth },
      ],
    }).compileComponents();
  });

  function create() {
    const fixture = TestBed.createComponent(AdminDashboardComponent);
    fixture.detectChanges();
    return fixture;
  }

  it('creates and loads orders and products on init', () => {
    const fixture = create();
    expect(fixture.componentInstance).toBeTruthy();
    expect(mockHttp.get).toHaveBeenCalled();
  });

  it('returns correct chip color per order status', () => {
    const fixture = create();
    const cmp = fixture.componentInstance;
    expect(cmp.color('Pending')).toBe('warn');
    expect(cmp.color('Shipped')).toBe('primary');
    expect(cmp.color('Delivered')).toBe('accent');
    expect(cmp.color('Other')).toBeUndefined();
  });
});
