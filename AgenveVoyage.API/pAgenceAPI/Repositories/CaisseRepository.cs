using System.Data;
using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

// Toutes les opérations passent par des procédures stockées
public class CaisseRepository : ICaisseRepository
{
    private readonly string _cs;

    public CaisseRepository(IConfiguration config)
        => _cs = config.GetConnectionString("DefaultConnection")!;

    // ─── Caisse ──────────────────────────────────────────────────────────────

    public async Task<List<CaisseModel>> GetAllAsync(int? idAgence = null)
    {
        using var db = new MySqlConnection(_cs);
        var result = (await db.QueryAsync<CaisseModel>("CALL SP_LISTE_caisseS()")).ToList();

        // Une caisse non encore rattachée à une agence (code_agence NULL) reste visible
        // partout jusqu'à ce qu'elle soit affectée manuellement.
        if (idAgence.HasValue)
        {
            var id = idAgence.Value.ToString();
            result = result.Where(c => string.IsNullOrEmpty(c.code_agence) || c.code_agence == id).ToList();
        }

        return result;
    }

    public async Task<CaisseModel?> GetByIdAsync(int id)
    {
        using var db = new MySqlConnection(_cs);
        return await db.QueryFirstOrDefaultAsync<CaisseModel>(
            "CALL SP_GET_caisse(@p_id)", new { p_id = id });
    }

    public async Task<int> AjouterAsync(CaisseModel model)
    {
        using var db = new MySqlConnection(_cs);
        var p = new DynamicParameters();
        p.Add("p_code_caisse",    model.code_caisse);
        p.Add("p_numcompte",      model.numcompte);
        p.Add("p_libelle",        model.libelle);
        p.Add("p_est_principale", model.est_principale ? 1 : 0);
        p.Add("p_code_agence",    model.code_agence);
        p.Add("p_statut",         model.statut);
        p.Add("p_id", dbType: System.Data.DbType.Int32,
              direction: System.Data.ParameterDirection.Output);

        await db.ExecuteAsync("SP_AJOUTER_caisse", p, commandType: CommandType.StoredProcedure);
        return p.Get<int>("p_id");
    }

    public async Task<bool> ModifierAsync(CaisseModel model)
    {
        using var db = new MySqlConnection(_cs);
        await db.ExecuteAsync(
            "CALL SP_MODIFIER_caisse(@p_id,@p_numcompte,@p_libelle,@p_est_principale,@p_code_agence,@p_statut)",
            new
            {
                p_id            = model.id_caisse,
                p_numcompte     = model.numcompte,
                p_libelle       = model.libelle,
                p_est_principale = model.est_principale ? 1 : 0,
                p_code_agence   = model.code_agence,
                p_statut        = model.statut
            });
        return true;
    }

    public async Task<bool> SupprimerAsync(int id)
    {
        using var db = new MySqlConnection(_cs);
        var p = new DynamicParameters();
        p.Add("p_id", id);
        p.Add("p_ok", dbType: System.Data.DbType.Boolean,
              direction: System.Data.ParameterDirection.Output);

        await db.ExecuteAsync("SP_SUPPRIMER_caisse", p, commandType: CommandType.StoredProcedure);
        return p.Get<bool>("p_ok");
    }

    // ─── Affectations caissier ↔ caisse ──────────────────────────────────────

    public async Task<List<AffectationCaissierModel>> GetAffectationsAsync(int? idCaisse = null)
    {
        using var db = new MySqlConnection(_cs);
        var result = await db.QueryAsync<AffectationCaissierModel>(
            "CALL SP_LISTE_AFFECTATIONS(@p_id_caisse)",
            new { p_id_caisse = idCaisse });
        return result.ToList();
    }

    public async Task<bool> AffecterAsync(AffectationCaissierModel model)
    {
        using var db = new MySqlConnection(_cs);
        var p = new DynamicParameters();
        p.Add("p_id_caisse",              model.id_caisse);
        p.Add("p_id_utilisateur",         model.id_utilisateur);
        p.Add("p_date_debut",             model.date_debut.ToString("yyyy-MM-dd"));
        p.Add("p_date_fin",               model.date_fin?.ToString("yyyy-MM-dd"));
        p.Add("p_id_utilisateur_createur",model.id_utilisateur_createur);
        p.Add("p_ok", dbType: System.Data.DbType.Boolean,
              direction: System.Data.ParameterDirection.Output);

        await db.ExecuteAsync("SP_AFFECTER_CAISSIER", p, commandType: CommandType.StoredProcedure);
        return p.Get<bool>("p_ok");
    }

    public async Task<bool> DesaffecterAsync(int idAffectation)
    {
        using var db = new MySqlConnection(_cs);
        var p = new DynamicParameters();
        p.Add("p_id", idAffectation);
        p.Add("p_ok", dbType: System.Data.DbType.Int32,
              direction: System.Data.ParameterDirection.Output);

        await db.ExecuteAsync("SP_DESAFFECTER_CAISSIER", p, commandType: CommandType.StoredProcedure);
        return p.Get<int>("p_ok") > 0;
    }
}
