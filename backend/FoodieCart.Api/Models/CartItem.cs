namespace FoodieCart.Api.Models;

public class CartItem
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
