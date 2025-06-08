using Microsoft.EntityFrameworkCore;
using ShortN.Routes;
using ShortN.Data;
using Microsoft.OpenApi.Models;
using ShortN.Services;
using FluentValidation;
using ShortN.Validators;
using ShortN.Models;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

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

// Add logging middleware
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Request started: {Method} {Path}", context.Request.Method, context.Request.Path);

    try
    {
        await next();
    }
    finally
    {
        logger.LogInformation("Request completed: {Method} {Path} with status code {StatusCode}",
            context.Request.Method, context.Request.Path, context.Response.StatusCode);
    }
});

app.UseHttpsRedirection();

// Map all routes
app.MapURLRoutes();

app.Run();

public partial class Program
{
    // ... existing code ...
}