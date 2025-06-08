namespace ShortN.Models;

public class UrlUpdateRequest
{
    public string LongUrl { get; set; } = string.Empty;
    public int? Ttl { get; set; }
} 