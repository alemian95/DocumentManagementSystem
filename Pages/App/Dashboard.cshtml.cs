using DocumentManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagementSystem.Pages.App;

[Authorize] // <-- Questo blocca l'accesso ai non autenticati!
public class DashboardModel : PageModel
{

    public ApplicationDbContext _db;

    public required List<Models.FileSystemNode> FileSystemNodes { get; set; } = new();

    [BindProperty]
    public string NewFolderName { get; set; } = string.Empty;

    public DashboardModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task OnGet()
    {
        FileSystemNodes = await _db.FileSystemNodes
            .Where(n => n.ParentId == null)
            .Include(n => n.FileMetadata)
            .OrderByDescending(n => n.IsDirectory)
            .ThenBy(n => n.Name)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostCreateFolderAsync(Guid? currentFolderId)
    {
        if (string.IsNullOrWhiteSpace(NewFolderName))
        {
            TempData["Error"] = "Il nome della cartella non può essere vuoto";
            return RedirectToPage(new { folderId = currentFolderId });
        }

        bool exists = await _db.FileSystemNodes
            .AnyAsync(n => n.ParentId == currentFolderId && n.Name.ToLower() == NewFolderName.ToLower());

        if (exists)
        {
            TempData["Error"] = "Una cartella o un file con questo nome esiste già.";
            return RedirectToPage(new { folderId = currentFolderId });
        }

        var folderNode = new Models.FileSystemNode
        {
            Id = Guid.NewGuid(),
            Name = NewFolderName.Trim(),
            IsDirectory = true,
            ParentId = currentFolderId
        };

        _db.FileSystemNodes.Add(folderNode);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Cartella creata con successo.";
        return RedirectToPage(new { folderId = currentFolderId });
    }
}