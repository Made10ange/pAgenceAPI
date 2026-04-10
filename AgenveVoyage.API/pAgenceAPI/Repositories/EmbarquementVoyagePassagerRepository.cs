using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class EmbarquementVoyagePassagerRepository : IEmbarquementVoyagePassagerRepository
    {
        private readonly string? _connectionString;

        public EmbarquementVoyagePassagerRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<EmbarquementVoyagePassagerModel>> GetAllAsync()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                return (await connection.QueryAsync<EmbarquementVoyagePassagerModel>(
                    "SELECT * FROM EMBARQUEMENT_VOYAGE_PASSAGER ORDER BY DATE_ENREGISTREMENT"
                )).ToList();
            }
        }

        public async Task<EmbarquementVoyagePassagerModel> GetByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<EmbarquementVoyagePassagerModel>(
                    "SELECT * FROM EMBARQUEMENT_VOYAGE_PASSAGER WHERE ID_EMBARQUEMENT = @Id",
                    new { Id = id }
                );
            }
        }

        public async Task<string> AddAsync(EmbarquementVoyagePassagerModel embarquement)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO EMBARQUEMENT_VOYAGE_PASSAGER (ID_VOYAGE, ID_PASSAGER, 
                                                                  STATUT_EMBARQUEMENT, NUMERO_SIEGE, DATE_ENREGISTREMENT)
                      VALUES (@Id_Voyage, @Id_Passager, 
                              @Statut_Embarquement, @Numero_Siege, @Date_Enregistrement)",
                    new
                    {
                        Id_Voyage = embarquement.Id_Voyage,
                        Id_Passager = embarquement.Id_Passager,
                        Statut_Embarquement = embarquement.Statut_Embarquement ?? "Confirmé",
                        Numero_Siege = embarquement.Numero_Siege,
                        Date_Enregistrement = embarquement.Date_Enregistrement
                    }
                );
                return "Embarquement ajouté avec succès !";
            }
        }

        public async Task<string> UpdateAsync(EmbarquementVoyagePassagerModel embarquement)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    @"UPDATE EMBARQUEMENT_VOYAGE_PASSAGER 
                      SET ID_VOYAGE = @Id_Voyage, ID_PASSAGER = @Id_Passager, 
                          STATUT_EMBARQUEMENT = @Statut_Embarquement, NUMERO_SIEGE = @Numero_Siege, 
                          DATE_ENREGISTREMENT = @Date_Enregistrement
                      WHERE ID_EMBARQUEMENT = @Id",
                    new
                    {
                        Id = embarquement.Id_Embarquement,
                        Id_Voyage = embarquement.Id_Voyage,
                        Id_Passager = embarquement.Id_Passager,
                        Statut_Embarquement = embarquement.Statut_Embarquement ?? "Confirmé",
                        Numero_Siege = embarquement.Numero_Siege,
                        Date_Enregistrement = embarquement.Date_Enregistrement
                    }
                );
                return "Embarquement modifié avec succès !";
            }
        }

        public async Task<string> DeleteAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "DELETE FROM EMBARQUEMENT_VOYAGE_PASSAGER WHERE ID_EMBARQUEMENT = @Id",
                    new { Id = id }
                );
                return "Embarquement supprimé avec succès !";
            }
        }
    }
}