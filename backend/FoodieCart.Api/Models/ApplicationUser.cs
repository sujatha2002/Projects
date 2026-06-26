using Microsoft.AspNetCore.Identity;

namespace FoodieCart.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? DietaryPreference { get; set; }
    public int? Age { get; set; }
    public string? OAuthProvider { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public UserPreference? Preference { get; set; }
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string Vendor = "Vendor";
    public const string Customer = "Customer";
}
