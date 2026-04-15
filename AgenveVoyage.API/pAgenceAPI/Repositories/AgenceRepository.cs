#nullable disable
using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using pAgenceAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pAgenceAPI.Repositories
{
    public class AgenceRepository : IAgenceRepository
    {
        private readonly string _connectionString;

        public AgenceRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration), "Connection string is missing");
        }

        public async Task<List<AgenceModel>> GetAllAsync()
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var result = await connection.QueryAsync<AgenceModel>(
                        @"SELECT ID_AGENCE AS Id_Agence, NOM_AGENCE AS Nom_Agence, VILLE, ADRESSE, 
                                 TELEPHONE, DATE_CREATION AS Date_Creation 
                          FROM AGENCE 
                          ORDER BY NOM_AGENCE");
                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur GetAllAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<AgenceModel?> GetByIdAsync(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return await connection.QueryFirstOrDefaultAsync<AgenceModel>(
                        @"SELECT ID_AGENCE AS Id_Agence, NOM_AGENCE AS Nom_Agence, VILLE, ADRESSE, 
                                 TELEPHONE, DATE_CREATION AS Date_Creation 
                          FROM AGENCE 
                          WHERE ID_AGENCE = @Id",
                        new { Id = id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur GetByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<string> AddAsync(AgenceModel agence)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    await connection.ExecuteAsync(
                        @"INSERT INTO AGENCE (NOM_AGENCE, VILLE, ADRESSE, TELEPHONE, DATE_CREATION) 
                          VALUES (@Nom_Agence, @Ville, @Adresse, @Telephone, @Date_Creation)",
                        new
                        {
                            Nom_Agence = agence.Nom_Agence,
                            Ville = agence.Ville,
                            Adresse = agence.Adresse,
                            Telephone = agence.Telephone,
                            Date_Creation = agence.Date_Creation
                        });
                    return "Agence ajoutée avec succès !";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur AddAsync: {ex.Message}");
                throw new Exception($"Erreur lors de l'ajout: {ex.Message}");
            }
        }

        public async Task<string> UpdateAsync(AgenceModel agence)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    await connection.ExecuteAsync(
                        @"UPDATE AGENCE 
                          SET NOM_AGENCE = @Nom_Agence, VILLE = @Ville, ADRESSE = @Adresse, 
                              TELEPHONE = @Telephone, DATE_CREATION = @Date_Creation 
                          WHERE ID_AGENCE = @Id_Agence",
                        new
                        {
                            Id_Agence = agence.Id_Agence,
                            Nom_Agence = agence.Nom_Agence,
                            Ville = agence.Ville,
                            Adresse = agence.Adresse,
                            Telephone = agence.Telephone,
                            Date_Creation = agence.Date_Creation
                        });
                    return "Agence modifiée avec succès !";
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
                        "DELETE FROM AGENCE WHERE ID_AGENCE = @Id",
                        new { Id = id });
                    return "Agence supprimée avec succès !";
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