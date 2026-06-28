import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { RegisterComponent } from './register.component';
import { AuthService } from '../../../core/services/auth.service';

describe('RegisterComponent', () => {
  let mockAuth: { register: ReturnType<typeof vi.fn> };
  let router: Router;

  beforeEach(async () => {
    mockAuth = { register: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [RegisterComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        { provide: AuthService, useValue: mockAuth },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl');
  });

  function create() {
    const fixture = TestBed.createComponent(RegisterComponent);
    fixture.detectChanges();
    return fixture;
  }

  it('creates and builds the register form', () => {
    const fixture = create();
    expect(fixture.componentInstance).toBeTruthy();
    expect(fixture.componentInstance.form.value.firstName).toBe('');
    expect(fixture.componentInstance.form.value.password).toBe('');
  });

  it('does not submit when form is invalid', () => {
    const fixture = create();
    fixture.componentInstance.submit();
    expect(mockAuth.register).not.toHaveBeenCalled();
  });

  it('calls register and navigates to root on success', () => {
    mockAuth.register.mockReturnValue(of({ accessToken: 't', refreshToken: 'r', user: {} }));
    const fixture = create();
    fixture.componentInstance.form.patchValue({
      firstName: 'Test',
      lastName: 'User',
      email: 'a@b.com',
      password: 'Password1!',
    });
    fixture.componentInstance.submit();
    expect(mockAuth.register).toHaveBeenCalled();
    expect(router.navigateByUrl).toHaveBeenCalledWith('/');
  });

  it('shows error on register failure', () => {
    mockAuth.register.mockReturnValue(throwError(() => new Error('Email taken')));
    const fixture = create();
    fixture.componentInstance.form.patchValue({
      firstName: 'Test',
      lastName: 'User',
      email: 'a@b.com',
      password: 'Password1!',
    });
    fixture.componentInstance.submit();
    expect(fixture.componentInstance.error()).toBeTruthy();
  });

  it('clears error before submitting', () => {
    const fixture = create();
    fixture.componentInstance.error.set('old error');
    mockAuth.register.mockReturnValue(of({ accessToken: 't', refreshToken: 'r', user: {} }));
    fixture.componentInstance.form.patchValue({
      firstName: 'T',
      lastName: 'U',
      email: 'a@b.com',
      password: 'Password1!',
    });
    fixture.componentInstance.submit();
    expect(fixture.componentInstance.error()).toBeNull();
  });
});
