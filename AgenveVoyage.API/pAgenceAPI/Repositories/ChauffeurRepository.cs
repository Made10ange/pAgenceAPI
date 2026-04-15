using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using pAgenceAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pAgenceAPI.Repositories
{
    public class ChauffeurRepository : IChauffeurRepository
    {
        private readonly string? _connectionString;

        public ChauffeurRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<ChauffeurModel>> GetAllAsync()
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var chauffeurs = (await connection.QueryAsync<ChauffeurModel>(
                        @"SELECT Id_Chauffeur, Nom, Prenom, Type_Piece, Telephone, Email, 
                                 Lieu_Naissance, Numero_Piece, Lieu_Delivrance, Signataire, 
                                 Profession, Nationalite, Sexe, Date_Naissance, 
                                 Date_Delivrance, Date_Expiration, Photo
                          FROM chauffeur 
                          ORDER BY Nom, Prenom"
                    )).ToList();

                    return chauffeurs;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur GetAllAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<ChauffeurModel?> GetByIdAsync(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var chauffeur = await connection.QueryFirstOrDefaultAsync<ChauffeurModel>(
                        @"SELECT Id_Chauffeur, Nom, Prenom, Type_Piece, Telephone, Email, 
                                 Lieu_Naissance, Numero_Piece, Lieu_Delivrance, Signataire, 
                                 Profession, Nationalite, Sexe, Date_Naissance, 
                                 Date_Delivrance, Date_Expiration, Photo
                          FROM chauffeur 
                          WHERE Id_Chauffeur = @Id",
                        new { Id = id }
                    );

                    return chauffeur;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur GetByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<string> AddAsync(ChauffeurModel chauffeur)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"INSERT INTO chauffeur 
                        (Nom, Prenom, Type_Piece, Telephone, Email, Lieu_Naissance, 
                         Numero_Piece, Lieu_Delivrance, Signataire, Profession, Nationalite, Sexe, 
                         Date_Naissance, Date_Delivrance, Date_Expiration, Photo) 
                        VALUES 
                        (@Nom, @Prenom, @Type_Piece, @Telephone, @Email, @Lieu_Naissance, 
                         @Numero_Piece, @Lieu_Delivrance, @Signataire, @Profession, @Nationalite, @Sexe, 
                         @Date_Naissance, @Date_Delivrance, @Date_Expiration, @Photo)";

                    // Convertir Photo_Base64 en byte[] pour le BLOB
                    byte[] photoBytes = null;
                    if (!string.IsNullOrEmpty(chauffeur.Photo_Base64))
                    {
                        photoBytes = Convert.FromBase64String(chauffeur.Photo_Base64);
                    }

                    await connection.ExecuteAsync(query, new
                    {
                        Nom = chauffeur.Nom,
                        Prenom = chauffeur.Prenom,
                        Type_Piece = chauffeur.Type_Piece ?? (object)DBNull.Value,
                        Telephone = chauffeur.Telephone ?? (object)DBNull.Value,
                        Email = chauffeur.Email ?? (object)DBNull.Value,
                        Lieu_Naissance = chauffeur.Lieu_Naissance ?? (object)DBNull.Value,
                        Numero_Piece = chauffeur.Numero_Piece ?? (object)DBNull.Value,
                        Lieu_Delivrance = chauffeur.Lieu_Delivrance ?? (object)DBNull.Value,
                        Signataire = chauffeur.Signataire ?? (object)DBNull.Value,
                        Profession = chauffeur.Profession ?? (object)DBNull.Value,
                        Nationalite = chauffeur.Nationalite ?? (object)DBNull.Value,
                        Sexe = chauffeur.Sexe ?? (object)DBNull.Value,
                        Date_Naissance = chauffeur.Date_Naissance.HasValue ? (object)chauffeur.Date_Naissance.Value : DBNull.Value,
                        Date_Delivrance = chauffeur.Date_Delivrance.HasValue ? (object)chauffeur.Date_Delivrance.Value : DBNull.Value,
                        Date_Expiration = chauffeur.Date_Expiration.HasValue ? (object)chauffeur.Date_Expiration.Value : DBNull.Value,
                        Photo = photoBytes ?? (object)DBNull.Value
                    });

                    return "Chauffeur ajouté avec succès !";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur AddAsync: {ex.Message}");
                throw new Exception($"Erreur lors de l'ajout: {ex.Message}");
            }
        }

        public async Task<string> UpdateAsync(ChauffeurModel chauffeur)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"UPDATE chauffeur 
                        SET Nom = @Nom, Prenom = @Prenom, Type_Piece = @Type_Piece, 
                            Telephone = @Telephone, Email = @Email, Lieu_Naissance = @Lieu_Naissance, 
                            Numero_Piece = @Numero_Piece, Lieu_Delivrance = @Lieu_Delivrance, 
                            Signataire = @Signataire, Profession = @Profession, Nationalite = @Nationalite, 
                            Sexe = @Sexe, Date_Naissance = @Date_Naissance, 
                            Date_Delivrance = @Date_Delivrance, Date_Expiration = @Date_Expiration, 
                            Photo = @Photo 
                        WHERE Id_Chauffeur = @Id";

                    // Convertir Photo_Base64 en byte[] pour le BLOB
                    byte[] photoBytes = null;
                    if (!string.IsNullOrEmpty(chauffeur.Photo_Base64))
                    {
                        photoBytes = Convert.FromBase64String(chauffeur.Photo_Base64);
                    }

                    await connection.ExecuteAsync(query, new
                    {
                        Id = chauffeur.Id_Chauffeur,
                        Nom = chauffeur.Nom,
                        Prenom = chauffeur.Prenom,
                        Type_Piece = chauffeur.Type_Piece ?? (object)DBNull.Value,
                        Telephone = chauffeur.Telephone ?? (object)DBNull.Value,
                        Email = chauffeur.Email ?? (object)DBNull.Value,
                        Lieu_Naissance = chauffeur.Lieu_Naissance ?? (object)DBNull.Value,
                        Numero_Piece = chauffeur.Numero_Piece ?? (object)DBNull.Value,
                        Lieu_Delivrance = chauffeur.Lieu_Delivrance ?? (object)DBNull.Value,
                        Signataire = chauffeur.Signataire ?? (object)DBNull.Value,
                        Profession = chauffeur.Profession ?? (object)DBNull.Value,
                        Nationalite = chauffeur.Nationalite ?? (object)DBNull.Value,
                        Sexe = chauffeur.Sexe ?? (object)DBNull.Value,
                        Date_Naissance = chauffeur.Date_Naissance.HasValue ? (object)chauffeur.Date_Naissance.Value : DBNull.Value,
                        Date_Delivrance = chauffeur.Date_Delivrance.HasValue ? (object)chauffeur.Date_Delivrance.Value : DBNull.Value,
                        Date_Expiration = chauffeur.Date_Expiration.HasValue ? (object)chauffeur.Date_Expiration.Value : DBNull.Value,
                        Photo = photoBytes ?? (object)DBNull.Value
                    });

                    return "Chauffeur modifié avec succès !";
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
                        "DELETE FROM chauffeur WHERE Id_Chauffeur = @Id",
                        new { Id = id }
                    );

                    return "Chauffeur supprimé avec succès !";
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