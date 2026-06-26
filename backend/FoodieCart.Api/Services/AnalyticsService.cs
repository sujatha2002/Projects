using FoodieCart.Api.Data;
using FoodieCart.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FoodieCart.Api.Services;

public class AnalyticsService(AppDbContext db)
{
    public async Task<AnalyticsDashboardDto> GetDashboardAsync()
    {
        var popularDishes = await GetPopularDishesAsync();
        var cartAbandonment = await GetCartAbandonmentAsync();
        var userActivity = await GetUserActivityAsync();
        var trendingInsights = await GetTrendingInsightsAsync();

        return new AnalyticsDashboardDto(popularDishes, cartAbandonment, userActivity, trendingInsights);
    }

    private async Task<List<PopularDishDto>> GetPopularDishesAsync()
    {
        var dishStats = await db.OrderItems
            .Include(oi => oi.Recipe)
            .Include(oi => oi.Order).ThenInclude(o => o.User)
            .GroupBy(oi => oi.Recipe.Name)
            .Select(g => new
            {
                Name = g.Key,
                OrderCount = g.Count(),
                RepeatUsers = g.Select(oi => oi.Order.UserId).Distinct().Count(),
                TotalUsers = g.Count(),
                AvgAge = g.Where(oi => oi.Order.User.Age != null).Average(oi => (double?)oi.Order.User.Age) ?? 25
            })
            .OrderByDescending(x => x.OrderCount)
            .Take(10)
            .ToListAsync();

        return dishStats.Select(d => new PopularDishDto(
            d.Name,
            d.OrderCount,
            d.TotalUsers > 0 ? Math.Round((double)d.RepeatUsers / d.TotalUsers * 100, 1) : 0,
            GetAgeGroup(d.AvgAge)
        )).ToList();
    }

    private async Task<List<CartAbandonmentDto>> GetCartAbandonmentAsync()
    {
        var categories = await db.Recipes.Select(r => r.Category).Distinct().ToListAsync();
        var result = new List<CartAbandonmentDto>();

        foreach (var category in categories)
        {
            var totalCarts = await db.CartItems
                .Include(c => c.Recipe)
                .CountAsync(c => c.Recipe.Category == category);

            var abandoned = await db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Recipe)
                .CountAsync(o => o.IsAbandoned && o.Items.Any(i => i.Recipe.Category == category));

            var rate = totalCarts > 0 ? Math.Round((double)abandoned / (totalCarts + abandoned) * 100, 1) : 0;
            result.Add(new CartAbandonmentDto(category, abandoned, totalCarts + abandoned, rate));
        }

        return result.OrderByDescending(r => r.AbandonmentRate).ToList();
    }

    private async Task<List<UserActivityDto>> GetUserActivityAsync()
    {
        var last7Days = Enumerable.Range(0, 7)
            .Select(i => DateTime.UtcNow.Date.AddDays(-i))
            .OrderBy(d => d)
            .ToList();

        var result = new List<UserActivityDto>();
        foreach (var date in last7Days)
        {
            var nextDay = date.AddDays(1);
            var activeUsers = await db.AnalyticsEvents
                .Where(e => e.Timestamp >= date && e.Timestamp < nextDay && e.UserId != null)
                .Select(e => e.UserId)
                .Distinct()
                .CountAsync();

            var orders = await db.Orders
                .CountAsync(o => o.OrderDate >= date && o.OrderDate < nextDay);

            var signups = await db.Users
                .CountAsync(u => u.CreatedAt >= date && u.CreatedAt < nextDay);

            result.Add(new UserActivityDto(date.ToString("MMM dd"), activeUsers, orders, signups));
        }

        return result;
    }

    private async Task<List<TrendingInsightDto>> GetTrendingInsightsAsync()
    {
        var insights = await db.OrderItems
            .Include(oi => oi.Recipe)
            .Include(oi => oi.Order).ThenInclude(o => o.User)
            .GroupBy(oi => oi.Recipe.Name)
            .Select(g => new
            {
                DishName = g.Key,
                OrderCount = g.Count(),
                RepeatOrders = g.GroupBy(oi => oi.Order.UserId).Count(ug => ug.Count() > 1),
                AvgAge = g.Where(oi => oi.Order.User.Age != null).Average(oi => (double?)oi.Order.User.Age) ?? 25
            })
            .OrderByDescending(x => x.OrderCount)
            .Take(5)
            .ToListAsync();

        return insights.Select(i =>
        {
            var ageGroup = GetAgeGroup(i.AvgAge);
            var trend = i.OrderCount > 10 ? "Rising" : i.OrderCount > 5 ? "Stable" : "Emerging";
            var insight = GenerateInsight(i.DishName, ageGroup, i.RepeatOrders, trend);
            return new TrendingInsightDto(i.DishName, trend, ageGroup, i.RepeatOrders, insight);
        }).ToList();
    }

    private static string GetAgeGroup(double age) => age switch
    {
        < 20 => "Under 20",
        < 30 => "20-30",
        < 40 => "30-40",
        < 50 => "40-50",
        _ => "50+"
    };

    private static string GenerateInsight(string dish, string ageGroup, int repeatOrders, string trend)
    {
        if (repeatOrders > 5)
            return $"{dish} has high repeat orders among users aged {ageGroup}";
        if (trend == "Rising")
            return $"{dish} is trending among users aged {ageGroup} with growing popularity";
        return $"{dish} shows steady interest in the {ageGroup} age group";
    }
}
