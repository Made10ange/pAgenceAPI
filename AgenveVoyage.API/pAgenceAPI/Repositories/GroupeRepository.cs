using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public class GroupeRepository : IGroupeRepository
{
    private readonly string _cs;
    public GroupeRepository(IConfiguration config)
        => _cs = config.GetConnectionString("DefaultConnection")!;

    public async Task<IEnumerable<GroupeModel>> GetAllAsync()
    {
        using var c = new MySqlConnection(_cs);
        return await c.QueryAsync<GroupeModel>(@"
            SELECT g.ID_GROUPE AS Id_Groupe, g.Libelle, g.Description, g.Couleur, g.Actif, g.Date_Creation,
                   COUNT(ag.Id_Utilisateur) AS Nb_Agents
            FROM GROUPE g
            LEFT JOIN UTILISATEUR_GROUPE ag ON g.ID_GROUPE = ag.ID_GROUPE
            GROUP BY g.ID_GROUPE ORDER BY g.Libelle");
    }

    public async Task<GroupeModel?> GetByIdAsync(int id)
    {
        using var c = new MySqlConnection(_cs);
        return await c.QueryFirstOrDefaultAsync<GroupeModel>(@"
            SELECT g.ID_GROUPE AS Id_Groupe, g.Libelle, g.Description, g.Couleur, g.Actif, g.Date_Creation,
                   COUNT(ag.Id_Utilisateur) AS Nb_Agents
            FROM GROUPE g
            LEFT JOIN UTILISATEUR_GROUPE ag ON g.ID_GROUPE = ag.ID_GROUPE
            WHERE g.ID_GROUPE = @Id GROUP BY g.ID_GROUPE", new { Id = id });
    }

    public async Task<int> AddAsync(GroupeModel groupe)
    {
        using var c = new MySqlConnection(_cs);
        return await c.ExecuteScalarAsync<int>(@"
            INSERT INTO GROUPE (Libelle, Description, Couleur, Actif)
            VALUES (@Libelle, @Description, @Couleur, @Actif);
            SELECT LAST_INSERT_ID();",
            new { groupe.Libelle, groupe.Description, groupe.Couleur, groupe.Actif });
    }

    public async Task<bool> UpdateAsync(int id, GroupeModel groupe)
    {
        using var c = new MySqlConnection(_cs);
        var rows = await c.ExecuteAsync(@"
            UPDATE GROUPE SET Libelle=@Libelle, Description=@Description,
            Couleur=@Couleur, Actif=@Actif WHERE ID_GROUPE=@Id",
            new { groupe.Libelle, groupe.Description, groupe.Couleur, groupe.Actif, Id = id });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var c = new MySqlConnection(_cs);
        var rows = await c.ExecuteAsync("DELETE FROM GROUPE WHERE ID_GROUPE=@Id", new { Id = id });
        return rows > 0;
    }

    public async Task<IEnumerable<AgentGroupeModel>> GetAgentsAsync(int idGroupe)
    {
        using var c = new MySqlConnection(_cs);
        return await c.QueryAsync<AgentGroupeModel>(@"
            SELECT ag.Id_Utilisateur AS Id_Utilisateur, ag.Id_Groupe AS Id_Groupe, ag.Date_Affectation,
                   a.Nom AS Nom_Agent, a.Prenom AS Prenom_Agent, a.Login AS Login_Agent
            FROM utilisateur_groupe ag
            JOIN utilisateur a ON ag.Id_Utilisateur = a.Id_Utilisateur
            WHERE ag.Id_Groupe = @Id ORDER BY a.Nom", new { Id = idGroupe });
    }

    public async Task<bool> AffecterAgentAsync(int idGroupe, int idAgent)
    {
        using var c = new MySqlConnection(_cs);
        var rows = await c.ExecuteAsync(@"
            INSERT IGNORE INTO UTILISATEUR_GROUPE (Id_Utilisateur, ID_GROUPE) VALUES (@IdAgent, @IdGroupe)",
            new { IdAgent = idAgent, IdGroupe = idGroupe });
        return rows > 0;
    }

    public async Task<bool> RetirerAgentAsync(int idGroupe, int idAgent)
    {
        using var c = new MySqlConnection(_cs);
        var rows = await c.ExecuteAsync(@"
            DELETE FROM UTILISATEUR_GROUPE WHERE Id_Utilisateur=@IdAgent AND ID_GROUPE=@IdGroupe",
            new { IdAgent = idAgent, IdGroupe = idGroupe });
        return rows > 0;
    }
}

