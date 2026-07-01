using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class VehiculeRepository : IVehiculeRepository
    {
        private readonly string _connectionString;

        public VehiculeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration), "Connection string is missing");
        }

        private const string BaseSql =
            @"SELECT V.ID_VEHICULE, V.ID_TYPE, V.IMMATRICULATION, V.STATUT, V.ETAT,
                     TV.LIBELLE_TYPE as Libelle_Type, TV.MARQUE as Marque,
                     TV.NOMBRE_PLACE as Nombre_Place, TV.ID_TYPE_VOYAGE as Id_Type_Voyage,
                     TY.LIBELLE_TYPE_VOYAGE as Libelle_Type_Voyage
              FROM VEHICULE V
              LEFT JOIN TYPE_VEHICULE TV ON V.ID_TYPE = TV.ID_TYPE
              LEFT JOIN TYPE_VOYAGE TY ON TY.ID_TYPE_VOYAGE = TV.ID_TYPE_VOYAGE";

        public async Task<List<VehiculeModel>> GetAllAsync(int? idAgence = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            var where = idAgence.HasValue ? " WHERE V.Id_Agence = @IdAgence" : "";
            return (await connection.QueryAsync<VehiculeModel>(
                BaseSql + where + " ORDER BY V.IMMATRICULATION",
                new { IdAgence = idAgence }
            )).ToList();
        }

        public async Task<VehiculeModel?> GetByIdAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<VehiculeModel>(
                BaseSql + " WHERE V.ID_VEHICULE = @Id",
                new { Id = id }
            );
        }

        public async Task<string> AddAsync(VehiculeModel vehicule)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.ExecuteAsync(
                "INSERT INTO VEHICULE (ID_TYPE, IMMATRICULATION, STATUT, ETAT, Id_Agence) VALUES (@Id_Type, @Immatriculation, @Statut, @Etat, @Id_Agence)",
                new
                {
                    vehicule.Id_Type,
                    vehicule.Immatriculation,
                    Statut    = vehicule.Statut    ?? "Disponible",
                    Etat      = vehicule.Etat      ?? "Bon",
                    Id_Agence = vehicule.Id_Agence.HasValue ? (object)vehicule.Id_Agence.Value : DBNull.Value
                }
            );
            return "Véhicule ajouté avec succès !";
        }

        public async Task<string> UpdateAsync(VehiculeModel vehicule)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE VEHICULE SET ID_TYPE = @Id_Type, IMMATRICULATION = @Immatriculation, STATUT = @Statut, ETAT = @Etat WHERE ID_VEHICULE = @Id",
                new
                {
                    Id     = vehicule.Id_Vehicule,
                    vehicule.Id_Type,
                    vehicule.Immatriculation,
                    Statut = vehicule.Statut ?? "Disponible",
                    Etat   = vehicule.Etat   ?? "Bon"
                }
            );
            return "Véhicule modifié avec succès !";
        }

        public async Task<string> DeleteAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "DELETE FROM VEHICULE WHERE ID_VEHICULE = @Id",
                    new { Id = id }
                );
                return "Véhicule supprimé avec succès !";
            }
        }
        // ✅ AJOUTE CETTE MÉTHODE À LA FIN DU FICHIER
        public async Task<List<VehiculeModel>> GetByStatutAsync(string statut)
        {
            using var connection = new MySqlConnection(_connectionString);
            return (await connection.QueryAsync<VehiculeModel>(
                BaseSql + " WHERE V.STATUT = @Statut ORDER BY V.IMMATRICULATION",
                new { Statut = statut }
            )).ToList();
        }
    }
}