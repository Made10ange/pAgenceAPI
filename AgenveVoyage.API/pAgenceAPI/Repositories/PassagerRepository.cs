using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class PassagerRepository : IPassagerRepository
    {
        private readonly string? _connectionString;
            
        public PassagerRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<PassagerModel>> GetAllAsync()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                return (await connection.QueryAsync<PassagerModel>(
                    "SELECT * FROM PASSAGER ORDER BY NOM"
                )).ToList();
            }
        }

        public async Task<PassagerModel?> GetByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<PassagerModel>(
                    "SELECT * FROM PASSAGER WHERE ID_PASSAGER = @Id",
                    new { Id = id }
                );
            }
        }

        public async Task<string> AddAsync(PassagerModel passager)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO PASSAGER (NOM, PRENOM, TYPE_PIECE, TELEPHONE, EMAIL, 
                                            DATE_NAISSANCE, LIEU_NAISSANCE, NUMERO_PIECE, 
                                            DATE_DELIVRANCE, LIEU_DELIVRANCE, SIGNATAIRE, 
                                            PROFESSION, DATE_EXPIRATION, NATIONALITE, SEXE, PHOTO)
                      VALUES (@Nom, @Prenom, @Type_Piece, @Telephone, @Email, 
                              @Date_Naissance, @Lieu_Naissance, @Numero_Piece, 
                              @Date_Delivrance, @Lieu_Delivrance, @Signataire, 
                              @Profession, @Date_Expiration, @Nationalite, @Sexe, @Photo)",
                    new
                    {
                        Nom = passager.Nom,
                        Prenom = passager.Prenom,
                        Type_Piece = passager.Type_Piece,
                        Telephone = passager.Telephone,
                        Email = passager.Email,
                        Date_Naissance = passager.Date_Naissance,
                        Lieu_Naissance = passager.Lieu_Naissance,
                        Numero_Piece = passager.Numero_Piece,
                        Date_Delivrance = passager.Date_Delivrance,
                        Lieu_Delivrance = passager.Lieu_Delivrance,
                        Signataire = passager.Signataire,
                        Profession = passager.Profession,
                        Date_Expiration = passager.Date_Expiration,
                        Nationalite = passager.Nationalite,
                        Sexe = passager.Sexe,
                        Photo = passager.Photo ?? Array.Empty<byte>()
                    }
                );
                return "Passager ajouté avec succès !";
            }
        }

        public async Task<string> UpdateAsync(PassagerModel passager)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    @"UPDATE PASSAGER 
                      SET NOM = @Nom, PRENOM = @Prenom, TYPE_PIECE = @Type_Piece, TELEPHONE = @Telephone, EMAIL = @Email, 
                          DATE_NAISSANCE = @Date_Naissance, LIEU_NAISSANCE = @Lieu_Naissance, NUMERO_PIECE = @Numero_Piece, 
                          DATE_DELIVRANCE = @Date_Delivrance, LIEU_DELIVRANCE = @Lieu_Delivrance, SIGNATAIRE = @Signataire, 
                          PROFESSION = @Profession, DATE_EXPIRATION = @Date_Expiration, NATIONALITE = @Nationalite, SEXE = @Sexe, PHOTO = @Photo
                      WHERE ID_PASSAGER = @Id",
                    new
                    {
                        Id = passager.Id_Passager,
                        Nom = passager.Nom,
                        Prenom = passager.Prenom,
                        Type_Piece = passager.Type_Piece,
                        Telephone = passager.Telephone,
                        Email = passager.Email,
                        Date_Naissance = passager.Date_Naissance,
                        Lieu_Naissance = passager.Lieu_Naissance,
                        Numero_Piece = passager.Numero_Piece,
                        Date_Delivrance = passager.Date_Delivrance,
                        Lieu_Delivrance = passager.Lieu_Delivrance,
                        Signataire = passager.Signataire,
                        Profession = passager.Profession,
                        Date_Expiration = passager.Date_Expiration,
                        Nationalite = passager.Nationalite,
                        Sexe = passager.Sexe,
                        Photo = passager.Photo ?? Array.Empty<byte>()
                    }
                );
                return "Passager modifié avec succès !";
            }
        }

        public async Task<string> DeleteAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "DELETE FROM PASSAGER WHERE ID_PASSAGER = @Id",
                    new { Id = id }
                );
                return "Passager supprimé avec succès !";
            }
        }
    }
}