namespace pAgenceAPI.Models;

public class UtilisateurModel
{
    public int Id_Utilisateur { get; set; }
    public string Nom { get; set; } = "";
    public string Prenom { get; set; } = "";
    public string Login { get; set; } = "";
    public string MotDePasse { get; set; } = "";
    public string Role { get; set; } = "Agent";
    public bool Actif { get; set; } = true;
    public DateTime Date_Creation { get; set; }
    public int? Id_Agence { get; set; }
    public string? Nom_Agence { get; set; }
    public string? Ville_Agence { get; set; }
}

public class LoginRequest
{
    public string Login { get; set; } = "";
    public string MotDePasse { get; set; } = "";
    public int? Id_Agence { get; set; }
}

public class LoginResponse
{
    public bool Succes { get; set; }
    public string? Message { get; set; }
    public int? Id_Utilisateur { get; set; }
    public string? Nom { get; set; }
    public string? Prenom { get; set; }
    public string? Role { get; set; }
    public int? Id_Agence { get; set; }
    public string? Nom_Agence { get; set; }
    public string? Ville_Agence { get; set; }
}

