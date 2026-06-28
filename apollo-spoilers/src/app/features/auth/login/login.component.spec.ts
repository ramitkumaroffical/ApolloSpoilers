import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { LoginComponent } from './login.component';
import { AuthService } from '../../../core/services/auth.service';

describe('LoginComponent', () => {
  let mockAuth: { login: ReturnType<typeof vi.fn> };
  let router: Router;
  let route: ActivatedRoute;

  beforeEach(async () => {
    mockAuth = { login: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        { provide: AuthService, useValue: mockAuth },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    route = TestBed.inject(ActivatedRoute);
    vi.spyOn(router, 'navigateByUrl');
  });

  function create() {
    const fixture = TestBed.createComponent(LoginComponent);
    fixture.detectChanges();
    return fixture;
  }

  it('creates and builds the login form', () => {
    const fixture = create();
    expect(fixture.componentInstance).toBeTruthy();
    expect(fixture.componentInstance.form.value.email).toBe('');
    expect(fixture.componentInstance.form.value.password).toBe('');
  });

  it('does not submit when form is invalid', () => {
    const fixture = create();
    fixture.componentInstance.submit();
    expect(mockAuth.login).not.toHaveBeenCalled();
  });

  it('calls login and navigates on success', () => {
    mockAuth.login.mockReturnValue(of({ accessToken: 't', refreshToken: 'r', user: {} }));
    const fixture = create();
    fixture.componentInstance.form.patchValue({ email: 'a@b.com', password: 'pass' });
    fixture.componentInstance.submit();
    expect(mockAuth.login).toHaveBeenCalledWith('a@b.com', 'pass');
    expect(router.navigateByUrl).toHaveBeenCalledWith('/');
  });

  it('shows error on login failure', () => {
    mockAuth.login.mockReturnValue(throwError(() => new Error('Bad creds')));
    const fixture = create();
    fixture.componentInstance.form.patchValue({ email: 'a@b.com', password: 'pass' });
    fixture.componentInstance.submit();
    expect(fixture.componentInstance.error()).toBeTruthy();
  });

  it('clears error before submitting', () => {
    const fixture = create();
    fixture.componentInstance.form.patchValue({ email: 'a@b.com', password: 'pass' });
    fixture.componentInstance.error.set('old error');
    mockAuth.login.mockReturnValue(of({ accessToken: 't', refreshToken: 'r', user: {} }));
    fixture.componentInstance.submit();
    expect(fixture.componentInstance.error()).toBeNull();
  });
});
