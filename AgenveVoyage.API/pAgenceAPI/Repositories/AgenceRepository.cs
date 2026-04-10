using Dapper;
using MySqlConnector;  // ← ← ← CHANGÉ (au lieu de MySql.Data.MySqlClient)
using pAgenceAPI.Models;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace pAgenceAPI.Repositories
{
    public class AgenceRepository : IAgenceRepository
    {
        private readonly string _connectionString;

        public AgenceRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<AgenceModel>> GetAllAsync()
        {
            await using var connection = new MySqlConnection(_connectionString);
            return (await connection.QueryAsync<AgenceModel>("SELECT * FROM AGENCE ORDER BY Nom_Agence")).ToList();
        }

        public async Task<AgenceModel> GetByIdAsync(int id)
        {
            await using var connection = new MySqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<AgenceModel>(
                "SELECT * FROM AGENCE WHERE ID_AGENCE = @Id", new { Id = id });
        }

        public async Task<string> AddAsync(AgenceModel agence)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.ExecuteAsync(
                "INSERT INTO AGENCE (Nom_Agence, Ville, Adresse, Telephone, Email, Date_Creation) VALUES (@Nom_Agence, @Ville, @Adresse, @Telephone, @Email, @Date_Creation)",
                agence);
            return "Agence ajoutée avec succès";
        }

        public async Task<string> UpdateAsync(AgenceModel agence)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE AGENCE SET Nom_Agence = @Nom_Agence, Ville = @Ville, Adresse = @Adresse, Telephone = @Telephone, Email = @Email, Date_Creation = @Date_Creation WHERE ID_AGENCE = @Id_Agence",
                agence);
            return "Agence modifiée avec succès";
        }

        public async Task<string> DeleteAsync(int id)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.ExecuteAsync("DELETE FROM AGENCE WHERE ID_AGENCE = @Id", new { Id = id });
            return "Agence supprimée avec succès";
        }
    }
}