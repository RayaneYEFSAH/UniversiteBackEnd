using UniversiteDomain.Entities;

namespace UniversiteDomain.DataAdapters;

public interface IUeRepository : IRepository<Ue>
{
    // Pour l'instant, les méthodes génériques (Create, Delete, FindByCondition...) suffisent.
    // Nous rajouterons des méthodes spécifiques ici si nécessaire plus tard.
}