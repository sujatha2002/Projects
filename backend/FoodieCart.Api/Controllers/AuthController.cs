using System.Security.Claims;
using FoodieCart.Api.Data;
using FoodieCart.Api.DTOs;
using FoodieCart.Api.Models;
using FoodieCart.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodieCart.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    JwtTokenService jwtService,
    AppDbContext db) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var allowedRoles = new[] { Roles.Customer, Roles.Vendor };
        var role = allowedRoles.Contains(request.Role) ? request.Role : Roles.Customer;

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        await userManager.AddToRoleAsync(user, role);
        await db.UserPreferences.AddAsync(new UserPreference { UserId = user.Id, DietaryType = "None" });
        await db.SaveChangesAsync();

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtService.GenerateToken(user, roles);

        return Ok(new AuthResponse(token.Token, user.Email!, user.FullName!, role, token.ExpiresAt));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized("Invalid email or password");

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            return Unauthorized("Invalid email or password");

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtService.GenerateToken(user, roles);

        return Ok(new AuthResponse(token.Token, user.Email!, user.FullName ?? "", roles.FirstOrDefault() ?? Roles.Customer, token.ExpiresAt));
    }

    [HttpPost("oauth-callback")]
    public async Task<ActionResult<AuthResponse>> OAuthCallback(OAuthCallbackRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                OAuthProvider = request.Provider,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(user);
            await userManager.AddToRoleAsync(user, Roles.Customer);
            await db.UserPreferences.AddAsync(new UserPreference { UserId = user.Id, DietaryType = "None" });
            await db.SaveChangesAsync();
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtService.GenerateToken(user, roles);

        return Ok(new AuthResponse(token.Token, user.Email!, user.FullName ?? "", roles.FirstOrDefault() ?? Roles.Customer, token.ExpiresAt));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await userManager.FindByIdAsync(userId!);
        if (user == null) return NotFound();

        var roles = await userManager.GetRolesAsync(user);
        var preference = await db.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);

        return Ok(new
        {
            user.Email,
            user.FullName,
            Role = roles.FirstOrDefault(),
            user.DietaryPreference,
            Preference = preference != null ? new UserPreferenceDto(preference.DietaryType, preference.FavoriteCategories, preference.Allergies, preference.SpiceLevel) : null
        });
    }
}
