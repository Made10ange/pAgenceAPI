using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class PassagerRepository : IPassagerRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<PassagerRepository> _logger;

        public PassagerRepository(IConfiguration configuration, ILogger<PassagerRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration), "Connection string is missing");
            _logger = logger;
        }

        public async Task<List<PassagerModel>> GetAllAsync(int? idAgence = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var where = idAgence.HasValue ? " WHERE Id_Agence = @IdAgence" : "";
                return (await connection.QueryAsync<PassagerModel>(
                    "SELECT * FROM PASSAGER" + where + " ORDER BY NOM",
                    new { IdAgence = idAgence }
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetAllAsync passagers");
                throw;
            }
        }

        public async Task<List<PassagerModel>> SearchAsync(string motCle, int? idAgence = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var pattern = $"%{motCle.Trim()}%";
                var agenceFilter = idAgence.HasValue ? " AND Id_Agence = @IdAgence" : "";
                return (await connection.QueryAsync<PassagerModel>(
                    @"SELECT * FROM PASSAGER
                      WHERE (NOM LIKE @Pattern
                         OR PRENOM LIKE @Pattern
                         OR NUMERO_PIECE LIKE @Pattern
                         OR TELEPHONE LIKE @Pattern)" + agenceFilter + @"
                      ORDER BY NOM",
                    new { Pattern = pattern, IdAgence = idAgence }
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur SearchAsync passagers motCle={MotCle}", motCle);
                throw;
            }
        }

        public async Task<PassagerModel?> GetByIdAsync(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return await connection.QueryFirstOrDefaultAsync<PassagerModel>(
                    "SELECT * FROM PASSAGER WHERE ID_PASSAGER = @Id",
                    new { Id = id }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetByIdAsync passager id={Id}", id);
                throw;
            }
        }

        public async Task<int> AddAsync(PassagerModel passager)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return await connection.ExecuteScalarAsync<int>(
                    @"INSERT INTO PASSAGER (NOM, PRENOM, TYPE_PIECE, TELEPHONE, EMAIL,
                                            DATE_NAISSANCE, LIEU_NAISSANCE, NUMERO_PIECE,
                                            DATE_DELIVRANCE, LIEU_DELIVRANCE, SIGNATAIRE,
                                            PROFESSION, DATE_EXPIRATION, NATIONALITE, SEXE, PHOTO, Id_Agence)
                      VALUES (@Nom, @Prenom, @Type_Piece, @Telephone, @Email,
                              @Date_Naissance, @Lieu_Naissance, @Numero_Piece,
                              @Date_Delivrance, @Lieu_Delivrance, @Signataire,
                              @Profession, @Date_Expiration, @Nationalite, @Sexe, @Photo, @Id_Agence);
                      SELECT LAST_INSERT_ID();",
                    new
                    {
                        passager.Nom,
                        passager.Prenom,
                        passager.Type_Piece,
                        passager.Telephone,
                        passager.Email,
                        passager.Date_Naissance,
                        passager.Lieu_Naissance,
                        passager.Numero_Piece,
                        passager.Date_Delivrance,
                        passager.Lieu_Delivrance,
                        passager.Signataire,
                        passager.Profession,
                        passager.Date_Expiration,
                        passager.Nationalite,
                        passager.Sexe,
                        Photo = passager.Photo ?? Array.Empty<byte>(),
                        passager.Id_Agence
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur AddAsync passager");
                throw;
            }
        }

        public async Task<string> UpdateAsync(PassagerModel passager)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    @"UPDATE PASSAGER
                      SET NOM = @Nom, PRENOM = @Prenom, TYPE_PIECE = @Type_Piece, TELEPHONE = @Telephone, EMAIL = @Email,
                          DATE_NAISSANCE = @Date_Naissance, LIEU_NAISSANCE = @Lieu_Naissance, NUMERO_PIECE = @Numero_Piece,
                          DATE_DELIVRANCE = @Date_Delivrance, LIEU_DELIVRANCE = @Lieu_Delivrance, SIGNATAIRE = @Signataire,
                          PROFESSION = @Profession, DATE_EXPIRATION = @Date_Expiration, NATIONALITE = @Nationalite,
                          SEXE = @Sexe, PHOTO = @Photo
                      WHERE ID_PASSAGER = @Id",
                    new
                    {
                        Id = passager.Id_Passager,
                        passager.Nom,
                        passager.Prenom,
                        passager.Type_Piece,
                        passager.Telephone,
                        passager.Email,
                        passager.Date_Naissance,
                        passager.Lieu_Naissance,
                        passager.Numero_Piece,
                        passager.Date_Delivrance,
                        passager.Lieu_Delivrance,
                        passager.Signataire,
                        passager.Profession,
                        passager.Date_Expiration,
                        passager.Nationalite,
                        passager.Sexe,
                        Photo = passager.Photo ?? Array.Empty<byte>()
                    }
                );
                return "Passager modifié avec succès !";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur UpdateAsync passager id={Id}", passager.Id_Passager);
                throw;
            }
        }

        public async Task<string> DeleteAsync(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    "DELETE FROM PASSAGER WHERE ID_PASSAGER = @Id",
                    new { Id = id }
                );
                return "Passager supprimé avec succès !";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur DeleteAsync passager id={Id}", id);
                throw;
            }
        }
    }
}
