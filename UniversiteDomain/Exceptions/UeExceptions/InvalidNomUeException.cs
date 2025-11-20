namespace UniversiteDomain.Exceptions.UeExceptions;

[Serializable]
public class InvalidNomUeException : Exception
{
    public InvalidNomUeException() : base() { }
    public InvalidNomUeException(string message) : base(message) { }
    public InvalidNomUeException(string message, Exception inner) : base(message, inner) { }
}