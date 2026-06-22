using DocumentManagementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagementSystem.Data;

public class ApplicationDbContext: IdentityDbContext
{

    public DbSet<FileSystemNode> FileSystemNodes { get; set; } = null!;
    public DbSet<FileMetadata> FileMetadata { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {}

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // vincolo di unicità
        builder.Entity<FileSystemNode>().HasIndex(n => new { n.ParentId, n.Name }).IsUnique();

        // relazione 1 a 1
        builder.Entity<FileSystemNode>().HasOne(n => n.FileMetadata).WithOne(m => m.Node).HasForeignKey<FileMetadata>(m => m.NodeId).OnDelete(DeleteBehavior.Cascade);
    }
}