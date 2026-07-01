namespace pAgenceAPI.Models;

public class BalanceGeneraleModel
{
    public string  numcompte               { get; set; } = "";
    public string  libelle_compte          { get; set; } = "";
    public decimal solde_debiteur_initial  { get; set; }
    public decimal solde_crediteur_initial { get; set; }
    public decimal mvt_debit               { get; set; }
    public decimal mvt_credit              { get; set; }
    public decimal solde_debiteur_final    { get; set; }
    public decimal solde_crediteur_final   { get; set; }
}
