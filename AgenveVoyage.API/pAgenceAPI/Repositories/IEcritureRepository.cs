using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public interface IEcritureRepository
{
    /// <summary>Enregistre une écriture complète (toutes les lignes) dans la table `operation`.</summary>
    Task EnregistrerAsync(EcritureComptableModel ecriture);

    Task<List<OperationComptableModel>> GetByDateAsync(DateTime date, int? idAgence = null);
    Task<List<OperationComptableModel>> GetByTransactionAsync(string numTransaction);

    /// <summary>Vérifie si une journée comptable est ouverte pour la date et l'agence données.</summary>
    Task<bool> JourneeOuverteAsync(DateTime date, int? idAgence = null);

    Task OuvrirJourneeAsync(int codeUser, int idAgence, DateTime? date = null);
    Task CloturerJourneeAsync(int codeUser, int idAgence, DateTime? date = null);

    /// <summary>Retourne la date et le statut de la dernière journée comptable enregistrée pour cette agence, ou null si aucune.</summary>
    Task<(DateTime Date, string Statut)?> GetDerniereJourneeAsync(int? idAgence = null);

    /// <summary>True si l'utilisateur est Admin ou caissier affecté à la caisse principale (seuls autorisés à ouvrir/clôturer la journée).</summary>
    Task<bool> PeutGererJourneeAsync(int? codeUser);

    /// <summary>
    /// Génère automatiquement l'écriture comptable d'une vente de billet.
    /// Retourne true si l'écriture a été créée, false si aucune journée n'est ouverte (sans lever d'exception).
    /// </summary>
    Task<List<OperationComptableModel>> GetBrouillardAsync(DateTime dateDebut, DateTime dateFin, int? idCaisse);

    Task<bool> EcritureVenteBilletAsync(string numTransaction, string numBillet, decimal montant,
        string? libelleType, string? trajet, int? idAgence, int? codeUser);

    /// <summary>Idem pour un enregistrement de colis.</summary>
    Task<bool> EcritureColisAsync(string numTransaction, string? refColis, decimal montant,
        string? expediteur, string? destinataire, int? idAgence, int? codeUser);

    /// <summary>Idem pour un enregistrement de bagage.</summary>
    Task<bool> EcritureBagageAsync(string numTransaction, string? refBagage, decimal montant,
        string? passager, int? idAgence, int? codeUser);

    /// <summary>Génère l'écriture comptable d'un paiement de salaire (6411 / caisse).</summary>
    Task<bool> EcritureSalaireAsync(string numTransaction, string nomEmploye, decimal montant,
        int? idAgence, int? codeUser);
}
