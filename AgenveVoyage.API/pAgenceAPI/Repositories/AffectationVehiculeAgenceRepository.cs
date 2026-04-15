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
                    @"SELECT a.ID_AFFECTATION_VEHICULE AS Id_Affectation_Vehicule,
                             a.ID_AGENCE AS Id_Agence,
                             a.ID_VEHICULE AS Id_Vehicule,
                             a.DATE_DEBUT AS Date_Debut,
                             a.DATE_FIN AS Date_Fin,
                             a.STATUT AS Statut,
                             a.OBSERVATIONS AS Observations,
                             COALESCE(vh.IMMATRICULATION, '') AS Immatriculation,
                             COALESCE(tv.MARQUE, '') AS Marque,
                             COALESCE(ag.NOM_AGENCE, '') AS Nom_Agence,
                             COALESCE(ag.VILLE, '') AS Ville
                      FROM AFFECTATION_VEHICULE_AGENCE a
                      LEFT JOIN VEHICULE vh ON a.ID_VEHICULE = vh.ID_VEHICULE
                      LEFT JOIN TYPE_VEHICULE tv ON vh.ID_TYPE = tv.ID_TYPE
                      LEFT JOIN AGENCE ag ON a.ID_AGENCE = ag.ID_AGENCE
                      ORDER BY a.DATE_DEBUT DESC"
                )).ToList();
            }
        }

        public async Task<List<AffectationVehiculeAgenceModel>> SearchAsync(string motCle)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                return (await connection.QueryAsync<AffectationVehiculeAgenceModel>(
                    @"SELECT a.ID_AFFECTATION_VEHICULE AS Id_Affectation_Vehicule,
                             a.ID_AGENCE AS Id_Agence,
                             a.ID_VEHICULE AS Id_Vehicule,
                             a.DATE_DEBUT AS Date_Debut,
                             a.DATE_FIN AS Date_Fin,
                             a.STATUT AS Statut,
                             a.OBSERVATIONS AS Observations,
                             COALESCE(vh.IMMATRICULATION, '') AS Immatriculation,
                             COALESCE(tv.MARQUE, '') AS Marque,
                             COALESCE(ag.NOM_AGENCE, '') AS Nom_Agence,
                             COALESCE(ag.VILLE, '') AS Ville
                      FROM AFFECTATION_VEHICULE_AGENCE a
                      LEFT JOIN VEHICULE vh ON a.ID_VEHICULE = vh.ID_VEHICULE
                      LEFT JOIN TYPE_VEHICULE tv ON vh.ID_TYPE = tv.ID_TYPE
                      LEFT JOIN AGENCE ag ON a.ID_AGENCE = ag.ID_AGENCE
                      WHERE COALESCE(vh.IMMATRICULATION, '') LIKE @MotCle
                         OR COALESCE(tv.MARQUE, '') LIKE @MotCle
                         OR COALESCE(ag.NOM_AGENCE, '') LIKE @MotCle
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
                    @"SELECT a.ID_AFFECTATION_VEHICULE AS Id_Affectation_Vehicule,
                             a.ID_AGENCE AS Id_Agence,
                             a.ID_VEHICULE AS Id_Vehicule,
                             a.DATE_DEBUT AS Date_Debut,
                             a.DATE_FIN AS Date_Fin,
                             a.STATUT AS Statut,
                             a.OBSERVATIONS AS Observations,
                             COALESCE(vh.IMMATRICULATION, '') AS Immatriculation,
                             COALESCE(tv.MARQUE, '') AS Marque,
                             COALESCE(ag.NOM_AGENCE, '') AS Nom_Agence,
                             COALESCE(ag.VILLE, '') AS Ville
                      FROM AFFECTATION_VEHICULE_AGENCE a
                      LEFT JOIN VEHICULE vh ON a.ID_VEHICULE = vh.ID_VEHICULE
                      LEFT JOIN TYPE_VEHICULE tv ON vh.ID_TYPE = tv.ID_TYPE
                      LEFT JOIN AGENCE ag ON a.ID_AGENCE = ag.ID_AGENCE
                      WHERE a.ID_AFFECTATION_VEHICULE = @Id",
                    new { Id = id }
                );
            }
        }

        public async Task<string> AddAsync(AffectationVehiculeAgenceModel affectation)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO AFFECTATION_VEHICULE_AGENCE (ID_AGENCE, ID_VEHICULE, DATE_DEBUT, DATE_FIN, STATUT, OBSERVATIONS)
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
                    @"UPDATE AFFECTATION_VEHICULE_AGENCE 
                      SET ID_AGENCE = @Id_Agence, ID_VEHICULE = @Id_Vehicule, 
                          DATE_DEBUT = @Date_Debut, DATE_FIN = @Date_Fin,
                          STATUT = @Statut, OBSERVATIONS = @Observations
                      WHERE ID_AFFECTATION_VEHICULE = @Id",
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
                    "DELETE FROM AFFECTATION_VEHICULE_AGENCE WHERE ID_AFFECTATION_VEHICULE = @Id",
                    new { Id = id }
                );
                return "Affectation véhicule-agence supprimée avec succès !";
            }
        }
    }
}