using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public class PrivilegeRepository : IPrivilegeRepository
{
    private readonly string _cs;
    public PrivilegeRepository(IConfiguration config)
        => _cs = config.GetConnectionString("DefaultConnection")!;

    public async Task<IEnumerable<PrivilegeModel>> GetByGroupeAsync(int idGroupe)
    {
        using var c = new MySqlConnection(_cs);
        return await c.QueryAsync<PrivilegeModel>(@"
            SELECT ID_PRIVILEGE AS Id_Privilege, ID_GROUPE AS Id_Groupe, Module, Action, Autorise
            FROM PRIVILEGE WHERE ID_GROUPE = @Id ORDER BY Module, Action", new { Id = idGroupe });
    }

    public async Task SauvegarderAsync(int idGroupe, IEnumerable<PrivilegeModel> privileges)
    {
        using var c = new MySqlConnection(_cs);
        await c.ExecuteAsync("DELETE FROM PRIVILEGE WHERE ID_GROUPE = @Id", new { Id = idGroupe });
        foreach (var p in privileges.Where(p => p.Autorise))
        {
            await c.ExecuteAsync(@"
                INSERT INTO PRIVILEGE (ID_GROUPE, Module, Action, Autorise)
                VALUES (@IdGroupe, @Module, @Action, 1)",
                new { IdGroupe = idGroupe, p.Module, p.Action });
        }
    }
}
