using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pAgenceAPI.Models
{
    public class BilletModel
    {
        public int      Id_Billet       { get; set; }
        public string   Numero_Billet   { get; set; } = string.Empty;

        [Required] public int     Id_Passager    { get; set; }
        [Required] public string  Point_Depart   { get; set; } = string.Empty;
        [Required] public string  Point_Arrivee  { get; set; } = string.Empty;

        public int?     Id_Type_Voyage    { get; set; }
        public decimal  Montant           { get; set; }
        public DateTime Date_Achat        { get; set; } = DateTime.Now;
        public DateTime Date_Validite     { get; set; } = DateTime.Now.AddMonths(6);
        public string   Statut            { get; set; } = "Valide";
        public int?     Id_Voyage_Prevu   { get; set; }
        public int?     Id_Voyage_Utilise { get; set; }
        public int?     Numero_Siege      { get; set; }
        public string   Mode_Paiement     { get; set; } = "Espèces";
        public string?  Vendu_Par         { get; set; }
        public DateTime Date_Creation     { get; set; } = DateTime.Now;
        public DateTime? Date_Modification { get; set; }

        // Champs joints (lecture seule)
        [NotMapped] public string? Nom_Passager      { get; set; }
        [NotMapped] public string? Prenom_Passager   { get; set; }
        [NotMapped] public string? Telephone_Passager { get; set; }
        [NotMapped] public string? Libelle_Type_Voyage { get; set; }
        [NotMapped] public string? Libelle_Voyage_Prevu { get; set; }
        [NotMapped] public string? Libelle_Voyage_Utilise { get; set; }

        [NotMapped]
        public string NomCompletPassager =>
            $"{Prenom_Passager} {Nom_Passager}".Trim();

        [NotMapped]
        public bool EstExpire => Date_Validite < DateTime.Now && Statut == "Valide";

        [NotMapped]
        public int JoursRestants => (int)(Date_Validite - DateTime.Now).TotalDays;
    }
}
