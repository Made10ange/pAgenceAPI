using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class AffectationVehiculeAgenceRepository : IAffectationVehiculeAgenceRepository
    {
        private readonly string? _connectionString;

        public AffectationVehiculeAgenceRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<AffectationVehiculeAgenceModel>> GetAllAsync()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                return (await connection.QueryAsync<AffectationVehiculeAgenceModel>(
                    "SELECT * FROM AFFECTATION_VEHICULE_AGENCE ORDER BY DATE_DEBUT"
                )).ToList();
            }
        }

        public async Task<AffectationVehiculeAgenceModel> GetByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<AffectationVehiculeAgenceModel>(
                    "SELECT * FROM AFFECTATION_VEHICULE_AGENCE WHERE ID_AFFECTATION_VEHICULE = @Id",
                    new { Id = id }
                );
            }
        }

        public async Task<string> AddAsync(AffectationVehiculeAgenceModel affectation)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO AFFECTATION_VEHICULE_AGENCE (ID_AGENCE, ID_VEHICULE, DATE_DEBUT, DATE_FIN)
                      VALUES (@Id_Agence, @Id_Vehicule, @Date_Debut, @Date_Fin)",
                    new
                    {
                        Id_Agence = affectation.Id_Agence,
                        Id_Vehicule = affectation.Id_Vehicule,
                        Date_Debut = affectation.Date_Debut,
                        Date_Fin = affectation.Date_Fin
                    }
                );
                return "Affectation véhicule-agence ajoutée avec succès !";
            }
        }

        public async Task<string> UpdateAsync(AffectationVehiculeAgenceModel affectation)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    @"UPDATE AFFECTATION_VEHICULE_AGENCE 
                      SET ID_AGENCE = @Id_Agence, ID_VEHICULE = @Id_Vehicule, 
                          DATE_DEBUT = @Date_Debut, DATE_FIN = @Date_Fin
                      WHERE ID_AFFECTATION_VEHICULE = @Id",
                    new
                    {
                        Id = affectation.Id_Affectation_Vehicule,
                        Id_Agence = affectation.Id_Agence,
                        Id_Vehicule = affectation.Id_Vehicule,
                        Date_Debut = affectation.Date_Debut,
                        Date_Fin = affectation.Date_Fin
                    }
                );
                return "Affectation véhicule-agence modifiée avec succès !";
            }
        }

        public async Task<string> DeleteAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "DELETE FROM AFFECTATION_VEHICULE_AGENCE WHERE ID_AFFECTATION_VEHICULE = @Id",
                    new { Id = id }
                );
                return "Affectation véhicule-agence supprimée avec succès !";
            }
        }
    }
}