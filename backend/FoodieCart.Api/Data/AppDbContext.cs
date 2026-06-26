using FoodieCart.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodieCart.Api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<BundleDeal> BundleDeals => Set<BundleDeal>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<AnalyticsEvent> AnalyticsEvents => Set<AnalyticsEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Recipe>().HasIndex(r => r.Category);
        builder.Entity<Recipe>().HasIndex(r => r.PopularityScore);
        builder.Entity<AnalyticsEvent>().HasIndex(e => e.Timestamp);
        builder.Entity<AnalyticsEvent>().HasIndex(e => e.EventType);
    }
}
