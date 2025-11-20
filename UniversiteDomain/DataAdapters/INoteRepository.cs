using UniversiteDomain.Entities;

namespace UniversiteDomain.DataAdapters;

public interface INoteRepository : IRepository<Note>
{
    // Méthode spécifique pour trouver une note par sa clé composite
    Task<Note?> FindAsync(long etudiantId, long ueId);
    
    // Méthode spécifique pour supprimer si nécessaire
    Task DeleteAsync(long etudiantId, long ueId);
}