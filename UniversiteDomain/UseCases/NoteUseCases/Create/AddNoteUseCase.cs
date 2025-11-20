using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.EtudiantExceptions;
using UniversiteDomain.Exceptions.NoteExceptions;
using UniversiteDomain.Exceptions.ParcoursExceptions;
using UniversiteDomain.Exceptions.UeExceptions;

namespace UniversiteDomain.UseCases.NoteUseCases.Create;

public class AddNoteUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<Note> ExecuteAsync(long idEtudiant, long idUe, float valeur)
    {
        await CheckBusinessRules(idEtudiant, idUe, valeur);
        
        var note = new Note 
        { 
            EtudiantId = idEtudiant, 
            UeId = idUe, 
            Valeur = valeur 
        };
        
        Note noteCree = await repositoryFactory.NoteRepository().CreateAsync(note);
        await repositoryFactory.SaveChangesAsync();
        
        return noteCree;
    }

    private async Task CheckBusinessRules(long idEtudiant, long idUe, float valeur)
    {
        ArgumentNullException.ThrowIfNull(repositoryFactory);

        // note entre 0 - 20 
        if (valeur < 0 || valeur > 20)
            throw new InvalidNoteValueException($"La note {valeur} doit être comprise entre 0 et 20.");

        // si l'étudiant existe 
        var etudiants = await repositoryFactory.EtudiantRepository().FindByConditionAsync(e => e.Id == idEtudiant);
        if (etudiants == null || etudiants.Count == 0) 
            throw new EtudiantNotFoundException(idEtudiant.ToString());
        
        var etudiant = etudiants.First();
        
        // l'existence de l'UE
        var ues = await repositoryFactory.UeRepository().FindByConditionAsync(u => u.Id == idUe);
        if (ues == null || ues.Count == 0) 
            throw new UeNotFoundException(idUe.ToString());

        // si l'étudiant a bien un parcours
        if (etudiant.ParcoursSuivi == null)
        {
            throw new EtudiantSansParcoursException($"L'étudiant {idEtudiant} n'est inscrit dans aucun parcours.");
        }
        
        // L'UE doit appartenir au parcours de l'étudiant
        var parcoursList = await repositoryFactory.ParcoursRepository()
            .FindByConditionAsync(p => p.Id == etudiant.ParcoursSuivi.Id);
        
        if (parcoursList == null || parcoursList.Count == 0)
             throw new ParcoursNotFoundException(etudiant.ParcoursSuivi.Id.ToString());

        var leParcours = parcoursList.First();

        // si l'UE demandée est dans la liste des UEs du parcours
        bool estDansLeParcours = leParcours.UesEnseignees != null 
                                 && leParcours.UesEnseignees.Any(ue => ue.Id == idUe);

        if (!estDansLeParcours)
        {
            throw new UeNotInParcoursException($"L'UE {idUe} ne fait pas partie du parcours {leParcours.NomParcours} de l'étudiant.");
        }

        // note unique 
        var noteExistante = await repositoryFactory.NoteRepository()
            .FindByConditionAsync(n => n.EtudiantId == idEtudiant && n.UeId == idUe);

        if (noteExistante is { Count: > 0 })
        {
            throw new DuplicateNoteException($"L'étudiant {idEtudiant} a déjà une note pour l'UE {idUe}.");
        }
    }
}