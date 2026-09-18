using System.Text.Json.Serialization;
using Voyago.BusService.Data;
using Voyago.BusService.Services;
using Voyago.BusService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<BusDbContext>(connectionName: "busdb");

builder.Services.AddProblemDetails();

builder.Services.AddScoped<IBusService, BusService>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<IStopService, StopService>();

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