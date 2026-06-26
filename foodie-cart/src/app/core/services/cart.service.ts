import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, tap, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CartItem, CartSummary, BundleOffer, CartSuggestion } from '../models';

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly api = `${environment.apiUrl}/cart`;
  cart = signal<CartSummary | null>(null);
  cartCount = signal(0);

  constructor(private http: HttpClient) {}

  getCart() {
    return this.http.get<unknown>(this.api).pipe(
      map(data => this.normalizeCart(data)),
      tap(c => this.updateCartState(c)),
      catchError(err => throwError(() => err))
    );
  }

  addToCart(recipeId: number, quantity = 1) {
    return this.http.post<unknown>(`${this.api}/add`, { recipeId, quantity }).pipe(
      map(data => this.normalizeCart(data)),
      tap(c => this.updateCartState(c)),
      catchError(err => throwError(() => err))
    );
  }

  updateQuantity(id: number, quantity: number) {
    return this.http.put<unknown>(`${this.api}/${id}/quantity`, quantity).pipe(
      map(data => this.normalizeCart(data)),
      tap(c => this.updateCartState(c))
    );
  }

  removeItem(id: number) {
    return this.http.delete<unknown>(`${this.api}/${id}`).pipe(
      map(data => this.normalizeCart(data)),
      tap(c => this.updateCartState(c))
    );
  }

  checkout() {
    return this.http.post<{ id: number; totalAmount: number; message: string }>(`${this.api}/checkout`, {}).pipe(
      tap(() => this.updateCartState({ items: [], total: 0, suggestions: [], bundleOffers: [] }))
    );
  }

  private normalizeCart(data: unknown): CartSummary {
    const raw = data as Record<string, unknown>;
    const itemsRaw = (raw['items'] ?? raw['Items'] ?? []) as Record<string, unknown>[];
    const suggestionsRaw = (raw['suggestions'] ?? raw['Suggestions'] ?? []) as Record<string, unknown>[];
    const bundleOffersRaw = (raw['bundleOffers'] ?? raw['BundleOffers'] ?? []) as Record<string, unknown>[];
    const total = Number(raw['total'] ?? raw['Total'] ?? 0);

    const items: CartItem[] = itemsRaw.map(i => ({
      id: Number(i['id'] ?? i['Id']),
      recipeId: Number(i['recipeId'] ?? i['RecipeId']),
      recipeName: String(i['recipeName'] ?? i['RecipeName'] ?? ''),
      price: Number(i['price'] ?? i['Price'] ?? 0),
      quantity: Number(i['quantity'] ?? i['Quantity'] ?? 0),
      subtotal: Number(i['subtotal'] ?? i['Subtotal'] ?? 0),
    }));

    const suggestions: CartSuggestion[] = suggestionsRaw.map(s => ({
      recipeId: Number(s['recipeId'] ?? s['RecipeId']),
      recipeName: String(s['recipeName'] ?? s['RecipeName'] ?? ''),
      price: Number(s['price'] ?? s['Price'] ?? 0),
      reason: String(s['reason'] ?? s['Reason'] ?? ''),
      confidence: Number(s['confidence'] ?? s['Confidence'] ?? 0),
    }));

    const bundleOffers: BundleOffer[] = bundleOffersRaw.map(b => ({
      bundleId: Number(b['bundleId'] ?? b['BundleId']),
      name: String(b['name'] ?? b['Name'] ?? ''),
      description: String(b['description'] ?? b['Description'] ?? ''),
      discountPercent: Number(b['discountPercent'] ?? b['DiscountPercent'] ?? 0),
      savingsAmount: Number(b['savingsAmount'] ?? b['SavingsAmount'] ?? 0),
      items: (b['items'] ?? b['Items'] ?? []) as BundleOffer['items'],
    }));

    return { items, suggestions, bundleOffers, total };
  }

  private updateCartState(c: CartSummary) {
    this.cart.set(c);
    this.cartCount.set(c.items?.reduce((sum, i) => sum + i.quantity, 0) ?? 0);
  }
}
