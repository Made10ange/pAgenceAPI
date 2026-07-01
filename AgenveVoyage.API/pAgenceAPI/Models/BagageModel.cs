using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pAgenceAPI.Models
{
    public class BagageModel
    {
        public int Id_Bagage { get; set; }

        [Required]
        public int Id_Passager { get; set; }

        public int? Id_Voyage_Passager { get; set; }

        public int? Id_Voyage_Bagage { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        public decimal? Poids { get; set; }

        [StringLength(50)]
        public string Statut { get; set; } = "En attente";

        public DateTime Date_Enregistrement { get; set; } = DateTime.Now;

        public decimal? Montant_Total { get; set; }
        public int Numero_Ordre { get; set; } = 1;
        public int Total_Bagages { get; set; } = 1;

        // Jointures
        [NotMapped] public string? Nom_Passager { get; set; }
        [NotMapped] public string? Trajet_Passager { get; set; }
        [NotMapped] public string? Trajet_Bagage { get; set; }
        [NotMapped] public string? Immatriculation_Bagage { get; set; }
    }
}
