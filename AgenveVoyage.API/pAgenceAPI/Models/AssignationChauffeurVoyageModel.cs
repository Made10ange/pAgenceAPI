namespace pAgenceAPI.Models
{
    public class AssignationChauffeurVoyageModel
    {
        public int Id_Assignation { get; set; }
        public int Id_Chauffeur { get; set; }
        public int Id_Voyage { get; set; }

        // ✅ Strings OPTIONNELS (peuvent être null → ajouter ?)
        public string? Point_Depart_Troncon { get; set; }
        public string? Point_Arrivee_Troncon { get; set; }

        // ✅ TimeSpan optionnels (déjà corrects avec ?)
        public TimeSpan? Heure_Debut { get; set; }
        public TimeSpan? Heure_Fin { get; set; }

        // ✅ int optionnel (déjà correct avec ?)
        public int? Ordre_Conduite { get; set; }
    }
}