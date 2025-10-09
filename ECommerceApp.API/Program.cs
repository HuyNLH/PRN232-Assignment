using ECommerceApp.API.Data;
using Microsoft.EntityFrameworkCore;
using ECommerceApp.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Get database connection string
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

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(finalConnectionString);
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "prn-232-assignment-rmjotwcar-huynlh04s-projects.vercel.app"
            )
            .SetIsOriginAllowed(origin =>
            {
                // Allow any Vercel app domain
                return origin.Contains("vercel.app") ||
                       origin.StartsWith("http://localhost") ||
                       origin.StartsWith("https://localhost");
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
});

var app = builder.Build();

// Apply database migrations automatically
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<ApplicationDbContext>();
    
    try
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
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during database migration: {Message}", ex.Message);
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
var port = Environment.GetEnvironmentVariable("PORT") ?? "80";
var urls = $"http://0.0.0.0:{port}";
app.Urls.Add(urls);

Console.WriteLine($"Application started successfully on {urls}");

app.Run();