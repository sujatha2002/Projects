namespace FoodieCart.Api.Models;

public class BundleDeal
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TriggerCategory { get; set; } = string.Empty;
    public string ComplementaryRecipeIds { get; set; } = string.Empty;
    public decimal DiscountPercent { get; set; }
    public bool IsActive { get; set; } = true;
}
