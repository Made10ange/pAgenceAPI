namespace pAgenceAPI.Models;

public class FichePayeModel
{
    public int ID_FICHE { get; set; }
    public int ID_PERSONNEL { get; set; }
    public string NomPersonnel { get; set; } = string.Empty;
    public string PrenomPersonnel { get; set; } = string.Empty;
    public string LibellePoste { get; set; } = string.Empty;
    public int Mois { get; set; }
    public int Annee { get; set; }
    public decimal Salaire_Base { get; set; }
    public decimal Primes { get; set; }
    public decimal Deductions { get; set; }
    public decimal Net_A_Payer { get; set; }
    public string Statut { get; set; } = "En attente";
    public DateTime? Date_Paiement { get; set; }
    public string? Note { get; set; }
    public DateTime Date_Creation { get; set; }
}
