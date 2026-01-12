# HYDRA Energy Intelligence Dashboard - Backend API

## Overview

This is the .NET 9 Web API backend for the HYDRA Energy Intelligence Dashboard. It provides a comprehensive API for energy monitoring, analytics, forecasting, and insights generation.

## Features

- **OAuth2 Authentication**: Secure authentication with HYDRA identity server
- **Energy Data Retrieval**: Fetch historical energy consumption data with kWh calculations
- **Weather Integration**: OpenWeatherMap API integration for weather correlation analysis
- **Advanced Analytics**:
  - 7-day moving average calculation
  - Anomaly detection using statistical methods
  - Consumption trends and patterns
- **Forecasting**: 3-day energy consumption prediction using linear regression and moving averages
- **Natural Language Insights**: Automated insight generation correlating energy usage with weather patterns

## Technology Stack

- **.NET 9**: Latest .NET framework
- **ASP.NET Core Web API**: RESTful API framework
- **Dependency Injection**: Built-in DI container
- **Swagger/OpenAPI**: Interactive API documentation
- **HttpClient**: For external API integration
- **Custom Middleware**: Error handling and logging

## Project Structure

```
Backend/
├── Controllers/          # API endpoints
│   ├── AuthController.cs
│   ├── EnergyController.cs
│   ├── WeatherController.cs
│   ├── AnalyticsController.cs
│   ├── ForecastController.cs
│   └── InsightsController.cs
├── Services/            # Business logic
│   ├── HydraAuthService.cs
│   ├── EnergyDataService.cs
│   ├── WeatherService.cs
│   ├── AnalyticsService.cs
│   ├── ForecastingService.cs
│   └── InsightsService.cs
├── Models/              # Data models and DTOs
│   └── DTOs/
├── Configuration/       # Configuration classes
├── Middleware/          # Custom middleware
├── Program.cs          # Application entry point
└── appsettings.json    # Configuration settings
```

## Prerequisites

- .NET 9 SDK
- OpenWeatherMap API key (optional, will use simulated data if not provided)
- HYDRA API credentials (pre-configured in appsettings.json)

## Installation & Setup

### 1. Install .NET 9 SDK

**Linux/macOS:**
```bash
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0
export PATH="$PATH:$HOME/.dotnet"
```

**Windows:**
Download and install from [.NET Download Page](https://dotnet.microsoft.com/download/dotnet/9.0)

### 2. Configure OpenWeatherMap API Key (Optional)

Edit `appsettings.json` and add your OpenWeatherMap API key:

```json
"WeatherSettings": {
  "ApiKey": "YOUR_OPENWEATHERMAP_API_KEY",
  ...
}
```

**Note**: If no API key is provided, the system will use simulated weather data for demonstration purposes.

### 3. Restore Dependencies

```bash
cd Backend
dotnet restore
```

### 4. Build the Project

```bash
dotnet build
```

### 5. Run the Application

```bash
dotnet run
```

The API will start and be available at:
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001
- **Swagger UI**: http://localhost:5000/swagger

## API Endpoints

### Authentication

#### POST `/api/auth/token`
Get OAuth2 access token from HYDRA identity server.

**Response:**
```json
{
  "success": true,
  "data": {
    "access_token": "eyJhb...",
    "expires_in": 2592000,
    "token_type": "Bearer",
    "scope": "api1"
  }
}
```

#### GET `/api/auth/validate`
Validate current access token.

### Energy Data

#### GET `/api/energy/data`
Get energy consumption data for a date range.

**Parameters:**
- `fromDate` (required): Start date (yyyy-MM-dd)
- `toDate` (required): End date (yyyy-MM-dd)

**Example:**
```
GET /api/energy/data?fromDate=2025-03-01&toDate=2025-03-31
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "sensorId": "470b1334-0000-0001-0000-0000",
      "year": 2025,
      "month": 3,
      "day": 1,
      "count": 144,
      "sum": 1296915800,
      "min": 152627.36,
      "max": 63993892.0,
      "kwhConsumption": 63841.26
    }
  ]
}
```

#### GET `/api/energy/total`
Get total energy consumption for a date range.

#### GET `/api/energy/average`
Get average daily energy consumption for a date range.

### Weather Data

#### GET `/api/weather/data`
Get weather data for a date range.

**Parameters:**
- `fromDate` (required): Start date (yyyy-MM-dd)
- `toDate` (required): End date (yyyy-MM-dd)

**Example:**
```
GET /api/weather/data?fromDate=2025-03-01&toDate=2025-03-31
```

#### GET `/api/weather/date/{date}`
Get weather data for a specific date.

### Analytics

#### GET `/api/analytics/summary`
Get comprehensive analytics including moving averages and anomaly detection.

**Parameters:**
- `fromDate` (required): Start date (yyyy-MM-dd)
- `toDate` (required): End date (yyyy-MM-dd)

**Response:**
```json
{
  "success": true,
  "data": {
    "totalEnergyUsed": 1842.56,
    "averageDailyUse": 61.42,
    "numberOfAnomalies": 3,
    "dailyResults": [
      {
        "date": "2025-03-01",
        "kwhConsumption": 63.84,
        "movingAverage7Day": 58.32,
        "isAnomaly": true,
        "deviationFromAverage": 5.52,
        "anomalyReason": "High consumption: 5.52 kWh above 7-day average"
      }
    ]
  }
}
```

#### GET `/api/analytics/anomalies`
Get only the detected anomalies.

### Forecasting

#### GET `/api/forecast`
Get energy consumption forecast for the next N days.

**Parameters:**
- `fromDate` (optional): Start date for forecast (default: tomorrow)
- `days` (optional): Number of days to forecast (default: 3, max: 30)

**Example:**
```
GET /api/forecast?days=3
```

**Response:**
```json
{
  "success": true,
  "data": {
    "forecasts": [
      {
        "date": "2025-03-15",
        "predictedKwh": 64.28,
        "confidenceLower": 52.14,
        "confidenceUpper": 76.42,
        "method": "Linear Trend (60%) + Moving Average (40%)"
      }
    ],
    "averageHistoricalConsumption": 61.42,
    "trendDirection": "Increasing",
    "trendStrength": 3.2
  }
}
```

#### GET `/api/forecast/predictions`
Get simple forecast predictions without full summary.

### Insights

#### GET `/api/insights`
Generate comprehensive insights combining energy, weather, and analytics data.

**Parameters:**
- `fromDate` (required): Start date (yyyy-MM-dd)
- `toDate` (required): End date (yyyy-MM-dd)

**Example:**
```
GET /api/insights?fromDate=2025-03-01&toDate=2025-03-31
```

**Response:**
```json
{
  "success": true,
  "data": {
    "insights": [
      {
        "type": "WeatherImpact",
        "message": "Energy consumption increased by 15.3% on hot days (avg 32.5°C) compared to overall average, likely due to increased cooling demand.",
        "severity": "info",
        "metadata": {
          "avgTempHotDays": 32.5,
          "increasePercent": 15.3
        }
      }
    ],
    "overallAssessment": "✅ Normal operation: Energy consumption is stable at 61.42 kWh per day on average. 3 anomaly(ies) detected over 30 days.",
    "generatedAt": "2025-03-14T10:30:00Z"
  }
}
```

#### GET `/api/insights/type/{type}`
Get insights filtered by type.

#### GET `/api/insights/severity/{severity}`
Get insights filtered by severity (info, warning, critical).

## Configuration

### HYDRA API Settings (appsettings.json)

```json
{
  "HydraSettings": {
    "AuthUrl": "https://identity.hydra.africa/connect/token",
    "ApiUrl": "https://hydra-api.azurewebsites.net/Sensor/exportAggregatedNumbers?binBy=day",
    "ClientId": "ro.client",
    "ClientSecret": "secret",
    "GrantType": "password",
    "Scope": "api1",
    "Username": "ll-wc-04@hydra.africa",
    "Password": "CpBzdnYM7Qb6b4q",
    "DeviceId": "38394d4c-cb8e-ef11-a81c-6045bd88aa3b",
    "SensorId": "470b1334-0000-0001-0000-0000"
  }
}
```

### Weather API Settings

```json
{
  "WeatherSettings": {
    "ApiKey": "YOUR_OPENWEATHERMAP_API_KEY",
    "ApiUrl": "https://api.openweathermap.org/data/2.5/forecast",
    "City": "Johannesburg",
    "CountryCode": "ZA",
    "Latitude": -26.2041,
    "Longitude": 28.0473
  }
}
```

### CORS Configuration

CORS is configured to allow requests from:
- http://localhost:3000
- http://localhost:5173
- http://localhost:8080

Update the `Cors:AllowedOrigins` section in `appsettings.json` to add more origins.

## Development

### Running in Development Mode

```bash
dotnet run --environment Development
```

### Building for Production

```bash
dotnet publish -c Release -o ./publish
```

### Running Tests

```bash
dotnet test
```

## Error Handling

The API uses a custom `ExceptionHandlingMiddleware` that catches all unhandled exceptions and returns consistent error responses:

```json
{
  "success": false,
  "message": "Error message",
  "errors": ["Detailed error information"],
  "timestamp": "2025-03-14T10:30:00Z"
}
```

## Logging

Logging is configured to output to:
- Console (Development)
- Debug output
- Application Insights (Production - when configured)

Log levels can be adjusted in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## Health Check

**GET** `/health`

Returns the health status of the API:

```json
{
  "status": "Healthy",
  "timestamp": "2025-03-14T10:30:00Z",
  "service": "HYDRA Energy Intelligence API",
  "version": "1.0.0"
}
```

## API Documentation

Interactive API documentation is available via Swagger UI at:
- http://localhost:5000/swagger (Development)

## Architecture & Design Patterns

### Services Layer
All business logic is encapsulated in service classes with dependency injection:
- **IHydraAuthService**: OAuth2 authentication with token caching
- **IEnergyDataService**: Energy data retrieval and calculations
- **IWeatherService**: Weather data integration
- **IAnalyticsService**: Analytics and anomaly detection
- **IForecastingService**: Energy consumption forecasting
- **IInsightsService**: Natural language insights generation

### Data Transfer Objects (DTOs)
All API responses use strongly-typed DTOs for consistency and type safety.

### Middleware Pipeline
1. Exception Handling Middleware
2. CORS Middleware
3. HTTPS Redirection
4. Authorization
5. Controllers

## Deployment

### Docker Deployment (Optional)

Create a `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["HydraEnergyAPI.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HydraEnergyAPI.dll"]
```

Build and run:
```bash
docker build -t hydra-energy-api .
docker run -p 5000:80 hydra-energy-api
```

### Azure App Service Deployment

```bash
az webapp up --name hydra-energy-api --resource-group myResourceGroup --plan myAppServicePlan
```

## Troubleshooting

### Issue: Cannot connect to HYDRA API
- Verify your internet connection
- Check if HYDRA API endpoints are accessible
- Validate credentials in appsettings.json

### Issue: Weather data not loading
- Verify OpenWeatherMap API key
- Check API rate limits
- The system will fall back to simulated data if API is unavailable

### Issue: CORS errors
- Ensure your frontend URL is listed in the CORS configuration
- Check that credentials are being sent correctly

## Support & Contact

For issues or questions, please contact:
- Email: priaash@awarenesscompany.co.za
- HYDRA Support: support@hydra.africa

## License

© 2025 The Awareness Company. All rights reserved.
