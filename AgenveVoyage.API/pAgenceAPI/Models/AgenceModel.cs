#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pAgenceAPI.Models
{
    [Table("AGENCE")]
    public class AgenceModel
    {
        [Key]
        [Column("ID_AGENCE")]
        public int Id_Agence { get; set; }

        [Required]
        [Column("NOM_AGENCE")]
        [StringLength(100)]
        public string Nom_Agence { get; set; } = string.Empty;

        [Required]
        [Column("VILLE")]
        [StringLength(100)]
        public string Ville { get; set; } = string.Empty;

        [Required]
        [Column("ADRESSE")]
        [StringLength(150)]
        public string Adresse { get; set; } = string.Empty;

        [Column("TELEPHONE")]
        [StringLength(20)]
        public string? Telephone { get; set; }

        [Column("DATE_CREATION")]
        public DateTime Date_Creation { get; set; }
    }
}