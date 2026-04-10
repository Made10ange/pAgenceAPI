namespace pAgenceAPI.Models
{
    public class TypeVehiculeModel
    {
        public int Id_Type { get; set; }

        // ✅ String avec valeur par défaut
        public string Libelle_Type { get; set; } = string.Empty;
    }
}