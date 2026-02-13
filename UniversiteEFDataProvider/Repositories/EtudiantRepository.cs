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
}