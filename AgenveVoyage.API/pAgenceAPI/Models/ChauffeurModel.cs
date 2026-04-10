#nullable disable
using System.ComponentModel.DataAnnotations.Schema;

namespace pAgenceAPI.Models
{
    public class ChauffeurModel
    {
        public int Id_Chauffeur { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Type_Piece { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string Lieu_Naissance { get; set; }
        public string Numero_Piece { get; set; }
        public string Lieu_Delivrance { get; set; }
        public string Signataire { get; set; }
        public string Profession { get; set; }
        public string Nationalite { get; set; }
        public string Sexe { get; set; }
        public DateTime? Date_Naissance { get; set; }
        public DateTime? Date_Delivrance { get; set; }
        public DateTime? Date_Expiration { get; set; }

        // ✅ Mapper la colonne PHOTO (BLOB) vers Photo_Base64 (string)
        [NotMapped]
        public string Photo_Base64 { get; set; }

        // Cette propriété est mappée à la colonne PHOTO dans la BDD
        [Column("Photo")]
        public byte[] Photo
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