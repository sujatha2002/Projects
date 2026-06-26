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
[Authorize]
public class CartController(AppDbContext db, CartOptimizationService cartService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult<CartSummaryDto>> GetCart()
    {
        return Ok(await cartService.GetOptimizedCartAsync(UserId));
    }

    [HttpPost("add")]
    public async Task<ActionResult<CartSummaryDto>> AddToCart(AddToCartRequest request)
    {
        var recipe = await db.Recipes.FindAsync(request.RecipeId);
        if (recipe == null) return NotFound("Recipe not found");

        var existing = await db.CartItems
            .FirstOrDefaultAsync(c => c.UserId == UserId && c.RecipeId == request.RecipeId);

        if (existing != null)
        {
            existing.Quantity += request.Quantity;
        }
        else
        {
            db.CartItems.Add(new Models.CartItem
            {
                UserId = UserId,
                RecipeId = request.RecipeId,
                Quantity = request.Quantity
            });
        }

        await db.SaveChangesAsync();
        return Ok(await cartService.GetOptimizedCartAsync(UserId));
    }

    [HttpPut("{id}/quantity")]
    public async Task<ActionResult<CartSummaryDto>> UpdateQuantity(int id, [FromBody] int quantity)
    {
        var item = await db.CartItems.FirstOrDefaultAsync(c => c.Id == id && c.UserId == UserId);
        if (item == null) return NotFound();

        if (quantity <= 0)
            db.CartItems.Remove(item);
        else
            item.Quantity = quantity;

        await db.SaveChangesAsync();
        return Ok(await cartService.GetOptimizedCartAsync(UserId));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<CartSummaryDto>> RemoveItem(int id)
    {
        var item = await db.CartItems.FirstOrDefaultAsync(c => c.Id == id && c.UserId == UserId);
        if (item == null) return NotFound();

        db.CartItems.Remove(item);
        await db.SaveChangesAsync();
        return Ok(await cartService.GetOptimizedCartAsync(UserId));
    }

    [HttpPost("checkout")]
    public async Task<ActionResult> Checkout()
    {
        var items = await db.CartItems
            .Include(c => c.Recipe)
            .Where(c => c.UserId == UserId)
            .ToListAsync();

        if (!items.Any()) return BadRequest("Cart is empty");

        var order = new Models.Order
        {
            UserId = UserId,
            TotalAmount = items.Sum(i => i.Recipe.Price * i.Quantity),
            Status = "Completed"
        };

        foreach (var item in items)
        {
            order.Items.Add(new Models.OrderItem
            {
                RecipeId = item.RecipeId,
                Quantity = item.Quantity,
                UnitPrice = item.Recipe.Price
            });
        }

        db.Orders.Add(order);
        db.CartItems.RemoveRange(items);
        await db.SaveChangesAsync();

        return Ok(new { order.Id, order.TotalAmount, Message = "Order placed successfully!" });
    }
}
