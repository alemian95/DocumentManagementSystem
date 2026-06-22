using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentManagementSystem.Models;

public class FileMetadata
{
    [Key]
    [ForeignKey("Node")]
    public Guid NodeId { get; set; }

    public virtual FileSystemNode Node { get; set; } = null!;

    public long SizeBytes { get; set; }

    [Required]
    [MaxLength(64)]
    public string MimeType { get; set; } = string.Empty;

    [Required]
    [MaxLength(8)]
    public string Extension { get; set; } = string.Empty;

    [Required]
    [MaxLength(4096)]
    public string StoragePath { get; set; } = string.Empty;
    
    public string? RawContent { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}