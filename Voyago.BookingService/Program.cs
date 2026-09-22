using System.Text.Json.Serialization;
using Voyago.BookingService.Clients.BusService;
using Voyago.BookingService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();


builder.Services.AddProblemDetails();

builder.AddNpgsqlDbContext<BookingDbContext>(connectionName: "bookingdb");

builder.Services.AddHttpClient("bus-service", client =>
{
    client.BaseAddress = new Uri("https://bus-service");
});

builder.Services.AddScoped<IBusServiceClient, BusServiceClient>();

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

app.MapControllers();

app.Run();