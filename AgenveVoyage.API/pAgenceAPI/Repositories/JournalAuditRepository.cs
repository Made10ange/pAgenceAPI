using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public class JournalAuditRepository : IJournalAuditRepository
{
    private readonly string _cs;
    public JournalAuditRepository(IConfiguration config)
        => _cs = config.GetConnectionString("DefaultConnection")!;

    public async Task<IEnumerable<JournalAuditModel>> GetAllAsync(int page, int pageSize)
    {
        using var c = new MySqlConnection(_cs);
        return await c.QueryAsync<JournalAuditModel>(@"
            SELECT ID_JOURNAL AS Id_Journal, Id_Utilisateur AS Id_Utilisateur, Login_Agent, Nom_Agent,
                   Module, Action, Details, Ancienne_Valeur, Nouvelle_Valeur,
                   IP_Address, User_Agent, Statut, Date_Action
            FROM JOURNAL_AUDIT
            ORDER BY Date_Action DESC
            LIMIT @PageSize OFFSET @Offset",
            new { PageSize = pageSize, Offset = (page - 1) * pageSize });
    }

    public async Task<IEnumerable<JournalAuditModel>> RechercherAsync(
        string? module, string? login, DateTime? dateDebut, DateTime? dateFin)
    {
        using var c = new MySqlConnection(_cs);
        var where = new List<string>();
        if (!string.IsNullOrEmpty(module)) where.Add("Module = @Module");
        if (!string.IsNullOrEmpty(login))  where.Add("Login_Agent LIKE @Login");
        if (dateDebut.HasValue) where.Add("DATE(Date_Action) >= @DateDebut");
        if (dateFin.HasValue)   where.Add("DATE(Date_Action) <= @DateFin");

        var sql = "SELECT ID_JOURNAL AS Id_Journal, Id_Utilisateur AS Id_Utilisateur, Login_Agent, Nom_Agent, " +
                  "Module, Action, Details, Ancienne_Valeur, Nouvelle_Valeur, " +
                  "IP_Address, User_Agent, Statut, Date_Action FROM JOURNAL_AUDIT" +
                  (where.Count > 0 ? " WHERE " + string.Join(" AND ", where) : "") +
                  " ORDER BY Date_Action DESC LIMIT 500";

        return await c.QueryAsync<JournalAuditModel>(sql, new
        {
            Module    = module,
            Login     = $"%{login}%",
            DateDebut = dateDebut?.Date,
            DateFin   = dateFin?.Date
        });
    }

    public async Task EnregistrerAsync(JournalAuditModel entry)
    {
        using var c = new MySqlConnection(_cs);
        await c.ExecuteAsync(@"
            INSERT INTO JOURNAL_AUDIT
                (Id_Utilisateur, Login_Agent, Nom_Agent, Module, Action, Details,
                 Ancienne_Valeur, Nouvelle_Valeur, IP_Address, User_Agent, Statut)
            VALUES
                (@Id_Utilisateur, @Login_Agent, @Nom_Agent, @Module, @Action, @Details,
                 @Ancienne_Valeur, @Nouvelle_Valeur, @IP_Address, @User_Agent, @Statut)",
            entry);
    }
}

