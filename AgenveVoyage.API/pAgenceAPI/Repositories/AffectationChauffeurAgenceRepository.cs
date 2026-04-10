using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using pAgenceAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pAgenceAPI.Repositories
{
    public class AffectationChauffeurAgenceRepository : IAffectationChauffeurAgenceRepository
    {
        private readonly string? _connectionString;

        public AffectationChauffeurAgenceRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<AffectationChauffeurAgenceModel>> GetAllAsync()
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var affectations = (await connection.QueryAsync<AffectationChauffeurAgenceModel>(
                        @"SELECT Id_Affectation_Chauffeur, Id_Chauffeur, Id_Agence, 
                                 Date_Debut, Date_Fin, Statut, Observations
                          FROM affectation_chauffeur_agence 
                          ORDER BY Date_Debut DESC"
                    )).ToList();

                    return affectations;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur GetAllAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<AffectationChauffeurAgenceModel> GetByIdAsync(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var affectation = await connection.QueryFirstOrDefaultAsync<AffectationChauffeurAgenceModel>(
                        @"SELECT Id_Affectation_Chauffeur, Id_Chauffeur, Id_Agence, 
                                 Date_Debut, Date_Fin, Statut, Observations
                          FROM affectation_chauffeur_agence 
                          WHERE Id_Affectation_Chauffeur = @Id",
                        new { Id = id }
                    );

                    return affectation;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur GetByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<string> AddAsync(AffectationChauffeurAgenceModel affectation)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"INSERT INTO affectation_chauffeur_agence 
                        (Id_Chauffeur, Id_Agence, Date_Debut, Date_Fin, Statut, Observations) 
                        VALUES 
                        (@Id_Chauffeur, @Id_Agence, @Date_Debut, @Date_Fin, @Statut, @Observations)";

                    await connection.ExecuteAsync(query, new
                    {
                        Id_Chauffeur = affectation.Id_Chauffeur,
                        Id_Agence = affectation.Id_Agence,
                        Date_Debut = affectation.Date_Debut,
                        Date_Fin = affectation.Date_Fin.HasValue ? (object)affectation.Date_Fin.Value : DBNull.Value,
                        Statut = affectation.Statut ?? "Active",
                        Observations = affectation.Observations ?? (object)DBNull.Value
                    });

                    return "Affectation enregistrée avec succès !";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur AddAsync: {ex.Message}");
                throw new Exception($"Erreur lors de l'enregistrement: {ex.Message}");
            }
        }

        public async Task<string> UpdateAsync(AffectationChauffeurAgenceModel affectation)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"UPDATE affectation_chauffeur_agence 
                        SET Id_Chauffeur = @Id_Chauffeur, 
                            Id_Agence = @Id_Agence, 
                            Date_Debut = @Date_Debut, 
                            Date_Fin = @Date_Fin, 
                            Statut = @Statut, 
                            Observations = @Observations 
                        WHERE Id_Affectation_Chauffeur = @Id";

                    await connection.ExecuteAsync(query, new
                    {
                        Id = affectation.Id_Affectation_Chauffeur,
                        Id_Chauffeur = affectation.Id_Chauffeur,
                        Id_Agence = affectation.Id_Agence,
                        Date_Debut = affectation.Date_Debut,
                        Date_Fin = affectation.Date_Fin.HasValue ? (object)affectation.Date_Fin.Value : DBNull.Value,
                        Statut = affectation.Statut ?? "Active",
                        Observations = affectation.Observations ?? (object)DBNull.Value
                    });

                    return "Affectation modifiée avec succès !";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur UpdateAsync: {ex.Message}");
                throw new Exception($"Erreur lors de la modification: {ex.Message}");
            }
        }

        public async Task<string> DeleteAsync(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    await connection.ExecuteAsync(
                        "DELETE FROM affectation_chauffeur_agence WHERE Id_Affectation_Chauffeur = @Id",
                        new { Id = id }
                    );

                    return "Affectation supprimée avec succès !";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur DeleteAsync: {ex.Message}");
                throw new Exception($"Erreur lors de la suppression: {ex.Message}");
            }
        }
    }
}