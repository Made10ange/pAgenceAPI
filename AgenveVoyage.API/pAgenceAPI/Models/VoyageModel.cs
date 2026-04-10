namespace pAgenceAPI.Models
{
    public class VoyageModel
    {
        public int Id_Voyage { get; set; }
        public int Id_Vehicule { get; set; }
        public int? Id_Type_Voyage { get; set; }
        public string Point_Depart { get; set; } = string.Empty;
        public string Point_Arrivee { get; set; } = string.Empty;
        public DateTime Date_Depart { get; set; }
        public DateTime Date_Arrivee { get; set; }

        // ✅ CHANGER EN TimeSpan (pas string)
        public TimeSpan Heure_Depart { get; set; }
        public TimeSpan Heure_Arrivee { get; set; }
        public TimeSpan Duree { get; set; }

        public string Statut { get; set; } = "Programmé";
    }
}