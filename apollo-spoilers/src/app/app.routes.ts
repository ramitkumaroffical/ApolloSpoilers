import { Routes } from '@angular/router';
import { authGuard, adminGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/products/product-list/product-list.component').then(m => m.ProductListComponent),
    title: 'Buy Premium Car Spoilers, GT Wings & Aero Parts Online India | Apollo Spoilers',
  },
  {
    path: 'products/:slug',
    loadComponent: () => import('./features/products/product-detail/product-detail.component').then(m => m.ProductDetailComponent),
    // Overridden dynamically by component Meta service
    title: 'Product Details — Apollo Spoilers',
  },
  {
    path: 'about',
    loadComponent: () => import('./features/about/about.component').then(m => m.AboutComponent),
    title: 'About Apollo Spoilers — India\'s #1 Premium Car Spoiler Brand',
  },
  {
    path: 'contact',
    loadComponent: () => import('./features/contact/contact.component').then(m => m.ContactComponent),
    title: 'Contact Apollo Spoilers — Support, Custom Orders & Enquiries',
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent),
    title: 'Login — Apollo Spoilers',
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent),
    title: 'Create Account — Apollo Spoilers',
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./features/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent),
    title: 'Forgot Password — Apollo Spoilers',
  },
  {
    path: 'reset-password',
    loadComponent: () => import('./features/auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent),
    title: 'Reset Password — Apollo Spoilers',
  },
  {
    path: 'cart',
    canActivate: [authGuard],
    loadComponent: () => import('./features/cart/cart.component').then(m => m.CartComponent),
    title: 'Your Cart — Apollo Spoilers',
  },
  {
    path: 'wishlist',
    canActivate: [authGuard],
    loadComponent: () => import('./features/wishlist/wishlist.component').then(m => m.WishlistComponent),
    title: 'Wishlist — Apollo Spoilers',
  },
  {
    path: 'checkout',
    canActivate: [authGuard],
    loadComponent: () => import('./features/checkout/checkout.component').then(m => m.CheckoutComponent),
    title: 'Secure Checkout — Apollo Spoilers',
  },
  {
    path: 'orders',
    canActivate: [authGuard],
    loadComponent: () => import('./features/orders/order-list/order-list.component').then(m => m.OrderListComponent),
    title: 'Order History — Apollo Spoilers',
  },
  {
    path: 'admin',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./features/admin/admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent),
    title: 'Admin — Apollo Spoilers',
  },
  {
    path: 'admin/products',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./features/admin/product-manage/product-manage.component').then(m => m.ProductManageComponent),
    title: 'Manage Products — Admin — Apollo Spoilers',
  },
  {
    path: 'admin/products/new',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./features/admin/product-form/product-form.component').then(m => m.ProductFormComponent),
    title: 'Add Product — Admin — Apollo Spoilers',
  },
  {
    path: 'admin/products/:id/edit',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./features/admin/product-form/product-form.component').then(m => m.ProductFormComponent),
    title: 'Edit Product — Admin — Apollo Spoilers',
  },
  { path: '**', redirectTo: '' },
];
