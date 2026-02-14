using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos.Notes;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.NoteUseCases.Csv;

public class ImportNotesCsvUseCase(IRepositoryFactory factory)
{
    public async Task ExecuteAsync(List<NoteCsvDto> lignesCsv)
    {
        var erreurs = new List<string>();
        int ligneNum = 1;
        
        foreach (var ligne in lignesCsv)
        {
            ligneNum++;
            
            // Si la case contient une note, on vérifie qu'elle est entre 0 et 20
            if (ligne.Note.HasValue && (ligne.Note.Value < 0 || ligne.Note.Value > 20))
            {
                erreurs.Add($"Ligne {ligneNum} : La note de {ligne.NumEtud} est invalide ({ligne.Note.Value}). Elle doit être entre 0 et 20.");
            }

            var etudiants = await factory.EtudiantRepository().FindByConditionAsync(e => e.NumEtud == ligne.NumEtud);
            if (etudiants == null || etudiants.Count == 0)
                erreurs.Add($"Ligne {ligneNum} : L'étudiant {ligne.NumEtud} est introuvable.");

            var ues = await factory.UeRepository().FindByConditionAsync(u => u.NumeroUe == ligne.NumeroUe);
            if (ues == null || ues.Count == 0)
                erreurs.Add($"Ligne {ligneNum} : L'UE {ligne.NumeroUe} est introuvable.");
        }

        if (erreurs.Count > 0) throw new Exception(string.Join(" | ", erreurs));

        foreach (var ligne in lignesCsv)
        {
            var etudiant = (await factory.EtudiantRepository().FindByConditionAsync(e => e.NumEtud == ligne.NumEtud)).First();
            var ue = (await factory.UeRepository().FindByConditionAsync(u => u.NumeroUe == ligne.NumeroUe)).First();

            var notesExistantes = await factory.NoteRepository().FindByConditionAsync(n => n.EtudiantId == etudiant.Id && n.UeId == ue.Id);
            var noteExistante = notesExistantes.FirstOrDefault();

            if (ligne.Note.HasValue)
            {
                // La case CSV contient une note on met à jour ou on crée
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
            else
            {
                // La case CSV est VIDE Si l'étudiant avait une note on la supprime 
                if (noteExistante != null) 
                {
                    await factory.NoteRepository().DeleteAsync(noteExistante);
                }
            }
        }

        await factory.SaveChangesAsync();
    }

    public bool IsAuthorized(string role)
    {
        return role.Equals(Roles.Scolarite);
    }
}