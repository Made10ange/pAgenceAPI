using System.ComponentModel.DataAnnotations.Schema;

namespace pAgenceAPI.Models
{
    public class ChauffeurModel
    {
        public int Id_Chauffeur { get; set; }
        public int? Id_Agence { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string? Type_Piece { get; set; }
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public string? Lieu_Naissance { get; set; }
        public string? Numero_Piece { get; set; }
        public string? Lieu_Delivrance { get; set; }
        public string? Signataire { get; set; }
        public string? Profession { get; set; }
        public string? Nationalite { get; set; }
        public string? Sexe { get; set; }
        public DateTime? Date_Naissance { get; set; }
        public DateTime? Date_Delivrance { get; set; }
        public DateTime? Date_Expiration { get; set; }

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
                Photo_Base64 = value == null ? null : Convert.ToBase64String(value);
            }
        }
    }
}
