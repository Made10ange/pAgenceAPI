using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public class TransfertCaisseRepository : ITransfertCaisseRepository
{
    private readonly string _cs;
    private readonly ILogger<TransfertCaisseRepository> _logger;

    private const string SQL_BASE = @"
        SELECT t.*,
               cd.libelle AS libelle_caisse_depart, cd.numcompte AS numcompte_depart,
               ca.libelle AS libelle_caisse_dest,   ca.numcompte AS numcompte_dest
        FROM   transfert_caisse t
        JOIN   caisse cd ON cd.id_caisse = t.id_caisse_depart
        JOIN   caisse ca ON ca.id_caisse = t.id_caisse_dest";

    public TransfertCaisseRepository(IConfiguration config, ILogger<TransfertCaisseRepository> logger)
    {
        _cs     = config.GetConnectionString("DefaultConnection") ?? "";
        _logger = logger;
    }

    public async Task<List<TransfertCaisseModel>> GetEnAttenteAsync(int? idAgence = null)
    {
        // Un transfert appartient à l'agence si l'une de ses deux caisses en fait partie.
        // Les caisses non encore rattachées (code_agence NULL) restent visibles partout.
        var where = " WHERE t.statut = 'En attente'";
        if (idAgence.HasValue)
            where += " AND (cd.code_agence IS NULL OR cd.code_agence = @idAgence OR ca.code_agence IS NULL OR ca.code_agence = @idAgence)";

        using var db = new MySqlConnection(_cs);
        return (await db.QueryAsync<TransfertCaisseModel>(
            SQL_BASE + where + " ORDER BY t.date_init DESC", new { idAgence })).ToList();
    }

    public async Task<List<TransfertCaisseModel>> GetHistoriqueAsync(DateTime? dateDebut = null, DateTime? dateFin = null, int? idAgence = null)
    {
        var where = " WHERE 1=1";
        if (dateDebut.HasValue) where += " AND DATE(t.date_init) >= DATE(@dateDebut)";
        if (dateFin.HasValue)   where += " AND DATE(t.date_init) <= DATE(@dateFin)";
        if (idAgence.HasValue)
            where += " AND (cd.code_agence IS NULL OR cd.code_agence = @idAgence OR ca.code_agence IS NULL OR ca.code_agence = @idAgence)";

        using var db = new MySqlConnection(_cs);
        return (await db.QueryAsync<TransfertCaisseModel>(
            SQL_BASE + where + " ORDER BY t.date_init DESC",
            new { dateDebut, dateFin, idAgence })).ToList();
    }

    public async Task<TransfertCaisseModel?> GetByIdAsync(int id)
    {
        using var db = new MySqlConnection(_cs);
        return await db.QueryFirstOrDefaultAsync<TransfertCaisseModel>(
            SQL_BASE + " WHERE t.id_transfert = @id", new { id });
    }

    public async Task<int> InitierAsync(TransfertCaisseModel model, int? codeUser)
    {
        using var db = new MySqlConnection(_cs);
        var p = new DynamicParameters();
        p.Add("p_id_caisse_depart", model.id_caisse_depart);
        p.Add("p_id_caisse_dest",   model.id_caisse_dest);
        p.Add("p_montant",          model.montant);
        p.Add("p_date",             model.date_transfert.Date);
        p.Add("p_num_piece",        model.num_piece);
        p.Add("p_motif",            model.motif);
        p.Add("p_code_user",        codeUser ?? 0);
        p.Add("p_id_transfert",     dbType: System.Data.DbType.Int32,
              direction: System.Data.ParameterDirection.Output);
        p.Add("p_message",          dbType: System.Data.DbType.String, size: 200,
              direction: System.Data.ParameterDirection.Output);

        await db.ExecuteAsync("sp_initier_transfert", p,
            commandType: System.Data.CommandType.StoredProcedure);

        return p.Get<int>("p_id_transfert");
    }

    public async Task ValiderAsync(int idTransfert, int? codeUser)
    {
        using var db = new MySqlConnection(_cs);
        var p = new DynamicParameters();
        p.Add("p_id_transfert", idTransfert);
        p.Add("p_code_user",    codeUser ?? 0);
        p.Add("p_message",      dbType: System.Data.DbType.String, size: 200,
              direction: System.Data.ParameterDirection.Output);

        await db.ExecuteAsync("sp_valider_transfert", p,
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task AnnulerAsync(int idTransfert, int? codeUser)
    {
        using var db = new MySqlConnection(_cs);
        var p = new DynamicParameters();
        p.Add("p_id_transfert", idTransfert);
        p.Add("p_code_user",    codeUser ?? 0);
        p.Add("p_message",      dbType: System.Data.DbType.String, size: 200,
              direction: System.Data.ParameterDirection.Output);

        await db.ExecuteAsync("sp_annuler_transfert", p,
            commandType: System.Data.CommandType.StoredProcedure);
    }
}
