using System.ComponentModel.DataAnnotations.Schema;

using System.ComponentModel.DataAnnotations;

namespace pAgenceAPI.Models
{
    public class VoyageModel
    {
        public int Id_Voyage { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Le vehicule est obligatoire.")]
        public int Id_Vehicule { get; set; }

        public int? Id_Type_Voyage { get; set; }

        [StringLength(150)]
        public string? Point_Depart { get; set; }

        [StringLength(150)]
        public string? Point_Arrivee { get; set; }

        [Required]
        public DateTime Date_Depart { get; set; }

        public DateTime? Date_Arrivee { get; set; }

        public TimeSpan? Heure_Depart { get; set; }

        public TimeSpan? Heure_Arrivee { get; set; }
        public TimeSpan? Duree { get; set; }

        public string Statut { get; set; } = "Programmé";
        public int? Id_Agence { get; set; }
        public int? Numero_Journalier { get; set; }

        // Chauffeur assigné (optionnel, géré via ASSIGNATION_CHAUFFEUR_VOYAGE)
        public int? Id_Chauffeur { get; set; }

        [NotMapped]
        public string? Immatriculation { get; set; }

        [NotMapped]
        public string? Libelle_Type_Voyage { get; set; }

        [NotMapped]
        public decimal Prix { get; set; }

        [NotMapped]
        public int Nombre_Place { get; set; }

        [NotMapped]
        public string? Nom_Chauffeur { get; set; }

        [NotMapped]
        public string? Nom_Agence { get; set; }
    }
}
