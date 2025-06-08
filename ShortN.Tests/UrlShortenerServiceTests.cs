using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ShortN.Data;
using ShortN.Models;
using ShortN.Services;
using ShortN.Exceptions;
using Xunit;

namespace ShortN.Tests;

public class UrlShortenerServiceTests
{
    private readonly Mock<ILogger<UrlShortenerService>> _loggerMock;
    private readonly ApplicationDbContext _context;
    private readonly UrlShortenerService _service;

    public UrlShortenerServiceTests()
    {
        _loggerMock = new Mock<ILogger<UrlShortenerService>>();
        
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        
        _service = new UrlShortenerService(_context, _loggerMock.Object);
    }

    [Fact]
    public void GenerateShortCode_ShouldReturnValidCode()
    {
        // Act
        var shortCode = _service.GenerateShortCode();

        // Assert
        Assert.NotNull(shortCode);
        Assert.Equal(6, shortCode.Length);
        Assert.Matches("^[A-Za-z0-9]{6}$", shortCode);
    }

    [Fact]
    public async Task CreateUrlEntryAsync_WithValidUrl_ShouldCreateEntry()
    {
        // Arrange
        var longUrl = "https://example.com";
        var customCode = "test123";
        var ttl = 60;

        // Act
        var result = await _service.CreateUrlEntryAsync(longUrl, customCode, ttl);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(longUrl, result.LongUrl);
        Assert.Equal(customCode, result.ShortCode);
        Assert.True(result.IsCustomCode);
        Assert.NotNull(result.ExpiresAt);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task CreateUrlEntryAsync_WithExistingCustomCode_ShouldThrowException()
    {
        // Arrange
        var longUrl = "https://example.com";
        var customCode = "test123";
        await _service.CreateUrlEntryAsync(longUrl, customCode, null);

        // Act & Assert
        await Assert.ThrowsAsync<CustomCodeNotAvailableException>(() => 
            _service.CreateUrlEntryAsync("https://another.com", customCode, null));
    }

    [Fact]
    public async Task GetUrlEntryByShortCodeAsync_WithExistingCode_ShouldReturnEntry()
    {
        // Arrange
        var longUrl = "https://example.com";
        var customCode = "test123";
        var entry = await _service.CreateUrlEntryAsync(longUrl, customCode, null);

        // Act
        var result = await _service.GetUrlEntryByShortCodeAsync(customCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entry.Id, result.Id);
        Assert.Equal(longUrl, result.LongUrl);
    }

    [Fact]
    public async Task GetUrlEntryByShortCodeAsync_WithNonExistingCode_ShouldReturnNull()
    {
        // Act
        var result = await _service.GetUrlEntryByShortCodeAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteUrlEntry_WithExistingEntry_ShouldReturnTrue()
    {
        // Arrange
        var entry = await _service.CreateUrlEntryAsync("https://example.com", "test123", null);

        // Act
        var result = await _service.DeleteUrlEntry(entry);

        // Assert
        Assert.True(result);
        Assert.Null(await _service.GetUrlEntryByShortCodeAsync("test123"));
    }

    [Fact]
    public async Task UpdateUrlEntry_WithValidChanges_ShouldReturnTrue()
    {
        // Arrange
        var entry = await _service.CreateUrlEntryAsync("https://example.com", "test123", null);
        entry.LongUrl = "https://updated.com";

        // Act
        var result = await _service.UpdateUrlEntry(entry);

        // Assert
        Assert.True(result);
        var updatedEntry = await _service.GetUrlEntryByShortCodeAsync("test123");
        Assert.Equal("https://updated.com", updatedEntry.LongUrl);
    }

    [Fact]
    public async Task GetUrlEntries_ShouldReturnAllEntries()
    {
        // Arrange
        await _service.CreateUrlEntryAsync("https://example1.com", "test1", null);
        await _service.CreateUrlEntryAsync("https://example2.com", "test2", null);

        // Act
        var entries = await _service.GetUrlEntries();

        // Assert
        Assert.Equal(2, entries.Count);
        Assert.Contains(entries, e => e.ShortCode == "test1");
        Assert.Contains(entries, e => e.ShortCode == "test2");
    }

    [Fact]
    public async Task DoesShortCodeExistAsync_WithExistingCode_ShouldReturnTrue()
    {
        // Arrange
        await _service.CreateUrlEntryAsync("https://example.com", "test123", null);

        // Act
        var result = await _service.DoesShortCodeExistAsync("test123");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DoesLongUrlExistAsync_WithExistingUrl_ShouldReturnTrue()
    {
        // Arrange
        var longUrl = "https://example.com";
        await _service.CreateUrlEntryAsync(longUrl, "test123", null);

        // Act
        var result = await _service.DoesLongUrlExistAsync(longUrl);

        // Assert
        Assert.True(result);
    }
} 