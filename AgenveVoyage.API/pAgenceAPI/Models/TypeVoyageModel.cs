namespace pAgenceAPI.Models
{
    public class TypeVoyageModel
    {
        public int     Id_Type_Voyage    { get; set; }
        public string  Libelle_Type_Voyage { get; set; } = string.Empty;
        public string? Point_Depart      { get; set; }
        public string? Point_Arrivee     { get; set; }
        public decimal Prix              { get; set; }
        public int?    Id_Agence         { get; set; }
    }
}