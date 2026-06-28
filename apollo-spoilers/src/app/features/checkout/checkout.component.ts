import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Meta } from '@angular/platform-browser';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatRadioModule } from '@angular/material/radio';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CartService } from '../../core/services/cart.service';
import { OrderService } from '../../core/services/order.service';
import { NotificationService } from '../../core/services/notification.service';
import { extractErrorMessage } from '../../core/interceptors/auth.interceptor';
import type { PaymentMethod, PaymentInfo } from '../../core/models/models';

type Step = 1 | 2 | 3;

function cardExpiryValidator(control: AbstractControl): ValidationErrors | null {
  const val = (control.value || '') as string;
  if (!/^\d{2}\/\d{2}$/.test(val)) return { pattern: true };
  const [mm] = val.split('/').map(Number);
  if (mm < 1 || mm > 12) return { month: true };
  return null;
}

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatRadioModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css',
})
export class CheckoutComponent implements OnInit {
  private fb = inject(FormBuilder);
  private cartSvc = inject(CartService);
  private orderSvc = inject(OrderService);
  private router = inject(Router);
  private notify = inject(NotificationService);
  private meta = inject(Meta);

  ngOnInit(): void {
    this.meta.updateTag({ name: 'robots', content: 'noindex, nofollow' });
  }

  readonly cart = this.cartSvc.cart;
  readonly error = signal<string | null>(null);
  readonly success = signal(false);
  readonly submitting = signal(false);

  // Step wizard
  readonly step = signal<Step>(1);

  // Payment methods for template iteration
  readonly paymentMethods: PaymentMethod[] = ['Card', 'UPI', 'Paytm', 'COD'];

  // Payment state
  readonly selectedMethod = signal<PaymentMethod | null>(null);
  readonly processingPayment = signal(false);

  // Card form
  readonly cardForm = this.fb.nonNullable.group({
    cardNumber: ['', [Validators.required, Validators.pattern(/^\d{16}$/)]],
    cardName: ['', Validators.required],
    expiry: ['', [Validators.required, cardExpiryValidator]],
    cvv: ['', [Validators.required, Validators.pattern(/^\d{3,4}$/)]],
  });

  // UPI form
  readonly upiForm = this.fb.nonNullable.group({
    upiId: ['', [Validators.required, Validators.pattern(/^[a-zA-Z0-9.\-_]+@[a-zA-Z]{2,}$/)]],
  });

  // Paytm form
  readonly paytmForm = this.fb.nonNullable.group({
    paytmNumber: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
  });

  // Shipping form
  readonly form = this.fb.nonNullable.group({
    shippingFullName: ['', Validators.required],
    shippingAddressLine: ['', Validators.required],
    shippingCity: ['', Validators.required],
    shippingState: ['', Validators.required],
    shippingPostalCode: ['', Validators.required],
    shippingCountry: ['', Validators.required],
    shippingPhone: [''],
  });

  // Payment info stored after simulated payment success
  readonly paymentInfo = signal<PaymentInfo | null>(null);

  // Masked display helpers
  readonly maskedCard = computed(() => {
    const num = this.cardForm.value.cardNumber;
    if (!num || num.length < 4) return '';
    return '•••• •••• •••• ' + num.slice(-4);
  });

  readonly maskedUpi = computed(() => {
    const id = this.upiForm.value.upiId;
    if (!id) return '';
    const [user] = id.split('@');
    return user.slice(0, 2) + '••••@' + id.split('@')[1];
  });

  readonly maskedPaytm = computed(() => {
    const num = this.paytmForm.value.paytmNumber;
    if (!num || num.length < 4) return '';
    return '••••••' + num.slice(-4);
  });

  // Formatted card number and expiry for template bindings
  readonly formattedCardNum = computed(() => this.formatCardNumber(this.cardForm.value.cardNumber));
  readonly formattedExpiry = computed(() => this.formatExpiry(this.cardForm.value.expiry));

  // Method helpers — typed, no cast needed in template
  getIcon(m: PaymentMethod): string {
    const map: Record<PaymentMethod, string> = {
      Card: 'credit_card', UPI: 'account_balance', Paytm: 'phone_android', COD: 'payments',
    };
    return map[m];
  }

  getLabel(m: PaymentMethod): string {
    const map: Record<PaymentMethod, string> = {
      Card: 'Credit / Debit Card', UPI: 'UPI (GPay / PhonePe / BHIM)', Paytm: 'Paytm Wallet', COD: 'Cash on Delivery',
    };
    return map[m];
  }

  getDesc(m: PaymentMethod): string {
    const map: Record<PaymentMethod, string> = {
      Card: 'Visa, Mastercard, RuPay accepted', UPI: 'Pay directly via any UPI app', Paytm: 'Pay using your Paytm wallet', COD: 'Pay when your order is delivered',
    };
    return map[m];
  }

  // Step navigation
  get canProceedShipping(): boolean {
    return this.form.valid;
  }

  get canProceedPayment(): boolean {
    const m = this.selectedMethod();
    if (!m) return false;
    if (m === 'Card') return this.cardForm.valid;
    if (m === 'UPI') return this.upiForm.valid;
    if (m === 'Paytm') return this.paytmForm.valid;
    return true; // COD needs no form
  }

  goToStep(s: Step): void {
    this.error.set(null);
    this.step.set(s);
  }

  nextFromShipping(): void {
    if (!this.canProceedShipping) return;
    this.goToStep(2);
  }

  backToShipping(): void {
    this.goToStep(1);
  }

  selectMethod(m: PaymentMethod): void {
    this.selectedMethod.set(m);
  }

  // Simulated payment processing
  processPayment(): void {
    if (!this.canProceedPayment) return;
    const m = this.selectedMethod();
    if (!m) return;

    this.processingPayment.set(true);
    this.error.set(null);

    // Simulate 1.5-2.5s payment gateway delay
    const delay = 1500 + Math.random() * 1000;
    setTimeout(() => {
      this.processingPayment.set(false);

      // Build masked reference for the receipt
      const info: PaymentInfo = { method: m };
      if (m === 'Card') info.reference = this.maskedCard();
      else if (m === 'UPI') info.reference = this.maskedUpi();
      else if (m === 'Paytm') info.reference = this.maskedPaytm();

      this.paymentInfo.set(info);

      // Move to review step
      this.goToStep(3);
    }, delay);
  }

  // Final order placement
  placeOrder(): void {
    if (this.form.invalid || !this.paymentInfo()) return;
    this.error.set(null);
    this.submitting.set(true);

    const shipping = this.form.getRawValue();
    const payment = this.paymentInfo()!;

    this.orderSvc.placeOrder({
      ...shipping,
      paymentMethod: payment.method,
      paymentReference: payment.reference,
    }).subscribe({
      next: () => {
        this.success.set(true);
        this.submitting.set(false);
        this.cartSvc.clear().subscribe();
        this.notify.success('Order placed successfully! 🎉');
        setTimeout(() => this.router.navigate(['/orders']), 2000);
      },
      error: (e) => {
        this.error.set(extractErrorMessage(e));
        this.submitting.set(false);
      },
    });
  }

  // Raw card number input handler
  onCardNumberInput(event: Event): void {
    const raw = (event.target as HTMLInputElement).value.replace(/\D/g, '').slice(0, 16);
    this.cardForm.get('cardNumber')?.setValue(raw, { emitEvent: false });
  }

  // Raw expiry input handler
  onExpiryInput(event: Event): void {
    const raw = (event.target as HTMLInputElement).value.replace(/\D/g, '').slice(0, 4);
    this.cardForm.get('expiry')?.setValue(raw, { emitEvent: false });
  }

  formatCardNumber(value: string | undefined): string {
    const v = value || '';
    return v.replace(/\D/g, '').replace(/(.{4})/g, '$1 ').trim().slice(0, 19);
  }

  formatExpiry(value: string | undefined): string {
    const clean = (value || '').replace(/\D/g, '').slice(0, 4);
    if (clean.length >= 3) return clean.slice(0, 2) + '/' + clean.slice(2);
    return clean;
  }
}
