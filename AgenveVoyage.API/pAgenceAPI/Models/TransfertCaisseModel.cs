using System.ComponentModel.DataAnnotations;

namespace pAgenceAPI.Models;

public class TransfertCaisseModel
{
    public int      id_transfert     { get; set; }
    [Required] public int id_caisse_depart { get; set; }
    [Required] public int id_caisse_dest   { get; set; }
    [Required][Range(1, double.MaxValue)] public decimal montant { get; set; }
    public DateTime date_transfert   { get; set; } = DateTime.Today;
    public string?  num_piece        { get; set; }
    public string?  motif            { get; set; }
    public string   statut           { get; set; } = "En attente";
    public int?     code_user_init   { get; set; }
    public int?     code_user_valid  { get; set; }
    public DateTime date_init        { get; set; } = DateTime.Now;
    public DateTime? date_validation { get; set; }
    public long?    num_transaction_cpt { get; set; }

    // Champs joints (lecture)
    public string? libelle_caisse_depart { get; set; }
    public string? libelle_caisse_dest   { get; set; }
    public string? numcompte_depart      { get; set; }
    public string? numcompte_dest        { get; set; }
}
