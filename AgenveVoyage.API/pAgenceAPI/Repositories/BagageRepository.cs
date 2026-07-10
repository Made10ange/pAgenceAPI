using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class BagageRepository : IBagageRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<BagageRepository> _logger;

        private const string BaseSelectSql =
            @"SELECT
                b.ID_bagage             AS Id_Bagage,
                b.ID_passager           AS Id_Passager,
                b.ID_voyage_passager    AS Id_Voyage_Passager,
                b.ID_voyage_bagage      AS Id_Voyage_Bagage,
                b.DESCRIPTION           AS Description,
                b.POIDS                 AS Poids,
                b.STATUT                AS Statut,
                b.DATE_ENREGISTREMENT   AS Date_Enregistrement,
                b.MONTANT_TOTAL         AS Montant_Total,
                b.NUMERO_ORDRE          AS Numero_Ordre,
                b.TOTAL_bagageS         AS Total_Bagages,
                CONCAT(p.NOM, ' ', p.PRENOM) AS Nom_Passager,
                CONCAT(COALESCE(tvp.POINT_DEPART,''), ' → ', COALESCE(tvp.POINT_ARRIVEE,'')) AS Trajet_Passager,
                CONCAT(COALESCE(tvb.POINT_DEPART,''), ' → ', COALESCE(tvb.POINT_ARRIVEE,'')) AS Trajet_Bagage,
                veh.IMMATRICULATION     AS Immatriculation_Bagage
              FROM bagage b
              LEFT JOIN passager p      ON b.ID_passager        = p.ID_passager
              LEFT JOIN voyage  vp      ON b.ID_voyage_passager = vp.ID_voyage
              LEFT JOIN type_voyage tvp ON vp.ID_type_voyage    = tvp.ID_type_voyage
              LEFT JOIN voyage  vb      ON b.ID_voyage_bagage   = vb.ID_voyage
              LEFT JOIN type_voyage tvb ON vb.ID_type_voyage    = tvb.ID_type_voyage
              LEFT JOIN vehicule veh    ON vb.ID_vehicule        = veh.ID_vehicule";

        public BagageRepository(IConfiguration configuration, ILogger<BagageRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<List<BagageModel>> GetAllAsync(int? idAgence = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                // Inclure les bagages dont le voyage passager OU le voyage bagage appartient à cette agence,
                // MAIS AUSSI les bagages sans voyage lié (créés directement, pas encore assignés)
                var sql = idAgence.HasValue
                    ? BaseSelectSql + @" WHERE (vp.Id_Agence = @IdAgence OR vb.Id_Agence = @IdAgence
                                               OR (vp.Id_Agence IS NULL AND vb.Id_Agence IS NULL))
                                        ORDER BY b.DATE_ENREGISTREMENT DESC"
                    : BaseSelectSql + " ORDER BY b.DATE_ENREGISTREMENT DESC";
                return (await connection.QueryAsync<BagageModel>(sql, new { IdAgence = idAgence })).ToList();
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur GetAllAsync bagages"); throw; }
        }

        public async Task<BagageModel?> GetByIdAsync(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return await connection.QueryFirstOrDefaultAsync<BagageModel>(
                    BaseSelectSql + " WHERE b.ID_bagage = @Id", new { Id = id });
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur GetByIdAsync bagage id={Id}", id); throw; }
        }

        public async Task<List<BagageModel>> GetByPassagerAsync(int idPassager)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return (await connection.QueryAsync<BagageModel>(
                    BaseSelectSql + " WHERE b.ID_passager = @Id ORDER BY b.DATE_ENREGISTREMENT DESC",
                    new { Id = idPassager }
                )).ToList();
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur GetByPassagerAsync id={Id}", idPassager); throw; }
        }

        public async Task<List<BagageModel>> GetByVoyageAsync(int idVoyage)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return (await connection.QueryAsync<BagageModel>(
                    BaseSelectSql + " WHERE b.ID_voyage_bagage = @Id ORDER BY b.DATE_ENREGISTREMENT DESC",
                    new { Id = idVoyage }
                )).ToList();
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur GetByVoyageAsync id={Id}", idVoyage); throw; }
        }

        public async Task<List<BagageModel>> SearchAsync(string motCle)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var pattern = $"%{motCle.Trim()}%";
                return (await connection.QueryAsync<BagageModel>(
                    BaseSelectSql + @"
                    WHERE CONCAT(p.NOM, ' ', p.PRENOM) LIKE @Pattern
                       OR b.DESCRIPTION LIKE @Pattern
                       OR b.STATUT LIKE @Pattern
                       OR vp.POINT_DEPART LIKE @Pattern
                       OR vb.POINT_DEPART LIKE @Pattern
                    ORDER BY b.DATE_ENREGISTREMENT DESC",
                    new { Pattern = pattern }
                )).ToList();
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur SearchAsync bagages"); throw; }
        }

        public async Task<int> AddAsync(BagageModel bagage)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var newId = await connection.ExecuteScalarAsync<int>(
                    @"INSERT INTO bagage
                      (ID_passager, ID_voyage_passager, ID_voyage_bagage, DESCRIPTION, POIDS, STATUT, DATE_ENREGISTREMENT, MONTANT_TOTAL)
                      VALUES (@Id_Passager, @Id_Voyage_Passager, @Id_Voyage_Bagage, @Description, @Poids, @Statut, @Date_Enregistrement, @Montant_Total);
                      SELECT LAST_INSERT_ID();",
                    new {
                        bagage.Id_Passager,
                        Id_Voyage_Passager = bagage.Id_Voyage_Passager.HasValue ? (object)bagage.Id_Voyage_Passager.Value : DBNull.Value,
                        Id_Voyage_Bagage   = bagage.Id_Voyage_Bagage.HasValue   ? (object)bagage.Id_Voyage_Bagage.Value   : DBNull.Value,
                        bagage.Description, bagage.Poids,
                        Statut = bagage.Statut ?? "En attente",
                        Date_Enregistrement = bagage.Date_Enregistrement == default ? DateTime.Now : bagage.Date_Enregistrement,
                        bagage.Montant_Total
                    });
                return newId;
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur AddAsync bagage"); throw; }
        }

        public async Task<string> UpdateAsync(BagageModel bagage)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    @"UPDATE bagage SET
                        ID_passager         = @Id_Passager,
                        ID_voyage_passager  = @Id_Voyage_Passager,
                        ID_voyage_bagage    = @Id_Voyage_Bagage,
                        DESCRIPTION         = @Description,
                        POIDS               = @Poids,
                        STATUT              = @Statut
                      WHERE ID_bagage = @Id_Bagage",
                    new {
                        bagage.Id_Bagage, bagage.Id_Passager, bagage.Id_Voyage_Passager,
                        bagage.Id_Voyage_Bagage, bagage.Description, bagage.Poids, bagage.Statut
                    });
                return "Bagage modifié avec succès !";
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur UpdateAsync bagage id={Id}", bagage.Id_Bagage); throw; }
        }

        public async Task<string> UpdateStatutAsync(int id, string statut)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    "UPDATE bagage SET STATUT = @Statut WHERE ID_bagage = @Id",
                    new { Id = id, Statut = statut });
                return "Statut mis à jour !";
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur UpdateStatutAsync bagage id={Id}", id); throw; }
        }

        public async Task<string> DeleteAsync(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync("DELETE FROM bagage WHERE ID_bagage = @Id", new { Id = id });
                return "Bagage supprimé avec succès !";
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur DeleteAsync bagage id={Id}", id); throw; }
        }

        public async Task EnregistrerParPassagerAsync(int idPassager, int idVoyage, decimal? montantTotal, List<BagageLigneRequest> bagages)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                await connection.ExecuteAsync(
                    "DELETE FROM bagage WHERE ID_passager = @IdPassager AND ID_voyage_passager = @IdVoyage",
                    new { IdPassager = idPassager, IdVoyage = idVoyage },
                    transaction
                );

                var lignesValides = bagages
                    .Where(l => !string.IsNullOrWhiteSpace(l.Description) || l.Poids.HasValue)
                    .ToList();

                int total = lignesValides.Count;
                int ordre = 1;

                foreach (var ligne in lignesValides)
                {
                    var idVoyageBagage = ligne.Id_Voyage_Bagage ?? idVoyage;
                    await connection.ExecuteAsync(
                        @"INSERT INTO bagage
                            (ID_passager, ID_voyage_passager, ID_voyage_bagage, DESCRIPTION, POIDS, STATUT,
                             DATE_ENREGISTREMENT, MONTANT_TOTAL, NUMERO_ORDRE, TOTAL_bagageS)
                          VALUES
                            (@IdPassager, @IdVoyage, @IdVoyageBagage, @Description, @Poids, 'En attente',
                             NOW(), @MontantTotal, @NumeroOrdre, @TotalBagages)",
                        new {
                            IdPassager = idPassager, IdVoyage = idVoyage,
                            IdVoyageBagage = idVoyageBagage,
                            ligne.Description, ligne.Poids,
                            MontantTotal = montantTotal,
                            NumeroOrdre  = ordre++,
                            TotalBagages = total
                        },
                        transaction
                    );
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<PassagerAvecBagagesDto>> GetPassagersAvecBagagesAsync(int idVoyage)
        {
            using var connection = new MySqlConnection(_connectionString);

            // Récupérer les passagers ayant des bagages sur ce voyage (via ID_voyage_bagage ou ID_voyage_passager)
            var passagers = (await connection.QueryAsync<PassagerAvecBagagesDto>(
                @"SELECT DISTINCT p.ID_passager, p.NOM, p.PRENOM, p.TELEPHONE
                  FROM passager p
                  INNER JOIN bagage b ON b.ID_passager = p.ID_passager
                  WHERE b.ID_voyage_bagage = @IdVoyage
                     OR b.ID_voyage_passager = @IdVoyage
                  ORDER BY NOM, PRENOM",
                new { IdVoyage = idVoyage }
            )).ToList();

            if (!passagers.Any()) return passagers;

            // Récupérer tous les bagages du voyage en une seule requête
            var bagages = (await connection.QueryAsync<BagageModel>(
                BaseSelectSql + " WHERE b.ID_voyage_bagage = @IdVoyage OR b.ID_voyage_passager = @IdVoyage ORDER BY b.ID_passager",
                new { IdVoyage = idVoyage }
            )).ToList();

            // Associer les bagages à chaque passager
            foreach (var passager in passagers)
                passager.Bagages = bagages.Where(b => b.Id_Passager == passager.Id_Passager).ToList();

            return passagers;
        }

        public async Task<List<BagageModel>> GetEnAttenteAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return (await connection.QueryAsync<BagageModel>(
                    BaseSelectSql + " WHERE b.ID_voyage_bagage IS NULL ORDER BY b.DATE_ENREGISTREMENT DESC"
                )).ToList();
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur GetEnAttenteAsync bagages"); throw; }
        }

        public async Task<string> AssignerVoyageAsync(int idBagage, int idVoyage)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    "UPDATE bagage SET ID_voyage_bagage = @IdVoyage, ID_voyage_passager = @IdVoyage, STATUT = 'En cours' WHERE ID_bagage = @IdBagage",
                    new { IdBagage = idBagage, IdVoyage = idVoyage });
                return "Bagage chargé sur le voyage !";
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur AssignerVoyageAsync bagage id={Id}", idBagage); throw; }
        }

        public async Task LivrerParVoyageAsync(int idVoyage)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    "UPDATE bagage SET STATUT = 'Terminé' WHERE ID_voyage_bagage = @IdVoyage AND STATUT NOT IN ('Terminé','Livré','Annulé')",
                    new { IdVoyage = idVoyage });
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur LivrerParVoyageAsync voyage id={Id}", idVoyage); throw; }
        }

        public async Task<List<BagageModel>> GetByPassagersEmbarquesAsync(int idVoyage)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);

                // Étape 1 : récupérer les IDs passagers éligibles (même logique que l'onglet Passagers)
                var passagerIds = (await connection.QueryAsync<int?>(@"
                    SELECT DISTINCT bl.Id_Passager
                    FROM billet bl
                    JOIN  voyage      v0  ON v0.Id_Voyage       = @idVoyage
                    LEFT JOIN type_voyage tv0 ON tv0.Id_Type_Voyage = v0.Id_Type_Voyage
                    LEFT JOIN type_voyage tvb ON tvb.Id_Type_Voyage = bl.Id_Type_Voyage
                    WHERE bl.Statut IN ('Valide','Reporté')
                      AND bl.Id_Passager IS NOT NULL
                      AND (
                          bl.Id_Voyage_Prevu = v0.Id_Voyage
                          OR bl.Id_Type_Voyage = v0.Id_Type_Voyage
                          OR (tvb.Libelle_Type_Voyage IS NOT NULL
                              AND tv0.Libelle_Type_Voyage IS NOT NULL
                              AND LOWER(tvb.Libelle_Type_Voyage) = LOWER(tv0.Libelle_Type_Voyage)
                              AND LOWER(COALESCE(bl.Point_Depart,''))  = LOWER(COALESCE(tv0.Point_Depart,''))
                              AND LOWER(COALESCE(bl.Point_Arrivee,'')) = LOWER(COALESCE(tv0.Point_Arrivee,'')))
                          OR (bl.Id_Type_Voyage IS NULL
                              AND tv0.Point_Depart IS NOT NULL
                              AND LOWER(COALESCE(bl.Point_Depart,''))  = LOWER(COALESCE(tv0.Point_Depart,''))
                              AND LOWER(COALESCE(bl.Point_Arrivee,'')) = LOWER(COALESCE(tv0.Point_Arrivee,'')))
                      )
                    UNION
                    SELECT Id_Passager FROM embarquement_voyage_passager WHERE Id_Voyage = @idVoyage",
                    new { idVoyage }
                )).Where(id => id.HasValue).Select(id => id!.Value).ToList();

                if (!passagerIds.Any()) return new List<BagageModel>();

                // Étape 2 : bagages En attente pour ces passagers
                return (await connection.QueryAsync<BagageModel>(
                    BaseSelectSql + @"
                    WHERE b.STATUT = 'En attente'
                      AND b.ID_passager IN @ids
                    ORDER BY b.DATE_ENREGISTREMENT DESC",
                    new { ids = passagerIds }
                )).ToList();
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur GetByPassagersEmbarquesAsync idVoyage={Id}", idVoyage); throw; }
        }

        public async Task<List<BagageModel>> GetByPassagerIdsAsync(List<int> passagerIds)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return (await connection.QueryAsync<BagageModel>(
                    BaseSelectSql + @"
                    WHERE b.STATUT = 'En attente'
                      AND b.ID_passager IN @ids
                    ORDER BY b.DATE_ENREGISTREMENT DESC",
                    new { ids = passagerIds }
                )).ToList();
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur GetByPassagerIdsAsync"); throw; }
        }

        public async Task<List<BagageModel>> GetArchivesAsync(int? idAgence = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var sql = idAgence.HasValue
                    ? BaseSelectSql + " WHERE b.STATUT = 'Livré' AND vp.Id_Agence = @IdAgence ORDER BY b.DATE_ENREGISTREMENT DESC"
                    : BaseSelectSql + " WHERE b.STATUT = 'Livré' ORDER BY b.DATE_ENREGISTREMENT DESC";
                return (await connection.QueryAsync<BagageModel>(sql, new { IdAgence = idAgence })).ToList();
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur GetArchivesAsync bagages"); throw; }
        }
    }
}
