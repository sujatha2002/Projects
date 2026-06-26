namespace FoodieCart.Api.Models;

public class Recipe
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsVegetarian { get; set; }
    public bool IsVegan { get; set; }
    public int PrepTimeMinutes { get; set; }
    public int PopularityScore { get; set; }
    public string Tags { get; set; } = string.Empty;
    public string? VendorId { get; set; }
    public ApplicationUser? Vendor { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
