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
                    @"SELECT a.ID_AFFECTATION_vehicule AS Id_Affectation_Vehicule,
                             a.ID_agence AS Id_Agence,
                             a.ID_vehicule AS Id_Vehicule,
                             a.DATE_DEBUT AS Date_Debut,
                             a.DATE_FIN AS Date_Fin,
                             a.STATUT AS Statut,
                             a.OBSERVATIONS AS Observations,
                             COALESCE(vh.IMMATRICULATION, '') AS Immatriculation,
                             COALESCE(tv.MARQUE, '') AS Marque,
                             COALESCE(ag.NOM_agence, '') AS Nom_Agence,
                             COALESCE(ag.VILLE, '') AS Ville
                      FROM affectation_vehicule_agence a
                      LEFT JOIN vehicule vh ON a.ID_vehicule = vh.ID_vehicule
                      LEFT JOIN type_vehicule tv ON vh.ID_TYPE = tv.ID_TYPE
                      LEFT JOIN agence ag ON a.ID_agence = ag.ID_agence
                      ORDER BY a.DATE_DEBUT DESC"
                )).ToList();
            }
        }

        public async Task<List<AffectationVehiculeAgenceModel>> SearchAsync(string motCle)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                return (await connection.QueryAsync<AffectationVehiculeAgenceModel>(
                    @"SELECT a.ID_AFFECTATION_vehicule AS Id_Affectation_Vehicule,
                             a.ID_agence AS Id_Agence,
                             a.ID_vehicule AS Id_Vehicule,
                             a.DATE_DEBUT AS Date_Debut,
                             a.DATE_FIN AS Date_Fin,
                             a.STATUT AS Statut,
                             a.OBSERVATIONS AS Observations,
                             COALESCE(vh.IMMATRICULATION, '') AS Immatriculation,
                             COALESCE(tv.MARQUE, '') AS Marque,
                             COALESCE(ag.NOM_agence, '') AS Nom_Agence,
                             COALESCE(ag.VILLE, '') AS Ville
                      FROM affectation_vehicule_agence a
                      LEFT JOIN vehicule vh ON a.ID_vehicule = vh.ID_vehicule
                      LEFT JOIN type_vehicule tv ON vh.ID_TYPE = tv.ID_TYPE
                      LEFT JOIN agence ag ON a.ID_agence = ag.ID_agence
                      WHERE COALESCE(vh.IMMATRICULATION, '') LIKE @MotCle
                         OR COALESCE(tv.MARQUE, '') LIKE @MotCle
                         OR COALESCE(ag.NOM_agence, '') LIKE @MotCle
                         OR COALESCE(ag.VILLE, '') LIKE @MotCle
                      ORDER BY a.DATE_DEBUT DESC",
                    new { MotCle = $"%{motCle}%" }
                )).ToList();
            }
        }

        public async Task<AffectationVehiculeAgenceModel?> GetByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<AffectationVehiculeAgenceModel>(
                    @"SELECT a.ID_AFFECTATION_vehicule AS Id_Affectation_Vehicule,
                             a.ID_agence AS Id_Agence,
                             a.ID_vehicule AS Id_Vehicule,
                             a.DATE_DEBUT AS Date_Debut,
                             a.DATE_FIN AS Date_Fin,
                             a.STATUT AS Statut,
                             a.OBSERVATIONS AS Observations,
                             COALESCE(vh.IMMATRICULATION, '') AS Immatriculation,
                             COALESCE(tv.MARQUE, '') AS Marque,
                             COALESCE(ag.NOM_agence, '') AS Nom_Agence,
                             COALESCE(ag.VILLE, '') AS Ville
                      FROM affectation_vehicule_agence a
                      LEFT JOIN vehicule vh ON a.ID_vehicule = vh.ID_vehicule
                      LEFT JOIN type_vehicule tv ON vh.ID_TYPE = tv.ID_TYPE
                      LEFT JOIN agence ag ON a.ID_agence = ag.ID_agence
                      WHERE a.ID_AFFECTATION_vehicule = @Id",
                    new { Id = id }
                );
            }
        }

        public async Task<string> AddAsync(AffectationVehiculeAgenceModel affectation)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO affectation_vehicule_agence (ID_agence, ID_vehicule, DATE_DEBUT, DATE_FIN, STATUT, OBSERVATIONS)
                      VALUES (@Id_Agence, @Id_Vehicule, @Date_Debut, @Date_Fin, @Statut, @Observations)",
                    new
                    {
                        Id_Agence = affectation.Id_Agence,
                        Id_Vehicule = affectation.Id_Vehicule,
                        Date_Debut = affectation.Date_Debut,
                        Date_Fin = affectation.Date_Fin.HasValue ? (object)affectation.Date_Fin.Value : DBNull.Value,
                        Statut = affectation.Statut ?? "Active",
                        Observations = affectation.Observations ?? (object)DBNull.Value
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
                    @"UPDATE affectation_vehicule_agence 
                      SET ID_agence = @Id_Agence, ID_vehicule = @Id_Vehicule, 
                          DATE_DEBUT = @Date_Debut, DATE_FIN = @Date_Fin,
                          STATUT = @Statut, OBSERVATIONS = @Observations
                      WHERE ID_AFFECTATION_vehicule = @Id",
                    new
                    {
                        Id = affectation.Id_Affectation_Vehicule,
                        Id_Agence = affectation.Id_Agence,
                        Id_Vehicule = affectation.Id_Vehicule,
                        Date_Debut = affectation.Date_Debut,
                        Date_Fin = affectation.Date_Fin.HasValue ? (object)affectation.Date_Fin.Value : DBNull.Value,
                        Statut = affectation.Statut ?? "Active",
                        Observations = affectation.Observations ?? (object)DBNull.Value
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
                    "DELETE FROM affectation_vehicule_agence WHERE ID_AFFECTATION_vehicule = @Id",
                    new { Id = id }
                );
                return "Affectation véhicule-agence supprimée avec succès !";
            }
        }
    }
}