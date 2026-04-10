namespace pAgenceAPI.Models
{
    public class EmbarquementVoyagePassagerModel
    {
        public int Id_Embarquement { get; set; }
        public int Id_Passager { get; set; }
        public int Id_Voyage { get; set; }
        public DateTime Date_Embarquement { get; set; }

        // ✅ PROPRIÉTÉS REQUISES
        public int Numero_Siege { get; set; }
        public DateTime Date_Enregistrement { get; set; }
        public string Statut_Embarquement { get; set; } = string.Empty;

        // ✅ Autres propriétés (pour les jointures)
        public string Statut { get; set; } = string.Empty;
        public string Nom_Passager { get; set; } = string.Empty;
        public string Details_Voyage { get; set; } = string.Empty;
    }
}