using UniversiteDomain.Entities;

namespace UniversiteDomain.DataAdapters;

public interface IEtudiantRepository : IRepository<Etudiant>
{
    public Task<Etudiant?> FindEtudiantCompletAsync(long idEtudiant);
}