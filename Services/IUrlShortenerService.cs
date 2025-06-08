using ShortN.Models;

namespace ShortN.Services;

public interface IUrlShortenerService
{
    String GenerateShortCode();

    Task<UrlEntry> GetUrlEntryByLongUrlAsync(string longUrl);

    Task<UrlEntry> GetUrlEntryByShortCodeAsync(string shortCode);

    Task<UrlEntry> CreateUrlEntryAsync(string longUrl, string? customCode, int? ttl);

    Task<Boolean> DeleteUrlEntry(UrlEntry entry);

    Task<Boolean> DoesShortCodeExistAsync(string shortCode);

    Task<Boolean> DoesLongUrlExistAsync(string longUrl);

    Task<List<UrlEntry>> GetUrlEntries();

    Task<Boolean> UpdateUrlEntry(UrlEntry entry);
}