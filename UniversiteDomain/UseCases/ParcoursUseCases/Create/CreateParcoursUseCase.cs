using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.ParcoursExceptions;

namespace UniversiteDomain.UseCases.ParcoursUseCases.Create;

public class CreateParcoursUseCase(IRepositoryFactory repositoryFactory)
{
    // Surcharge pour appeler avec des types primitifs
    public async Task<Parcours> ExecuteAsync(string nomParcours, int anneeFormation)
    {
        var parcours = new Parcours { NomParcours = nomParcours, AnneeFormation = anneeFormation };
        return await ExecuteAsync(parcours);
    }

    // Méthode principale avec l'entité
    public async Task<Parcours> ExecuteAsync(Parcours parcours)
    {
        await CheckBusinessRules(parcours);
        
        // On utilise la factory pour récupérer le repository parcours
        Parcours p = await repositoryFactory.ParcoursRepository().CreateAsync(parcours);
        
        // On sauvegarde les changements au niveau de la factory (transaction globale)
        await repositoryFactory.SaveChangesAsync();
        
        return p;
    }

    private async Task CheckBusinessRules(Parcours parcours)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        ArgumentNullException.ThrowIfNull(parcours.NomParcours);
        ArgumentNullException.ThrowIfNull(repositoryFactory);

        // Règle 1 : Le nom du parcours doit avoir une taille minimale (ex: 3 caractères)
        if (parcours.NomParcours.Length < 3)
        {
            throw new InvalidNomParcoursException(parcours.NomParcours + " - Le nom du parcours doit contenir au moins 3 caractères");
        }

        // Règle 2 : Unicité du parcours
        // On considère qu'un doublon est un parcours ayant le même Nom ET la même Année
        var existingParcours = await repositoryFactory.ParcoursRepository()
            .FindByConditionAsync(p => p.NomParcours.Equals(parcours.NomParcours) && p.AnneeFormation == parcours.AnneeFormation);

        if (existingParcours is { Count: > 0 })
        {
            throw new DuplicateParcoursException($"Le parcours {parcours.NomParcours} (M{parcours.AnneeFormation}) existe déjà.");
        }
    }
}