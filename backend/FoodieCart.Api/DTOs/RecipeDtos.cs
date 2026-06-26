namespace FoodieCart.Api.DTOs;

public record RecipeDto(
    int Id, string Name, string Description, string Category,
    decimal Price, string ImageUrl, bool IsVegetarian, bool IsVegan,
    int PrepTimeMinutes, int PopularityScore, string Tags);

public record RecipeSuggestionDto(
    RecipeDto Recipe, double Score, string Reason);

public record UserPreferenceDto(
    string DietaryType, string FavoriteCategories, string Allergies, int SpiceLevel);
