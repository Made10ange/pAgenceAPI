using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class VoyageRepository : IVoyageRepository
    {
        private readonly string? _connectionString;
        private readonly ILogger<VoyageRepository> _logger;

        private const string BaseSelectSql =
            @"SELECT
                v.ID_VOYAGE, v.ID_VEHICULE, v.ID_TYPE_VOYAGE,
                tv.POINT_DEPART, tv.POINT_ARRIVEE,
                v.DATE_DEPART, v.DATE_ARRIVEE, v.HEURE_DEPART, v.HEURE_ARRIVEE, v.DUREE, v.STATUT,
                v.Numero_Journalier,
                COALESCE(vh.IMMATRICULATION, '') as Immatriculation,
                COALESCE(tv.LIBELLE_TYPE_VOYAGE, '') as Libelle_Type_Voyage,
                COALESCE(tv.PRIX, 0) as Prix,
                COALESCE(tpv.NOMBRE_PLACE, 0) as Nombre_Place,
                acv.ID_CHAUFFEUR,
                CASE WHEN c.ID_CHAUFFEUR IS NOT NULL THEN CONCAT(c.NOM, ' ', c.PRENOM) ELSE NULL END as Nom_Chauffeur,
                ag.NOM_AGENCE as Nom_Agence
              FROM VOYAGE v
              LEFT JOIN VEHICULE vh ON v.ID_VEHICULE = vh.ID_VEHICULE
              LEFT JOIN TYPE_VEHICULE tpv ON vh.ID_TYPE = tpv.ID_TYPE
              LEFT JOIN TYPE_VOYAGE tv ON v.ID_TYPE_VOYAGE = tv.ID_TYPE_VOYAGE
              LEFT JOIN ASSIGNATION_CHAUFFEUR_VOYAGE acv ON v.ID_VOYAGE = acv.ID_VOYAGE
              LEFT JOIN CHAUFFEUR c ON acv.ID_CHAUFFEUR = c.ID_CHAUFFEUR
              LEFT JOIN AGENCE ag ON v.Id_Agence = ag.ID_AGENCE";

        public VoyageRepository(IConfiguration configuration, ILogger<VoyageRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task<List<VoyageModel>> GetAllAsync(int? idAgence = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            try
            {
                var where = idAgence.HasValue ? " WHERE v.Id_Agence = @IdAgence" : "";
                return (await connection.QueryAsync<VoyageModel>(
                    BaseSelectSql + where + " ORDER BY v.DATE_DEPART DESC",
                    new { IdAgence = idAgence }
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetAllAsync voyages");
                throw;
            }
        }

        public async Task<List<VoyageModel>> GetByStatutAsync(string statut, int? idAgence = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            try
            {
                var where = idAgence.HasValue
                    ? " WHERE v.STATUT = @Statut AND v.Id_Agence = @IdAgence"
                    : " WHERE v.STATUT = @Statut";
                return (await connection.QueryAsync<VoyageModel>(
                    BaseSelectSql + where + " ORDER BY v.DATE_DEPART DESC",
                    new { Statut = statut, IdAgence = idAgence }
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetByStatutAsync statut={Statut}", statut);
                throw;
            }
        }

        public async Task<List<VoyageModel>> GetByVehiculeAsync(int idVehicule)
        {
            using var connection = new MySqlConnection(_connectionString);
            try
            {
                return (await connection.QueryAsync<VoyageModel>(
                    BaseSelectSql + " WHERE v.ID_VEHICULE = @IdVehicule ORDER BY v.DATE_DEPART DESC, v.HEURE_DEPART DESC",
                    new { IdVehicule = idVehicule }
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetByVehiculeAsync idVehicule={IdVehicule}", idVehicule);
                throw;
            }
        }

        public async Task<List<VoyageModel>> SearchAsync(string motCle, int? idAgence = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            try
            {
                var pattern = $"%{motCle.Trim()}%";
                var agenceFilter = idAgence.HasValue ? " AND v.Id_Agence = @IdAgence" : "";
                return (await connection.QueryAsync<VoyageModel>(
                    BaseSelectSql + @"
                      WHERE (CAST(v.ID_VOYAGE AS CHAR) LIKE @Pattern
                         OR tv.POINT_DEPART LIKE @Pattern
                         OR tv.POINT_ARRIVEE LIKE @Pattern
                         OR v.STATUT LIKE @Pattern
                         OR vh.IMMATRICULATION LIKE @Pattern
                         OR tv.LIBELLE_TYPE_VOYAGE LIKE @Pattern)" + agenceFilter + @"
                      ORDER BY v.DATE_DEPART DESC",
                    new { Pattern = pattern, IdAgence = idAgence }
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur SearchAsync motCle={MotCle}", motCle);
                throw;
            }
        }

        public async Task<VoyageModel?> GetByIdAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            try
            {
                return await connection.QueryFirstOrDefaultAsync<VoyageModel>(
                    BaseSelectSql + " WHERE v.ID_VOYAGE = @Id",
                    new { Id = id }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetByIdAsync voyage id={Id}", id);
                throw;
            }
        }

        public async Task<bool> AddAsync(VoyageModel voyage)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();

                _logger.LogInformation("Insertion voyage: Véhicule={IdVehicule}, Type={IdType}, DateDepart={DateDepart}",
                    voyage.Id_Vehicule, voyage.Id_Type_Voyage, voyage.Date_Depart);

                // Calculer le prochain numéro journalier pour ce type de voyage ce jour-là
                var numeroJournalier = await connection.ExecuteScalarAsync<int>(
                    @"SELECT COALESCE(MAX(Numero_Journalier), 0) + 1
                      FROM VOYAGE
                      WHERE ID_TYPE_VOYAGE = @IdType
                        AND DATE(DATE_DEPART) = DATE(@DateDepart)",
                    new { IdType = voyage.Id_Type_Voyage, DateDepart = voyage.Date_Depart },
                    transaction
                );
                voyage.Numero_Journalier = numeroJournalier;

                var rowsAffected = await connection.ExecuteAsync(
                    @"INSERT INTO VOYAGE (ID_VEHICULE, ID_TYPE_VOYAGE,
                                          DATE_DEPART, DATE_ARRIVEE, HEURE_DEPART, HEURE_ARRIVEE, DUREE, STATUT, Id_Agence, Numero_Journalier)
                      VALUES (@Id_Vehicule, @Id_Type_Voyage,
                              @Date_Depart, @Date_Arrivee, @Heure_Depart, @Heure_Arrivee, @Duree, @Statut, @Id_Agence, @Numero_Journalier)",
                    new
                    {
                        voyage.Id_Vehicule,
                        voyage.Id_Type_Voyage,
                        voyage.Date_Depart,
                        Date_Arrivee = (object?)voyage.Date_Arrivee ?? DBNull.Value,
                        Heure_Depart = (object?)voyage.Heure_Depart ?? DBNull.Value,
                        voyage.Heure_Arrivee,
                        voyage.Duree,
                        voyage.Statut,
                        voyage.Id_Agence,
                        voyage.Numero_Journalier
                    },
                    transaction
                );

                if (rowsAffected > 0 && voyage.Id_Chauffeur.HasValue)
                {
                    var newId = await connection.ExecuteScalarAsync<int>("SELECT LAST_INSERT_ID()", transaction: transaction);

                    // Vérifier que le chauffeur n'est pas déjà sur un autre voyage au même moment
                    var conflit = await connection.ExecuteScalarAsync<int>(
                        @"SELECT COUNT(1) FROM ASSIGNATION_CHAUFFEUR_VOYAGE acv
                          JOIN VOYAGE v2 ON v2.ID_VOYAGE = acv.ID_VOYAGE
                          WHERE acv.ID_CHAUFFEUR = @IdChauffeur
                            AND TIMESTAMP(v2.DATE_DEPART, v2.HEURE_DEPART)  < @Fin
                            AND TIMESTAMP(v2.DATE_ARRIVEE, COALESCE(v2.HEURE_ARRIVEE,'23:59:59')) > @Debut",
                        new
                        {
                            IdChauffeur = voyage.Id_Chauffeur.Value,
                            Debut = voyage.Date_Depart.Date.Add(voyage.Heure_Depart ?? TimeSpan.Zero),
                            Fin   = (voyage.Date_Arrivee ?? voyage.Date_Depart).Date.Add(voyage.Heure_Arrivee ?? TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)))
                        },
                        transaction
                    );
                    if (conflit > 0)
                        throw new InvalidOperationException("Ce chauffeur est déjà assigné à un autre voyage sur ce créneau horaire.");

                    await connection.ExecuteAsync(
                        @"INSERT INTO ASSIGNATION_CHAUFFEUR_VOYAGE (ID_CHAUFFEUR, ID_VOYAGE)
                          VALUES (@Id_Chauffeur, @Id_Voyage)",
                        new { Id_Chauffeur = voyage.Id_Chauffeur.Value, Id_Voyage = newId },
                        transaction
                    );
                }

                await transaction.CommitAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur AddAsync voyage");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(VoyageModel voyage)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();

                var rowsAffected = await connection.ExecuteAsync(
                    @"UPDATE VOYAGE
                      SET ID_VEHICULE = @Id_Vehicule, ID_TYPE_VOYAGE = @Id_Type_Voyage,
                          DATE_DEPART = @Date_Depart, DATE_ARRIVEE = @Date_Arrivee,
                          HEURE_DEPART = @Heure_Depart, HEURE_ARRIVEE = @Heure_Arrivee,
                          DUREE = @Duree, STATUT = @Statut
                      WHERE ID_VOYAGE = @Id",
                    new
                    {
                        Id = voyage.Id_Voyage,
                        voyage.Id_Vehicule,
                        voyage.Id_Type_Voyage,
                        voyage.Date_Depart,
                        Date_Arrivee = (object?)voyage.Date_Arrivee ?? DBNull.Value,
                        Heure_Depart = (object?)voyage.Heure_Depart ?? DBNull.Value,
                        voyage.Heure_Arrivee,
                        voyage.Duree,
                        voyage.Statut
                    },
                    transaction
                );

                // Mettre à jour l'assignation chauffeur
                await connection.ExecuteAsync(
                    "DELETE FROM ASSIGNATION_CHAUFFEUR_VOYAGE WHERE ID_VOYAGE = @Id_Voyage",
                    new { Id_Voyage = voyage.Id_Voyage },
                    transaction
                );

                if (voyage.Id_Chauffeur.HasValue)
                {
                    // Vérifier conflit chauffeur (exclure ce voyage)
                    var conflit = await connection.ExecuteScalarAsync<int>(
                        @"SELECT COUNT(1) FROM ASSIGNATION_CHAUFFEUR_VOYAGE acv
                          JOIN VOYAGE v2 ON v2.ID_VOYAGE = acv.ID_VOYAGE
                          WHERE acv.ID_CHAUFFEUR = @IdChauffeur
                            AND acv.ID_VOYAGE <> @IdVoyage
                            AND TIMESTAMP(v2.DATE_DEPART, v2.HEURE_DEPART)  < @Fin
                            AND TIMESTAMP(v2.DATE_ARRIVEE, COALESCE(v2.HEURE_ARRIVEE,'23:59:59')) > @Debut",
                        new
                        {
                            IdChauffeur = voyage.Id_Chauffeur.Value,
                            IdVoyage    = voyage.Id_Voyage,
                            Debut = voyage.Date_Depart.Date.Add(voyage.Heure_Depart ?? TimeSpan.Zero),
                            Fin   = (voyage.Date_Arrivee ?? voyage.Date_Depart).Date.Add(voyage.Heure_Arrivee ?? TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)))
                        },
                        transaction
                    );
                    if (conflit > 0)
                        throw new InvalidOperationException("Ce chauffeur est déjà assigné à un autre voyage sur ce créneau horaire.");

                    await connection.ExecuteAsync(
                        @"INSERT INTO ASSIGNATION_CHAUFFEUR_VOYAGE (ID_CHAUFFEUR, ID_VOYAGE)
                          VALUES (@Id_Chauffeur, @Id_Voyage)",
                        new { Id_Chauffeur = voyage.Id_Chauffeur.Value, Id_Voyage = voyage.Id_Voyage },
                        transaction
                    );
                }

                await transaction.CommitAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur UpdateAsync voyage id={Id}", voyage.Id_Voyage);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                // Nettoyer les enregistrements opérationnels liés au voyage avant suppression
                await connection.ExecuteAsync(
                    "DELETE FROM ASSIGNATION_CHAUFFEUR_VOYAGE WHERE ID_VOYAGE = @Id",
                    new { Id = id }, transaction);

                await connection.ExecuteAsync(
                    "DELETE FROM EMBARQUEMENT_VOYAGE_PASSAGER WHERE ID_VOYAGE = @Id",
                    new { Id = id }, transaction);

                var rowsAffected = await connection.ExecuteAsync(
                    "DELETE FROM VOYAGE WHERE ID_VOYAGE = @Id",
                    new { Id = id }, transaction);

                await transaction.CommitAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erreur DeleteAsync voyage id={Id}", id);
                throw;
            }
        }

        public async Task<bool> UpdateStatutAsync(int id, string statut)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var rowsAffected = await connection.ExecuteAsync(
                    "UPDATE VOYAGE SET STATUT = @Statut WHERE ID_VOYAGE = @Id",
                    new { Id = id, Statut = statut }
                );

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur UpdateStatutAsync voyage id={Id}", id);
                throw;
            }
        }

        public async Task<bool> HasScheduleConflictAsync(
            int idVehicule,
            DateTime dateDepart,
            DateTime dateArrivee,
            TimeSpan heureDepart,
            TimeSpan heureArrivee,
            int? excludeVoyageId = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var proposedStart = dateDepart.Date.Add(heureDepart);
            var proposedEnd = dateArrivee.Date.Add(heureArrivee);

            var count = await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(1)
                  FROM VOYAGE
                  WHERE ID_VEHICULE = @IdVehicule
                    AND (@ExcludeVoyageId IS NULL OR ID_VOYAGE <> @ExcludeVoyageId)
                    AND TIMESTAMP(DATE_DEPART, HEURE_DEPART) < @ProposedEnd
                    AND TIMESTAMP(DATE_ARRIVEE, COALESCE(HEURE_ARRIVEE, '23:59:59')) > @ProposedStart",
                new
                {
                    IdVehicule = idVehicule,
                    ExcludeVoyageId = excludeVoyageId,
                    ProposedStart = proposedStart,
                    ProposedEnd = proposedEnd
                }
            );

            return count > 0;
        }
    }
}
