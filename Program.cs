using Microsoft.EntityFrameworkCore;
using ShortN.Routes;
using ShortN.Data;
using Microsoft.OpenApi.Models;
using ShortN.Services;
using FluentValidation;
using ShortN.Validators;
using ShortN.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ShortN API", Version = "v1" });
});

//Add database context (PostgreSQL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register URL Shortener Service
builder.Services.AddScoped<IUrlShortenerService, UrlShortenerService>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<UrlRequestValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShortN API V1");
    });
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map all routes
app.MapURLRoutes();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
