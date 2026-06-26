namespace FoodieCart.Api.Models;

public class Order
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Completed";
    public bool IsAbandoned { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
