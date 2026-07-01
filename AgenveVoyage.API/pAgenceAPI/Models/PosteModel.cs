namespace pAgenceAPI.Models;

public class PosteModel
{
    public int ID_POSTE { get; set; }
    public string Libelle { get; set; } = string.Empty;
    public string? Description { get; set; }
}
