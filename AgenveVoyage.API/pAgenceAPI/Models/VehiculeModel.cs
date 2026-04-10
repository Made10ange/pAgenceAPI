namespace pAgenceAPI.Models
{
    public class VehiculeModel
    {
        public int Id_Vehicule { get; set; }
        public int Id_Type { get; set; }

        // ✅ String avec valeur par défaut
        public string Immatriculation { get; set; } = string.Empty;
        public string Statut { get; set; } = "Disponible";

        // ✅ String nullable (peut être null)
        public string? Libelle_Type { get; set; }
    }
}