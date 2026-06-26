using FoodieCart.Api.Data;
using FoodieCart.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FoodieCart.Api.Services;

public class CartOptimizationService(AppDbContext db)
{
    private static readonly Dictionary<string, List<(string Name, string Reason)>> ComplementaryMap = new()
    {
        ["Burger"] = [("French Fries", "Classic burger side"), ("Soft Drink", "Complete your meal")],
        ["Pasta"] = [("Garlic Bread", "Perfect pasta pairing"), ("Caesar Salad", "Light complement")],
        ["Pizza"] = [("Garlic Bread", "Great starter"), ("Soft Drink", "Wash it down")],
        ["Indian"] = [("Naan Bread", "Essential Indian side"), ("Mango Lassi", "Traditional drink")],
        ["Salad"] = [("Soup", "Warm complement to salad"), ("Bread Roll", "Add some carbs")],
    };

    public async Task<CartSummaryDto> GetOptimizedCartAsync(string userId)
    {
        var items = await db.CartItems
            .Include(c => c.Recipe)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        var cartDtos = items.Select(c => new CartItemDto(
            c.Id, c.RecipeId, c.Recipe.Name, c.Recipe.Price, c.Quantity,
            c.Recipe.Price * c.Quantity)).ToList();

        var total = cartDtos.Sum(i => i.Subtotal);
        var cartRecipeIds = items.Select(i => i.RecipeId).ToHashSet();
        var cartCategories = items.Select(i => i.Recipe.Category).Distinct().ToList();

        var suggestions = await GenerateSuggestionsAsync(cartCategories, cartRecipeIds);
        var bundleOffers = await GenerateBundleOffersAsync(items, cartRecipeIds);

        return new CartSummaryDto(cartDtos, total, suggestions, bundleOffers);
    }

    private async Task<List<CartSuggestionDto>> GenerateSuggestionsAsync(
        List<string> categories, HashSet<int> cartRecipeIds)
    {
        var suggestions = new List<CartSuggestionDto>();

        foreach (var category in categories)
        {
            if (!ComplementaryMap.TryGetValue(category, out var complements)) continue;

            foreach (var (name, reason) in complements)
            {
                var recipe = await db.Recipes
                    .FirstOrDefaultAsync(r => r.Name.Contains(name) && !cartRecipeIds.Contains(r.Id));

                if (recipe != null)
                {
                    suggestions.Add(new CartSuggestionDto(
                        recipe.Id, recipe.Name, recipe.Price, reason, 0.85));
                }
            }
        }

        var popular = await db.Recipes
            .Where(r => !cartRecipeIds.Contains(r.Id))
            .OrderByDescending(r => r.PopularityScore)
            .Take(3)
            .ToListAsync();

        foreach (var recipe in popular)
        {
            if (suggestions.All(s => s.RecipeId != recipe.Id))
            {
                suggestions.Add(new CartSuggestionDto(
                    recipe.Id, recipe.Name, recipe.Price,
                    "Popular add-on among customers", 0.7));
            }
        }

        return suggestions.Take(5).ToList();
    }

    private async Task<List<BundleOfferDto>> GenerateBundleOffersAsync(
        List<Models.CartItem> cartItems, HashSet<int> cartRecipeIds)
    {
        var offers = new List<BundleOfferDto>();
        var bundles = await db.BundleDeals.Where(b => b.IsActive).ToListAsync();

        foreach (var bundle in bundles)
        {
            var triggerInCart = cartItems.Any(i =>
                i.Recipe.Category.Equals(bundle.TriggerCategory, StringComparison.OrdinalIgnoreCase));

            if (!triggerInCart) continue;

            var complementaryIds = bundle.ComplementaryRecipeIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse).ToList();

            var missingIds = complementaryIds.Where(id => !cartRecipeIds.Contains(id)).ToList();
            if (missingIds.Count == 0) continue;

            var bundleRecipes = await db.Recipes
                .Where(r => complementaryIds.Contains(r.Id))
                .ToListAsync();

            var bundleTotal = bundleRecipes.Sum(r => r.Price);
            var savings = bundleTotal * (bundle.DiscountPercent / 100m);

            offers.Add(new BundleOfferDto(
                bundle.Id, bundle.Name, bundle.Description,
                bundle.DiscountPercent, Math.Round(savings, 2),
                bundleRecipes.Select(r => new RecipeDto(
                    r.Id, r.Name, r.Description, r.Category, r.Price, r.ImageUrl,
                    r.IsVegetarian, r.IsVegan, r.PrepTimeMinutes, r.PopularityScore, r.Tags)).ToList()));
        }

        return offers;
    }
}
