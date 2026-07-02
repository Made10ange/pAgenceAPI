using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public class HistoriqueConnexionRepository : IHistoriqueConnexionRepository
{
    private readonly string _cs;
    public HistoriqueConnexionRepository(IConfiguration config)
        => _cs = config.GetConnectionString("DefaultConnection")!;

    public async Task<IEnumerable<HistoriqueConnexionModel>> GetAllAsync(int page, int pageSize)
    {
        using var c = new MySqlConnection(_cs);
        return await c.QueryAsync<HistoriqueConnexionModel>(@"
            SELECT ID_CONNEXION AS Id_Connexion, Id_Utilisateur AS Id_Utilisateur, Login_Tente,
                   Nom_Agent, Statut, Motif_Echec, IP_Address, User_Agent, Date_Connexion
            FROM historique_connexion
            ORDER BY Date_Connexion DESC
            LIMIT @PageSize OFFSET @Offset",
            new { PageSize = pageSize, Offset = (page - 1) * pageSize });
    }

    public async Task<IEnumerable<HistoriqueConnexionModel>> GetEchecsRecentsAsync(int minutes)
    {
        using var c = new MySqlConnection(_cs);
        return await c.QueryAsync<HistoriqueConnexionModel>(@"
            SELECT ID_CONNEXION AS Id_Connexion, Id_Utilisateur AS Id_Utilisateur, Login_Tente,
                   Nom_Agent, Statut, Motif_Echec, IP_Address, User_Agent, Date_Connexion
            FROM historique_connexion
            WHERE Statut = 'Échec'
              AND Date_Connexion >= DATE_SUB(NOW(), INTERVAL @Minutes MINUTE)
            ORDER BY Date_Connexion DESC",
            new { Minutes = minutes });
    }

    public async Task EnregistrerAsync(HistoriqueConnexionModel entry)
    {
        using var c = new MySqlConnection(_cs);
        await c.ExecuteAsync(@"
            INSERT INTO historique_connexion
                (Id_Utilisateur, Login_Tente, Nom_Agent, Statut, Motif_Echec, IP_Address, User_Agent)
            VALUES
                (@Id_Utilisateur, @Login_Tente, @Nom_Agent, @Statut, @Motif_Echec, @IP_Address, @User_Agent)",
            entry);
    }
}

