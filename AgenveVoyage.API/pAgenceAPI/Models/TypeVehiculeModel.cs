#nullable disable
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
    }
}