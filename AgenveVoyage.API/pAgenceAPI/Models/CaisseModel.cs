namespace pAgenceAPI.Models;

public class CaisseModel
{
    public int id_caisse { get; set; }
    public string code_caisse { get; set; } = string.Empty;
    public string numcompte { get; set; } = string.Empty;
    public string libelle { get; set; } = string.Empty;
    public bool est_principale { get; set; } = false;
    public string? code_agence { get; set; }
    public string statut { get; set; } = "Active";
    public DateTime date_creation { get; set; } = DateTime.Now;

    // Champs calculés (jointures)
    public string? nom_agence { get; set; }
    public string? libelle_compte { get; set; }
    public decimal solde_actuel { get; set; } = 0;
}

public class AffectationCaissierModel
{
    public int id_affectation { get; set; }
    public int id_caisse { get; set; }
    public int id_utilisateur { get; set; }
    public DateTime date_debut { get; set; }
    public DateTime? date_fin { get; set; }
    public string statut { get; set; } = "Active";
    public int? id_utilisateur_createur { get; set; }
    public DateTime date_creation { get; set; } = DateTime.Now;

    // Champs calculés (jointures)
    public string? libelle_caisse { get; set; }
    public string? nom_caissier { get; set; }
    public string? prenom_caissier { get; set; }
    public string? login_caissier { get; set; }
}
