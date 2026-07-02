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
            SELECT ID_privilege AS Id_Privilege, ID_groupe AS Id_Groupe, Module, Action, Autorise
            FROM privilege WHERE ID_groupe = @Id ORDER BY Module, Action", new { Id = idGroupe });
    }

    public async Task SauvegarderAsync(int idGroupe, IEnumerable<PrivilegeModel> privileges)
    {
        using var c = new MySqlConnection(_cs);
        await c.ExecuteAsync("DELETE FROM privilege WHERE ID_groupe = @Id", new { Id = idGroupe });
        foreach (var p in privileges.Where(p => p.Autorise))
        {
            await c.ExecuteAsync(@"
                INSERT INTO privilege (ID_groupe, Module, Action, Autorise)
                VALUES (@IdGroupe, @Module, @Action, 1)",
                new { IdGroupe = idGroupe, p.Module, p.Action });
        }
    }
}
