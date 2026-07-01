using System.ComponentModel.DataAnnotations;

namespace pAgenceAPI.Models;

/// <summary>Requête de saisie d'une écriture comptable (envoyée par le frontend).</summary>
public class EcritureComptableModel
{
    [Required]
    [StringLength(20)]
    public string num_transaction { get; set; } = string.Empty; // groupant toutes les lignes

    [Required]
    public DateTime date_ecriture { get; set; } = DateTime.Today;

    [StringLength(200)]
    public string? libelle { get; set; }

    public string code_journal { get; set; } = "JMAN"; // JMAN par défaut
    public string type_saisie { get; set; } = "Mono";  // Mono | Multi
    public int? id_agence { get; set; }
    public int? id_utilisateur { get; set; }

    [Required]
    public List<LigneEcritureModel> Lignes { get; set; } = new();
}

/// <summary>Une ligne comptable : un compte + montant débit OU crédit.</summary>
public class LigneEcritureModel
{
    [Required]
    [StringLength(30)]
    public string numcompte { get; set; } = string.Empty;

    public decimal debit { get; set; }
    public decimal credit { get; set; }

    [StringLength(200)]
    public string? libelle_ligne { get; set; }
}

/// <summary>Ligne lue depuis la table `operation` (pour les listes).</summary>
public class OperationComptableModel
{
    public int code_operation { get; set; }
    public DateTime date_operation { get; set; }
    public string? numcompte { get; set; }
    public string? libelle_compte { get; set; }
    public decimal debit { get; set; }
    public decimal credit { get; set; }
    public string? libelle { get; set; }
    public string? num_transaction { get; set; }
    public string? code_agence { get; set; }
    public int? code_user { get; set; }
}
