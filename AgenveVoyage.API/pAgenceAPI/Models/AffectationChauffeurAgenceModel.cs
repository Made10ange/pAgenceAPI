#nullable disable
using System.ComponentModel.DataAnnotations;

namespace pAgenceAPI.Models
{
    public class AffectationChauffeurAgenceModel
    {
        [Key]
        public int Id_Affectation_Chauffeur { get; set; }

        public int Id_Chauffeur { get; set; }
        public int Id_Agence { get; set; }
        public DateTime Date_Debut { get; set; }
        public DateTime? Date_Fin { get; set; }
        public string Statut { get; set; } = "Active";
        public string Observations { get; set; }

        // Champs de lecture pour le nom du chauffeur et de l'agence
        public string? Nom_Chauffeur { get; set; }
        public string? Prenom_Chauffeur { get; set; }
        public string? Nom_Agence { get; set; }
    }
}