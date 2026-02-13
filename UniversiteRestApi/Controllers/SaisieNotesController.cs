using System.Globalization;
using System.Security.Claims;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos.Notes;
using UniversiteDomain.UseCases.NoteUseCases.Csv;
using UniversiteDomain.UseCases.SecurityUseCases.Get;

namespace UniversiteRestApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SaisieNotesController(IRepositoryFactory repositoryFactory) : ControllerBase
{
    [HttpGet("export/{ueId}")]
    public async Task<IActionResult> ExportCsv(long ueId)
    {
        try { CheckSecu(out string role, out _); 
              var uc = new ExportNotesCsvUseCase(repositoryFactory);
              if (!uc.IsAuthorized(role)) return Unauthorized("Seule la scolarité peut générer ce fichier.");

              var donnees = await uc.ExecuteAsync(ueId);

              // Utilisation de CsvHelper pour générer le fichier en memoire
              var memoryStream = new MemoryStream();
              var streamWriter = new StreamWriter(memoryStream);
              var csv = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" });
              
              await csv.WriteRecordsAsync(donnees);
              await streamWriter.FlushAsync();
              memoryStream.Position = 0;

              return File(memoryStream, "text/csv", $"Notes_UE_{ueId}.csv");
        }
        catch (Exception e) { return BadRequest(e.Message); }
    }
    
    [HttpPost("import")]
    public async Task<IActionResult> ImportCsv(IFormFile fichier)
    {
        if (fichier == null || fichier.Length == 0) return BadRequest("Fichier vide ou manquant.");

        try { CheckSecu(out string role, out _);
              var uc = new ImportNotesCsvUseCase(repositoryFactory);
              if (!uc.IsAuthorized(role)) return Unauthorized("Seule la scolarité peut importer des notes.");

              // Lecture du fichier uploadé avec CsvHelper
              using var streamReader = new StreamReader(fichier.OpenReadStream());
              using var csv = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" });
              
              var lignesCsv = csv.GetRecords<NoteCsvDto>().ToList();

              // On envoie les données lues au UseCase métier
              await uc.ExecuteAsync(lignesCsv);

              return Ok("Les notes ont été importées et sauvegardées avec succès.");
        }
        catch (Exception e)
        {
            return BadRequest($"Erreur d'importation : {e.Message}. Aucune note n'a été sauvegardée.");
        }
    }

    private void CheckSecu(out string role, out string email)
    {
        var claims = HttpContext.User;
        if (claims.Identity?.IsAuthenticated != true) throw new UnauthorizedAccessException();
        email = claims.FindFirst(ClaimTypes.Email)?.Value ?? throw new UnauthorizedAccessException();
        var ident = claims.Identities.FirstOrDefault() ?? throw new UnauthorizedAccessException();
        role = ident.FindFirst(ClaimTypes.Role)?.Value ?? throw new UnauthorizedAccessException();
        
        if (!new IsInRoleUseCase(repositoryFactory).ExecuteAsync(email, role).Result) 
            throw new UnauthorizedAccessException();
    }
}