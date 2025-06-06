using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace ShortN.Models;

public class UrlEntry
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Url]
    public required string LongUrl { get; set; }

    [Required]
    [Url]
    public required string ShortCode { get; set; }

    public Boolean IsCustomCode { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }
}