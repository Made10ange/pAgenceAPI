namespace pAgenceAPI.Models
{
    public class BagageParPassagerRequest
    {
        public int Id_Passager { get; set; }
        public int Id_Voyage { get; set; }
        public decimal? Montant_Total { get; set; }
        public List<BagageLigneRequest> Bagages { get; set; } = new();
    }

    public class BagageLigneRequest
    {
        public string? Description { get; set; }
        public decimal? Poids { get; set; }
        public int? Id_Voyage_Bagage { get; set; }
    }
}
