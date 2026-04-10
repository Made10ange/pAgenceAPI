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
                    "SELECT * FROM ASSIGNATION_CHAUFFEUR_VOYAGE ORDER BY ID_VOYAGE, ORDRE_CONDUITE"
                )).ToList();
            }
        }

        public async Task<AssignationChauffeurVoyageModel> GetByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<AssignationChauffeurVoyageModel>(
                    "SELECT * FROM ASSIGNATION_CHAUFFEUR_VOYAGE WHERE ID_ASSIGNATION = @Id",
                    new { Id = id }
                );
            }
        }

        public async Task<string> AddAsync(AssignationChauffeurVoyageModel assignation)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO ASSIGNATION_CHAUFFEUR_VOYAGE (ID_CHAUFFEUR, ID_VOYAGE, 
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
                    @"UPDATE ASSIGNATION_CHAUFFEUR_VOYAGE 
                      SET ID_CHAUFFEUR = @Id_Chauffeur, ID_VOYAGE = @Id_Voyage, 
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
                    "DELETE FROM ASSIGNATION_CHAUFFEUR_VOYAGE WHERE ID_ASSIGNATION = @Id",
                    new { Id = id }
                );
                return "Assignation chauffeur-voyage supprimée avec succès !";
            }
        }
    }
}