namespace FoodieCart.Api.DTOs;

public record PopularDishDto(string Name, int OrderCount, double RepeatRate, string AgeGroup);
public record CartAbandonmentDto(string Category, int AbandonedCount, int TotalCarts, double AbandonmentRate);
public record UserActivityDto(string Date, int ActiveUsers, int Orders, int NewSignups);
public record TrendingInsightDto(string DishName, string Trend, string AgeGroup, int RepeatOrders, string Insight);
public record AnalyticsDashboardDto(
    List<PopularDishDto> PopularDishes,
    List<CartAbandonmentDto> CartAbandonment,
    List<UserActivityDto> UserActivity,
    List<TrendingInsightDto> TrendingInsights);
