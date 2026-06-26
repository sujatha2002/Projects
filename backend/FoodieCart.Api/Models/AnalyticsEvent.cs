namespace FoodieCart.Api.Models;

public class AnalyticsEvent
{
    public int Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public int? RecipeId { get; set; }
    public string? Metadata { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
