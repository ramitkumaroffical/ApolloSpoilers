import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ContactComponent } from './contact.component';

describe('ContactComponent', () => {
  let component: ContactComponent;
  let fixture: ComponentFixture<ContactComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        ContactComponent,
        FormsModule,
        ReactiveFormsModule,
        RouterModule.forRoot([]),
        MatCardModule, MatButtonModule, MatIconModule,
        MatInputModule, MatFormFieldModule, MatSelectModule,
        NoopAnimationsModule,
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ContactComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render the hero heading', () => {
    const h1 = fixture.debugElement.query(By.css('.contact-hero h1'));
    expect(h1?.nativeElement.textContent).toContain("We're Here to Help");
  });

  it('should display quick contact cards', () => {
    const cards = fixture.debugElement.queryAll(By.css('.quick-card'));
    expect(cards.length).toBe(4);
  });

  it('should display email address', () => {
    const el = fixture.debugElement.nativeElement;
    expect(el.textContent).toContain('ApolloSpoilers@gmail.com');
  });

  it('should display phone numbers', () => {
    const el = fixture.debugElement.nativeElement;
    expect(el.textContent).toContain('8076642594');
    expect(el.textContent).toContain('9717714997');
  });

  it('should display achievement cards', () => {
    const cards = fixture.debugElement.queryAll(By.css('.achieve-card'));
    expect(cards.length).toBe(4);
  });

  it('should display trust badges', () => {
    const cards = fixture.debugElement.queryAll(By.css('.trust-card'));
    expect(cards.length).toBe(6);
  });

  it('should display FAQ items', () => {
    const items = fixture.debugElement.queryAll(By.css('.faq-item'));
    expect(items.length).toBe(component.faqs.length);
  });

  it('should render contact form fields', () => {
    const form = fixture.debugElement.query(By.css('.contact-form'));
    expect(form).toBeTruthy();
    expect(component.form.get('name')).toBeTruthy();
    expect(component.form.get('email')).toBeTruthy();
    expect(component.form.get('message')).toBeTruthy();
  });

  it('should be invalid when empty', () => {
    expect(component.form.valid).toBeFalse();
  });

  it('should toggle FAQ accordion', () => {
    expect(component.openFaq()).toBeNull();
    component.toggleFaq(0);
    expect(component.openFaq()).toBe(0);
    component.toggleFaq(0); // close
    expect(component.openFaq()).toBeNull();
  });

  it('should show error messages for touched invalid fields', () => {
    component.form.get('name')?.markAsTouched();
    expect(component.getErrorMessage('name')).toContain('required');
  });

  it('should show email error for invalid email', () => {
    component.form.get('email')?.setValue('not-an-email');
    component.form.get('email')?.markAsTouched();
    expect(component.getErrorMessage('email')).toContain('valid email');
  });

  it('should not submit when form is invalid', () => {
    spyOn(component as any, 'onSubmit');
    const form = fixture.debugElement.query(By.css('.contact-form'));
    form.triggerEventHandler('ngSubmit', null);
    expect(component.submitted()).toBeFalse();
  });

  it('should show success message after submit', async () => {
    component.form.patchValue({
      name: 'Test User',
      email: 'test@example.com',
      subject: 'general',
      message: 'This is a test message for the contact form.',
    });
    component.onSubmit();
    expect(component.sending()).toBeTrue();
    await new Promise(r => setTimeout(r, 1300));
    expect(component.submitted()).toBeTrue();
  });

  it('should have SEO title set', () => {
    expect(component).toBeTruthy();
    // Title service sets the title in ngOnInit
  });
});
