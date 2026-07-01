using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public class EcritureRepository : IEcritureRepository
{
    private readonly string _cs;
    private readonly ILogger<EcritureRepository> _logger;

    public EcritureRepository(IConfiguration config, ILogger<EcritureRepository> logger)
    {
        _cs     = config.GetConnectionString("DefaultConnection") ?? "";
        _logger = logger;
    }

    // ── Enregistrement d'une écriture (toutes les lignes en transaction) ──
    public async Task EnregistrerAsync(EcritureComptableModel e)
    {
        using var db = new MySqlConnection(_cs);
        await db.OpenAsync();
        using var tx = await db.BeginTransactionAsync();
        try
        {
            foreach (var ligne in e.Lignes)
            {
                var p = new DynamicParameters();
                p.Add("p_num_transaction", e.num_transaction);
                p.Add("p_date",            e.date_ecriture);
                p.Add("p_numcompte",       ligne.numcompte);
                p.Add("p_debit",           ligne.debit);
                p.Add("p_credit",          ligne.credit);
                p.Add("p_libelle",         ligne.libelle_ligne ?? e.libelle);
                p.Add("p_code_journal",    e.code_journal);
                p.Add("p_code_agence",     e.id_agence);
                p.Add("p_code_user",       e.id_utilisateur);
                p.Add("p_code_operation",  dbType: System.Data.DbType.Int32,
                      direction: System.Data.ParameterDirection.Output);

                await db.ExecuteAsync("sp_enregistrer_operation", p,
                    commandType: System.Data.CommandType.StoredProcedure,
                    transaction: tx);
            }

            await tx.CommitAsync();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            _logger.LogError(ex, "Erreur EnregistrerAsync écriture num_transaction={Num}", e.num_transaction);
            throw;
        }
    }

    // ── Opérations d'une date ─────────────────────────────────────────────
    public async Task<List<OperationComptableModel>> GetByDateAsync(DateTime date, int? idAgence = null)
    {
        const string sql = @"
            SELECT o.code_operation, o.date_operation, o.numcompte,
                   c.libelle_compte, o.debit, o.credit, o.libelle,
                   o.num_transaction, o.code_agence, o.code_user
            FROM   operation o
            LEFT JOIN compte c ON c.numcompte = o.numcompte
            WHERE  DATE(o.date_operation) = DATE(@date)
              AND  (@idAgence IS NULL OR o.code_agence = @idAgence)
            ORDER  BY o.num_transaction, o.code_operation";

        using var db = new MySqlConnection(_cs);
        return (await db.QueryAsync<OperationComptableModel>(sql, new { date, idAgence })).ToList();
    }

    // ── Lignes d'une transaction ──────────────────────────────────────────
    public async Task<List<OperationComptableModel>> GetByTransactionAsync(string numTransaction)
    {
        const string sql = @"
            SELECT o.code_operation, o.date_operation, o.numcompte,
                   c.libelle_compte, o.debit, o.credit, o.libelle,
                   o.num_transaction, o.code_agence, o.code_user
            FROM   operation o
            LEFT JOIN compte c ON c.numcompte = o.numcompte
            WHERE  o.num_transaction = @numTransaction
            ORDER  BY o.code_operation";

        using var db = new MySqlConnection(_cs);
        return (await db.QueryAsync<OperationComptableModel>(sql, new { numTransaction })).ToList();
    }

    // ── Vérifier journée ouverte (par agence) ──────────────────────────────
    public async Task<bool> JourneeOuverteAsync(DateTime date, int? idAgence = null)
    {
        using var db = new MySqlConnection(_cs);
        var count = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM journee_comptable WHERE DATE(date_journee)=DATE(@date) AND code_agence=@idAgence AND statut='Ouverte'",
            new { date, idAgence });
        return count > 0;
    }

    // ── Ouvrir la journée d'une agence ──────────────────────────────────────
    public async Task OuvrirJourneeAsync(int codeUser, int idAgence, DateTime? date = null)
    {
        using var db = new MySqlConnection(_cs);
        var p = new DynamicParameters();
        p.Add("p_code_user",   codeUser);
        p.Add("p_code_agence", idAgence);
        p.Add("p_date",        (date ?? DateTime.Today).Date);
        p.Add("p_message",     dbType: System.Data.DbType.String, size: 200,
              direction: System.Data.ParameterDirection.Output);

        await db.ExecuteAsync("sp_ouvrir_journee", p,
            commandType: System.Data.CommandType.StoredProcedure);
    }

    // ── Clôturer la journée d'une agence ────────────────────────────────────
    public async Task CloturerJourneeAsync(int codeUser, int idAgence, DateTime? date = null)
    {
        using var db = new MySqlConnection(_cs);
        var p = new DynamicParameters();
        p.Add("p_code_user",   codeUser);
        p.Add("p_code_agence", idAgence);
        p.Add("p_date",        (date ?? DateTime.Today).Date);
        p.Add("p_message",     dbType: System.Data.DbType.String, size: 200,
              direction: System.Data.ParameterDirection.Output);

        await db.ExecuteAsync("sp_cloturer_journee", p,
            commandType: System.Data.CommandType.StoredProcedure);
    }

    // ── Dernière journée connue (d'une agence) ───────────────────────────
    public async Task<(DateTime Date, string Statut)?> GetDerniereJourneeAsync(int? idAgence = null)
    {
        using var db = new MySqlConnection(_cs);
        var row = await db.QueryFirstOrDefaultAsync(
            "SELECT date_journee, statut FROM journee_comptable WHERE code_agence=@idAgence ORDER BY date_journee DESC LIMIT 1",
            new { idAgence });
        if (row == null) return null;
        return ((DateTime)row.date_journee, (string)row.statut);
    }

    // ── Rôle : le user est-il Admin, ou caissier affecté à la caisse principale ? ──
    public async Task<bool> PeutGererJourneeAsync(int? codeUser)
    {
        if (codeUser is null) return false;
        using var db = new MySqlConnection(_cs);

        var role = await db.ExecuteScalarAsync<string?>(
            "SELECT Role FROM utilisateur WHERE Id_Utilisateur = @codeUser", new { codeUser });
        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)) return true;

        var estCaissierPrincipal = await db.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM affectation_caissier_caisse acc
            JOIN caisse c ON c.id_caisse = acc.id_caisse
            WHERE acc.code_user = @codeUser
              AND acc.statut = 'Active'
              AND (acc.date_fin IS NULL OR acc.date_fin >= CURDATE())
              AND c.est_principale = 1",
            new { codeUser });
        return estCaissierPrincipal > 0;
    }

    // ── Brouillard de caisse ─────────────────────────────────────────────
    public async Task<List<OperationComptableModel>> GetBrouillardAsync(
        DateTime dateDebut, DateTime dateFin, int? idCaisse)
    {
        const string sql = @"
            SELECT o.code_operation, o.date_operation, o.numcompte,
                   c.libelle_compte, o.debit, o.credit, o.libelle,
                   o.num_transaction, o.numpiece
            FROM   operation o
            LEFT JOIN compte c ON c.numcompte = o.numcompte
            WHERE  DATE(o.date_operation) BETWEEN DATE(@dateDebut) AND DATE(@dateFin)
              AND  (@numcompte IS NULL OR o.numcompte = @numcompte)
            ORDER  BY o.date_operation, o.code_operation";

        string? numcompte = null;
        if (idCaisse.HasValue)
        {
            using var dbC = new MySqlConnection(_cs);
            numcompte = await dbC.ExecuteScalarAsync<string>(
                "SELECT numcompte FROM caisse WHERE id_caisse = @id", new { id = idCaisse });
        }

        using var db = new MySqlConnection(_cs);
        return (await db.QueryAsync<OperationComptableModel>(
            sql, new { dateDebut, dateFin, numcompte })).ToList();
    }

    // ── Écritures automatiques ────────────────────────────────────────────

    public async Task<bool> EcritureVenteBilletAsync(string numTransaction, string numBillet,
        decimal montant, string? libelleType, string? trajet, int? idAgence, int? codeUser)
    {
        try
        {
            using var db = new MySqlConnection(_cs);
            var p = new DynamicParameters();
            p.Add("p_num_transaction", numTransaction);
            p.Add("p_num_billet",      numBillet);
            p.Add("p_montant",         montant);
            p.Add("p_libelle_type",    libelleType ?? "Classique");
            p.Add("p_trajet",          trajet);
            p.Add("p_id_agence",       idAgence);
            p.Add("p_code_user",       codeUser);
            p.Add("p_ok",              dbType: System.Data.DbType.Byte,
                  direction: System.Data.ParameterDirection.Output);

            await db.ExecuteAsync("sp_ecriture_vente_billet_auto", p,
                commandType: System.Data.CommandType.StoredProcedure);

            return p.Get<byte>("p_ok") == 1;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "EcritureVenteBillet ignorée — {Num}", numTransaction);
            return false;
        }
    }

    public async Task<bool> EcritureColisAsync(string numTransaction, string? refColis,
        decimal montant, string? expediteur, string? destinataire, int? idAgence, int? codeUser)
    {
        try
        {
            using var db = new MySqlConnection(_cs);
            var p = new DynamicParameters();
            p.Add("p_num_transaction", numTransaction);
            p.Add("p_ref_colis",       refColis ?? "");
            p.Add("p_montant",         montant);
            p.Add("p_expediteur",      expediteur);
            p.Add("p_destinataire",    destinataire);
            p.Add("p_id_agence",       idAgence);
            p.Add("p_code_user",       codeUser);
            p.Add("p_ok",              dbType: System.Data.DbType.Byte,
                  direction: System.Data.ParameterDirection.Output);

            await db.ExecuteAsync("sp_ecriture_colis_auto", p,
                commandType: System.Data.CommandType.StoredProcedure);

            return p.Get<byte>("p_ok") == 1;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "EcritureColis ignorée — {Num}", numTransaction);
            return false;
        }
    }

    public async Task<bool> EcritureBagageAsync(string numTransaction, string? refBagage,
        decimal montant, string? passager, int? idAgence, int? codeUser)
    {
        try
        {
            using var db = new MySqlConnection(_cs);
            var p = new DynamicParameters();
            p.Add("p_num_transaction", numTransaction);
            p.Add("p_ref_bagage",      refBagage ?? "");
            p.Add("p_montant",         montant);
            p.Add("p_passager",        passager);
            p.Add("p_id_agence",       idAgence);
            p.Add("p_code_user",       codeUser);
            p.Add("p_ok",              dbType: System.Data.DbType.Byte,
                  direction: System.Data.ParameterDirection.Output);

            await db.ExecuteAsync("sp_ecriture_bagage_auto", p,
                commandType: System.Data.CommandType.StoredProcedure);

            return p.Get<byte>("p_ok") == 1;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "EcritureBagage ignorée — {Num}", numTransaction);
            return false;
        }
    }
}
