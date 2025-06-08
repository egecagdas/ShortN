using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ShortN.Data;
using ShortN.Models;
using ShortN.Exceptions;
using Microsoft.Extensions.Logging;

namespace ShortN.Services;

public class UrlShortenerService : IUrlShortenerService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UrlShortenerService> _logger;

    public UrlShortenerService(ApplicationDbContext context, ILogger<UrlShortenerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public string GenerateShortCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var shortCode = new string(Enumerable.Repeat(chars, 6).Select(c => c[random.Next(c.Length)]).ToArray());
        _logger.LogDebug("Generated new short code: {ShortCode}", shortCode);
        return shortCode;
    }

    public async Task<UrlEntry> GetUrlEntryByLongUrlAsync(string longUrl)
    {
        _logger.LogDebug("Looking up URL entry by long URL: {LongUrl}", longUrl);
        var entry = await _context.UrlEntries.FirstOrDefaultAsync(e => e.LongUrl == longUrl);
        if (entry == null)
        {
            _logger.LogDebug("No URL entry found for long URL: {LongUrl}", longUrl);
        }
        return entry;
    }

    public async Task<UrlEntry> GetUrlEntryByShortCodeAsync(string shortCode)
    {
        _logger.LogDebug("Looking up URL entry by short code: {ShortCode}", shortCode);
        var entry = await _context.UrlEntries.FirstOrDefaultAsync(e => e.ShortCode == shortCode);
        if (entry == null)
        {
            _logger.LogDebug("No URL entry found for short code: {ShortCode}", shortCode);
        }
        return entry;
    }

    public async Task<UrlEntry> CreateUrlEntryAsync(string longUrl, string? customCode, int? ttl)
    {
        _logger.LogInformation("Creating new URL entry for {LongUrl} with custom code: {CustomCode}, TTL: {Ttl}", 
            longUrl, customCode ?? "none", ttl ?? 0);

        Boolean _IsCustomCode = false;
        var shortCode = "";

        if (customCode is null)
        {
            shortCode = GenerateShortCode();
        }
        else 
        {
            if (await DoesShortCodeExistAsync(customCode))
            {
                _logger.LogWarning("Custom code already exists: {CustomCode}", customCode);
                throw new CustomCodeNotAvailableException($"The custom code '{customCode}' is already in use");
            }
            shortCode = customCode;
            _IsCustomCode = true;
        }

        while (await DoesShortCodeExistAsync(shortCode))
        {
            _logger.LogDebug("Generated short code already exists, generating new one");
            shortCode = GenerateShortCode();
        }

        var urlEntry = new UrlEntry 
        { 
            LongUrl = longUrl, 
            ShortCode = shortCode, 
            CreatedAt = DateTime.UtcNow, 
            IsCustomCode = _IsCustomCode 
        };

        if (ttl is not null && ttl != 0)
        {
            urlEntry.ExpiresAt = urlEntry.CreatedAt.AddMinutes((double)ttl);
            _logger.LogDebug("Set expiration time to: {ExpiresAt}", urlEntry.ExpiresAt);
        }
        
        _context.UrlEntries.Add(urlEntry);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Successfully created URL entry: {ShortCode} -> {LongUrl}", shortCode, longUrl);
        return urlEntry;
    }

    public async Task<Boolean> DeleteUrlEntry(UrlEntry entry)
    {
        _logger.LogInformation("Attempting to delete URL entry: {ShortCode}", entry.ShortCode);
        try
        {
            _context.UrlEntries.Remove(entry);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully deleted URL entry: {ShortCode}", entry.ShortCode);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete URL entry: {ShortCode}", entry.ShortCode);
            return false;
        }
    }

    public async Task<Boolean> DoesShortCodeExistAsync(string shortCode)
    {
        _logger.LogDebug("Checking if short code exists: {ShortCode}", shortCode);
        return await _context.UrlEntries.AnyAsync(e => e.ShortCode == shortCode);
    }

    public async Task<Boolean> DoesLongUrlExistAsync(string longUrl)
    {
        _logger.LogDebug("Checking if long URL exists: {LongUrl}", longUrl);
        return await _context.UrlEntries.AnyAsync(e => e.LongUrl == longUrl);
    }

    public async Task<List<UrlEntry>> GetUrlEntries()
    {
        _logger.LogDebug("Retrieving all URL entries");
        var entries = await _context.UrlEntries.ToListAsync();
        _logger.LogDebug("Retrieved {Count} URL entries", entries.Count);
        return entries;
    }
}