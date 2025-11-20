namespace UniversiteDomain.Exceptions.NoteExceptions;

[Serializable]
public class InvalidNoteValueException : Exception
{
    public InvalidNoteValueException(string message) : base(message) { }
}