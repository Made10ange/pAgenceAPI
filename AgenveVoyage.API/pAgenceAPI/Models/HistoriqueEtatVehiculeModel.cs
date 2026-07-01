namespace pAgenceAPI.Models
{
    public class HistoriqueEtatVehiculeModel
    {
        public int Id_Historique { get; set; }
        public int Id_Vehicule { get; set; }
        public string? Ancien_Etat { get; set; }
        public string Nouvel_Etat { get; set; } = string.Empty;
        public int? Ancien_Type { get; set; }
        public int? Nouveau_Type { get; set; }
        public string? Motif { get; set; }
        public DateTime Date_Changement { get; set; }
        public string? Modifie_Par { get; set; }

        // Jointures affichage
        public string? Immatriculation { get; set; }
        public string? Libelle_Ancien_Type { get; set; }
        public string? Libelle_Nouveau_Type { get; set; }
    }
}
