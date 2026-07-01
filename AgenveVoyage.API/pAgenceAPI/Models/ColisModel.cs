using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pAgenceAPI.Models
{
    public class ColisModel
    {
        public int Id_Colis { get; set; }

        [StringLength(30)]
        public string Reference_Colis { get; set; } = string.Empty;

        public int? Id_Voyage { get; set; }

        public int? Id_Agence { get; set; }

        [Required]
        [StringLength(150)]
        public string Nom_Expediteur { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Tel_Expediteur { get; set; }

        [Required]
        [StringLength(150)]
        public string Nom_Destinataire { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Tel_Destinataire { get; set; }

        [Required]
        [StringLength(100)]
        public string Ville_Depart { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Ville_Arrivee { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        public decimal? Poids { get; set; }

        public decimal Valeur_Declaree { get; set; } = 0;

        public decimal Prix_Transport { get; set; } = 0;

        [StringLength(50)]
        public string Mode_Paiement { get; set; } = "Espèces";

        [StringLength(50)]
        public string Statut { get; set; } = "En attente";

        public DateTime Date_Envoi { get; set; } = DateTime.Now;

        public DateTime? Date_Livraison_Prevue { get; set; }

        public DateTime? Date_Livraison_Effective { get; set; }

        // Jointures
        [NotMapped] public string? Trajet_Voyage { get; set; }
        [NotMapped] public string? Nom_Agence { get; set; }
    }
}
