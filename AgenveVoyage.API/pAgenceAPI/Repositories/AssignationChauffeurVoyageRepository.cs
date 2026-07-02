using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class AssignationChauffeurVoyageRepository : IAssignationChauffeurVoyageRepository
    {
        private readonly string? _connectionString;

        public AssignationChauffeurVoyageRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<AssignationChauffeurVoyageModel>> GetAllAsync()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                return (await connection.QueryAsync<AssignationChauffeurVoyageModel>(
                    "SELECT * FROM assignation_chauffeur_voyage ORDER BY ID_voyage, ORDRE_CONDUITE"
                )).ToList();
            }
        }

        public async Task<AssignationChauffeurVoyageModel?> GetByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<AssignationChauffeurVoyageModel>(
                    "SELECT * FROM assignation_chauffeur_voyage WHERE ID_ASSIGNATION = @Id",
                    new { Id = id }
                );
            }
        }

        public async Task<string> AddAsync(AssignationChauffeurVoyageModel assignation)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO assignation_chauffeur_voyage (ID_chauffeur, ID_voyage, 
                                                                  POINT_DEPART_TRONCON, POINT_ARRIVEE_TRONCON, 
                                                                  HEURE_DEBUT, HEURE_FIN, ORDRE_CONDUITE)
                      VALUES (@Id_Chauffeur, @Id_Voyage, 
                              @Point_Depart_Troncon, @Point_Arrivee_Troncon, 
                              @Heure_Debut, @Heure_Fin, @Ordre_Conduite)",
                    new
                    {
                        Id_Chauffeur = assignation.Id_Chauffeur,
                        Id_Voyage = assignation.Id_Voyage,
                        Point_Depart_Troncon = assignation.Point_Depart_Troncon,
                        Point_Arrivee_Troncon = assignation.Point_Arrivee_Troncon,
                        Heure_Debut = assignation.Heure_Debut,
                        Heure_Fin = assignation.Heure_Fin,
                        Ordre_Conduite = assignation.Ordre_Conduite
                    }
                );
                return "Assignation chauffeur-voyage ajoutée avec succès !";
            }
        }

        public async Task<string> UpdateAsync(AssignationChauffeurVoyageModel assignation)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    @"UPDATE assignation_chauffeur_voyage 
                      SET ID_chauffeur = @Id_Chauffeur, ID_voyage = @Id_Voyage, 
                          POINT_DEPART_TRONCON = @Point_Depart_Troncon, POINT_ARRIVEE_TRONCON = @Point_Arrivee_Troncon, 
                          HEURE_DEBUT = @Heure_Debut, HEURE_FIN = @Heure_Fin, ORDRE_CONDUITE = @Ordre_Conduite
                      WHERE ID_ASSIGNATION = @Id",
                    new
                    {
                        Id = assignation.Id_Assignation,
                        Id_Chauffeur = assignation.Id_Chauffeur,
                        Id_Voyage = assignation.Id_Voyage,
                        Point_Depart_Troncon = assignation.Point_Depart_Troncon,
                        Point_Arrivee_Troncon = assignation.Point_Arrivee_Troncon,
                        Heure_Debut = assignation.Heure_Debut,
                        Heure_Fin = assignation.Heure_Fin,
                        Ordre_Conduite = assignation.Ordre_Conduite
                    }
                );
                return "Assignation chauffeur-voyage modifiée avec succès !";
            }
        }

        public async Task<string> DeleteAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "DELETE FROM assignation_chauffeur_voyage WHERE ID_ASSIGNATION = @Id",
                    new { Id = id }
                );
                return "Assignation chauffeur-voyage supprimée avec succès !";
            }
        }
    }
}