using HydraEnergyAPI.Configuration;
using HydraEnergyAPI.Middleware;
using HydraEnergyAPI.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:8080")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Configure Settings
builder.Services.Configure<HydraSettings>(builder.Configuration.GetSection("HydraSettings"));
builder.Services.Configure<WeatherSettings>(builder.Configuration.GetSection("WeatherSettings"));

// Register HttpClient
builder.Services.AddHttpClient();

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HYDRA Energy Intelligence API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

// Use custom exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enable CORS
app.UseCors("AllowFrontend");

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

// Root endpoint
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Logger.LogInformation("HYDRA Energy Intelligence API starting...");
app.Logger.LogInformation("Swagger UI available at: http://localhost:5000 (or configured port)");

app.Run();
