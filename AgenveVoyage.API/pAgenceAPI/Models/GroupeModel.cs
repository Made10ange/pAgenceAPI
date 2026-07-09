namespace pAgenceAPI.Models;

public class GroupeModel
{
    public int Id_Groupe { get; set; }
    public string Libelle { get; set; } = "";
    public string? Description { get; set; }
    public string Couleur { get; set; } = "#7C3AED";
    public bool Actif { get; set; } = true;
    public DateTime Date_Creation { get; set; } = DateTime.Now;
    public int Nb_Agents { get; set; }
}

public class AgentGroupeModel
{
    public int Id_Utilisateur { get; set; }
    public int Id_Groupe { get; set; }
    public DateTime Date_Affectation { get; set; }
    public string? Nom_Agent { get; set; }
    public string? Prenom_Agent { get; set; }
    public string? Login_Agent { get; set; }
}

public class PrivilegeModel
{
    public int Id_Privilege { get; set; }
    public int Id_Groupe { get; set; }
    public string Module { get; set; } = "";
    public string Action { get; set; } = "";
    public bool Autorise { get; set; } = true;
}

public class GroupePrivilegeDto
{
    public int Id_Groupe { get; set; }
    public List<int> Ids_Privileges { get; set; } = new();
}
