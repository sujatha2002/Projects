namespace FoodieCart.Api.Services;

public static class RecipeImageDownloader
{
    private static readonly Dictionary<string, (string FileName, string SourceUrl)> ImageMap = new()
    {
        ["Classic Beef Burger"] = ("classic-beef-burger.jpg", "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=600&q=80"),
        ["Veggie Burger"] = ("veggie-burger.jpg", "https://images.unsplash.com/photo-1520072959219-c595dc870360?w=600&q=80"),
        ["French Fries"] = ("french-fries.jpg", "https://loremflickr.com/600/400/french,fries,food/all"),
        ["Soft Drink"] = ("soft-drink.jpg", "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=600&q=80"),
        ["Spaghetti Carbonara"] = ("spaghetti-carbonara.jpg", "https://loremflickr.com/600/400/spaghetti,carbonara,food/all"),
        ["Spinach Alfredo"] = ("spinach-alfredo.jpg", "https://images.unsplash.com/photo-1621996346565-e3dbc646d9a9?w=600&q=80"),
        ["Vegan Carbonara"] = ("vegan-carbonara.jpg", "https://images.unsplash.com/photo-1563379926898-05f4575a45d8?w=600&q=80"),
        ["Garlic Bread"] = ("garlic-bread.jpg", "https://loremflickr.com/600/400/garlic,bread,food/all"),
        ["Caesar Salad"] = ("caesar-salad.jpg", "https://images.unsplash.com/photo-1546793665-c74683f339c1?w=600&q=80"),
        ["Margherita Pizza"] = ("margherita-pizza.jpg", "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=600&q=80"),
        ["Paneer Butter Masala"] = ("paneer-butter-masala.jpg", "https://loremflickr.com/600/400/paneer,curry,indian/all"),
        ["Chicken Tikka Masala"] = ("chicken-tikka-masala.jpg", "https://images.unsplash.com/photo-1565557623262-b51c2513a641?w=600&q=80"),
        ["Naan Bread"] = ("naan-bread.jpg", "https://images.unsplash.com/photo-1601050690597-df0568f70950?w=600&q=80"),
        ["Mango Lassi"] = ("mango-lassi.jpg", "https://loremflickr.com/600/400/mango,lassi,drink/all"),
        ["Tomato Soup"] = ("tomato-soup.jpg", "https://images.unsplash.com/photo-1547592166-23ac45744acd?w=600&q=80"),
        ["Bread Roll"] = ("bread-roll.jpg", "https://loremflickr.com/600/400/bread,roll,bakery/all")
    };

    public static async Task EnsureImagesAsync(IWebHostEnvironment env)
    {
        var dir = Path.Combine(env.WebRootPath, "images", "recipes");
        Directory.CreateDirectory(dir);

        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("FoodieCart/1.0");

        foreach (var entry in ImageMap.Values)
        {
            var path = Path.Combine(dir, entry.FileName);
            if (File.Exists(path) && new FileInfo(path).Length > 5000) continue;

            try
            {
                var bytes = await http.GetByteArrayAsync(entry.SourceUrl);
                if (bytes.Length > 5000)
                    await File.WriteAllBytesAsync(path, bytes);
            }
            catch
            {
                // Keep existing bundled image if download fails
            }
        }
    }

    public static string GetLocalUrl(string recipeName) =>
        ImageMap.TryGetValue(recipeName, out var entry)
            ? $"/images/recipes/{entry.FileName}"
            : "/images/recipes/classic-beef-burger.jpg";
}
