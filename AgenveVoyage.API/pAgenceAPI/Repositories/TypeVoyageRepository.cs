using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class TypeVoyageRepository : ITypeVoyageRepository
    {
        private readonly string? _connectionString;

        public TypeVoyageRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<TypeVoyageModel>> GetAllAsync()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                return (await connection.QueryAsync<TypeVoyageModel>(
                    "SELECT * FROM TYPE_VOYAGE ORDER BY LIBELLE_TYPE_VOYAGE"
                )).ToList();
            }
        }

        public async Task<TypeVoyageModel> GetByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<TypeVoyageModel>(
                    "SELECT * FROM TYPE_VOYAGE WHERE ID_TYPE_VOYAGE = @Id",
                    new { Id = id }
                );
            }
        }

        public async Task<string> AddAsync(TypeVoyageModel type)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "INSERT INTO TYPE_VOYAGE (LIBELLE_TYPE_VOYAGE, PRIX) VALUES (@Libelle, @Prix)",
                    new
                    {
                        Libelle = type.Libelle_Type_Voyage,
                        Prix = type.Prix
                    }
                );
                return "Type de voyage ajouté avec succès !";
            }
        }

        public async Task<string> UpdateAsync(TypeVoyageModel type)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "UPDATE TYPE_VOYAGE SET LIBELLE_TYPE_VOYAGE = @Libelle, PRIX = @Prix WHERE ID_TYPE_VOYAGE = @Id",
                    new
                    {
                        Id = type.Id_Type_Voyage,
                        Libelle = type.Libelle_Type_Voyage,
                        Prix = type.Prix
                    }
                );
                return "Type de voyage modifié avec succès !";
            }
        }

        public async Task<string> DeleteAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "DELETE FROM TYPE_VOYAGE WHERE ID_TYPE_VOYAGE = @Id",
                    new { Id = id }
                );
                return "Type de voyage supprimé avec succès !";
            }
        }
    }
}