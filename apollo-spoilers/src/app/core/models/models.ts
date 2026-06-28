// ====== Models matching backend DTOs ======

export interface UserProfile {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  fullName: string;
  roles: string[];
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  refreshTokenExpiry: string;
  user: UserProfile;
}

export interface Category {
  id: string;
  name: string;
  slug: string;
  description?: string;
  parentCategoryId?: string;
  productCount: number;
}

export interface ProductImage {
  id: string;
  productId: string;
  imageUrl: string;
  altText?: string;
  isPrimary: boolean;
  displayOrder: number;
}

export interface ProductListItem {
  id: string;
  name: string;
  slug: string;
  price: number;
  compareAtPrice?: number;
  carBrand?: string;
  carModel?: string;
  primaryImageUrl?: string;
  averageRating: number;
  reviewCount: number;
  stockQuantity: number;
  isFeatured: boolean;
  categoryName?: string;
}

export interface Review {
  id: string;
  productId: string;
  rating: number;
  comment?: string;
  authorName: string;
  createdAt: string;
  isApproved: boolean;
}

export interface ProductDetail {
  id: string;
  name: string;
  slug: string;
  description: string;
  price: number;
  compareAtPrice?: number;
  material?: string;
  color?: string;
  carBrand?: string;
  carModel?: string;
  fitYearFrom?: number;
  fitYearTo?: number;
  categoryId: string;
  categoryName?: string;
  isActive: boolean;
  isFeatured: boolean;
  averageRating: number;
  reviewCount: number;
  stockQuantity: number;
  lowStockThreshold: number;
  images: ProductImage[];
  recentReviews: Review[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface CartItem {
  id: string;
  productId: string;
  productName: string;
  productSlug?: string;
  primaryImageUrl?: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  availableStock: number;
}

export interface Cart {
  id: string;
  items: CartItem[];
  subtotal: number;
  totalItems: number;
}

export interface WishlistItem {
  id: string;
  productId: string;
  productName: string;
  productSlug?: string;
  price: number;
  primaryImageUrl?: string;
}

export interface Wishlist {
  id: string;
  items: WishlistItem[];
}

export type OrderStatus = 'Pending' | 'Confirmed' | 'Shipped' | 'Delivered' | 'Cancelled';

// Simulated payment methods supported at checkout
export type PaymentMethod = 'Card' | 'UPI' | 'Paytm' | 'COD';

export interface PaymentInfo {
  method: PaymentMethod;
  /** Masked reference, e.g. last 4 of card, masked UPI id, masked wallet — for receipts only */
  reference?: string;
}

export interface OrderItem {
  id: string;
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface Order {
  id: string;
  orderNumber: string;
  createdAt: string;
  status: OrderStatus;
  paymentMethod: PaymentMethod;
  paymentReference?: string;
  subtotal: number;
  shippingCost: number;
  totalAmount: number;
  shippingFullName: string;
  shippingAddressLine: string;
  shippingCity: string;
  shippingState: string;
  shippingPostalCode: string;
  shippingCountry: string;
  shippingPhone?: string;
  items: OrderItem[];
}

export interface ChatSource {
  type: string;
  productId?: string;
  productSlug?: string;
  productName?: string;
  score: number;
}

export interface ChatResponse {
  sessionId: string;
  answer: string;
  sources: ChatSource[];
}

export interface ChatMessage {
  id: string;
  role: string;
  content: string;
  createdAt: string;
}

export interface ProductQuery {
  search?: string;
  categoryId?: string;
  carBrand?: string;
  carModel?: string;
  minPrice?: number;
  maxPrice?: number;
  isFeatured?: boolean;
  sortBy?: 'Newest' | 'PriceAscending' | 'PriceDescending' | 'Rating' | 'NameAscending';
  page?: number;
  pageSize?: number;
}

// ====== Admin Product DTOs ======

export interface CreateProductRequest {
  name: string;
  description: string;
  price: number;
  compareAtPrice?: number;
  material?: string;
  color?: string;
  carBrand?: string;
  carModel?: string;
  fitYearFrom?: number;
  fitYearTo?: number;
  categoryId: string;
  isActive: boolean;
  isFeatured: boolean;
  initialStock: number;
  lowStockThreshold: number;
}

export interface UpdateProductRequest extends CreateProductRequest {}

export interface ImageUploadResponse {
  imageUrl: string;
}
