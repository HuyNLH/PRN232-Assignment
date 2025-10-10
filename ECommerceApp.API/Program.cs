using ECommerceApp.API.Data;
using ECommerceApp.API.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework - Use InMemory database for development, PostgreSQL for production
var isDevelopment = builder.Environment.IsDevelopment();

if (isDevelopment)
{
    // Use InMemory database for development
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseInMemoryDatabase("ECommerceDb");
    });
    Console.WriteLine("Using InMemory database for development");
}
else
{
    // Use PostgreSQL for production
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("No database connection string found.");
    }

    // Convert URI format to Npgsql connection string format
    string finalConnectionString;
    try
    {
        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':');
        var username = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : "";
        var host = uri.Host;
        var database = uri.AbsolutePath.TrimStart('/');

        finalConnectionString = $"Host={host};Database={database};Username={username};Password={password};SSL Mode=Prefer;Trust Server Certificate=true";
        
        Console.WriteLine($"Database connection configured: Host={host}, Database={database}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Connection string parsing failed: {ex.Message}");
        finalConnectionString = connectionString; // Fallback to original
    }

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(finalConnectionString);
    });
}

// Add CORS - Allow all Vercel deployments and localhost
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.SetIsOriginAllowed(origin =>
            {
                // Allow any Vercel deployment domain
                if (origin.Contains("vercel.app")) return true;
                
                // Allow localhost for development
                if (origin.StartsWith("http://localhost") || origin.StartsWith("https://localhost")) return true;
                
                return false;
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("X-Total-Count", "X-Page", "X-Page-Size"); // Expose custom headers
        });
});

var app = builder.Build();

// Apply database migrations automatically (only for relational databases)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<ApplicationDbContext>();
    
    try
    {
        // Only apply migrations for relational databases (not InMemory)
        if (context.Database.IsInMemory())
        {
            logger.LogInformation("Using InMemory database - ensuring database is created");
            await context.Database.EnsureCreatedAsync();
            
            // Seed sample data for InMemory database
            if (!context.Products.Any())
            {
                logger.LogInformation("Seeding sample products...");
                var sampleProducts = new[]
                {
                    new Product { Name = "Classic T-Shirt", Description = "Comfortable cotton t-shirt", Price = 19.99m, Image = "https://via.placeholder.com/300x400?text=T-Shirt", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Product { Name = "Denim Jeans", Description = "Stylish blue jeans", Price = 49.99m, Image = "https://via.placeholder.com/300x400?text=Jeans", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Product { Name = "Leather Jacket", Description = "Premium leather jacket", Price = 199.99m, Image = "https://via.placeholder.com/300x400?text=Jacket", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Product { Name = "Summer Dress", Description = "Light and breezy dress", Price = 39.99m, Image = "https://via.placeholder.com/300x400?text=Dress", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Product { Name = "Sneakers", Description = "Comfortable running sneakers", Price = 79.99m, Image = "https://via.placeholder.com/300x400?text=Sneakers", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Product { Name = "Winter Coat", Description = "Warm winter coat", Price = 149.99m, Image = "https://via.placeholder.com/300x400?text=Coat", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Product { Name = "Baseball Cap", Description = "Stylish baseball cap", Price = 24.99m, Image = "https://via.placeholder.com/300x400?text=Cap", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Product { Name = "Hoodie", Description = "Cozy pullover hoodie", Price = 59.99m, Image = "https://via.placeholder.com/300x400?text=Hoodie", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Product { Name = "Shorts", Description = "Casual summer shorts", Price = 29.99m, Image = "https://via.placeholder.com/300x400?text=Shorts", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Product { Name = "Scarf", Description = "Elegant silk scarf", Price = 34.99m, Image = "https://via.placeholder.com/300x400?text=Scarf", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                };
                
                context.Products.AddRange(sampleProducts);
                await context.SaveChangesAsync();
                logger.LogInformation($"Seeded {sampleProducts.Length} sample products");
            }
        }
        else
        {
            logger.LogInformation("Checking for pending database migrations...");
            
            // Get pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            
            if (pendingMigrations.Any())
            {
                logger.LogInformation($"Found {pendingMigrations.Count()} pending migration(s)");
                foreach (var migration in pendingMigrations)
                {
                    logger.LogInformation($"  - {migration}");
                }
                
                logger.LogInformation("Applying migrations...");
                await context.Database.MigrateAsync();
                logger.LogInformation("All migrations applied successfully!");
            }
            else
            {
                logger.LogInformation("Database is up to date - no pending migrations");
            }
            
            // Log applied migrations
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
            logger.LogInformation($"Total applied migrations: {appliedMigrations.Count()}");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during database initialization: {Message}", ex.Message);
        logger.LogError("Application will continue but database operations may fail");
        // Don't throw - let the app start
    }
}

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ECommerce API V1");
    c.RoutePrefix = "swagger";
});

// Middleware pipeline
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

// Configure port
var port = Environment.GetEnvironmentVariable("PORT") ?? (app.Environment.IsDevelopment() ? "5000" : "80");
var urls = $"http://0.0.0.0:{port}";
app.Urls.Clear(); // Clear default URLs
app.Urls.Add(urls);

Console.WriteLine($"Application started successfully on {urls}");

app.Run();