using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.UeExceptions;

namespace UniversiteDomain.UseCases.UeUseCases.Create;

public class CreateUeUseCase(IRepositoryFactory repositoryFactory)
{
    // Surcharge pour faciliter l'appel avec des types primitifs
    public async Task<Ue> ExecuteAsync(string numeroUe, string intitule)
    {
        var ue = new Ue { NumeroUe = numeroUe, Intitule = intitule };
        return await ExecuteAsync(ue);
    }

    public async Task<Ue> ExecuteAsync(Ue ue)
    {
        await CheckBusinessRules(ue);
        
        Ue ueCree = await repositoryFactory.UeRepository().CreateAsync(ue);
        await repositoryFactory.SaveChangesAsync();
        
        return ueCree;
    }

    private async Task CheckBusinessRules(Ue ue)
    {
        ArgumentNullException.ThrowIfNull(ue);
        ArgumentNullException.ThrowIfNull(ue.NumeroUe);
        ArgumentNullException.ThrowIfNull(ue.Intitule);
        ArgumentNullException.ThrowIfNull(repositoryFactory);

        // Règle 1 : L'intitulé doit avoir plus de 3 caractères
        if (ue.Intitule.Trim().Length <= 3)
        {
            throw new InvalidNomUeException($"L'intitulé de l'UE '{ue.Intitule}' est trop court (doit être > 3 caractères).");
        }

        // Règle 2 : Le numéro de l'UE doit être unique
        // On cherche s'il existe déjà une UE avec le même numéro
        var existingUe = await repositoryFactory.UeRepository()
            .FindByConditionAsync(u => u.NumeroUe.Equals(ue.NumeroUe));

        if (existingUe is { Count: > 0 })
        {
            throw new DuplicateUeException($"Le numéro d'UE '{ue.NumeroUe}' existe déjà.");
        }
    }
}