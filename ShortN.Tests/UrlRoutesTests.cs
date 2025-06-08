using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShortN.Data;
using ShortN.Models;
using ShortN.Services;
using Xunit;

namespace ShortN.Tests;

public class UrlRoutesTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ApplicationDbContext _context;

    public UrlRoutesTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove all existing DbContext registrations
                var descriptors = services.Where(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                         d.ServiceType == typeof(ApplicationDbContext)).ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                // Ensure all required services are registered
                services.AddScoped<IUrlShortenerService, UrlShortenerService>();
            });
        });

        _client = _factory.CreateClient();

        // Get the DbContext from the service provider
        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Ensure the database is created
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }

    [Fact]
    public async Task CreateShortUrl_WithValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var request = new UrlRequest
        {
            LongUrl = "https://example.com",
            CustomCode = "test123",
            Ttl = 60
        };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/urls", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UrlEntry>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(result);
        Assert.Equal(request.LongUrl, result.LongUrl);
        Assert.Equal(request.CustomCode, result.ShortCode);
    }

    [Fact]
    public async Task CreateShortUrl_WithInvalidUrl_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new UrlRequest
        {
            LongUrl = "invalid-url",
            CustomCode = "test123"
        };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/urls", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUrl_WithExistingCode_ShouldRedirect()
    {
        // Arrange
        var request = new UrlRequest
        {
            LongUrl = "https://example.com",
            CustomCode = "test123"
        };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");
        await _client.PostAsync("/urls", content);

        // Act
        var response = await _client.GetAsync("/test123");

        // Assert
        Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
        Assert.Equal("https://example.com", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task GetUrl_WithNonExistingCode_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/nonexistent");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUrl_WithExistingCode_ShouldReturnNoContent()
    {
        // Arrange
        var request = new UrlRequest
        {
            LongUrl = "https://example.com",
            CustomCode = "test123"
        };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");
        await _client.PostAsync("/urls", content);

        // Act
        var response = await _client.DeleteAsync("/urls/test123");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the URL is actually deleted
        var getResponse = await _client.GetAsync("/test123");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteUrl_WithNonExistingCode_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/urls/nonexistent");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
} 