using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class TypeVehiculeRepository : ITypeVehiculeRepository
    {
        private readonly string? _connectionString;

        public TypeVehiculeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<TypeVehiculeModel>> GetAllAsync()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                return (await connection.QueryAsync<TypeVehiculeModel>(
                    "SELECT * FROM TYPE_VEHICULE ORDER BY LIBELLE_TYPE"
                )).ToList();
            }
        }

        public async Task<TypeVehiculeModel> GetByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<TypeVehiculeModel>(
                    "SELECT * FROM TYPE_VEHICULE WHERE ID_TYPE = @Id",
                    new { Id = id }
                );
            }
        }

        public async Task<string> AddAsync(TypeVehiculeModel type)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "INSERT INTO TYPE_VEHICULE (LIBELLE_TYPE) VALUES (@Libelle)",
                    new { Libelle = type.Libelle_Type }
                );
                return "Type de véhicule ajouté avec succès !";
            }
        }

        public async Task<string> UpdateAsync(TypeVehiculeModel type)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "UPDATE TYPE_VEHICULE SET LIBELLE_TYPE = @Libelle WHERE ID_TYPE = @Id",
                    new { Id = type.Id_Type, Libelle = type.Libelle_Type }
                );
                return "Type de véhicule modifié avec succès !";
            }
        }

        public async Task<string> DeleteAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "DELETE FROM TYPE_VEHICULE WHERE ID_TYPE = @Id",
                    new { Id = id }
                );
                return "Type de véhicule supprimé avec succès !";
            }
        }
    }
}