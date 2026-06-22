using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentManagementSystem.Models;

public class FileSystemNode
{
    
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    public bool IsDirectory { get; set; }

    public Guid? ParentId { get; set; }

    [ForeignKey("ParentId")]
    public virtual FileSystemNode? Parent { get; set; }

    public virtual ICollection<FileSystemNode> Children { get; set; } = new List<FileSystemNode>();

    public virtual FileMetadata? FileMetadata { get; set; }
}