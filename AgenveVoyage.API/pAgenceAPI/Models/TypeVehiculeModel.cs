#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pAgenceAPI.Models
{
    [Table("TYPE_VEHICULE")]
    public class TypeVehiculeModel
    {
        [Key]
        [Column("ID_TYPE")]
        public int Id_Type { get; set; }

        [Column("LIBELLE_TYPE")]
        [StringLength(100)]
        public string Libelle_Type { get; set; }

        [Column("MARQUE")]
        [StringLength(100)]
        public string Marque { get; set; }

        [Column("NOMBRE_PLACE")]
        public int Nombre_Place { get; set; }

        [Column("ID_TYPE_VOYAGE")]
        public int? Id_Type_Voyage { get; set; }

        [Column("Id_Agence")]
        public int? Id_Agence { get; set; }

        // Jointure — libellé du type de voyage associé
        [NotMapped]
        public string? Libelle_Type_Voyage { get; set; }
    }
}