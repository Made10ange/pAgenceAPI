#nullable enable
using BCrypt.Net;
using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public class UtilisateurRepository : IUtilisateurRepository
{
    private readonly string _connectionString;

    public UtilisateurRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string manquante.");
    }

    public async Task<UtilisateurModel?> GetByLoginAsync(string login, int? idAgence = null)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = @"
            SELECT a.*, ag.NOM_AGENCE AS Nom_Agence, ag.VILLE AS Ville_Agence
            FROM UTILISATEUR a
            LEFT JOIN AGENCE ag ON ag.ID_AGENCE = a.Id_Agence
            WHERE a.Login = @Login AND a.Actif = 1
              AND (@IdAgence IS NULL OR a.Id_Agence = @IdAgence)";
        return await connection.QueryFirstOrDefaultAsync<UtilisateurModel>(sql,
            new { Login = login, IdAgence = idAgence });
    }

    public async Task<bool> ExisteAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM UTILISATEUR");
        return count > 0;
    }

    public async Task CreerAdminParDefautAsync()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("Admin@2025");
        using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            @"INSERT INTO UTILISATEUR (Nom, Prenom, Login, MotDePasse, Role)
              VALUES ('Administrateur', 'Système', 'admin', @Hash, 'Admin')",
            new { Hash = hash });
    }

    public async Task<IEnumerable<UtilisateurModel>> GetAllAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        return await connection.QueryAsync<UtilisateurModel>(@"
            SELECT a.Id_Utilisateur, a.Nom, a.Prenom, a.Login, a.Role, a.Actif, a.Date_Creation,
                   a.Id_Agence, ag.NOM_AGENCE AS Nom_Agence, ag.VILLE AS Ville_Agence
            FROM UTILISATEUR a
            LEFT JOIN AGENCE ag ON ag.ID_AGENCE = a.Id_Agence
            ORDER BY a.Nom, a.Prenom");
    }

    public async Task<UtilisateurModel?> GetByIdAsync(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<UtilisateurModel>(@"
            SELECT a.Id_Utilisateur, a.Nom, a.Prenom, a.Login, a.Role, a.Actif, a.Date_Creation,
                   a.Id_Agence, ag.NOM_AGENCE AS Nom_Agence, ag.VILLE AS Ville_Agence
            FROM UTILISATEUR a
            LEFT JOIN AGENCE ag ON ag.ID_AGENCE = a.Id_Agence
            WHERE a.Id_Utilisateur = @Id", new { Id = id });
    }

    public async Task<int> AddAsync(UtilisateurModel agent)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(agent.MotDePasse);
        using var connection = new MySqlConnection(_connectionString);
        var id = await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO UTILISATEUR (Nom, Prenom, Login, MotDePasse, Role, Actif, Id_Agence)
              VALUES (@Nom, @Prenom, @Login, @Hash, @Role, @Actif, @Id_Agence);
              SELECT LAST_INSERT_ID();",
            new { agent.Nom, agent.Prenom, Login = agent.Login.Trim().ToLower(), Hash = hash, agent.Role, agent.Actif, agent.Id_Agence });

        if (agent.Role == "Caissier")
        {
            // Crée automatiquement la fiche RH correspondante (poste "Caissière")
            await connection.ExecuteAsync(
                @"INSERT INTO personnel (Nom, Prenom, ID_POSTE, Type_Contrat, Salaire_Base, Date_Embauche, Statut, ID_UTILISATEUR)
                  VALUES (@Nom, @Prenom, 2, 'CDI', 0, CURDATE(), @Statut, @IdUtilisateur)",
                new { agent.Nom, agent.Prenom, Statut = agent.Actif ? "Actif" : "Inactif", IdUtilisateur = id });
        }

        return id;
    }

    public async Task<bool> UpdateAsync(UtilisateurModel agent)
    {
        using var connection = new MySqlConnection(_connectionString);
        var rows = await connection.ExecuteAsync(
            "UPDATE UTILISATEUR SET Nom=@Nom, Prenom=@Prenom, Login=@Login, Role=@Role, Actif=@Actif, Id_Agence=@Id_Agence WHERE Id_Utilisateur=@Id",
            new { agent.Nom, agent.Prenom, Login = agent.Login.Trim().ToLower(), agent.Role, agent.Actif, agent.Id_Agence, Id = agent.Id_Utilisateur });

        if (agent.Role == "Caissier")
        {
            // Synchronise la fiche RH liée, ou la crée si elle n'existait pas encore
            var updated = await connection.ExecuteAsync(
                @"UPDATE personnel SET Nom=@Nom, Prenom=@Prenom, Statut=@Statut WHERE ID_UTILISATEUR=@Id",
                new { agent.Nom, agent.Prenom, Statut = agent.Actif ? "Actif" : "Inactif", Id = agent.Id_Utilisateur });

            if (updated == 0)
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO personnel (Nom, Prenom, ID_POSTE, Type_Contrat, Salaire_Base, Date_Embauche, Statut, ID_UTILISATEUR)
                      VALUES (@Nom, @Prenom, 2, 'CDI', 0, CURDATE(), @Statut, @IdUtilisateur)",
                    new { agent.Nom, agent.Prenom, Statut = agent.Actif ? "Actif" : "Inactif", IdUtilisateur = agent.Id_Utilisateur });
            }
        }

        return rows > 0;
    }

    public async Task<bool> UpdatePasswordAsync(int id, string hashedPassword)
    {
        using var connection = new MySqlConnection(_connectionString);
        var rows = await connection.ExecuteAsync(
            "UPDATE UTILISATEUR SET MotDePasse=@Hash WHERE Id_Utilisateur=@Id",
            new { Hash = hashedPassword, Id = id });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        var rows = await connection.ExecuteAsync("DELETE FROM UTILISATEUR WHERE Id_Utilisateur=@Id", new { Id = id });
        return rows > 0;
    }
}

