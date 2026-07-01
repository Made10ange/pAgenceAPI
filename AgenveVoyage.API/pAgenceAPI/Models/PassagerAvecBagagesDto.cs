namespace pAgenceAPI.Models
{
    public class PassagerAvecBagagesDto
    {
        public int Id_Passager { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public List<BagageModel> Bagages { get; set; } = new();
    }
}
