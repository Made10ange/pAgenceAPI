using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class TypeVoyageRepository : ITypeVoyageRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<TypeVoyageRepository> _logger;

        public TypeVoyageRepository(IConfiguration configuration, ILogger<TypeVoyageRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration), "Connection string is missing");
            _logger = logger;
        }

        public async Task<List<TypeVoyageModel>> GetAllAsync(int? idAgence = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var where = idAgence.HasValue ? " WHERE Id_Agence = @IdAgence" : "";
                return (await connection.QueryAsync<TypeVoyageModel>(
                    "SELECT * FROM TYPE_VOYAGE" + where + " ORDER BY LIBELLE_TYPE_VOYAGE",
                    new { IdAgence = idAgence }
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetAllAsync type voyages");
                throw;
            }
        }

        public async Task<List<TypeVoyageModel>> SearchAsync(string motCle, int? idAgence = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var pattern = $"%{motCle.Trim()}%";
                var where = idAgence.HasValue ? " AND Id_Agence = @IdAgence" : "";
                return (await connection.QueryAsync<TypeVoyageModel>(
                    @"SELECT * FROM TYPE_VOYAGE
                      WHERE LIBELLE_TYPE_VOYAGE LIKE @Pattern" + where + @"
                      ORDER BY LIBELLE_TYPE_VOYAGE",
                    new { Pattern = pattern, IdAgence = idAgence }
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur SearchAsync type voyages motCle={MotCle}", motCle);
                throw;
            }
        }

        public async Task<TypeVoyageModel?> GetByIdAsync(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return await connection.QueryFirstOrDefaultAsync<TypeVoyageModel>(
                    "SELECT * FROM TYPE_VOYAGE WHERE ID_TYPE_VOYAGE = @Id",
                    new { Id = id }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetByIdAsync type voyage id={Id}", id);
                throw;
            }
        }

        public async Task<string> AddAsync(TypeVoyageModel type)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    @"INSERT INTO TYPE_VOYAGE (LIBELLE_TYPE_VOYAGE, POINT_DEPART, POINT_ARRIVEE, PRIX, Id_Agence)
                      VALUES (@Libelle, @Depart, @Arrivee, @Prix, @IdAgence)",
                    new { Libelle = type.Libelle_Type_Voyage, Depart = type.Point_Depart, Arrivee = type.Point_Arrivee, Prix = type.Prix, IdAgence = type.Id_Agence }
                );
                return "Type de voyage ajouté avec succès !";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur AddAsync type voyage");
                throw;
            }
        }

        public async Task<string> UpdateAsync(TypeVoyageModel type)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    @"UPDATE TYPE_VOYAGE
                      SET LIBELLE_TYPE_VOYAGE = @Libelle, POINT_DEPART = @Depart, POINT_ARRIVEE = @Arrivee, PRIX = @Prix
                      WHERE ID_TYPE_VOYAGE = @Id",
                    new { Id = type.Id_Type_Voyage, Libelle = type.Libelle_Type_Voyage, Depart = type.Point_Depart, Arrivee = type.Point_Arrivee, Prix = type.Prix }
                );
                return "Type de voyage modifié avec succès !";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur UpdateAsync type voyage id={Id}", type.Id_Type_Voyage);
                throw;
            }
        }

        public async Task<string> DeleteAsync(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    "DELETE FROM TYPE_VOYAGE WHERE ID_TYPE_VOYAGE = @Id",
                    new { Id = id }
                );
                return "Type de voyage supprimé avec succès !";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur DeleteAsync type voyage id={Id}", id);
                throw;
            }
        }
    }
}
