namespace UniversiteDomain.Exceptions.NoteExceptions;

[Serializable]
public class UeNotInParcoursException : Exception
{
    public UeNotInParcoursException(string message) : base(message) { }
}