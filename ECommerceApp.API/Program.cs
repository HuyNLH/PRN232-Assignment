using ECommerceApp.API.Data;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

// Load environment variables from .env file (development only)
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production")
{
    var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
    if (File.Exists(envPath))
    {
        Env.Load(envPath);
        Console.WriteLine($"Loaded .env from: {envPath}");
    }
    else if (File.Exists(".env"))
    {
        Env.Load();
        Console.WriteLine("Loaded .env from current directory");
    }
    else
    {
        Console.WriteLine("No .env file found - using system environment variables");
    }
}
else
{
    Console.WriteLine("Production environment - using system environment variables only");
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Build PostgreSQL connection string from environment variables
var connectionString = BuildConnectionString() 
    ?? GetSafeConnectionString() 
    ?? "Host=localhost;Port=5432;Database=ecommerce;Username=postgres;Password=postgres;Connect Timeout=30;Command Timeout=30";

Console.WriteLine($"Using connection: {MaskConnectionString(connectionString)}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.CommandTimeout(30); // 30 second timeout
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    }));

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

    var connStr = $"Host={host};Port={port ?? "5432"};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;Timeout=30";
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
// Enable Swagger in all environments for testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ECommerceApp API V1");
    c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
});

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

// Log database configuration without testing connection
Console.WriteLine("Database configuration completed");
Console.WriteLine("API will connect to database on first request");

app.Run();
