using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using Voyago.BookingService.Clients.BusService;
using Voyago.BookingService.Data;
using Voyago.BookingService.Services;
using Voyago.BookingService.Services.Interfaces;
using Voyago.BusService.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.AddNpgsqlDbContext<BookingDbContext>(connectionName: "bookingdb");

builder.Services
    .AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection(JwtSettings.SectionName))
    .Validate(settings =>
        !string.IsNullOrWhiteSpace(settings.Issuer),
        "JWT Issuer is required.")
    .Validate(settings =>
        !string.IsNullOrWhiteSpace(settings.Audience),
        "JWT Audience is required.")
    .Validate(settings =>
        !string.IsNullOrWhiteSpace(settings.SecretKey),
        "JWT SecretKey is required.")
    .Validate(settings =>
        settings.AccessTokenExpirationMinutes > 0,
        "Access token expiration must be greater than zero.")
    .ValidateOnStart();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration
            .GetSection(JwtSettings.SectionName)
            .Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT configuration is missing.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpClient("bus-service", client =>
{
    client.BaseAddress = new Uri("https://bus-service");
});

builder.Services.AddScoped<IBusServiceClient, BusServiceClient>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddScoped<IBookingService, BookingService>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();