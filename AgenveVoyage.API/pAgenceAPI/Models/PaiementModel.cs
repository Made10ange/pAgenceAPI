using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pAgenceAPI.Models
{
    public class PaiementModel
    {
        public int Id_Paiement { get; set; }

        [Required]
        [StringLength(20)]
        public string Type_Paiement { get; set; } = "Passager"; // Passager | Colis

        public int? Id_Passager { get; set; }
        public int? Id_Colis { get; set; }
        public int? Id_Voyage { get; set; }

        [Required]
        public decimal Montant { get; set; }

        [Required]
        [StringLength(50)]
        public string Mode_Paiement { get; set; } = "Espèces"; // Espèces | Mobile Money | Virement | Chèque

        [StringLength(30)]
        public string Statut { get; set; } = "Payé"; // Payé | En attente | Annulé

        public DateTime Date_Paiement { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? Notes { get; set; }

        // Jointures
        [NotMapped] public string? Nom_Passager { get; set; }
        [NotMapped] public string? Reference_Colis { get; set; }
        [NotMapped] public string? Trajet_Voyage { get; set; }
    }
}
