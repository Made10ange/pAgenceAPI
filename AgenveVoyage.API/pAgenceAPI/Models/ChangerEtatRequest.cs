namespace pAgenceAPI.Models
{
    public class ChangerEtatRequest
    {
        public string Nouvel_Etat { get; set; } = string.Empty;
        public int? Nouveau_Type { get; set; }
        public string? Motif { get; set; }
        public string? Modifie_Par { get; set; }
    }
}
