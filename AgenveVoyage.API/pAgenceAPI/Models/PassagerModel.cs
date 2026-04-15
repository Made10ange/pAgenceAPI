using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pAgenceAPI.Models
{
    public class PassagerModel
    {
        public int Id_Passager { get; set; }

        // ✅ Strings OBLIGATOIRES
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;

        // Alias de compatibilite pour les vues / appels encore bases sur l'ancien contrat.
        [NotMapped]
        public string Nom_Passager
        {
            get => Nom;
            set => Nom = value;
        }

        [NotMapped]
        public string Prenom_Passager
        {
            get => Prenom;
            set => Prenom = value;
        }

        [NotMapped]
        public string? Numero_CNI_Passager
        {
            get => Numero_Piece;
            set => Numero_Piece = value;
        }

        // ✅ Strings OPTIONNELS (ajouter ?)
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

        // ✅ DateTime optionnels
        public DateTime? Date_Naissance { get; set; }
        public DateTime? Date_Delivrance { get; set; }
        public DateTime? Date_Expiration { get; set; }

        // ✅ Photo
        public string? Photo_Base64 { get; set; }

        [NotMapped]
        public byte[]? Photo
        {
            get
            {
                if (string.IsNullOrEmpty(Photo_Base64))
                    return null;

                try
                {
                    return Convert.FromBase64String(Photo_Base64);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                if (value == null)
                    Photo_Base64 = null;
                else
                    Photo_Base64 = Convert.ToBase64String(value);
            }
        }
    }
}
