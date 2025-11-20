namespace UniversiteDomain.Exceptions.ParcoursExceptions;

[Serializable]
public class InvalidNomParcoursException : Exception
{
    public InvalidNomParcoursException() : base() { }
    public InvalidNomParcoursException(string message) : base(message) { }
    public InvalidNomParcoursException(string message, Exception inner) : base(message, inner) { }
}