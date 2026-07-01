using System.ComponentModel.DataAnnotations;

namespace pAgenceAPI.Models;

// Correspond à la table `compte` du plan comptable OHADA/COBAC
public class CompteModel
{
    [Required]
    [StringLength(30)]
    public string numcompte { get; set; } = string.Empty;

    [StringLength(30)]
    public string? numcompte_pere { get; set; }

    [Required]
    [StringLength(150)]
    public string libelle_compte { get; set; } = string.Empty;

    public decimal solde { get; set; } = 0;
    public decimal cumul_credit { get; set; } = 0;
    public decimal cumul_debit { get; set; } = 0;

    [StringLength(30)]
    public string? type_compte { get; set; }

    [StringLength(10)]
    public string sens { get; set; } = "DC"; // D, C, DC

    [StringLength(10)]
    public string statut { get; set; } = "Actif";

    [StringLength(20)]
    public string devise { get; set; } = "XAF";

    [StringLength(5)]
    public string ferme { get; set; } = "Non";

    public DateTime date_creation { get; set; } = DateTime.Now;

    // Champ calculé (jointure) — libellé du compte parent
    public string? libelle_parent { get; set; }
}
