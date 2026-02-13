using Microsoft.EntityFrameworkCore;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;

namespace UniversiteEFDataProvider.Repositories;

public class EtudiantRepository(UniversiteDbContext context) : Repository<Etudiant>(context), IEtudiantRepository
{
    public async Task<Etudiant?> FindEtudiantCompletAsync(long idEtudiant)
    {
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        return await Context.Etudiants
            .Include(e => e.ParcoursSuivi) // pour avoir le parcour en non null
            .Include(e => e.NotesObtenues)
            .ThenInclude(n => n.Ue)
            .FirstOrDefaultAsync(e => e.Id == idEtudiant);
    }
    public async Task<List<Etudiant>> FindEtudiantsPourUeAsync(long ueId)
    {
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        return await Context.Etudiants
            // On inclut uniquement la note correspondant à cette UE (s'il y en a une)
            .Include(e => e.NotesObtenues.Where(n => n.UeId == ueId)) 
            .Where(e => e.ParcoursSuivi != null && e.ParcoursSuivi.UesEnseignees.Any(ue => ue.Id == ueId))
            .ToListAsync();
    }
}