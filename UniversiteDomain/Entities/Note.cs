namespace UniversiteDomain.Entities;

public class Note
{
    public float Valeur { get; set; }

    // Clés étrangères
    public long EtudiantId { get; set; }
    public Etudiant Etudiant { get; set; } = null!;

    public long UeId { get; set; }
    public Ue Ue { get; set; } = null!;
    
    public override string ToString()
    {
        return $"Note : {Valeur}/20 (Etudiant {EtudiantId} - UE {UeId})";
    }
}