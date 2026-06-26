# Foodie Cart – Intelligent Recipe & Cart Manager

A full-stack application demonstrating AI-powered recipe suggestions, smart cart optimization, and secure JWT/OAuth2 authentication with role-based access control.

## About This Project

Foodie Cart is a recipe discovery and ordering app built as a full-stack demo. Customers browse a menu of dishes, get personalized recipe recommendations, and manage a shopping cart with AI-driven upsell suggestions and bundle deals. Admins can view analytics on popular dishes, cart abandonment, and user activity.

The backend is an ASP.NET Core Web API with SQLite and Entity Framework Core. It handles authentication (JWT and optional Google/Facebook OAuth), recipe and cart APIs, and ML.NET-based recommendations trained on order history. The frontend is an Angular single-page app with standalone components, route guards, and an admin dashboard with charts.

Recipe images are stored as real food photos in `backend/FoodieCart.Api/wwwroot/images/recipes/` and served locally from `/images/recipes/*.jpg` — no external image hosts required at runtime. Prices are shown in Indian Rupees (₹).

## How It Works

Foodie Cart follows a typical full-stack flow: the Angular frontend talks to the ASP.NET Core API, which reads and writes data in SQLite through Entity Framework Core.

### User Journey

1. **Browse & discover** — Anyone can view the recipe catalog and filter by category (Burger, Pasta, Pizza, Indian, Salad). Logged-in users also see personalized AI suggestions and trending dishes on the home page.
2. **Sign in** — Users register or log in with email/password, or optionally via Google/Facebook OAuth. A JWT token is stored in the browser and sent with every protected API request.
3. **Add to cart** — Authenticated customers add recipes to their cart. Each add/update triggers the smart cart engine, which recalculates totals, complementary suggestions, and bundle offers.
4. **Checkout** — Placing an order converts cart items into an order record, clears the cart, and feeds new data back into the recommendation model for future suggestions.
5. **Admin analytics** — Admin users open the dashboard to view order trends, cart abandonment, user activity charts, and AI-generated insights.

### Request Flow

```
Angular App (localhost:4200)
    ↓  HTTP via proxy (/api, /images)
ASP.NET Core API (localhost:5149)
    ↓  JWT validation + role checks
Services Layer (Recommendations, Cart AI, Analytics)
    ↓  Entity Framework Core
SQLite Database (recipes, users, orders, cart, bundles)
```

On first startup, the backend auto-migrates the database, seeds 16 sample recipes, demo users, historical orders, and bundle deals. Recipe images are stored in `wwwroot/images/recipes/` and served as static files.

## AI Features

Foodie Cart includes several intelligent features powered by ML.NET, rule-based logic, and order analytics.

### 1. Personalized Recipe Recommendations (ML.NET)

**Service:** `RecipeRecommendationService`

Uses **ML.NET Matrix Factorization** (collaborative filtering) to predict which recipes a user is likely to enjoy based on order history across all users.

| Step | What happens |
|------|----------------|
| Build rating matrix | Past orders are converted into user–recipe ratings (quantity-based, capped at 5) |
| Train model | Matrix factorization runs with rank-8 approximation over 20 iterations |
| Score recipes | Each recipe gets a predicted score for the current user |
| Apply filters | Scores are adjusted for dietary preferences (Vegetarian/Vegan), favorite categories, and popularity |
| Generate reasons | Human-readable explanations like *"Great vegetarian pasta based on your preferences"* |

If there is not enough order data (fewer than 3 ratings), the system falls back to popularity-based scoring.

**API:** `GET /api/recipes/suggestions` (requires login)

### 2. Trending Recipes

**Service:** `RecipeRecommendationService.GetTrendingAsync`

Surfaces the most popular dishes ranked by `PopularityScore`, with messages such as *"Trending with 96 orders this week"*. This gives new users useful picks even before the ML model has data on them.

**API:** `GET /api/recipes/trending`

### 3. Smart Cart Suggestions

**Service:** `CartOptimizationService`

When items are in the cart, the system suggests complementary add-ons based on category rules:

| Cart category | Suggested complements |
|---------------|----------------------|
| Burger | French Fries, Soft Drink |
| Pasta | Garlic Bread, Caesar Salad |
| Pizza | Garlic Bread, Soft Drink |
| Indian | Naan Bread, Mango Lassi |
| Salad | Tomato Soup, Bread Roll |

Suggestions exclude items already in the cart. The engine also adds popular add-ons from the menu and attaches a confidence score (e.g. 0.85 for category matches, 0.7 for popular items).

**API:** Returned automatically with `GET /api/cart`, `POST /api/cart/add`, and cart update endpoints.

### 4. Bundle Deal Detection

**Service:** `CartOptimizationService.GenerateBundleOffersAsync`

Predefined bundle deals (e.g. Burger Combo) are stored in the database with trigger categories and complementary recipe IDs. When a user adds an item from a trigger category, the cart checks whether bundle items are missing and calculates:

- Bundle total price
- Discount percentage
- Savings amount (₹)

Eligible bundles appear in the cart sidebar so users can complete the deal with one click.

### 5. Dietary & Preference Intelligence

**Service:** `RecipeRecommendationService` (scoring layer)

User preferences from registration and profile data influence recommendations:

- **Vegetarian users** — Non-vegetarian recipes receive a heavily reduced score
- **Vegan users** — Only vegan recipes score highly
- **Favorite categories** — Matching categories get a 1.5× score boost
- **Order history** — Users who frequently order a category see more from that category

### 6. Analytics Insights (Admin)

**Service:** `AnalyticsService`

The admin dashboard uses order and user data to produce:

| Insight | Description |
|---------|-------------|
| Popular dishes | Top 10 dishes by order count, repeat rate, and dominant age group |
| Cart abandonment | Abandonment rate per food category |
| User activity | 7-day chart of active users, orders, and signups |
| Trending insights | Auto-generated text like *"Paneer Butter Masala has high repeat orders among users aged 20–30"* |

Trend labels (Rising, Stable, Emerging) are derived from order volume thresholds.

**API:** `GET /api/analytics/dashboard` (Admin role only)

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 9 Web API |
| Frontend | Angular 22 (standalone components) |
| Database | SQLite (via Entity Framework Core) |
| Auth | JWT + OAuth2 (Google, Facebook) |
| AI/ML | ML.NET Matrix Factorization for recommendations |
| Charts | Chart.js + ng2-charts |

## Features

### Authentication & Authorization
- JWT-based login/signup with role-based access (Admin, Vendor, Customer)
- OAuth2 social login via Google and Facebook
- Protected routes with auth and role guards

### AI-Based Recipe Suggestions
- ML.NET collaborative filtering based on order history
- Dietary preference filtering (Vegetarian, Vegan)
- Personalized reasons for each suggestion
- Trending recipes display

### Smart Cart Optimization
- AI complementary item suggestions (e.g., garlic bread with pasta)
- Bundle deal detection with discount calculations
- Real-time cart total optimization

### Analytics Dashboard (Admin)
- Popular dishes with repeat rates and age demographics
- Cart abandonment rates by category
- 7-day user activity trends
- AI-generated trending insights with charts

## Project Structure

```
Foodie Cart/
├── backend/
│   └── FoodieCart.Api/          # .NET Core Web API
│       ├── Controllers/         # Auth, Recipes, Cart, Analytics
│       ├── Services/            # JWT, ML recommendations, Cart AI, Analytics
│       ├── Models/              # Entity models
│       ├── Data/                # DbContext, seeder
│       └── DTOs/                # Request/response models
└── foodie-cart/                 # Angular frontend
    └── src/app/
        ├── core/                # Services, guards, interceptors, models
        ├── features/            # Auth, recipes, cart, admin dashboard
        └── shared/              # Navbar component
```

## Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- Angular CLI (`npm install -g @angular/cli`)

### 1. Start the Backend

```bash
cd backend/FoodieCart.Api
dotnet run --launch-profile http
```

API runs at **http://localhost:5149**  
Swagger UI: **http://localhost:5149/swagger**

The database is auto-created and seeded with sample recipes, users, and orders on first run.

### 2. Start the Frontend

```bash
cd foodie-cart
npm install
ng serve
```

App runs at **http://localhost:4200**

## Demo Accounts

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@foodiecart.com | Admin@123 |
| Vendor | vendor@foodiecart.com | Vendor@123 |
| Customer (Vegetarian) | alice@example.com | Customer@123 |
| Customer | bob@example.com | Customer@123 |

## User Roles

Foodie Cart uses role-based access control. Each role has different permissions in the app.

### Customer

**Demo accounts:** `alice@example.com`, `bob@example.com`, `carol@example.com` (password: `Customer@123`)

**Purpose:** Browse and order food.

**What they can do:**
- Browse recipes and trending dishes
- Get AI personalized suggestions (based on dietary preferences and order history)
- Add items to cart, use smart cart suggestions, and checkout
- Register with dietary preferences (Vegetarian, Vegan, etc.)

**What they cannot do:**
- Create new recipes on the API
- Access the Admin Analytics dashboard

Customers are the main end-users of the app — people who discover food and place orders.

### Vendor

**Demo account:** `vendor@foodiecart.com` / `Vendor@123`

**Purpose:** Represent a restaurant or chef who can add menu items.

**What they can do:**
- Everything a customer can do (browse, cart, checkout — same UI)
- Create new recipes via the API (`POST /api/recipes`) — the recipe is linked to them as `VendorId`

**What they cannot do:**
- Access the Admin Analytics dashboard (Admin only)

### Admin

**Demo account:** `admin@foodiecart.com` / `Admin@123`

**Purpose:** View platform analytics and business insights.

**What they can do:**
- Create recipes (same as Vendor)
- Access the Analytics dashboard with charts for popular dishes, cart abandonment, user activity, and trending insights

### Role comparison

| Feature | Customer | Vendor | Admin |
|---------|----------|--------|-------|
| Browse recipes | Yes | Yes | Yes |
| AI suggestions | Yes (with dietary prefs) | Yes | Yes |
| Cart & checkout | Yes | Yes | Yes |
| Create recipes | No | Yes (API only) | Yes (API only) |
| Analytics dashboard | No | No | Yes |

**Note:** There is no separate vendor UI in the frontend. Vendors see the same pages as customers (Recipes, Cart). The vendor-only ability exists on the backend API — creating recipes. The navbar shows the role name (e.g. "Chef Vendor (Vendor)" vs "Alice Johnson (Customer)"), but the screens are the same.

## OAuth2 Setup (Optional)

To enable Google/Facebook login, update `backend/FoodieCart.Api/appsettings.json`:

```json
"Authentication": {
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID",
    "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
  },
  "Facebook": {
    "AppId": "YOUR_FACEBOOK_APP_ID",
    "AppSecret": "YOUR_FACEBOOK_APP_SECRET"
  }
}
```

Set redirect URIs to `http://localhost:5149/api/auth/google-callback` and `http://localhost:5149/api/auth/facebook-callback`.

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | No | Register new user |
| POST | `/api/auth/login` | No | Login with JWT |
| GET | `/api/auth/me` | Yes | Get current user profile |
| GET | `/api/recipes` | No | List all recipes |
| GET | `/api/recipes/suggestions` | Yes | AI recipe suggestions |
| GET | `/api/recipes/trending` | No | Trending recipes |
| GET | `/api/cart` | Yes | Get cart with AI suggestions |
| POST | `/api/cart/add` | Yes | Add item to cart |
| POST | `/api/cart/checkout` | Yes | Place order |
| GET | `/api/analytics/dashboard` | Admin | Analytics dashboard data |

## Example Scenarios

**Vegetarian user browsing pasta:**
Log in as `alice@example.com` → filter by "Pasta" → see AI suggestions like "Spinach Alfredo" and "Vegan Carbonara" with personalized reasons.

**Smart cart with burger:**
Add a Classic Beef Burger → cart suggests French Fries and Soft Drink → Burger Combo bundle offers 15% discount.

**Admin analytics:**
Log in as `admin@foodiecart.com` → navigate to Analytics → view "Paneer Butter Masala" trending among users aged 20–30 with high repeat orders.
