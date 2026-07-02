using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class ChauffeurRepository : IChauffeurRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<ChauffeurRepository> _logger;

        private const string BaseSelectSql =
            @"SELECT Id_Chauffeur, Nom, Prenom, Type_Piece, Telephone, Email,
                     Lieu_Naissance, Numero_Piece, Lieu_Delivrance, Signataire,
                     Profession, Nationalite, Sexe, Date_Naissance,
                     Date_Delivrance, Date_Expiration, Photo
              FROM chauffeur";

        public ChauffeurRepository(IConfiguration configuration, ILogger<ChauffeurRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration), "Connection string is missing");
            _logger = logger;
        }

        public async Task<List<ChauffeurModel>> GetDisponiblesAsync(int? idAgence = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);

                // Chauffeurs avec une affectation Active dans cette agence
                // ET qui ne sont pas déjà sur un voyage non terminé
                var sql = idAgence.HasValue
                    ? @"
                        SELECT c.Id_Chauffeur, c.Nom, c.Prenom, c.Telephone, c.Email
                        FROM chauffeur c
                        WHERE c.Id_Agence = @IdAgence
                          AND c.Id_Chauffeur NOT IN (
                            SELECT acv.ID_chauffeur
                            FROM assignation_chauffeur_voyage acv
                            JOIN voyage v ON v.ID_voyage = acv.ID_voyage
                            WHERE v.STATUT NOT IN ('Terminé', 'Annulé')
                        )
                        ORDER BY c.Nom, c.Prenom"
                    : @"
                        SELECT c.Id_Chauffeur, c.Nom, c.Prenom, c.Telephone, c.Email
                        FROM chauffeur c
                        WHERE c.Id_Chauffeur NOT IN (
                            SELECT acv.ID_chauffeur
                            FROM assignation_chauffeur_voyage acv
                            JOIN voyage v ON v.ID_voyage = acv.ID_voyage
                            WHERE v.STATUT NOT IN ('Terminé', 'Annulé')
                        )
                        ORDER BY c.Nom, c.Prenom";

                return (await connection.QueryAsync<ChauffeurModel>(sql, new { IdAgence = idAgence })).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetDisponiblesAsync chauffeurs");
                throw;
            }
        }

        public async Task<List<ChauffeurModel>> GetAllAsync(int? idAgence = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var where = idAgence.HasValue ? " WHERE Id_Agence = @IdAgence" : "";
                return (await connection.QueryAsync<ChauffeurModel>(
                    BaseSelectSql + where + " ORDER BY Nom, Prenom",
                    new { IdAgence = idAgence }
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetAllAsync chauffeurs");
                throw;
            }
        }

        public async Task<List<ChauffeurModel>> SearchAsync(string motCle)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var pattern = $"%{motCle.Trim()}%";
                return (await connection.QueryAsync<ChauffeurModel>(
                    BaseSelectSql + @"
                      WHERE Nom LIKE @Pattern
                         OR Prenom LIKE @Pattern
                         OR Numero_Piece LIKE @Pattern
                         OR Telephone LIKE @Pattern
                      ORDER BY Nom, Prenom",
                    new { Pattern = pattern }
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur SearchAsync chauffeurs motCle={MotCle}", motCle);
                throw;
            }
        }

        public async Task<ChauffeurModel?> GetByIdAsync(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return await connection.QueryFirstOrDefaultAsync<ChauffeurModel>(
                    BaseSelectSql + " WHERE Id_Chauffeur = @Id",
                    new { Id = id }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetByIdAsync chauffeur id={Id}", id);
                throw;
            }
        }

        public async Task<string> AddAsync(ChauffeurModel chauffeur)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                byte[]? photoBytes = string.IsNullOrEmpty(chauffeur.Photo_Base64)
                    ? null
                    : Convert.FromBase64String(chauffeur.Photo_Base64);

                var idChauffeur = await connection.ExecuteScalarAsync<int>(
                    @"INSERT INTO chauffeur
                        (Nom, Prenom, Type_Piece, Telephone, Email, Lieu_Naissance,
                         Numero_Piece, Lieu_Delivrance, Signataire, Profession, Nationalite, Sexe,
                         Date_Naissance, Date_Delivrance, Date_Expiration, Photo, Id_Agence)
                        VALUES
                        (@Nom, @Prenom, @Type_Piece, @Telephone, @Email, @Lieu_Naissance,
                         @Numero_Piece, @Lieu_Delivrance, @Signataire, @Profession, @Nationalite, @Sexe,
                         @Date_Naissance, @Date_Delivrance, @Date_Expiration, @Photo, @Id_Agence);
                      SELECT LAST_INSERT_ID();",
                    new
                    {
                        chauffeur.Nom,
                        chauffeur.Prenom,
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
                        Photo = photoBytes ?? (object)DBNull.Value,
                        Id_Agence = chauffeur.Id_Agence.HasValue ? (object)chauffeur.Id_Agence.Value : DBNull.Value
                    });

                // Crée automatiquement la fiche RH correspondante (poste "Chauffeur")
                await connection.ExecuteAsync(
                    @"INSERT INTO personnel (Nom, Prenom, Telephone, Email, ID_poste, Type_Contrat, Salaire_Base, Date_Embauche, Statut, ID_chauffeur)
                      VALUES (@Nom, @Prenom, @Telephone, @Email, 1, 'CDI', 0, CURDATE(), 'Actif', @IdChauffeur)",
                    new
                    {
                        chauffeur.Nom,
                        chauffeur.Prenom,
                        Telephone = chauffeur.Telephone ?? (object)DBNull.Value,
                        Email = chauffeur.Email ?? (object)DBNull.Value,
                        IdChauffeur = idChauffeur
                    });

                return "Chauffeur ajouté avec succès !";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur AddAsync chauffeur");
                throw;
            }
        }

        public async Task<string> UpdateAsync(ChauffeurModel chauffeur)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                byte[]? photoBytes = string.IsNullOrEmpty(chauffeur.Photo_Base64)
                    ? null
                    : Convert.FromBase64String(chauffeur.Photo_Base64);

                await connection.ExecuteAsync(
                    @"UPDATE chauffeur
                        SET Nom = @Nom, Prenom = @Prenom, Type_Piece = @Type_Piece,
                            Telephone = @Telephone, Email = @Email, Lieu_Naissance = @Lieu_Naissance,
                            Numero_Piece = @Numero_Piece, Lieu_Delivrance = @Lieu_Delivrance,
                            Signataire = @Signataire, Profession = @Profession, Nationalite = @Nationalite,
                            Sexe = @Sexe, Date_Naissance = @Date_Naissance,
                            Date_Delivrance = @Date_Delivrance, Date_Expiration = @Date_Expiration,
                            Photo = @Photo
                        WHERE Id_Chauffeur = @Id",
                    new
                    {
                        Id = chauffeur.Id_Chauffeur,
                        chauffeur.Nom,
                        chauffeur.Prenom,
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

                // Synchronise la fiche RH liée (si elle existe)
                await connection.ExecuteAsync(
                    @"UPDATE personnel SET Nom=@Nom, Prenom=@Prenom, Telephone=@Telephone, Email=@Email
                      WHERE ID_chauffeur=@Id",
                    new
                    {
                        Id = chauffeur.Id_Chauffeur,
                        chauffeur.Nom,
                        chauffeur.Prenom,
                        Telephone = chauffeur.Telephone ?? (object)DBNull.Value,
                        Email = chauffeur.Email ?? (object)DBNull.Value
                    });

                return "Chauffeur modifié avec succès !";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur UpdateAsync chauffeur id={Id}", chauffeur.Id_Chauffeur);
                throw;
            }
        }

        public async Task<string> DeleteAsync(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    "DELETE FROM chauffeur WHERE Id_Chauffeur = @Id",
                    new { Id = id }
                );
                return "Chauffeur supprimé avec succès !";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur DeleteAsync chauffeur id={Id}", id);
                throw;
            }
        }
    }
}
