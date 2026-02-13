using UniversiteDomain.Entities;

namespace UniversiteDomain.DataAdapters;

public interface IUniversiteUserRepository : IRepository<IUniversiteUser>
{
    // Ajout d'un utilisateur 
    Task<IUniversiteUser?> AddUserAsync(string login, string email, string password, string role, Etudiant? etudiant);
    
    // Recherche d'un utilisateur par email
    Task<IUniversiteUser?> FindByEmailAsync(string email);
    
    // Mise à jour
    Task UpdateAsync(IUniversiteUser entity, string userName, string email);
    
    // Récupération des rôles
    Task<IList<string>> GetRolesAsync(IUniversiteUser user);
    
    // Vérification d'un rôle
    Task<bool> IsInRoleAsync(string email, string role);
    
    // Vérification du mot de passe
    Task<bool> CheckPasswordAsync(IUniversiteUser user, string password);
}