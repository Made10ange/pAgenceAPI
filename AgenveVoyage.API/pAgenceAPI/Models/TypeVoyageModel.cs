namespace pAgenceAPI.Models
{
    public class TypeVoyageModel
    {
        public int Id_Type_Voyage { get; set; }

        // ✅ String avec valeur par défaut
        public string Libelle_Type_Voyage { get; set; } = string.Empty;
        public decimal Prix { get; set; }
    }
}