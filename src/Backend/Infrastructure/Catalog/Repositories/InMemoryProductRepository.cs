using Domain.Catalog;

namespace Infrastructure.Catalog.Repositories;

public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products = new()
    {
        new Product
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "Avocados",
            Description = "Creamy Hass avocados picked at peak ripeness. Ideal for smashing into guacamole or topping tacos. Perfectly ripe and ready for slicing. Rich in healthy fats and naturally creamy.",
            Price = 1.00m,
            SKU = "AVO-001",
            Category = "Produce",
            Brand = "Fresh Farm",
            ImageUrl = "https://images.unsplash.com/photo-1523049673857-eb18f1d7b578?w=800&h=800&fit=crop",
            ImageUrls = new List<string>
            {
                "https://images.unsplash.com/photo-1523049673857-eb18f1d7b578?w=800&h=800&fit=crop",
                "https://images.unsplash.com/photo-1601034913836-a1f3e82555b0?w=800&h=800&fit=crop",
                "https://images.unsplash.com/photo-1584270354949-c26b0d5b4a0c?w=800&h=800&fit=crop"
            },
            Stock = 50,
            Attributes = new Dictionary<string, string>
            {
                { "Fiber", "7g" },
                { "Fat", "15g" },
                { "Potassium", "485mg" },
                { "Calories", "160" },
                { "Package", "3 ct" },
                { "PricePerUnit", "$1.00/ea" }
            },
            Features = new List<string>
            {
                "Picked at peak ripeness",
                "Perfect for guacamole",
                "Rich in healthy fats",
                "Naturally creamy texture",
                "Ready to eat"
            }
        },
        new Product
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Name = "Hojicha Pizza",
            Description = "Delicious pizza with hojicha sauce & honey. A unique blend of roasted green tea flavor with sweet honey drizzle.",
            Price = 12.99m,
            SKU = "PIZ-001",
            Category = "Food",
            Brand = "Artisan Pizza",
            ImageUrl = "https://images.unsplash.com/photo-1513104890138-7c749659a591?w=800&h=800&fit=crop",
            ImageUrls = new List<string>
            {
                "https://images.unsplash.com/photo-1513104890138-7c749659a591?w=800&h=800&fit=crop",
                "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=800&h=800&fit=crop",
                "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=800&h=800&fit=crop"
            },
            Stock = 20,
            Attributes = new Dictionary<string, string>
            {
                { "Protein", "18g" },
                { "Carbs", "35g" },
                { "Fat", "12g" },
                { "Calories", "320" },
                { "Size", "12 inch" }
            },
            Features = new List<string>
            {
                "Roasted green tea flavor",
                "Sweet honey drizzle",
                "Hand-crafted dough",
                "Fresh ingredients",
                "12 inch size"
            }
        },
        new Product
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Name = "Pesto & Tomatoes Pizza",
            Description = "Fresh pizza topped with basil pesto and cherry tomatoes. A classic combination of flavors.",
            Price = 11.99m,
            SKU = "PIZ-002",
            Category = "Food",
            Brand = "Artisan Pizza",
            ImageUrl = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=800&h=800&fit=crop",
            ImageUrls = new List<string>
            {
                "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=800&h=800&fit=crop",
                "https://images.unsplash.com/photo-1513104890138-7c749659a591?w=800&h=800&fit=crop",
                "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=800&h=800&fit=crop"
            },
            Stock = 15,
            Attributes = new Dictionary<string, string>
            {
                { "Protein", "16g" },
                { "Carbs", "38g" },
                { "Fat", "14g" },
                { "Calories", "340" },
                { "Size", "12 inch" }
            },
            Features = new List<string>
            {
                "Basil pesto sauce",
                "Cherry tomatoes",
                "Classic Italian flavors",
                "Fresh mozzarella",
                "12 inch size"
            }
        },
        new Product
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
            Name = "Meaty Chicken Drumsticks",
            Description = "Juicy chicken drumsticks seasoned to perfection. Great for grilling or baking.",
            Price = 8.99m,
            SKU = "CHK-001",
            Category = "Meat",
            Brand = "Farm Fresh",
            ImageUrl = "https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800&h=800&fit=crop",
            ImageUrls = new List<string>
            {
                "https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800&h=800&fit=crop",
                "https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800&h=800&fit=crop&q=80",
                "https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800&h=800&fit=crop&q=60"
            },
            Stock = 30,
            Attributes = new Dictionary<string, string>
            {
                { "Protein", "28g" },
                { "Carbs", "0g" },
                { "Fat", "8g" },
                { "Calories", "190" },
                { "Package", "4 pieces" }
            },
            Features = new List<string>
            {
                "Premium quality chicken",
                "Perfectly seasoned",
                "Great for grilling",
                "Ideal for baking",
                "4 pieces per package"
            }
        }
    };

    public Task<IEnumerable<Product>> GetAllAsync(string shopKey, CancellationToken cancellationToken = default)
    {
        // shopKey is ignored for InMemory repository
        return Task.FromResult<IEnumerable<Product>>(_products);
    }

    public Task<Product?> GetByIdAsync(Guid id, string shopKey, CancellationToken cancellationToken = default)
    {
        // shopKey is ignored for InMemory repository
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }

    public Task<IEnumerable<Product>> GetByCategoryAsync(string category, string shopKey, CancellationToken cancellationToken = default)
    {
        // shopKey is ignored for InMemory repository
        var products = _products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(products);
    }

    public Task<IEnumerable<Product>> SearchAsync(string searchTerm, string shopKey, CancellationToken cancellationToken = default)
    {
        // shopKey is ignored for InMemory repository
        var products = _products.Where(p =>
            p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            p.SKU.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(products);
    }
}
