namespace pAgenceAPI.Models
{
    public class AffectationVehiculeAgenceModel
    {
        public int Id_Affectation_Vehicule { get; set; }
        public int Id_Agence { get; set; }
        public int Id_Vehicule { get; set; }
        public DateTime Date_Debut { get; set; }
        public DateTime? Date_Fin { get; set; }
    }
}
