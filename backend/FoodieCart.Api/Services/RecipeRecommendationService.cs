using FoodieCart.Api.Data;
using FoodieCart.Api.DTOs;
using FoodieCart.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace FoodieCart.Api.Services;

public class RecipeRecommendationService(AppDbContext db)
{
    private readonly MLContext _ml = new(seed: 42);

    public async Task<List<RecipeSuggestionDto>> GetSuggestionsAsync(string userId, string? category = null, int count = 6)
    {
        var user = await db.Users
            .Include(u => u.Preference)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var allRecipes = await db.Recipes.ToListAsync();
        var userOrders = await db.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Recipe)
            .Where(oi => oi.Order.UserId == userId)
            .ToListAsync();

        var ratings = BuildRatingMatrix(userOrders, allRecipes);
        var mlScores = ComputeMlScores(ratings, userId, allRecipes);

        var dietary = user?.Preference?.DietaryType ?? user?.DietaryPreference ?? "None";
        var favorites = (user?.Preference?.FavoriteCategories ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);

        var suggestions = allRecipes
            .Where(r => category == null || r.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .Select(r =>
            {
                var score = mlScores.GetValueOrDefault(r.Id, 0.3);
                var reason = BuildReason(r, dietary, favorites, userOrders);

                if (dietary.Equals("Vegetarian", StringComparison.OrdinalIgnoreCase) && !r.IsVegetarian)
                    score *= 0.1;
                if (dietary.Equals("Vegan", StringComparison.OrdinalIgnoreCase) && !r.IsVegan)
                    score *= 0.05;
                if (favorites.Any(f => r.Category.Contains(f.Trim(), StringComparison.OrdinalIgnoreCase)))
                    score *= 1.5;
                score += r.PopularityScore * 0.01;

                return new RecipeSuggestionDto(ToDto(r), Math.Round(score, 2), reason);
            })
            .OrderByDescending(s => s.Score)
            .Take(count)
            .ToList();

        return suggestions;
    }

    public async Task<List<RecipeSuggestionDto>> GetTrendingAsync(int count = 6)
    {
        var recipes = await db.Recipes
            .OrderByDescending(r => r.PopularityScore)
            .Take(count)
            .ToListAsync();

        return recipes.Select(r => new RecipeSuggestionDto(
            ToDto(r), r.PopularityScore / 100.0,
            $"Trending with {r.PopularityScore} orders this week")).ToList();
    }

    private Dictionary<int, double> ComputeMlScores(
        List<RecipeRating> ratings, string userId, List<Recipe> recipes)
    {
        var scores = new Dictionary<int, double>();
        if (ratings.Count < 3)
        {
            foreach (var r in recipes)
                scores[r.Id] = r.PopularityScore / 100.0;
            return scores;
        }

        try
        {
            var dataView = _ml.Data.LoadFromEnumerable(ratings);
            var options = new Microsoft.ML.Trainers.MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = nameof(RecipeRating.UserKey),
                MatrixRowIndexColumnName = nameof(RecipeRating.RecipeKey),
                LabelColumnName = nameof(RecipeRating.Rating),
                NumberOfIterations = 20,
                ApproximationRank = 8
            };

            var pipeline = _ml.Transforms.Conversion.MapValueToKey(
                    outputColumnName: "UserKeyEncoded", inputColumnName: nameof(RecipeRating.UserKey))
                .Append(_ml.Transforms.Conversion.MapValueToKey(
                    outputColumnName: "RecipeKeyEncoded", inputColumnName: nameof(RecipeRating.RecipeKey)))
                .Append(_ml.Recommendation().Trainers.MatrixFactorization(options));

            var model = pipeline.Fit(dataView);
            var predictionEngine = _ml.Model.CreatePredictionEngine<RecipeRating, RecipeScorePrediction>(model);

            var userKey = ratings.FirstOrDefault(r => r.UserId == userId)?.UserKey ?? 0;
            foreach (var recipe in recipes)
            {
                var recipeKey = ratings.FirstOrDefault(r => r.RecipeId == recipe.Id)?.RecipeKey ?? (uint)recipe.Id;
                var prediction = predictionEngine.Predict(new RecipeRating
                {
                    UserKey = userKey,
                    RecipeKey = recipeKey
                });
                scores[recipe.Id] = Math.Max(0, prediction.Score);
            }
        }
        catch
        {
            foreach (var r in recipes)
                scores[r.Id] = r.PopularityScore / 100.0;
        }

        return scores;
    }

    private static List<RecipeRating> BuildRatingMatrix(
        List<OrderItem> userOrders, List<Recipe> allRecipes)
    {
        var ratings = new List<RecipeRating>();
        var userKeys = new Dictionary<string, uint>();
        var recipeKeys = new Dictionary<int, uint>();
        uint nextUserKey = 1, nextRecipeKey = 1;

        foreach (var order in userOrders.GroupBy(oi => oi.Order.UserId))
        {
            if (!userKeys.ContainsKey(order.Key))
                userKeys[order.Key] = nextUserKey++;

            foreach (var item in order)
            {
                if (!recipeKeys.ContainsKey(item.RecipeId))
                    recipeKeys[item.RecipeId] = nextRecipeKey++;

                ratings.Add(new RecipeRating
                {
                    UserId = order.Key,
                    RecipeId = item.RecipeId,
                    UserKey = userKeys[order.Key],
                    RecipeKey = recipeKeys[item.RecipeId],
                    Rating = Math.Min(5f, item.Quantity * 1.5f)
                });
            }
        }

        foreach (var recipe in allRecipes)
        {
            if (!recipeKeys.ContainsKey(recipe.Id))
                recipeKeys[recipe.Id] = nextRecipeKey++;
        }

        return ratings;
    }

    private static string BuildReason(Recipe recipe, string dietary, string[] favorites, List<OrderItem> orders)
    {
        if (dietary.Equals("Vegetarian", StringComparison.OrdinalIgnoreCase) && recipe.IsVegetarian)
            return $"Great vegetarian {recipe.Category.ToLower()} based on your preferences";
        if (dietary.Equals("Vegan", StringComparison.OrdinalIgnoreCase) && recipe.IsVegan)
            return $"Perfect vegan match for your diet";
        if (orders.Any(o => o.Recipe.Category == recipe.Category))
            return $"Because you enjoy {recipe.Category} dishes";
        if (recipe.PopularityScore > 80)
            return "Trending among Foodie Cart users";
        return $"Recommended {recipe.Category} dish for you";
    }

    private static RecipeDto ToDto(Recipe r) => new(
        r.Id, r.Name, r.Description, r.Category, r.Price, r.ImageUrl,
        r.IsVegetarian, r.IsVegan, r.PrepTimeMinutes, r.PopularityScore, r.Tags);
}

public class RecipeRating
{
    public string UserId { get; set; } = "";
    public int RecipeId { get; set; }
    [ColumnName("Label")]
    public float Rating { get; set; }
    public uint UserKey { get; set; }
    public uint RecipeKey { get; set; }
}

public class RecipeScorePrediction
{
    public float Score { get; set; }
}
