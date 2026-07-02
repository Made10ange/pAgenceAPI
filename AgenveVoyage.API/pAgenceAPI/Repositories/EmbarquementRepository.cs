using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class EmbarquementRepository : IEmbarquementRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<EmbarquementRepository> _logger;

        private const string BaseSelectSql =
            @"SELECT
                evp.ID_EMBARQUEMENT     AS Id_Embarquement,
                evp.ID_voyage           AS Id_Voyage,
                evp.ID_passager         AS Id_Passager,
                evp.STATUT_EMBARQUEMENT AS Statut_Embarquement,
                evp.NUMERO_SIEGE        AS Numero_Siege,
                evp.DATE_ENREGISTREMENT AS Date_Enregistrement,
                p.NOM                   AS Nom_Passager,
                p.PRENOM                AS Prenom_Passager,
                p.TELEPHONE             AS Telephone,
                p.SEXE                  AS Sexe,
                CONCAT(tv.POINT_DEPART, ' → ', tv.POINT_ARRIVEE) AS Trajet,
                COALESCE(bag.Nb_Bagages, 0)       AS Nb_Bagages,
                bag.Poids_Total_Bagages            AS Poids_Total_Bagages,
                bag.Montant_Bagages                AS Montant_Bagages
              FROM embarquement_voyage_passager evp
              LEFT JOIN passager p    ON evp.ID_passager   = p.ID_passager
              LEFT JOIN voyage v      ON evp.ID_voyage     = v.ID_voyage
              LEFT JOIN type_voyage tv ON v.ID_type_voyage = tv.ID_type_voyage
              LEFT JOIN (
                SELECT ID_passager, ID_voyage_passager,
                       COUNT(*)           AS Nb_Bagages,
                       SUM(POIDS)         AS Poids_Total_Bagages,
                       MAX(MONTANT_TOTAL) AS Montant_Bagages
                FROM bagage
                GROUP BY ID_passager, ID_voyage_passager
              ) bag ON bag.ID_passager = evp.ID_passager AND bag.ID_voyage_passager = evp.ID_voyage";

        public EmbarquementRepository(IConfiguration configuration, ILogger<EmbarquementRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration), "Connection string is missing");
            _logger = logger;
        }

        public async Task<List<EmbarquementVoyagePassagerModel>> GetAllAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return (await connection.QueryAsync<EmbarquementVoyagePassagerModel>(
                    BaseSelectSql + " ORDER BY evp.ID_EMBARQUEMENT DESC"
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetAllAsync embarquements");
                throw;
            }
        }

        public async Task<List<EmbarquementVoyagePassagerModel>> SearchAsync(string motCle)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var pattern = $"%{motCle.Trim()}%";
                return (await connection.QueryAsync<EmbarquementVoyagePassagerModel>(
                    BaseSelectSql + @"
                      WHERE CONCAT(p.nom, ' ', p.prenom) LIKE @Pattern
                         OR p.prenom LIKE @Pattern
                         OR CONCAT(v.point_depart, ' → ', v.point_arrivee) LIKE @Pattern
                         OR CAST(evp.NUMERO_SIEGE AS CHAR) LIKE @Pattern
                      ORDER BY evp.ID_EMBARQUEMENT DESC",
                    new { Pattern = pattern }
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur SearchAsync embarquements motCle={MotCle}", motCle);
                throw;
            }
        }

        public async Task<List<EmbarquementVoyagePassagerModel>> GetByVoyageAsync(int idVoyage)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return (await connection.QueryAsync<EmbarquementVoyagePassagerModel>(
                    BaseSelectSql + " WHERE evp.ID_voyage = @IdVoyage ORDER BY evp.NUMERO_SIEGE",
                    new { IdVoyage = idVoyage }
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetByVoyageAsync idVoyage={IdVoyage}", idVoyage);
                throw;
            }
        }

        public async Task<List<EmbarquementVoyagePassagerModel>> GetByPassagerAsync(int idPassager)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return (await connection.QueryAsync<EmbarquementVoyagePassagerModel>(
                    BaseSelectSql + " WHERE evp.ID_passager = @IdPassager ORDER BY evp.ID_EMBARQUEMENT DESC",
                    new { IdPassager = idPassager }
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetByPassagerAsync idPassager={IdPassager}", idPassager);
                throw;
            }
        }

        public async Task<List<EmbarquementVoyagePassagerModel>> GetByStatutAsync(string statut)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return (await connection.QueryAsync<EmbarquementVoyagePassagerModel>(
                    BaseSelectSql + " WHERE evp.STATUT_EMBARQUEMENT = @Statut ORDER BY evp.ID_EMBARQUEMENT DESC",
                    new { Statut = statut }
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetByStatutAsync statut={Statut}", statut);
                throw;
            }
        }

        public async Task<EmbarquementVoyagePassagerModel?> GetByIdAsync(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return await connection.QueryFirstOrDefaultAsync<EmbarquementVoyagePassagerModel>(
                    BaseSelectSql + " WHERE evp.ID_EMBARQUEMENT = @Id",
                    new { Id = id }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetByIdAsync embarquement id={Id}", id);
                throw;
            }
        }

        public async Task<string> AddAsync(EmbarquementVoyagePassagerModel embarquement)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    @"INSERT INTO embarquement_voyage_passager
                      (ID_voyage, ID_passager, STATUT_EMBARQUEMENT, NUMERO_SIEGE, DATE_ENREGISTREMENT)
                      VALUES (@Id_Voyage, @Id_Passager, @Statut_Embarquement, @Numero_Siege, @Date_Enregistrement)",
                    new
                    {
                        embarquement.Id_Voyage,
                        embarquement.Id_Passager,
                        Statut_Embarquement = embarquement.Statut_Embarquement ?? "Confirmé",
                        embarquement.Numero_Siege,
                        Date_Enregistrement = embarquement.Date_Enregistrement ?? DateTime.Now
                    });
                return "Embarquement enregistré avec succès !";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur AddAsync embarquement");
                throw;
            }
        }

        public async Task<string> UpdateAsync(EmbarquementVoyagePassagerModel embarquement)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    @"UPDATE embarquement_voyage_passager
                      SET ID_voyage = @Id_Voyage, ID_passager = @Id_Passager,
                          STATUT_EMBARQUEMENT = @Statut_Embarquement,
                          NUMERO_SIEGE = @Numero_Siege,
                          DATE_ENREGISTREMENT = @Date_Enregistrement
                      WHERE ID_EMBARQUEMENT = @Id_Embarquement",
                    new
                    {
                        embarquement.Id_Embarquement,
                        embarquement.Id_Voyage,
                        embarquement.Id_Passager,
                        Statut_Embarquement = embarquement.Statut_Embarquement ?? "Confirmé",
                        embarquement.Numero_Siege,
                        embarquement.Date_Enregistrement
                    });
                return "Embarquement modifié avec succès !";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur UpdateAsync embarquement id={Id}", embarquement.Id_Embarquement);
                throw;
            }
        }

        public async Task<string> DeleteAsync(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    "DELETE FROM embarquement_voyage_passager WHERE ID_EMBARQUEMENT = @Id",
                    new { Id = id }
                );
                return "Embarquement supprimé avec succès !";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur DeleteAsync embarquement id={Id}", id);
                throw;
            }
        }
    }
}
