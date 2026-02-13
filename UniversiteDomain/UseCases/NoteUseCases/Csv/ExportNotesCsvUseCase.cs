using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos.Notes;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.NoteUseCases.Csv;

public class ExportNotesCsvUseCase(IRepositoryFactory factory)
{
    public async Task<List<NoteCsvDto>> ExecuteAsync(long ueId)
    {
        var ue = await factory.UeRepository().FindAsync(ueId);
        if (ue == null) throw new Exception("L'UE specifiee n'existe pas.");

        var etudiants = await factory.EtudiantRepository().FindEtudiantsPourUeAsync(ueId);
        var listeExport = new List<NoteCsvDto>();

        foreach (var e in etudiants)
        {
            var noteExistante = e.NotesObtenues.FirstOrDefault();
            listeExport.Add(new NoteCsvDto
            {
                NumEtud = e.NumEtud,
                Nom = e.Nom,
                Prenom = e.Prenom,
                NumeroUe = ue.NumeroUe,
                Intitule = ue.Intitule,
                Note = noteExistante?.Valeur
            });
        }
        return listeExport;
    }

    public bool IsAuthorized(string role)
    {
        return role.Equals(Roles.Scolarite);
    }
}