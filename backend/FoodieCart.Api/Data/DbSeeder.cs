using FoodieCart.Api.Models;
using FoodieCart.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodieCart.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        await context.Database.MigrateAsync();
        await RecipeImageDownloader.EnsureImagesAsync(env);

        foreach (var role in new[] { Roles.Admin, Roles.Vendor, Roles.Customer })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        if (!context.Recipes.Any())
        {
            await SeedRecipesAsync(context);
            await SeedBundleDealsAsync(context);
            await SeedUsersAndOrdersAsync(context, userManager);
        }
        else
        {
            await UpdateRecipePricesAsync(context);
            await SyncLocalRecipeImagesAsync(context);
        }
    }

    private static async Task SyncLocalRecipeImagesAsync(AppDbContext context)
    {
        var recipes = await context.Recipes.ToListAsync();
        foreach (var recipe in recipes)
            recipe.ImageUrl = RecipeImageDownloader.GetLocalUrl(recipe.Name);

        await context.SaveChangesAsync();
    }

    private static async Task SeedRecipesAsync(AppDbContext context)
    {
        var recipes = new List<Recipe>
        {
            new() { Name = "Classic Beef Burger", Description = "Juicy beef patty with lettuce, tomato, and special sauce", Category = "Burger", Price = 299m, ImageUrl = RecipeImageDownloader.GetLocalUrl("Classic Beef Burger"), IsVegetarian = false, PrepTimeMinutes = 15, PopularityScore = 95, Tags = "beef,classic,grilled" },
            new() { Name = "Veggie Burger", Description = "Plant-based patty with avocado and sprouts", Category = "Burger", Price = 249m, ImageUrl = RecipeImageDownloader.GetLocalUrl("Veggie Burger"), IsVegetarian = true, IsVegan = true, PrepTimeMinutes = 12, PopularityScore = 78, Tags = "vegan,healthy" },
            new() { Name = "French Fries", Description = "Crispy golden fries with sea salt", Category = "Burger", Price = 99m, ImageUrl = RecipeImageDownloader.GetLocalUrl("French Fries"), IsVegetarian = true, IsVegan = true, PrepTimeMinutes = 8, PopularityScore = 90, Tags = "side,crispy" },
            new() { Name = "Soft Drink", Description = "Refreshing cola or lemonade", Category = "Burger", Price = 49m, ImageUrl = RecipeImageDownloader.GetLocalUrl("Soft Drink"), IsVegetarian = true, IsVegan = true, PrepTimeMinutes = 1, PopularityScore = 85, Tags = "drink,refreshing" },
            new() { Name = "Spaghetti Carbonara", Description = "Creamy pasta with pancetta and parmesan", Category = "Pasta", Price = 349m, ImageUrl = RecipeImageDownloader.GetLocalUrl("Spaghetti Carbonara"), IsVegetarian = false, PrepTimeMinutes = 25, PopularityScore = 88, Tags = "italian,creamy" },
            new() { Name = "Spinach Alfredo", Description = "Creamy Alfredo with fresh spinach", Category = "Pasta", Price = 329m, ImageUrl = RecipeImageDownloader.GetLocalUrl("Spinach Alfredo"), IsVegetarian = true, PrepTimeMinutes = 20, PopularityScore = 82, Tags = "vegetarian,creamy" },
            new() { Name = "Vegan Carbonara", Description = "Silky cashew cream with smoked tofu", Category = "Pasta", Price = 339m, ImageUrl = RecipeImageDownloader.GetLocalUrl("Vegan Carbonara"), IsVegetarian = true, IsVegan = true, PrepTimeMinutes = 22, PopularityScore = 75, Tags = "vegan,italian" },
            new() { Name = "Garlic Bread", Description = "Toasted bread with garlic butter and herbs", Category = "Pasta", Price = 149m, ImageUrl = RecipeImageDownloader.GetLocalUrl("Garlic Bread"), IsVegetarian = true, PrepTimeMinutes = 10, PopularityScore = 80, Tags = "side,garlic" },
            new() { Name = "Caesar Salad", Description = "Romaine lettuce with Caesar dressing and croutons", Category = "Salad", Price = 249m, ImageUrl = RecipeImageDownloader.GetLocalUrl("Caesar Salad"), IsVegetarian = true, PrepTimeMinutes = 10, PopularityScore = 70, Tags = "salad,light" },
            new() { Name = "Margherita Pizza", Description = "Classic tomato, mozzarella, and basil", Category = "Pizza", Price = 349m, ImageUrl = RecipeImageDownloader.GetLocalUrl("Margherita Pizza"), IsVegetarian = true, PrepTimeMinutes = 20, PopularityScore = 92, Tags = "italian,classic" },
            new() { Name = "Paneer Butter Masala", Description = "Creamy tomato curry with cottage cheese", Category = "Indian", Price = 399m, ImageUrl = RecipeImageDownloader.GetLocalUrl("Paneer Butter Masala"), IsVegetarian = true, PrepTimeMinutes = 30, PopularityScore = 96, Tags = "indian,curry,spicy" },
            new() { Name = "Chicken Tikka Masala", Description = "Grilled chicken in spiced tomato cream sauce", Category = "Indian", Price = 429m, ImageUrl = RecipeImageDownloader.GetLocalUrl("Chicken Tikka Masala"), IsVegetarian = false, PrepTimeMinutes = 35, PopularityScore = 94, Tags = "indian,curry" },
            new() { Name = "Naan Bread", Description = "Soft tandoor-baked flatbread", Category = "Indian", Price = 49m, ImageUrl = RecipeImageDownloader.GetLocalUrl("Naan Bread"), IsVegetarian = true, PrepTimeMinutes = 5, PopularityScore = 88, Tags = "bread,indian" },
            new() { Name = "Mango Lassi", Description = "Sweet yogurt drink with mango", Category = "Indian", Price = 79m, ImageUrl = RecipeImageDownloader.GetLocalUrl("Mango Lassi"), IsVegetarian = true, PrepTimeMinutes = 3, PopularityScore = 72, Tags = "drink,indian" },
            new() { Name = "Tomato Soup", Description = "Rich and creamy tomato basil soup", Category = "Salad", Price = 149m, ImageUrl = RecipeImageDownloader.GetLocalUrl("Tomato Soup"), IsVegetarian = true, IsVegan = true, PrepTimeMinutes = 15, PopularityScore = 65, Tags = "soup,warm" },
            new() { Name = "Bread Roll", Description = "Freshly baked artisan bread roll", Category = "Salad", Price = 39m, ImageUrl = RecipeImageDownloader.GetLocalUrl("Bread Roll"), IsVegetarian = true, PrepTimeMinutes = 2, PopularityScore = 60, Tags = "bread" },
        };

        context.Recipes.AddRange(recipes);
        await context.SaveChangesAsync();
    }

    private static async Task UpdateRecipePricesAsync(AppDbContext context)
    {
        var inrPrices = new Dictionary<string, decimal>
        {
            ["Classic Beef Burger"] = 299m,
            ["Veggie Burger"] = 249m,
            ["French Fries"] = 99m,
            ["Soft Drink"] = 49m,
            ["Spaghetti Carbonara"] = 349m,
            ["Spinach Alfredo"] = 329m,
            ["Vegan Carbonara"] = 339m,
            ["Garlic Bread"] = 149m,
            ["Caesar Salad"] = 249m,
            ["Margherita Pizza"] = 349m,
            ["Paneer Butter Masala"] = 399m,
            ["Chicken Tikka Masala"] = 429m,
            ["Naan Bread"] = 49m,
            ["Mango Lassi"] = 79m,
            ["Tomato Soup"] = 149m,
            ["Bread Roll"] = 39m
        };

        var recipes = await context.Recipes.ToListAsync();
        foreach (var recipe in recipes)
        {
            if (inrPrices.TryGetValue(recipe.Name, out var price))
                recipe.Price = price;
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedBundleDealsAsync(AppDbContext context)
    {
        var recipes = await context.Recipes.ToListAsync();
        var fries = recipes.First(r => r.Name == "French Fries");
        var drink = recipes.First(r => r.Name == "Soft Drink");
        var garlic = recipes.First(r => r.Name == "Garlic Bread");
        var naan = recipes.First(r => r.Name == "Naan Bread");
        var lassi = recipes.First(r => r.Name == "Mango Lassi");

        context.BundleDeals.AddRange(
            new BundleDeal { Name = "Burger Combo", Description = "Add fries and drink at 15% off", TriggerCategory = "Burger", ComplementaryRecipeIds = $"{fries.Id},{drink.Id}", DiscountPercent = 15 },
            new BundleDeal { Name = "Pasta Feast", Description = "Garlic bread bundle at 10% off", TriggerCategory = "Pasta", ComplementaryRecipeIds = $"{garlic.Id}", DiscountPercent = 10 },
            new BundleDeal { Name = "Indian Thali", Description = "Naan and lassi at 20% off", TriggerCategory = "Indian", ComplementaryRecipeIds = $"{naan.Id},{lassi.Id}", DiscountPercent = 20 }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedUsersAndOrdersAsync(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        var admin = new ApplicationUser { UserName = "admin@foodiecart.com", Email = "admin@foodiecart.com", FullName = "Admin User", EmailConfirmed = true };
        if (await userManager.FindByEmailAsync(admin.Email) == null)
        {
            await userManager.CreateAsync(admin, "Admin@123");
            await userManager.AddToRoleAsync(admin, Roles.Admin);
        }

        var vendor = new ApplicationUser { UserName = "vendor@foodiecart.com", Email = "vendor@foodiecart.com", FullName = "Chef Vendor", EmailConfirmed = true };
        if (await userManager.FindByEmailAsync(vendor.Email) == null)
        {
            await userManager.CreateAsync(vendor, "Vendor@123");
            await userManager.AddToRoleAsync(vendor, Roles.Vendor);
        }

        var customers = new[]
        {
            new ApplicationUser { UserName = "alice@example.com", Email = "alice@example.com", FullName = "Alice Johnson", DietaryPreference = "Vegetarian", Age = 25, EmailConfirmed = true },
            new ApplicationUser { UserName = "bob@example.com", Email = "bob@example.com", FullName = "Bob Smith", DietaryPreference = "None", Age = 32, EmailConfirmed = true },
            new ApplicationUser { UserName = "carol@example.com", Email = "carol@example.com", FullName = "Carol Davis", DietaryPreference = "Vegan", Age = 28, EmailConfirmed = true },
        };

        foreach (var customer in customers)
        {
            if (await userManager.FindByEmailAsync(customer.Email) == null)
            {
                await userManager.CreateAsync(customer, "Customer@123");
                await userManager.AddToRoleAsync(customer, Roles.Customer);

                context.UserPreferences.Add(new UserPreference
                {
                    UserId = customer.Id,
                    DietaryType = customer.DietaryPreference ?? "None",
                    FavoriteCategories = customer.DietaryPreference == "Vegetarian" ? "Pasta,Indian" : "Burger,Pizza",
                    SpiceLevel = 3
                });
            }
        }

        await context.SaveChangesAsync();

        var recipes = await context.Recipes.ToListAsync();
        var alice = await userManager.FindByEmailAsync("alice@example.com");
        var bob = await userManager.FindByEmailAsync("bob@example.com");

        if (alice != null && !context.Orders.Any(o => o.UserId == alice.Id))
        {
            var pastaOrders = recipes.Where(r => r.Category == "Pasta" || r.Category == "Indian").Take(4).ToList();
            foreach (var recipe in pastaOrders)
            {
                var order = new Order { UserId = alice.Id, TotalAmount = recipe.Price, Status = "Completed" };
                order.Items.Add(new OrderItem { RecipeId = recipe.Id, Quantity = 1, UnitPrice = recipe.Price });
                context.Orders.Add(order);
            }
        }

        if (bob != null && !context.Orders.Any(o => o.UserId == bob.Id))
        {
            var burgerOrders = recipes.Where(r => r.Category == "Burger").Take(3).ToList();
            foreach (var recipe in burgerOrders)
            {
                var order = new Order { UserId = bob.Id, TotalAmount = recipe.Price, Status = "Completed" };
                order.Items.Add(new OrderItem { RecipeId = recipe.Id, Quantity = 1, UnitPrice = recipe.Price });
                context.Orders.Add(order);
            }
        }

        var abandonedOrder = new Order
        {
            UserId = alice!.Id,
            TotalAmount = 25.98m,
            Status = "Abandoned",
            IsAbandoned = true
        };
        abandonedOrder.Items.Add(new OrderItem { RecipeId = recipes.First(r => r.Category == "Pasta").Id, Quantity = 1, UnitPrice = 14.99m });
        context.Orders.Add(abandonedOrder);

        for (int i = 0; i < 7; i++)
        {
            context.AnalyticsEvents.Add(new AnalyticsEvent
            {
                EventType = "page_view",
                UserId = alice.Id,
                Timestamp = DateTime.UtcNow.AddDays(-i)
            });
            context.AnalyticsEvents.Add(new AnalyticsEvent
            {
                EventType = "recipe_view",
                UserId = bob!.Id,
                RecipeId = recipes.First().Id,
                Timestamp = DateTime.UtcNow.AddDays(-i)
            });
        }

        await context.SaveChangesAsync();
    }
}
