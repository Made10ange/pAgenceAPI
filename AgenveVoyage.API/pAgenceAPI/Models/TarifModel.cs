using System.ComponentModel.DataAnnotations;

namespace pAgenceAPI.Models
{
    public class TarifModel
    {
        public int Id_Tarif { get; set; }

        [Required]
        [StringLength(150)]
        public string Libelle { get; set; } = string.Empty;

        public int? Id_Type_Voyage { get; set; }

        [StringLength(150)]
        public string? Point_Depart { get; set; }

        [StringLength(150)]
        public string? Point_Arrivee { get; set; }

        [Required]
        [StringLength(50)]
        public string Type_Passager { get; set; } = "Adulte";

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Prix { get; set; }

        public DateTime? Date_Debut { get; set; }
        public DateTime? Date_Fin   { get; set; }

        public bool Actif { get; set; } = true;

        public DateTime Date_Creation { get; set; }

        // jointure
        public string? Libelle_Type_Voyage { get; set; }
    }
}
