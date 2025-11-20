using System;

namespace UniversiteDomain.Exceptions.ParcoursExceptions; // Assure-toi que le namespace est bon

[Serializable]
public class EtudiantSansParcoursException : Exception
{
    public EtudiantSansParcoursException() : base() { }
    
    // C'est ici la correction : on passe le message au parent avec ": base(message)"
    public EtudiantSansParcoursException(string message) : base(message) { }
    
    public EtudiantSansParcoursException(string message, Exception inner) : base(message, inner) { }
}