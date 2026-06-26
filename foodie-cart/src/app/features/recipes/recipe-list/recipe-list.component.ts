import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RecipeService } from '../../../core/services/recipe.service';
import { CartService } from '../../../core/services/cart.service';
import { AuthService } from '../../../core/services/auth.service';
import { Recipe, RecipeSuggestion } from '../../../core/models';

@Component({
  selector: 'app-recipe-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page">
      <header class="hero">
        <h1>Discover Delicious Recipes</h1>
        <p>AI-powered suggestions tailored to your taste</p>
      </header>

      <div class="filters">
        @for (cat of categories; track cat) {
          <button [class.active]="selectedCategory === cat" (click)="filterCategory(cat)">
            {{ cat }}
          </button>
        }
      </div>

      @if (auth.isLoggedIn() && suggestions.length) {
        <section class="suggestions-section">
          <h2>✨ AI Suggestions For You</h2>
          <div class="suggestions-grid">
            @for (s of suggestions; track s.recipe.id) {
              <div class="suggestion-card">
                <img [src]="s.recipe.imageUrl" [alt]="s.recipe.name" />
                <div class="suggestion-info">
                  <h3>{{ s.recipe.name }}</h3>
                  <p class="reason">{{ s.reason }}</p>
                  <div class="score-bar">
                    <div class="score-fill" [style.width.%]="s.score * 20"></div>
                  </div>
                  <div class="card-footer">
                    <span class="price">₹{{ s.recipe.price | number:'1.0-0' }}</span>
                    <button class="btn-add" (click)="addToCart(s.recipe.id)">Add to Cart</button>
                  </div>
                </div>
              </div>
            }
          </div>
        </section>
      }

      @if (trending.length) {
        <section class="trending-section">
          <h2>🔥 Trending Now</h2>
          <div class="trending-scroll">
            @for (t of trending; track t.recipe.id) {
              <div class="trending-card">
                <img [src]="t.recipe.imageUrl" [alt]="t.recipe.name" />
                <h4>{{ t.recipe.name }}</h4>
                <span class="price">₹{{ t.recipe.price | number:'1.0-0' }}</span>
              </div>
            }
          </div>
        </section>
      }

      @if (cartMessage) {
        <div class="toast">{{ cartMessage }}</div>
      }

      <section class="recipes-section">
        <h2>All Recipes</h2>
        @if (loadError) {
          <div class="error-msg">{{ loadError }}</div>
        } @else if (!recipes.length && !loading) {
          <div class="empty">No recipes found. Make sure the backend API is running.</div>
        } @else {
        <div class="recipes-grid">
          @for (recipe of recipes; track recipe.id) {
            <div class="recipe-card">
              <img [src]="recipe.imageUrl" [alt]="recipe.name" />
              <div class="recipe-info">
                <div class="badges">
                  @if (recipe.isVegan) { <span class="badge vegan">Vegan</span> }
                  @else if (recipe.isVegetarian) { <span class="badge veg">Veg</span> }
                  <span class="badge category">{{ recipe.category }}</span>
                </div>
                <h3>{{ recipe.name }}</h3>
                <p>{{ recipe.description }}</p>
                <div class="meta">
                  <span>⏱ {{ recipe.prepTimeMinutes }} min</span>
                  <span>⭐ {{ recipe.popularityScore }}</span>
                </div>
                <div class="card-footer">
                  <span class="price">₹{{ recipe.price | number:'1.0-0' }}</span>
                  @if (auth.isLoggedIn()) {
                    <button class="btn-add" (click)="addToCart(recipe.id)">Add to Cart</button>
                  } @else {
                    <span class="login-hint">Login to order</span>
                  }
                </div>
              </div>
            </div>
          }
        </div>
        }
      </section>
    </div>
  `,
  styles: [`
    .page { max-width: 1200px; margin: 0 auto; padding: 2rem; }
    .hero { text-align: center; margin-bottom: 2rem; }
    .hero h1 { font-size: 2.2rem; color: #2c3e50; margin-bottom: 0.5rem; }
    .hero p { color: #7f8c8d; font-size: 1.1rem; }
    .filters { display: flex; gap: 0.5rem; flex-wrap: wrap; margin-bottom: 2rem; justify-content: center; }
    .filters button { padding: 0.5rem 1.2rem; border: 1px solid #ddd; border-radius: 20px; background: white; cursor: pointer; transition: all 0.2s; }
    .filters button.active, .filters button:hover { background: #e74c3c; color: white; border-color: #e74c3c; }
    section { margin-bottom: 3rem; }
    section h2 { color: #2c3e50; margin-bottom: 1.5rem; }
    .suggestions-grid, .recipes-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 1.5rem; }
    .suggestion-card, .recipe-card { background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.08); transition: transform 0.2s; }
    .suggestion-card:hover, .recipe-card:hover { transform: translateY(-4px); }
    .suggestion-card img, .recipe-card img { width: 100%; height: 180px; object-fit: cover; }
    .suggestion-info, .recipe-info { padding: 1rem; }
    .reason { color: #e74c3c; font-size: 0.85rem; font-style: italic; }
    .score-bar { height: 4px; background: #eee; border-radius: 2px; margin: 0.5rem 0; }
    .score-fill { height: 100%; background: linear-gradient(90deg, #e74c3c, #f39c12); border-radius: 2px; }
    .badges { display: flex; gap: 0.4rem; margin-bottom: 0.5rem; }
    .badge { font-size: 0.7rem; padding: 0.2rem 0.5rem; border-radius: 10px; font-weight: 600; }
    .badge.vegan { background: #d5f5e3; color: #27ae60; }
    .badge.veg { background: #eafaf1; color: #2ecc71; }
    .badge.category { background: #ebf5fb; color: #2980b9; }
    h3 { margin: 0.3rem 0; color: #2c3e50; }
    .meta { display: flex; gap: 1rem; color: #95a5a6; font-size: 0.85rem; margin: 0.5rem 0; }
    .card-footer { display: flex; justify-content: space-between; align-items: center; margin-top: 0.75rem; }
    .price { font-size: 1.2rem; font-weight: 700; color: #e74c3c; }
    .btn-add { background: #e74c3c; color: white; border: none; padding: 0.5rem 1rem; border-radius: 8px; cursor: pointer; font-weight: 500; }
    .btn-add:hover { background: #c0392b; }
    .login-hint { color: #95a5a6; font-size: 0.85rem; }
    .trending-scroll { display: flex; gap: 1rem; overflow-x: auto; padding-bottom: 0.5rem; }
    .trending-card { min-width: 160px; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.08); text-align: center; }
    .trending-card img { width: 100%; height: 100px; object-fit: cover; }
    .trending-card h4 { padding: 0.5rem; margin: 0; font-size: 0.9rem; }
    .trending-card .price { display: block; padding-bottom: 0.5rem; color: #e74c3c; font-weight: 600; }
    .error-msg { background: #fde8e8; color: #c0392b; padding: 1rem; border-radius: 8px; margin-bottom: 1rem; }
    .empty { text-align: center; padding: 2rem; color: #95a5a6; }
    .toast { position: fixed; bottom: 2rem; right: 2rem; background: #27ae60; color: white; padding: 0.75rem 1.5rem; border-radius: 8px; z-index: 1000; }
  `]
})
export class RecipeListComponent implements OnInit {
  recipes: Recipe[] = [];
  suggestions: RecipeSuggestion[] = [];
  trending: RecipeSuggestion[] = [];
  categories = ['All', 'Burger', 'Pasta', 'Pizza', 'Indian', 'Salad'];
  selectedCategory = 'All';
  loading = false;
  loadError = '';
  cartMessage = '';

  constructor(
    private recipeService: RecipeService,
    private cartService: CartService,
    public auth: AuthService
  ) {}

  ngOnInit() {
    this.loadRecipes();
    this.recipeService.getTrending().subscribe(t => this.trending = t);
    if (this.auth.isLoggedIn()) this.loadSuggestions();
  }

  filterCategory(cat: string) {
    this.selectedCategory = cat;
    this.loadRecipes();
    if (this.auth.isLoggedIn()) this.loadSuggestions();
  }

  loadRecipes() {
    this.loading = true;
    this.loadError = '';
    const cat = this.selectedCategory === 'All' ? undefined : this.selectedCategory;
    this.recipeService.getAll(cat).subscribe({
      next: r => { this.recipes = r; this.loading = false; },
      error: () => { this.loadError = 'Could not load recipes. Please ensure the backend is running on port 5149.'; this.loading = false; }
    });
  }

  loadSuggestions() {
    const cat = this.selectedCategory === 'All' ? undefined : this.selectedCategory;
    this.recipeService.getSuggestions(cat).subscribe({
      next: s => this.suggestions = s,
      error: () => this.suggestions = []
    });
  }

  addToCart(recipeId: number) {
    this.cartService.addToCart(recipeId).subscribe({
      next: () => {
        this.cartMessage = 'Added to cart!';
        setTimeout(() => this.cartMessage = '', 2000);
      },
      error: () => {
        this.cartMessage = 'Please log in to add items to cart.';
        setTimeout(() => this.cartMessage = '', 3000);
      }
    });
  }
}
