using System.ComponentModel.DataAnnotations.Schema;

namespace pAgenceAPI.Models;

public class CaissierModel
{
    public int Id_Utilisateur { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string? Sexe { get; set; }
    public string? Telephone { get; set; }
    public string? Email { get; set; }
    public DateTime? Date_Naissance { get; set; }
    public string? Lieu_Naissance { get; set; }
    public string? Nationalite { get; set; }
    public string? Profession { get; set; }
    public string? Type_Piece { get; set; }
    public string? Numero_Piece { get; set; }
    public DateTime? Date_Delivrance { get; set; }
    public string? Lieu_Delivrance { get; set; }
    public string? Signataire { get; set; }
    public DateTime? Date_Expiration { get; set; }
    public string Login { get; set; } = string.Empty;
    public string? MotDePasse { get; set; }
    public string Role { get; set; } = "Caissier";
    public bool Actif { get; set; } = true;
    public DateTime Date_Creation { get; set; } = DateTime.Now;
    public int? Id_Agence { get; set; }
    public int? Id_Groupe { get; set; }

    [NotMapped]
    public string? Photo_Base64 { get; set; }

    [Column("Photo")]
    public byte[]? Photo
    {
        get
        {
            if (string.IsNullOrEmpty(Photo_Base64)) return null;
            try { return Convert.FromBase64String(Photo_Base64); }
            catch { return null; }
        }
        set
        {
            if (value == null) Photo_Base64 = null;
            else Photo_Base64 = Convert.ToBase64String(value);
        }
    }

    // Champs calculés (jointures)
    public string? Nom_Agence { get; set; }
    public string? Nom_Groupe { get; set; }
    public int nb_caisses_actives { get; set; }
    public string NomComplet => $"{Prenom} {Nom}".Trim();
}
