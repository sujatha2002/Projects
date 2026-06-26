import { Component, OnInit } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { CartService } from '../../../core/services/cart.service';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  template: `
    <div class="page">
      <h1>Your Cart</h1>

      @if (loadError) {
        <div class="error-msg">{{ loadError }}</div>
      }

      @if (!cartService.cart()?.items?.length) {
        <div class="empty">Your cart is empty. Browse recipes to add items!</div>
      } @else {
        <div class="cart-layout">
          <div class="cart-items">
            @for (item of cartService.cart()!.items; track item.id) {
              <div class="cart-item">
                <div class="item-info">
                  <h3>{{ item.recipeName }}</h3>
                  <span class="price">₹{{ item.price | number:'1.0-0' }} each</span>
                </div>
                <div class="quantity-controls">
                  <button (click)="updateQty(item.id, item.quantity - 1)">-</button>
                  <span>{{ item.quantity }}</span>
                  <button (click)="updateQty(item.id, item.quantity + 1)">+</button>
                </div>
                <span class="subtotal">₹{{ item.subtotal | number:'1.0-0' }}</span>
                <button class="btn-remove" (click)="removeItem(item.id)">✕</button>
              </div>
            }
            <div class="total-row">
              <strong>Total</strong>
              <strong class="total">₹{{ cartService.cart()!.total | number:'1.0-0' }}</strong>
            </div>
            <button class="btn-checkout" (click)="checkout()">Place Order</button>
          </div>

          <div class="sidebar">
            @if (cartService.cart()!.suggestions.length) {
              <section class="suggestions">
                <h2>AI Suggestions</h2>
                <p class="hint">Complete your meal with these picks</p>
                @for (s of cartService.cart()!.suggestions; track s.recipeId) {
                  <div class="suggestion-item">
                    <div>
                      <strong>{{ s.recipeName }}</strong>
                      <p>{{ s.reason }}</p>
                    </div>
                    <div class="suggestion-action">
                      <span>₹{{ s.price | number:'1.0-0' }}</span>
                      <button (click)="addSuggestion(s.recipeId)">Add</button>
                    </div>
                  </div>
                }
              </section>
            }

            @if (cartService.cart()!.bundleOffers.length) {
              <section class="bundles">
                <h2>Bundle Deals</h2>
                @for (b of cartService.cart()!.bundleOffers; track b.bundleId) {
                  <div class="bundle-card">
                    <h3>{{ b.name }}</h3>
                    <p>{{ b.description }}</p>
                    <div class="bundle-items">
                      @for (item of b.items; track item.id) {
                        <span>{{ item.name }}</span>
                      }
                    </div>
                    <div class="bundle-footer">
                      <span class="savings">Save ₹{{ b.savingsAmount | number:'1.0-0' }} ({{ b.discountPercent }}% off)</span>
                      <button (click)="addBundle(b)">Add Bundle</button>
                    </div>
                  </div>
                }
              </section>
            }
          </div>
        </div>
      }

      @if (orderMessage) {
        <div class="success">{{ orderMessage }}</div>
      }
    </div>
  `,
  styles: [`
    .page { max-width: 1000px; margin: 0 auto; padding: 2rem; }
    h1 { color: #2c3e50; }
    .empty { text-align: center; padding: 4rem; color: #95a5a6; font-size: 1.1rem; }
    .error-msg { background: #fde8e8; color: #c0392b; padding: 1rem; border-radius: 8px; margin-bottom: 1rem; }
    .cart-layout { display: grid; grid-template-columns: 1fr 360px; gap: 2rem; }
    .cart-item { display: flex; align-items: center; gap: 1rem; padding: 1rem; background: white; border-radius: 10px; margin-bottom: 0.75rem; box-shadow: 0 2px 8px rgba(0,0,0,0.06); }
    .item-info { flex: 1; }
    .item-info h3 { margin: 0; color: #2c3e50; }
    .quantity-controls { display: flex; align-items: center; gap: 0.5rem; }
    .quantity-controls button { width: 30px; height: 30px; border: 1px solid #ddd; border-radius: 6px; background: white; cursor: pointer; }
    .subtotal { font-weight: 600; min-width: 70px; text-align: right; }
    .btn-remove { background: none; border: none; color: #e74c3c; cursor: pointer; font-size: 1.1rem; }
    .total-row { display: flex; justify-content: space-between; padding: 1rem 0; border-top: 2px solid #eee; margin-top: 1rem; font-size: 1.2rem; }
    .total { color: #e74c3c; }
    .btn-checkout { width: 100%; padding: 1rem; background: #27ae60; color: white; border: none; border-radius: 10px; font-size: 1.1rem; font-weight: 600; cursor: pointer; margin-top: 1rem; }
    .sidebar section { background: white; border-radius: 12px; padding: 1.25rem; margin-bottom: 1.5rem; box-shadow: 0 2px 10px rgba(0,0,0,0.06); }
    .sidebar h2 { margin: 0 0 0.5rem; color: #2c3e50; font-size: 1.1rem; }
    .hint { color: #95a5a6; font-size: 0.85rem; margin-bottom: 1rem; }
    .suggestion-item { display: flex; justify-content: space-between; align-items: center; padding: 0.75rem 0; border-bottom: 1px solid #f0f0f0; }
    .suggestion-item p { margin: 0.2rem 0 0; font-size: 0.8rem; color: #e74c3c; font-style: italic; }
    .suggestion-action { text-align: right; }
    .suggestion-action button { background: #e74c3c; color: white; border: none; padding: 0.3rem 0.8rem; border-radius: 6px; cursor: pointer; margin-top: 0.3rem; }
    .bundle-card { border: 2px dashed #f39c12; border-radius: 10px; padding: 1rem; margin-bottom: 1rem; }
    .bundle-card h3 { margin: 0 0 0.3rem; color: #e67e22; }
    .bundle-items { display: flex; flex-wrap: wrap; gap: 0.3rem; margin: 0.5rem 0; }
    .bundle-items span { background: #fef9e7; padding: 0.2rem 0.5rem; border-radius: 6px; font-size: 0.8rem; }
    .bundle-footer { display: flex; justify-content: space-between; align-items: center; }
    .savings { color: #27ae60; font-weight: 600; font-size: 0.9rem; }
    .bundle-footer button { background: #f39c12; color: white; border: none; padding: 0.4rem 1rem; border-radius: 6px; cursor: pointer; }
    .success { background: #d5f5e3; color: #27ae60; padding: 1rem; border-radius: 10px; margin-top: 1rem; text-align: center; }
    @media (max-width: 768px) { .cart-layout { grid-template-columns: 1fr; } }
  `]
})
export class CartComponent implements OnInit {
  orderMessage = '';
  loadError = '';

  constructor(public cartService: CartService) {}

  ngOnInit() { this.loadCart(); }

  loadCart() {
    this.cartService.getCart().subscribe({
      next: () => { this.loadError = ''; },
      error: () => this.loadError = 'Could not load cart. Please log in again.'
    });
  }

  updateQty(id: number, qty: number) {
    this.cartService.updateQuantity(id, qty).subscribe();
  }

  removeItem(id: number) {
    this.cartService.removeItem(id).subscribe();
  }

  addSuggestion(recipeId: number) {
    this.cartService.addToCart(recipeId).subscribe();
  }

  addBundle(bundle: { items: { id: number }[] }) {
    bundle.items.forEach(item => {
      this.cartService.addToCart(item.id).subscribe();
    });
  }

  checkout() {
    this.cartService.checkout().subscribe(res => {
      this.orderMessage = res.message;
      this.loadCart();
    });
  }
}
