using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class VehiculeRepository : IVehiculeRepository
    {
        private readonly string? _connectionString;

        public VehiculeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<VehiculeModel>> GetAllAsync()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                return (await connection.QueryAsync<VehiculeModel>(
                    @"SELECT V.*, TV.LIBELLE_TYPE as Libelle_Type, TV.MARQUE as Marque, TV.NOMBRE_PLACE as Nombre_Place 
              FROM VEHICULE V 
              LEFT JOIN TYPE_VEHICULE TV ON V.ID_TYPE = TV.ID_TYPE 
              ORDER BY V.IMMATRICULATION"
                )).ToList();
            }
        }

        public async Task<VehiculeModel?> GetByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<VehiculeModel>(
                    @"SELECT V.*, TV.LIBELLE_TYPE as Libelle_Type, TV.MARQUE as Marque, TV.NOMBRE_PLACE as Nombre_Place 
              FROM VEHICULE V 
              LEFT JOIN TYPE_VEHICULE TV ON V.ID_TYPE = TV.ID_TYPE 
              WHERE V.ID_VEHICULE = @Id",
                    new { Id = id }
                );
            }
        }

        public async Task<string> AddAsync(VehiculeModel vehicule)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "INSERT INTO VEHICULE (ID_TYPE, IMMATRICULATION, STATUT) VALUES (@Id_Type, @Immatriculation, @Statut)",
                    new
                    {
                        Id_Type = vehicule.Id_Type,
                        Immatriculation = vehicule.Immatriculation,
                        Statut = vehicule.Statut ?? "Disponible"
                    }
                );
                return "Véhicule ajouté avec succès !";
            }
        }

        public async Task<string> UpdateAsync(VehiculeModel vehicule)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "UPDATE VEHICULE SET ID_TYPE = @Id_Type, IMMATRICULATION = @Immatriculation, STATUT = @Statut WHERE ID_VEHICULE = @Id",
                    new
                    {
                        Id = vehicule.Id_Vehicule,
                        Id_Type = vehicule.Id_Type,
                        Immatriculation = vehicule.Immatriculation,
                        Statut = vehicule.Statut ?? "Disponible"
                    }
                );
                return "Véhicule modifié avec succès !";
            }
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
            var sql = @"SELECT V.*, TV.LIBELLE_TYPE as Libelle_Type, TV.MARQUE as Marque, TV.NOMBRE_PLACE as Nombre_Place 
               FROM VEHICULE V 
               LEFT JOIN TYPE_VEHICULE TV ON V.ID_TYPE = TV.ID_TYPE 
               WHERE V.STATUT = @Statut 
               ORDER BY V.IMMATRICULATION";
            return (await connection.QueryAsync<VehiculeModel>(sql, new { Statut = statut })).ToList();
        }
    }
}