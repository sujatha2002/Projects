using FoodieCart.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodieCart.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AnalyticsController(AnalyticsService analyticsService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult> GetDashboard()
    {
        return Ok(await analyticsService.GetDashboardAsync());
    }
}
