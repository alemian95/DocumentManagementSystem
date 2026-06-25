using DocumentManagementSystem.Data;
using DocumentManagementSystem.Models;
using DocumentManagementSystem.Support;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DocumentManagementSystem.Controllers;

[Authorize] // Solo utenti loggati
[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly StorageSettings _storageSettings;

    public FilesController(ApplicationDbContext db, StorageSettings storageSettings)
    {
        _db = db;
        _storageSettings = storageSettings;
    }

    [HttpPost("upload")]
    [DisableRequestSizeLimit] // Utile in futuro per file grandi
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] Guid? parentId)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Nessun file selezionato.");

        // 1. Controllo unicità del nome nel database
        bool exists = await _db.FileSystemNodes
            .AnyAsync(n => n.ParentId == parentId && n.Name.ToLower() == file.FileName.ToLower());

        if (exists)
            return Conflict("Un file o una cartella con questo nome esiste già in questa posizione.");

        // 2. Generiamo i percorsi fisici sul disco
        var fileId = Guid.NewGuid();
        var extension = Path.GetExtension(file.FileName).ToLower();
        
        // Creiamo la cartella di storage se non esiste
        System.IO.Directory.CreateDirectory(_storageSettings.BasePath);
        
        // Salviamo il file usando il suo GUID come nome fisico (evita problemi di caratteri speciali su OS diversi)
        var physicalName = $"{fileId}{extension}";
        var fullPath = Path.Combine(_storageSettings.BasePath, physicalName);

        // 3. Salvataggio fisico del file sul disco
        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 4. Salvataggio nel Database (Logica a due tabelle)
        var nodeFile = new FileSystemNode
        {
            Id = fileId,
            Name = file.FileName,
            IsDirectory = false,
            ParentId = parentId
        };

        var metadata = new FileMetadata
        {
            NodeId = fileId,
            SizeBytes = file.Length,
            MimeType = file.ContentType,
            Extension = extension,
            StoragePath = fullPath, // Percorso assoluto sul server
            RawContent = null // Qui in futuro metterai il testo estratto per l'AI
        };

        _db.FileSystemNodes.Add(nodeFile);
        _db.FileMetadata.Add(metadata);
        await _db.SaveChangesAsync();

        return Ok(new { id = fileId, name = file.FileName });
    }
}