namespace ShortN.Models;

public class UrlRequest
{
    public string LongUrl { get; set; } = string.Empty;
    public string? CustomCode { get; set; }
    public int? Ttl { get; set; }
}