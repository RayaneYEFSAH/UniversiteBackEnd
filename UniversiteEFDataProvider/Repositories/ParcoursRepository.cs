using Microsoft.EntityFrameworkCore;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;

namespace UniversiteEFDataProvider.Repositories;

public class ParcoursRepository(UniversiteDbContext context) : Repository<Parcours>(context), IParcoursRepository
{
    // --- GESTION DES ETUDIANTS (OneToMany) ---

    public async Task<Parcours> AddEtudiantAsync(long idParcours, long idEtudiant)
    {
        // On charge le parcours avec sa liste d'inscrits
        var parcours = await Context.Parcours
            .Include(p => p.Inscrits)
            .FirstOrDefaultAsync(p => p.Id == idParcours);

        var etudiant = await Context.Etudiants.FindAsync(idEtudiant);

        if (parcours != null && etudiant != null)
        {
            parcours.Inscrits?.Add(etudiant);
            await Context.SaveChangesAsync();
        }
        return parcours!;
    }

    public async Task<Parcours> AddEtudiantAsync(Parcours parcours, Etudiant etudiant)
    {
        return await AddEtudiantAsync(parcours.Id, etudiant.Id);
    }

    public async Task<Parcours> AddEtudiantAsync(long idParcours, long[] idEtudiants)
    {
        var parcours = await Context.Parcours
            .Include(p => p.Inscrits)
            .FirstOrDefaultAsync(p => p.Id == idParcours);
            
        // On récupère tous les étudiants d'un coup
        var etudiants = await Context.Etudiants
            .Where(e => idEtudiants.Contains(e.Id))
            .ToListAsync();

        if (parcours != null && parcours.Inscrits != null)
        {
            foreach (var etudiant in etudiants)
            {
                parcours.Inscrits.Add(etudiant);
            }
            await Context.SaveChangesAsync();
        }
        return parcours!;
    }

    public async Task<Parcours> AddEtudiantAsync(Parcours? parcours, List<Etudiant> etudiants)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        return await AddEtudiantAsync(parcours.Id, etudiants.Select(e => e.Id).ToArray());
    }


    // --- GESTION DES UES (ManyToMany) ---

    public async Task<Parcours> AddUeAsync(long idParcours, long idUe)
    {
        // Important : Include pour charger la table de jointure
        var parcours = await Context.Parcours
            .Include(p => p.UesEnseignees)
            .FirstOrDefaultAsync(p => p.Id == idParcours);

        var ue = await Context.Ues.FindAsync(idUe);

        if (parcours != null && ue != null)
        {
            parcours.UesEnseignees?.Add(ue);
            await Context.SaveChangesAsync();
        }
        return parcours!;
    }

    public async Task<Parcours> AddUeAsync(Parcours parcours, Ue ue)
    {
        return await AddUeAsync(parcours.Id, ue.Id);
    }

    public async Task<Parcours> AddUeAsync(long idParcours, long[] idUes)
    {
        var parcours = await Context.Parcours
            .Include(p => p.UesEnseignees)
            .FirstOrDefaultAsync(p => p.Id == idParcours);

        var ues = await Context.Ues
            .Where(u => idUes.Contains(u.Id))
            .ToListAsync();

        if (parcours != null && parcours.UesEnseignees != null)
        {
            foreach (var ue in ues)
            {
                parcours.UesEnseignees.Add(ue);
            }
            await Context.SaveChangesAsync();
        }
        return parcours!;
    }

    public async Task<Parcours> AddUeAsync(Parcours? parcours, List<Ue> ues)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        return await AddUeAsync(parcours.Id, ues.Select(u => u.Id).ToArray());
    }
}