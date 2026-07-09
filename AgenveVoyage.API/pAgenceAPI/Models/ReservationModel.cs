using System.ComponentModel.DataAnnotations;

namespace pAgenceAPI.Models
{
    public class ReservationModel
    {
        public int Id_Reservation { get; set; }

        public string Reference { get; set; } = string.Empty;

        [Required]
        public int Id_Voyage { get; set; }

        public int? Id_Passager { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom_Client { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Prenom_Client { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Telephone_Client { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Numero_Cni_Client { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Email_Client { get; set; }

        public int? Numero_Siege { get; set; }

        [StringLength(20)]
        public string? Sexe_Client { get; set; } = "Non précisé";

        [Required]
        public decimal Montant { get; set; }

        public string Statut_Paiement { get; set; } = "En attente";

        public string? Provider_Paiement { get; set; }

        public string? Reference_Paiement { get; set; }

        public DateTime? Date_Paiement { get; set; }

        public string Statut_Reservation { get; set; } = "En attente";

        public string? Validee_Par { get; set; }

        public DateTime? Date_Validation { get; set; }

        public DateTime Date_Creation { get; set; } = DateTime.Now;

        // Champs joints (lecture seule)
        public string? Point_Depart { get; set; }
        public string? Point_Arrivee { get; set; }
        public DateTime? Date_Depart { get; set; }
        public TimeSpan? Heure_Depart { get; set; }
        public string? Immatriculation { get; set; }
        public string? Libelle_Type_Voyage { get; set; }
    }

    public class PaiementLogModel
    {
        public int Id_Log { get; set; }
        public int Id_Reservation { get; set; }
        public string Evenement { get; set; } = string.Empty;
        public decimal? Montant { get; set; }
        public string? Reference_Externe { get; set; }
        public string? Payload_Brut { get; set; }
        public DateTime Date_Evenement { get; set; } = DateTime.Now;
    }
}
