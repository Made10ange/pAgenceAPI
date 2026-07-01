using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public class BalanceRepository : IBalanceRepository
{
    private readonly string _cs;

    public BalanceRepository(IConfiguration config)
    {
        _cs = config.GetConnectionString("DefaultConnection") ?? "";
    }

    public async Task<List<BalanceGeneraleModel>> GetBalanceGeneraleAsync(
        DateTime dateDebut, DateTime dateFin, int? idAgence)
    {
        using var db = new MySqlConnection(_cs);
        var p = new DynamicParameters();
        p.Add("p_date_debut", dateDebut.Date);
        p.Add("p_date_fin",   dateFin.Date);
        p.Add("p_id_agence",  idAgence);

        var rows = await db.QueryAsync<BalanceGeneraleModel>(
            "sp_balance_generale", p,
            commandType: System.Data.CommandType.StoredProcedure);

        return rows.ToList();
    }
}
