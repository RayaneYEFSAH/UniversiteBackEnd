using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos.Notes;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.NoteUseCases.Csv;

public class ImportNotesCsvUseCase(IRepositoryFactory factory)
{
    public async Task ExecuteAsync(List<NoteCsvDto> lignesCsv)
    {
        // On traite les lignes une par une.
        foreach (var ligne in lignesCsv)
        {
            if (!ligne.Note.HasValue) continue; // On ignore les cases vides

            if (ligne.Note.Value < 0 || ligne.Note.Value > 20)
                throw new Exception($"La note de l'étudiant {ligne.NumEtud} est invalide ({ligne.Note.Value}). Elle doit être entre 0 et 20.");

            var etudiants = await factory.EtudiantRepository().FindByConditionAsync(e => e.NumEtud == ligne.NumEtud);
            var etudiant = etudiants.FirstOrDefault() ?? throw new Exception($"L'étudiant {ligne.NumEtud} est introuvable.");

            var ues = await factory.UeRepository().FindByConditionAsync(u => u.NumeroUe == ligne.NumeroUe);
            var ue = ues.FirstOrDefault() ?? throw new Exception($"L'UE {ligne.NumeroUe} est introuvable.");

            var notesExistantes = await factory.NoteRepository().FindByConditionAsync(n => n.EtudiantId == etudiant.Id && n.UeId == ue.Id);
            var noteExistante = notesExistantes.FirstOrDefault();

            if (noteExistante != null)
            {
                noteExistante.Valeur = ligne.Note.Value;
                await factory.NoteRepository().UpdateAsync(noteExistante);
            }
            else
            {
                await factory.NoteRepository().CreateAsync(new Note { EtudiantId = etudiant.Id, UeId = ue.Id, Valeur = ligne.Note.Value });
            }
        }
        
        await factory.SaveChangesAsync();
    }

    public bool IsAuthorized(string role)
    {
        return role.Equals(Roles.Scolarite);
    }
}