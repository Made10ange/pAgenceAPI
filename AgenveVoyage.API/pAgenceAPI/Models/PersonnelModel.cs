namespace pAgenceAPI.Models;

public class PersonnelModel
{
    public int ID_PERSONNEL { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string? Telephone { get; set; }
    public string? Email { get; set; }
    public int ID_POSTE { get; set; }
    public string LibellePoste { get; set; } = string.Empty;
    public string Type_Contrat { get; set; } = "CDI";
    public decimal Salaire_Base { get; set; }
    public DateTime Date_Embauche { get; set; }
    public string Statut { get; set; } = "Actif";
    public string? Notes { get; set; }
    public DateTime Date_Creation { get; set; }
    public int? ID_CHAUFFEUR { get; set; }
    public int? ID_UTILISATEUR { get; set; }
    public int? Id_Agence { get; set; }
    public string? Nom_Agence { get; set; }
}
