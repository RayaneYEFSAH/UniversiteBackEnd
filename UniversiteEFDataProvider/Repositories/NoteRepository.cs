using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;

namespace UniversiteEFDataProvider.Repositories;

public class NoteRepository(UniversiteDbContext context) : Repository<Note>(context), INoteRepository
{
    // Implémentation spécifique pour la recherche par clé composite
    public async Task<Note?> FindAsync(long etudiantId, long ueId)
    {
        // FindAsync accepte params object[] keyValues, donc on peut passer les deux clés
        return await Context.Notes.FindAsync(etudiantId, ueId);
    }

    public async Task DeleteAsync(long etudiantId, long ueId)
    {
        var note = await FindAsync(etudiantId, ueId);
        if (note != null)
        {
            Context.Notes.Remove(note);
            await Context.SaveChangesAsync();
        }
    }
}