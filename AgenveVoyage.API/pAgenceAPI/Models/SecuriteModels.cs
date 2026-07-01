namespace pAgenceAPI.Models;

public class GroupeModel
{
    public int Id_Groupe { get; set; }
    public string Libelle { get; set; } = "";
    public string? Description { get; set; }
    public string Couleur { get; set; } = "#7C3AED";
    public bool Actif { get; set; } = true;
    public DateTime Date_Creation { get; set; }
    public int Nb_Agents { get; set; }  // jointure
}

public class AgentGroupeModel
{
    public int Id_Utilisateur { get; set; }
    public int Id_Groupe { get; set; }
    public DateTime Date_Affectation { get; set; }
    // jointure
    public string? Nom_Agent { get; set; }
    public string? Prenom_Agent { get; set; }
    public string? Login_Agent { get; set; }
    public string? Libelle_Groupe { get; set; }
}

public class PrivilegeModel
{
    public int Id_Privilege { get; set; }
    public int Id_Groupe { get; set; }
    public string Module { get; set; } = "";
    public string Action { get; set; } = "";
    public bool Autorise { get; set; } = true;
    // jointure
    public string? Libelle_Groupe { get; set; }
}

public class JournalAuditModel
{
    public int Id_Journal { get; set; }
    public int? Id_Utilisateur { get; set; }
    public string? Login_Agent { get; set; }
    public string? Nom_Agent { get; set; }
    public string? Module { get; set; }
    public string? Action { get; set; }
    public string? Details { get; set; }
    public string? Ancienne_Valeur { get; set; }
    public string? Nouvelle_Valeur { get; set; }
    public string? IP_Address { get; set; }
    public string? User_Agent { get; set; }
    public string Statut { get; set; } = "Succès";
    public DateTime Date_Action { get; set; }
}

public class HistoriqueConnexionModel
{
    public int Id_Connexion { get; set; }
    public int? Id_Utilisateur { get; set; }
    public string Login_Tente { get; set; } = "";
    public string? Nom_Agent { get; set; }
    public string Statut { get; set; } = "Succès";
    public string? Motif_Echec { get; set; }
    public string? IP_Address { get; set; }
    public string? User_Agent { get; set; }
    public DateTime Date_Connexion { get; set; }
}

