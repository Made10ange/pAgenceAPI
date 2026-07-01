using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class AgenceRepository : IAgenceRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<AgenceRepository> _logger;

        private const string BaseSelectSql =
            @"SELECT ID_AGENCE AS Id_Agence, NOM_AGENCE AS Nom_Agence, VILLE, ADRESSE,
                     TELEPHONE, DATE_CREATION AS Date_Creation
              FROM AGENCE";

        public AgenceRepository(IConfiguration configuration, ILogger<AgenceRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration), "Connection string is missing");
            _logger = logger;
        }

        public async Task<List<AgenceModel>> GetAllAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return (await connection.QueryAsync<AgenceModel>(BaseSelectSql + " ORDER BY NOM_AGENCE")).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetAllAsync agences");
                throw;
            }
        }

        public async Task<List<AgenceModel>> SearchAsync(string motCle)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var pattern = $"%{motCle.Trim()}%";
                return (await connection.QueryAsync<AgenceModel>(
                    BaseSelectSql + @"
                      WHERE NOM_AGENCE LIKE @Pattern
                         OR VILLE LIKE @Pattern
                         OR ADRESSE LIKE @Pattern
                      ORDER BY NOM_AGENCE",
                    new { Pattern = pattern }
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur SearchAsync agences motCle={MotCle}", motCle);
                throw;
            }
        }

        public async Task<AgenceModel?> GetByIdAsync(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return await connection.QueryFirstOrDefaultAsync<AgenceModel>(
                    BaseSelectSql + " WHERE ID_AGENCE = @Id",
                    new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetByIdAsync agence id={Id}", id);
                throw;
            }
        }

        public async Task<string> AddAsync(AgenceModel agence)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    @"INSERT INTO AGENCE (NOM_AGENCE, VILLE, ADRESSE, TELEPHONE, DATE_CREATION)
                      VALUES (@Nom_Agence, @Ville, @Adresse, @Telephone, @Date_Creation)",
                    new
                    {
                        agence.Nom_Agence,
                        agence.Ville,
                        agence.Adresse,
                        agence.Telephone,
                        agence.Date_Creation
                    });
                return "Agence ajoutée avec succès !";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur AddAsync agence");
                throw;
            }
        }

        public async Task<string> UpdateAsync(AgenceModel agence)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    @"UPDATE AGENCE
                      SET NOM_AGENCE = @Nom_Agence, VILLE = @Ville, ADRESSE = @Adresse,
                          TELEPHONE = @Telephone, DATE_CREATION = @Date_Creation
                      WHERE ID_AGENCE = @Id_Agence",
                    new
                    {
                        agence.Id_Agence,
                        agence.Nom_Agence,
                        agence.Ville,
                        agence.Adresse,
                        agence.Telephone,
                        agence.Date_Creation
                    });
                return "Agence modifiée avec succès !";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur UpdateAsync agence id={Id}", agence.Id_Agence);
                throw;
            }
        }

        public async Task<string> DeleteAsync(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    "DELETE FROM AGENCE WHERE ID_AGENCE = @Id",
                    new { Id = id });
                return "Agence supprimée avec succès !";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur DeleteAsync agence id={Id}", id);
                throw;
            }
        }
    }
}
