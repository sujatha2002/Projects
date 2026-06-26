using System.Security.Claims;
using FoodieCart.Api.Data;
using FoodieCart.Api.DTOs;
using FoodieCart.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodieCart.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipesController(AppDbContext db, RecipeRecommendationService recommendationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<RecipeDto>>> GetAll([FromQuery] string? category)
    {
        var query = db.Recipes.AsQueryable();
        if (!string.IsNullOrEmpty(category))
            query = query.Where(r => r.Category == category);

        var recipes = await query.OrderByDescending(r => r.PopularityScore).ToListAsync();
        return Ok(recipes.Select(r => ToDto(r)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RecipeDto>> GetById(int id)
    {
        var recipe = await db.Recipes.FindAsync(id);
        if (recipe == null) return NotFound();
        return Ok(ToDto(recipe));
    }

    [Authorize]
    [HttpGet("suggestions")]
    public async Task<ActionResult<List<RecipeSuggestionDto>>> GetSuggestions([FromQuery] string? category)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var suggestions = await recommendationService.GetSuggestionsAsync(userId, category);
        return Ok(suggestions);
    }

    [HttpGet("trending")]
    public async Task<ActionResult<List<RecipeSuggestionDto>>> GetTrending()
    {
        return Ok(await recommendationService.GetTrendingAsync());
    }

    [Authorize(Roles = "Vendor,Admin")]
    [HttpPost]
    public async Task<ActionResult<RecipeDto>> Create(RecipeDto dto)
    {
        var recipe = new Models.Recipe
        {
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Category,
            Price = dto.Price,
            ImageUrl = dto.ImageUrl,
            IsVegetarian = dto.IsVegetarian,
            IsVegan = dto.IsVegan,
            PrepTimeMinutes = dto.PrepTimeMinutes,
            PopularityScore = 0,
            Tags = dto.Tags,
            VendorId = User.FindFirstValue(ClaimTypes.NameIdentifier)
        };

        db.Recipes.Add(recipe);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = recipe.Id }, ToDto(recipe));
    }

    private static RecipeDto ToDto(Models.Recipe r) => new(
        r.Id, r.Name, r.Description, r.Category, r.Price, r.ImageUrl,
        r.IsVegetarian, r.IsVegan, r.PrepTimeMinutes, r.PopularityScore, r.Tags);
}
