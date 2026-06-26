using System.Text;
using FoodieCart.Api.Data;
using FoodieCart.Api.Models;
using FoodieCart.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
})
.AddFacebook(options =>
{
    options.AppId = builder.Configuration["Authentication:Facebook:AppId"]!;
    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"]!;
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<RecipeRecommendationService>();
builder.Services.AddScoped<CartOptimizationService>();
builder.Services.AddScoped<AnalyticsService>();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Foodie Cart API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins(
                "http://localhost:4200",
                "http://127.0.0.1:4200",
                builder.Configuration["FrontendUrl"] ?? "http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/api/auth/google", () => Results.Challenge(
    new Microsoft.AspNetCore.Authentication.AuthenticationProperties { RedirectUri = "/api/auth/google-callback" },
    new[] { "Google" }));

app.MapGet("/api/auth/facebook", () => Results.Challenge(
    new Microsoft.AspNetCore.Authentication.AuthenticationProperties { RedirectUri = "/api/auth/facebook-callback" },
    new[] { "Facebook" }));

app.MapGet("/api/auth/google-callback", async (HttpContext context, UserManager<ApplicationUser> userManager, JwtTokenService jwt, AppDbContext db) =>
{
    var email = context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    var name = context.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? email;
    if (email == null) return Results.Redirect($"{builder.Configuration["FrontendUrl"]}/login?error=oauth_failed");

    var user = await userManager.FindByEmailAsync(email);
    if (user == null)
    {
        user = new ApplicationUser { UserName = email, Email = email, FullName = name, OAuthProvider = "Google", EmailConfirmed = true };
        await userManager.CreateAsync(user);
        await userManager.AddToRoleAsync(user, Roles.Customer);
        await db.UserPreferences.AddAsync(new UserPreference { UserId = user.Id });
        await db.SaveChangesAsync();
    }

    var roles = await userManager.GetRolesAsync(user);
    var token = jwt.GenerateToken(user, roles);
    return Results.Redirect($"{builder.Configuration["FrontendUrl"]}/oauth-callback?token={token.Token}&email={email}&role={roles.FirstOrDefault()}");
});

await DbSeeder.SeedAsync(app.Services);

app.Run();
