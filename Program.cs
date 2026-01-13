using HydraEnergyAPI.Configuration;
using HydraEnergyAPI.Middleware;
using HydraEnergyAPI.Services;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure CORS
var corsOrigins = builder.Configuration["CORS_ORIGINS"]?.Split(',') ?? new[]
{
    "http://localhost:3000",
    "http://localhost:5173",
    "http://localhost:8080"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // Check if wildcard is specified
        if (corsOrigins.Contains("*"))
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// Configure Settings
builder.Services.Configure<HydraSettings>(builder.Configuration.GetSection("HydraSettings"));
builder.Services.Configure<WeatherSettings>(builder.Configuration.GetSection("WeatherSettings"));

// Register HttpClient with base configuration
builder.Services.AddHttpClient();

// Configure named HttpClient for HYDRA API
builder.Services.AddHttpClient("HydraAPI", (serviceProvider, client) =>
{
    var hydraSettings = serviceProvider.GetRequiredService<IOptions<HydraSettings>>().Value;
    // Don't set BaseAddress here - services will use full URLs
});

// Register Services
builder.Services.AddScoped<IHydraAuthService, HydraAuthService>();
builder.Services.AddScoped<IEnergyDataService, EnergyDataService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IForecastingService, ForecastingService>();
builder.Services.AddScoped<IInsightsService, InsightsService>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HYDRA Energy Intelligence API",
        Version = "v1",
        Description = "API for HYDRA Energy Intelligence Dashboard - Provides energy data, analytics, forecasting, and insights",
        Contact = new OpenApiContact
        {
            Name = "HYDRA Dashboard",
            Email = "support@hydra.africa"
        }
    });

    // Add XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HYDRA Energy Intelligence API v1");
    c.RoutePrefix = "swagger";
});

// Use custom exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enable CORS
app.UseCors("AllowFrontend");

// Commented out for Render deployment (uses internal HTTP with external HTTPS)
//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    service = "HYDRA Energy Intelligence API",
    version = "1.0.0"
}));

// Root endpoint - returns API info
app.MapGet("/", () => Results.Ok(new
{
    message = "HYDRA Energy Intelligence API",
    version = "1.0.0",
    swagger = "/swagger",
    health = "/health",
    endpoints = new
    {
        auth = "/api/auth/token",
        energy = "/api/energy",
        analytics = "/api/analytics",
        forecast = "/api/forecast",
        insights = "/api/insights",
        weather = "/api/weather"
    }
}));

app.Logger.LogInformation("HYDRA Energy Intelligence API starting...");
app.Logger.LogInformation($"Environment: {app.Environment.EnvironmentName}");
app.Logger.LogInformation("Swagger UI available at: /swagger");

app.Run();