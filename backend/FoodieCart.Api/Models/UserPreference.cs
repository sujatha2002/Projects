namespace FoodieCart.Api.Models;

public class UserPreference
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public string DietaryType { get; set; } = "None";
    public string FavoriteCategories { get; set; } = string.Empty;
    public string Allergies { get; set; } = string.Empty;
    public int SpiceLevel { get; set; } = 2;
}
