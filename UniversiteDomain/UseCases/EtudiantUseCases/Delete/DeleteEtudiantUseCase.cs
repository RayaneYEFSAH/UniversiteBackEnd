using UniversiteDomain.DataAdapters.DataAdaptersFactory;

namespace UniversiteDomain.UseCases.EtudiantUseCases.Delete;

public class DeleteEtudiantUseCase(IRepositoryFactory factory)
{
    public async Task ExecuteAsync(long id)
    {
        // Attention : La méthode s'appelle DeleteAsync, pas Delete
        await factory.EtudiantRepository().DeleteAsync(id);
        await factory.SaveChangesAsync();
    }
    
    // (Optionnel) Ajoute cette méthode pour la sécurité plus tard
    public bool IsAuthorized(string role)
    {
        // Seuls les admins et la scolarité peuvent supprimer
        return role.Equals(UniversiteDomain.Entities.Roles.Responsable) || 
               role.Equals(UniversiteDomain.Entities.Roles.Scolarite);
    }
}