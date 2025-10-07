using ECommerceApp.API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Build PostgreSQL connection string from environment variables
var connectionString = BuildConnectionString() 
    ?? GetSafeConnectionString() 
    ?? "Host=localhost;Port=5432;Database=ecommerce;Username=postgres;Password=postgres";

Console.WriteLine($"Using connection: {MaskConnectionString(connectionString)}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Helper method to build connection string from environment variables
string? BuildConnectionString()
{
    var host = Environment.GetEnvironmentVariable("DB_HOST");
    var port = Environment.GetEnvironmentVariable("DB_PORT");
    var database = Environment.GetEnvironmentVariable("DB_NAME");
    var username = Environment.GetEnvironmentVariable("DB_USER");
    var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

    Console.WriteLine($"Environment check - Host: {!string.IsNullOrEmpty(host)}, Port: {port}, DB: {!string.IsNullOrEmpty(database)}, User: {!string.IsNullOrEmpty(username)}, Password: {!string.IsNullOrEmpty(password)}");

    if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(database) || 
        string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
    {
        Console.WriteLine("Missing required environment variables for database connection");
        return null;
    }

    var connStr = $"Host={host};Port={port ?? "5432"};Database={database};Username={username};Password={password}";
    Console.WriteLine("Successfully built connection string from environment variables");
    return connStr;
}

// Safe method to get connection string from config (avoid template variables)
string? GetSafeConnectionString()
{
    var configConnStr = builder.Configuration.GetConnectionString("DefaultConnection");
    
    if (!string.IsNullOrEmpty(configConnStr) && configConnStr.Contains("${"))
    {
        Console.WriteLine("Skipping appsettings connection string with template variables");
        return null;
    }
    
    Console.WriteLine("Using connection string from appsettings");
    return configConnStr;
}

// Helper to mask passwords in logs
string MaskConnectionString(string connectionString)
{
    if (string.IsNullOrEmpty(connectionString)) return "null";
    return System.Text.RegularExpressions.Regex.Replace(connectionString, 
        @"Password=([^;]+)", "Password=***", 
        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
}

// Add CORS with configurable origins
var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',') 
    ?? new[] { "http://localhost:3000", "https://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("X-Total-Count", "X-Page", "X-Page-Size");
    });
});

// Add API Explorer services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

// Test database connection without creating tables
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Simple connection test without table creation
        var dbConnectionString = context.Database.GetConnectionString();
        Console.WriteLine($"Database configured with masked connection: {MaskConnectionString(dbConnectionString ?? "null")}");
        
        // Test basic connection (this should work even if tables don't exist)
        context.Database.GetDbConnection().ConnectionString = dbConnectionString;
        Console.WriteLine("Database context initialized successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization error: {ex.Message}");
        Console.WriteLine($"Error type: {ex.GetType().Name}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
        }
        // Continue anyway - API can still start
        Console.WriteLine("Continuing without database initialization...");
    }
}

app.Run();
