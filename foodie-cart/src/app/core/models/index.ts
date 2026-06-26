export interface AuthResponse {
  token: string;
  email: string;
  fullName: string;
  role: string;
  expiresAt: string;
}

export interface Recipe {
  id: number;
  name: string;
  description: string;
  category: string;
  price: number;
  imageUrl: string;
  isVegetarian: boolean;
  isVegan: boolean;
  prepTimeMinutes: number;
  popularityScore: number;
  tags: string;
}

export interface RecipeSuggestion {
  recipe: Recipe;
  score: number;
  reason: string;
}

export interface CartItem {
  id: number;
  recipeId: number;
  recipeName: string;
  price: number;
  quantity: number;
  subtotal: number;
}

export interface CartSuggestion {
  recipeId: number;
  recipeName: string;
  price: number;
  reason: string;
  confidence: number;
}

export interface BundleOffer {
  bundleId: number;
  name: string;
  description: string;
  discountPercent: number;
  savingsAmount: number;
  items: Recipe[];
}

export interface CartSummary {
  items: CartItem[];
  total: number;
  suggestions: CartSuggestion[];
  bundleOffers: BundleOffer[];
}

export interface PopularDish {
  name: string;
  orderCount: number;
  repeatRate: number;
  ageGroup: string;
}

export interface CartAbandonment {
  category: string;
  abandonedCount: number;
  totalCarts: number;
  abandonmentRate: number;
}

export interface UserActivity {
  date: string;
  activeUsers: number;
  orders: number;
  newSignups: number;
}

export interface TrendingInsight {
  dishName: string;
  trend: string;
  ageGroup: string;
  repeatOrders: number;
  insight: string;
}

export interface AnalyticsDashboard {
  popularDishes: PopularDish[];
  cartAbandonment: CartAbandonment[];
  userActivity: UserActivity[];
  trendingInsights: TrendingInsight[];
}
