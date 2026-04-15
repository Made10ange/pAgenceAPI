namespace pAgenceAPI.Models
{
    public class AffectationVehiculeAgenceModel
    {
        public int Id_Affectation_Vehicule { get; set; }
        public int Id_Agence { get; set; }
        public int Id_Vehicule { get; set; }
        public DateTime Date_Debut { get; set; }
        public DateTime? Date_Fin { get; set; }
        public string? Statut { get; set; } = "Active";
        public string? Observations { get; set; }

        // Champs affichage joints
        public string? Immatriculation { get; set; }
        public string? Marque { get; set; }
        public string? Nom_Agence { get; set; }
        public string? Ville { get; set; }
    }
}
