using ECommerceApp.API.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Helper: detect template placeholders (Render/CI templates like ${DB_PORT})
static bool IsTemplate(string? s) => !string.IsNullOrEmpty(s) && s.Contains("${");

// Build a validated connection string or return null if missing/invalid
string? BuildConnectionString()
{
    // 1) Try DATABASE_URL (Heroku-style)
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrEmpty(databaseUrl) && !IsTemplate(databaseUrl))
    {
        try
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':', 2);
            var username = userInfo.Length > 0 ? userInfo[0] : string.Empty;
            var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath?.TrimStart('/') ?? string.Empty;

            var csb = new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Port = port,
                Database = database,
                Username = username,
                Password = password,
                SslMode = SslMode.Prefer,
                Timeout = 30
            };

            Console.WriteLine($"Database connection configured from DATABASE_URL: Host={host}, Database={database}");
            return csb.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection string parsing failed: {ex.Message}");
            // fallback to per-field env
        }
    }

    // 2) Try individual environment variables
    var hostEnv = Environment.GetEnvironmentVariable("DB_HOST");
    var portEnv = Environment.GetEnvironmentVariable("DB_PORT");
    var nameEnv = Environment.GetEnvironmentVariable("DB_NAME");
    var userEnv = Environment.GetEnvironmentVariable("DB_USER");
    var passEnv = Environment.GetEnvironmentVariable("DB_PASSWORD");

    if (IsTemplate(hostEnv) || IsTemplate(portEnv) || IsTemplate(nameEnv) || IsTemplate(userEnv) || IsTemplate(passEnv))
    {
        Console.WriteLine("Detected template placeholders in DB env vars; skipping using them at runtime.");
        return null;
    }

    if (string.IsNullOrEmpty(hostEnv) || string.IsNullOrEmpty(nameEnv) || string.IsNullOrEmpty(userEnv) || string.IsNullOrEmpty(passEnv))
    {
        Console.WriteLine("One or more DB_* environment variables are missing; skipping DB configuration.");
        return null;
    }

    if (!string.IsNullOrEmpty(portEnv) && !int.TryParse(portEnv, out var portValue))
    {
        Console.WriteLine($"Invalid DB_PORT value: '{portEnv}'");
        return null;
    }

    var portFinal = string.IsNullOrEmpty(portEnv) ? 5432 : int.Parse(portEnv!);

    var builderCsb = new NpgsqlConnectionStringBuilder
    {
        Host = hostEnv,
        Port = portFinal,
        Database = nameEnv,
        Username = userEnv,
        Password = passEnv,
    SslMode = SslMode.Prefer,
        Timeout = 30
    };

    Console.WriteLine($"Database connection configured from DB_* env: Host={hostEnv}, Database={nameEnv}");
    return builderCsb.ToString();
}

// Determine final connection string: env-built or appsettings (if appsettings not templated)
var finalConnectionString = BuildConnectionString();
if (string.IsNullOrEmpty(finalConnectionString))
{
    var cfg = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(cfg) && !IsTemplate(cfg))
    {
        finalConnectionString = cfg;
        Console.WriteLine("Using connection string from configuration (appsettings).");
    }
}

// Register DbContext. If no valid connection string, register an in-memory DB so the app can start.
if (!string.IsNullOrEmpty(finalConnectionString))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(finalConnectionString);
    });
}
else
{
    Console.WriteLine("No valid database connection available - registering InMemory provider so the app can start.");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseInMemoryDatabase("InMemoryDb");
    });
}

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "https://prn-232-assignment-gamma.vercel.app"
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

// Apply database migrations automatically, but only when we have a relational DB
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<ApplicationDbContext>();

    try
    {
        if (string.IsNullOrEmpty(finalConnectionString))
        {
            logger.LogInformation("No valid connection string - skipping database migrations.");
        }
        else if (!context.Database.IsRelational())
        {
            logger.LogInformation("Database provider is not relational - skipping relational migration operations.");
        }
        else
        {
            logger.LogInformation("Checking for pending database migrations...");

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

            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
            logger.LogInformation($"Total applied migrations: {appliedMigrations.Count()}");
        }
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