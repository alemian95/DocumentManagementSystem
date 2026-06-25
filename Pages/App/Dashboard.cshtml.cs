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

    public List<Models.FileSystemNode> Breadcrumbs { get; set; } = new();

    public Models.FileSystemNode? CurrentFolder { get; set; }


    [BindProperty]
    public string NewFolderName { get; set; } = string.Empty;

    public DashboardModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task OnGet(Guid? folderId)
    {

        if (folderId.HasValue)
        {
            CurrentFolder = await _db.FileSystemNodes.FirstOrDefaultAsync(n => n.Id == folderId.Value && n.IsDirectory);
            if (CurrentFolder != null)
            {
                FileSystemNodes = await _db.FileSystemNodes
                    .Where(n => n.ParentId == CurrentFolder.Id)
                    .Include(n => n.FileMetadata)
                    .OrderByDescending(n => n.IsDirectory)
                    .ThenBy(n => n.Name)
                    .ToListAsync();
            }
        }

        if (CurrentFolder == null)
        {
            FileSystemNodes = await _db.FileSystemNodes
                .Where(n => n.ParentId == null)
                .Include(n => n.FileMetadata)
                .OrderByDescending(n => n.IsDirectory)
                .ThenBy(n => n.Name)
                .ToListAsync();
        }

        if (CurrentFolder != null)
        {
            var path = new List<Models.FileSystemNode>();
            var tmp = CurrentFolder;

            while (tmp != null)
            {
                path.Add(tmp);
                if (tmp.ParentId.HasValue)
                {
                    tmp = await _db.FileSystemNodes.FirstOrDefaultAsync(n => n.Id == tmp.ParentId.Value);
                }
                else
                {
                    tmp = null;
                }
            }
            path.Reverse();
            Breadcrumbs = path;
        }
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