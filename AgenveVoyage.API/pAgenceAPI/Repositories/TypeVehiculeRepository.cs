using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class TypeVehiculeRepository : ITypeVehiculeRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<TypeVehiculeRepository> _logger;

        public TypeVehiculeRepository(IConfiguration configuration, ILogger<TypeVehiculeRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration), "Connection string is missing");
            _logger = logger;
        }

        private const string BaseSelect = @"
            SELECT tv.ID_TYPE AS Id_Type, tv.LIBELLE_TYPE AS Libelle_Type,
                   tv.MARQUE AS Marque, tv.NOMBRE_PLACE AS Nombre_Place,
                   tv.ID_type_voyage AS Id_Type_Voyage, tv.Id_Agence AS Id_Agence,
                   ty.LIBELLE_type_voyage AS Libelle_Type_Voyage
            FROM type_vehicule tv
            LEFT JOIN type_voyage ty ON ty.ID_type_voyage = tv.ID_type_voyage ";

        public async Task<List<TypeVehiculeModel>> GetAllAsync(int? idAgence = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var where = idAgence.HasValue ? "WHERE tv.Id_Agence = @IdAgence " : "";
                return (await connection.QueryAsync<TypeVehiculeModel>(
                    BaseSelect + where + "ORDER BY tv.LIBELLE_TYPE",
                    new { IdAgence = idAgence }
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetAllAsync type véhicules");
                throw;
            }
        }

        public async Task<List<TypeVehiculeModel>> SearchAsync(string motCle, int? idAgence = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var pattern = $"%{motCle.Trim()}%";
                var where = idAgence.HasValue ? " AND tv.Id_Agence = @IdAgence" : "";
                return (await connection.QueryAsync<TypeVehiculeModel>(
                    BaseSelect + "WHERE (tv.LIBELLE_TYPE LIKE @Pattern OR tv.MARQUE LIKE @Pattern)" + where + " ORDER BY tv.LIBELLE_TYPE",
                    new { Pattern = pattern, IdAgence = idAgence }
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur SearchAsync type véhicules motCle={MotCle}", motCle);
                throw;
            }
        }

        public async Task<TypeVehiculeModel?> GetByIdAsync(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return await connection.QueryFirstOrDefaultAsync<TypeVehiculeModel>(
                    BaseSelect + "WHERE tv.ID_TYPE = @Id",
                    new { Id = id }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetByIdAsync type véhicule id={Id}", id);
                throw;
            }
        }

        public async Task<string> AddAsync(TypeVehiculeModel type)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    @"INSERT INTO type_vehicule (LIBELLE_TYPE, MARQUE, NOMBRE_PLACE, ID_type_voyage, Id_Agence)
                      VALUES (@Libelle, @Marque, @Places, @IdTypeVoyage, @IdAgence)",
                    new { Libelle = type.Libelle_Type, Marque = type.Marque,
                          Places = type.Nombre_Place, IdTypeVoyage = type.Id_Type_Voyage, IdAgence = type.Id_Agence }
                );
                return "Type de véhicule ajouté avec succès !";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur AddAsync type véhicule");
                throw;
            }
        }

        public async Task<string> UpdateAsync(TypeVehiculeModel type)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    @"UPDATE type_vehicule
                      SET LIBELLE_TYPE = @Libelle, MARQUE = @Marque,
                          NOMBRE_PLACE = @Places, ID_type_voyage = @IdTypeVoyage
                      WHERE ID_TYPE = @Id",
                    new { Id = type.Id_Type, Libelle = type.Libelle_Type, Marque = type.Marque,
                          Places = type.Nombre_Place, IdTypeVoyage = type.Id_Type_Voyage }
                );
                return "Type de véhicule modifié avec succès !";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur UpdateAsync type véhicule id={Id}", type.Id_Type);
                throw;
            }
        }

        public async Task<string> DeleteAsync(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    "DELETE FROM type_vehicule WHERE ID_TYPE = @Id",
                    new { Id = id }
                );
                return "Type de véhicule supprimé avec succès !";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur DeleteAsync type véhicule id={Id}", id);
                throw;
            }
        }
    }
}
