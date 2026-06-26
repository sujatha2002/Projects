namespace FoodieCart.Api.DTOs;

public record RegisterRequest(string Email, string Password, string FullName, string Role = "Customer");
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string Email, string FullName, string Role, DateTime ExpiresAt);
public record OAuthCallbackRequest(string Provider, string Email, string FullName, string? ProviderId);
