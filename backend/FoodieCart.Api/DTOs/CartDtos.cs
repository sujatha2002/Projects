namespace FoodieCart.Api.DTOs;

public record CartItemDto(int Id, int RecipeId, string RecipeName, decimal Price, int Quantity, decimal Subtotal);
public record CartSummaryDto(List<CartItemDto> Items, decimal Total, List<CartSuggestionDto> Suggestions, List<BundleOfferDto> BundleOffers);
public record CartSuggestionDto(int RecipeId, string RecipeName, decimal Price, string Reason, double Confidence);
public record BundleOfferDto(int BundleId, string Name, string Description, decimal DiscountPercent, decimal SavingsAmount, List<RecipeDto> Items);
public record AddToCartRequest(int RecipeId, int Quantity = 1);
