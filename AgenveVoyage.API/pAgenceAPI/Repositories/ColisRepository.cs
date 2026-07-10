using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class ColisRepository : IColisRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<ColisRepository> _logger;

        private const string BaseSelectSql =
            @"SELECT
                c.ID_COLIS                   AS Id_Colis,
                c.REFERENCE_COLIS            AS Reference_Colis,
                c.ID_voyage                  AS Id_Voyage,
                c.ID_agence                  AS Id_Agence,
                c.NOM_EXPEDITEUR             AS Nom_Expediteur,
                c.TEL_EXPEDITEUR             AS Tel_Expediteur,
                c.NOM_DESTINATAIRE           AS Nom_Destinataire,
                c.TEL_DESTINATAIRE           AS Tel_Destinataire,
                c.VILLE_DEPART               AS Ville_Depart,
                c.VILLE_ARRIVEE              AS Ville_Arrivee,
                c.DESCRIPTION                AS Description,
                c.POIDS                      AS Poids,
                c.VALEUR_DECLAREE            AS Valeur_Declaree,
                c.PRIX_TRANSPORT             AS Prix_Transport,
                c.MODE_paiement             AS Mode_Paiement,
                c.STATUT                     AS Statut,
                c.DATE_ENVOI                 AS Date_Envoi,
                c.DATE_LIVRAISON_PREVUE      AS Date_Livraison_Prevue,
                c.DATE_LIVRAISON_EFFECTIVE   AS Date_Livraison_Effective,
                CONCAT(COALESCE(tv.POINT_DEPART,''), ' → ', COALESCE(tv.POINT_ARRIVEE,'')) AS Trajet_Voyage,
                a.NOM_agence AS Nom_Agence
              FROM colis c
              LEFT JOIN voyage  v  ON c.ID_voyage  = v.ID_voyage
              LEFT JOIN type_voyage tv ON v.ID_type_voyage = tv.ID_type_voyage
              LEFT JOIN agence  a  ON c.ID_agence  = a.ID_agence";

        public ColisRepository(IConfiguration configuration, ILogger<ColisRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<List<ColisModel>> GetAllAsync(int? idAgence = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var sql = idAgence.HasValue
                    ? BaseSelectSql + " WHERE c.ID_agence = @IdAgence ORDER BY c.DATE_ENVOI DESC"
                    : BaseSelectSql + " ORDER BY c.DATE_ENVOI DESC";
                return (await connection.QueryAsync<ColisModel>(sql, new { IdAgence = idAgence })).ToList();
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur GetAllAsync colis"); throw; }
        }

        public async Task<ColisModel?> GetByIdAsync(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return await connection.QueryFirstOrDefaultAsync<ColisModel>(
                    BaseSelectSql + " WHERE c.ID_COLIS = @Id", new { Id = id });
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur GetByIdAsync colis id={Id}", id); throw; }
        }

        public async Task<ColisModel?> GetByReferenceAsync(string reference)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return await connection.QueryFirstOrDefaultAsync<ColisModel>(
                    BaseSelectSql + " WHERE c.REFERENCE_COLIS = @Ref", new { Ref = reference });
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur GetByReferenceAsync colis ref={Ref}", reference); throw; }
        }

        public async Task<List<ColisModel>> GetByVoyageAsync(int idVoyage)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return (await connection.QueryAsync<ColisModel>(
                    BaseSelectSql + " WHERE c.ID_voyage = @Id ORDER BY c.DATE_ENVOI DESC",
                    new { Id = idVoyage }
                )).ToList();
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur GetByVoyageAsync colis id={Id}", idVoyage); throw; }
        }

        public async Task<List<ColisModel>> GetByTrajetVoyageAsync(int idVoyage)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return (await connection.QueryAsync<ColisModel>(
                    BaseSelectSql + @"
                    JOIN voyage v ON v.ID_voyage = @idVoyage
                    LEFT JOIN type_voyage tv ON tv.ID_type_voyage = v.ID_type_voyage
                    WHERE (
                        c.ID_VOYAGE = @idVoyage
                        OR (
                            tv.POINT_DEPART IS NOT NULL
                            AND LOWER(c.VILLE_DEPART)  = LOWER(tv.POINT_DEPART)
                            AND LOWER(c.VILLE_ARRIVEE) = LOWER(tv.POINT_ARRIVEE)
                        )
                    )
                      AND c.STATUT NOT IN ('Livré', 'Annulé')
                    ORDER BY c.DATE_ENVOI DESC",
                    new { idVoyage }
                )).ToList();
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur GetByTrajetVoyageAsync idVoyage={Id}", idVoyage); throw; }
        }

        public async Task<List<ColisModel>> GetByStatutAsync(string statut)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                return (await connection.QueryAsync<ColisModel>(
                    BaseSelectSql + " WHERE c.STATUT = @Statut ORDER BY c.DATE_ENVOI DESC",
                    new { Statut = statut }
                )).ToList();
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur GetByStatutAsync colis statut={Statut}", statut); throw; }
        }

        public async Task<List<ColisModel>> SearchAsync(string motCle)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var pattern = $"%{motCle.Trim()}%";
                return (await connection.QueryAsync<ColisModel>(
                    BaseSelectSql + @"
                    WHERE c.REFERENCE_COLIS    LIKE @Pattern
                       OR c.NOM_EXPEDITEUR     LIKE @Pattern
                       OR c.NOM_DESTINATAIRE   LIKE @Pattern
                       OR c.VILLE_DEPART       LIKE @Pattern
                       OR c.VILLE_ARRIVEE      LIKE @Pattern
                       OR c.STATUT             LIKE @Pattern
                       OR c.DESCRIPTION        LIKE @Pattern
                    ORDER BY c.DATE_ENVOI DESC",
                    new { Pattern = pattern }
                )).ToList();
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur SearchAsync colis"); throw; }
        }

        public async Task<string> GenererReferenceAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var today = DateTime.Now.ToString("yyyyMMdd");
                var count = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM colis WHERE DATE(DATE_ENVOI) = CURDATE()");
                return $"COL-{today}-{(count + 1):D4}";
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur GenererReferenceAsync"); throw; }
        }

        public async Task<int> AddAsync(ColisModel colis)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                if (string.IsNullOrWhiteSpace(colis.Reference_Colis))
                    colis.Reference_Colis = await GenererReferenceAsync();

                var newId = await connection.ExecuteScalarAsync<int>(
                    @"INSERT INTO colis
                      (REFERENCE_COLIS, ID_voyage, ID_agence, NOM_EXPEDITEUR, TEL_EXPEDITEUR,
                       NOM_DESTINATAIRE, TEL_DESTINATAIRE, VILLE_DEPART, VILLE_ARRIVEE,
                       DESCRIPTION, POIDS, VALEUR_DECLAREE, PRIX_TRANSPORT, MODE_paiement, STATUT,
                       DATE_ENVOI, DATE_LIVRAISON_PREVUE, DATE_LIVRAISON_EFFECTIVE)
                      VALUES
                      (@Reference_Colis, @Id_Voyage, @Id_Agence, @Nom_Expediteur, @Tel_Expediteur,
                       @Nom_Destinataire, @Tel_Destinataire, @Ville_Depart, @Ville_Arrivee,
                       @Description, @Poids, @Valeur_Declaree, @Prix_Transport, @Mode_Paiement, @Statut,
                       @Date_Envoi, @Date_Livraison_Prevue, @Date_Livraison_Effective);
                      SELECT LAST_INSERT_ID();",
                    new {
                        colis.Reference_Colis,
                        Id_Voyage = colis.Id_Voyage == 0 ? (int?)null : colis.Id_Voyage,
                        colis.Id_Agence,
                        colis.Nom_Expediteur, colis.Tel_Expediteur,
                        colis.Nom_Destinataire, colis.Tel_Destinataire,
                        colis.Ville_Depart, colis.Ville_Arrivee,
                        colis.Description, colis.Poids, colis.Valeur_Declaree, colis.Prix_Transport,
                        Mode_Paiement = colis.Mode_Paiement ?? "Espèces",
                        Statut = colis.Statut ?? "En attente",
                        Date_Envoi = colis.Date_Envoi == default ? DateTime.Now : colis.Date_Envoi,
                        colis.Date_Livraison_Prevue, colis.Date_Livraison_Effective
                    });
                return newId;
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur AddAsync colis"); throw; }
        }

        public async Task<string> UpdateAsync(ColisModel colis)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    @"UPDATE colis SET
                        ID_voyage                 = @Id_Voyage,
                        ID_agence                 = @Id_Agence,
                        NOM_EXPEDITEUR            = @Nom_Expediteur,
                        TEL_EXPEDITEUR            = @Tel_Expediteur,
                        NOM_DESTINATAIRE          = @Nom_Destinataire,
                        TEL_DESTINATAIRE          = @Tel_Destinataire,
                        VILLE_DEPART              = @Ville_Depart,
                        VILLE_ARRIVEE             = @Ville_Arrivee,
                        DESCRIPTION               = @Description,
                        POIDS                     = @Poids,
                        VALEUR_DECLAREE           = @Valeur_Declaree,
                        PRIX_TRANSPORT            = @Prix_Transport,
                        STATUT                    = @Statut,
                        DATE_LIVRAISON_PREVUE     = @Date_Livraison_Prevue,
                        DATE_LIVRAISON_EFFECTIVE  = @Date_Livraison_Effective
                      WHERE ID_COLIS = @Id_Colis",
                    new {
                        colis.Id_Colis, colis.Id_Voyage, colis.Id_Agence,
                        colis.Nom_Expediteur, colis.Tel_Expediteur,
                        colis.Nom_Destinataire, colis.Tel_Destinataire,
                        colis.Ville_Depart, colis.Ville_Arrivee,
                        colis.Description, colis.Poids, colis.Valeur_Declaree, colis.Prix_Transport,
                        colis.Statut, colis.Date_Livraison_Prevue, colis.Date_Livraison_Effective
                    });
                return "Colis modifié avec succès !";
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur UpdateAsync colis id={Id}", colis.Id_Colis); throw; }
        }

        public async Task<string> UpdateStatutAsync(int id, string statut)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var extra = statut == "Livré"
                    ? ", DATE_LIVRAISON_EFFECTIVE = CURDATE()"
                    : "";
                await connection.ExecuteAsync(
                    $"UPDATE colis SET STATUT = @Statut{extra} WHERE ID_COLIS = @Id",
                    new { Id = id, Statut = statut });
                return "Statut mis à jour !";
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur UpdateStatutAsync colis id={Id}", id); throw; }
        }

        public async Task<int> UpdateStatutByVoyageAsync(int idVoyage, string statutActuel, string nouveauStatut)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var extra = nouveauStatut == "Livré" ? ", DATE_LIVRAISON_EFFECTIVE = CURDATE()" : "";
                return await connection.ExecuteAsync(
                    $"UPDATE colis SET STATUT = @NouveauStatut{extra} WHERE ID_voyage = @IdVoyage AND STATUT = @StatutActuel",
                    new { IdVoyage = idVoyage, StatutActuel = statutActuel, NouveauStatut = nouveauStatut });
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur UpdateStatutByVoyageAsync voyage id={Id}", idVoyage); throw; }
        }

        public async Task<string> AssignerVoyageAsync(int idColis, int idVoyage)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    "UPDATE colis SET ID_voyage = @IdVoyage, STATUT = 'En cours' WHERE ID_COLIS = @IdColis",
                    new { IdColis = idColis, IdVoyage = idVoyage });
                return "Colis chargé sur le voyage !";
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur AssignerVoyageAsync colis id={Id}", idColis); throw; }
        }

        public async Task LivrerParVoyageAsync(int idVoyage)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    "UPDATE colis SET STATUT = 'Livré', DATE_LIVRAISON_EFFECTIVE = CURDATE() WHERE ID_voyage = @IdVoyage AND STATUT NOT IN ('Livré','Retourné','Annulé')",
                    new { IdVoyage = idVoyage });
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur LivrerParVoyageAsync voyage id={Id}", idVoyage); throw; }
        }

        public async Task<string> DeleteAsync(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync("DELETE FROM colis WHERE ID_COLIS = @Id", new { Id = id });
                return "Colis supprimé avec succès !";
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur DeleteAsync colis id={Id}", id); throw; }
        }
    }
}
