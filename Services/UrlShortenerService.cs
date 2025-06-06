using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ShortN.Data;
using ShortN.Models;
using ShortN.Exceptions;

namespace ShortN.Services;

public class UrlShortenerService : IUrlShortenerService
{
    private readonly ApplicationDbContext _context;

    public UrlShortenerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public string GenerateShortCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6).Select(c => c[random.Next(c.Length)]).ToArray());
    }

    public async Task<UrlEntry> GetUrlEntryByLongUrlAsync(string longUrl)
    {
        return await _context.UrlEntries.FirstOrDefaultAsync(e => e.LongUrl == longUrl);
    }

    public async Task<UrlEntry> GetUrlEntryByShortCodeAsync(string shortCode)
    {
        return await _context.UrlEntries.FirstOrDefaultAsync(e => e.ShortCode == shortCode);
    }

    public async Task<UrlEntry> CreateUrlEntryAsync(string longUrl, string? customCode, int? ttl)
    {
        Boolean _IsCustomCode = false;

        var shortCode = "";
        if (customCode is null){
            shortCode = GenerateShortCode();
        }
        else {
            if (await DoesShortCodeExistAsync(customCode)){
                throw new CustomCodeNotAvailableException($"The custom code '{customCode}' is already in use");
            }
            shortCode = customCode;
            _IsCustomCode = true;
        }

        while (await DoesShortCodeExistAsync(shortCode)){
            shortCode = GenerateShortCode();
        }

        var urlEntry = new UrlEntry { LongUrl = longUrl, ShortCode = shortCode, CreatedAt = DateTime.UtcNow, IsCustomCode = _IsCustomCode };
        if (ttl is not null && ttl != 0){
            urlEntry.ExpiresAt = urlEntry.CreatedAt.AddMinutes((double) ttl);
        }
        
        _context.UrlEntries.Add(urlEntry);
        await _context.SaveChangesAsync();
        return urlEntry;
    }

    public async Task<Boolean> DeleteUrlEntry(UrlEntry entry)
    {
        try
        {
            _context.UrlEntries.Remove(entry);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Boolean> DoesShortCodeExistAsync(string shortCode)
    {
        return await _context.UrlEntries.AnyAsync(e => e.ShortCode == shortCode);
    }

    public async Task<Boolean> DoesLongUrlExistAsync(string longUrl)
    {
        return await _context.UrlEntries.AnyAsync(e => e.LongUrl == longUrl);
    }

    public async Task<List<UrlEntry>> GetUrlEntries()
    {
        return await _context.UrlEntries.ToListAsync();
    }
}