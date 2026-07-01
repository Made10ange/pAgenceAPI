using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<ReservationRepository> _logger;

        private const string BaseSelect = @"
            SELECT
                r.ID_RESERVATION    as Id_Reservation,
                r.REFERENCE         as Reference,
                r.ID_VOYAGE         as Id_Voyage,
                r.ID_PASSAGER       as Id_Passager,
                r.NOM_CLIENT        as Nom_Client,
                r.PRENOM_CLIENT     as Prenom_Client,
                r.TELEPHONE_CLIENT  as Telephone_Client,
                r.NUMERO_CNI_CLIENT as Numero_Cni_Client,
                r.EMAIL_CLIENT      as Email_Client,
                r.NUMERO_SIEGE      as Numero_Siege,
                r.MONTANT           as Montant,
                r.STATUT_PAIEMENT   as Statut_Paiement,
                r.PROVIDER_PAIEMENT as Provider_Paiement,
                r.REFERENCE_PAIEMENT as Reference_Paiement,
                r.DATE_PAIEMENT     as Date_Paiement,
                r.STATUT_RESERVATION as Statut_Reservation,
                r.VALIDEE_PAR       as Validee_Par,
                r.DATE_VALIDATION   as Date_Validation,
                r.DATE_CREATION     as Date_Creation,
                tv.POINT_DEPART     as Point_Depart,
                tv.POINT_ARRIVEE    as Point_Arrivee,
                v.DATE_DEPART       as Date_Depart,
                v.HEURE_DEPART      as Heure_Depart,
                vh.IMMATRICULATION  as Immatriculation
            FROM RESERVATION r
            LEFT JOIN VOYAGE v       ON r.ID_VOYAGE        = v.ID_VOYAGE
            LEFT JOIN TYPE_VOYAGE tv ON v.ID_TYPE_VOYAGE   = tv.ID_TYPE_VOYAGE
            LEFT JOIN VEHICULE vh    ON v.ID_VEHICULE      = vh.ID_VEHICULE";

        public ReservationRepository(IConfiguration configuration, ILogger<ReservationRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string manquante.");
            _logger = logger;
        }

        public async Task<List<ReservationModel>> GetAllAsync()
        {
            using var conn = new MySqlConnection(_connectionString);
            var result = await conn.QueryAsync<ReservationModel>(
                BaseSelect + " ORDER BY r.DATE_CREATION DESC");
            return result.ToList();
        }

        public async Task<List<ReservationModel>> SearchAsync(string motCle)
        {
            using var conn = new MySqlConnection(_connectionString);
            var sql = BaseSelect + @"
                WHERE r.REFERENCE LIKE @q
                   OR r.NOM_CLIENT LIKE @q
                   OR r.PRENOM_CLIENT LIKE @q
                   OR r.TELEPHONE_CLIENT LIKE @q
                   OR v.POINT_DEPART LIKE @q
                   OR v.POINT_ARRIVEE LIKE @q
                ORDER BY r.DATE_CREATION DESC";
            var result = await conn.QueryAsync<ReservationModel>(sql, new { q = $"%{motCle}%" });
            return result.ToList();
        }

        public async Task<ReservationModel?> GetByIdAsync(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<ReservationModel>(
                BaseSelect + " WHERE r.ID_RESERVATION = @Id", new { Id = id });
        }

        public async Task<ReservationModel?> GetByReferenceAsync(string reference)
        {
            using var conn = new MySqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<ReservationModel>(
                BaseSelect + " WHERE r.REFERENCE = @Reference", new { Reference = reference });
        }

        public async Task<List<ReservationModel>> GetByVoyageAsync(int idVoyage)
        {
            using var conn = new MySqlConnection(_connectionString);
            var result = await conn.QueryAsync<ReservationModel>(
                BaseSelect + " WHERE r.ID_VOYAGE = @Id ORDER BY r.NUMERO_SIEGE",
                new { Id = idVoyage });
            return result.ToList();
        }

        public async Task<int> AddAsync(ReservationModel r)
        {
            // Générer la référence unique : RES-YYYYMMDD-XXXX
            var date = DateTime.Now.ToString("yyyyMMdd");
            var rand = new Random().Next(1000, 9999);
            r.Reference = $"RES-{date}-{rand}";

            using var conn = new MySqlConnection(_connectionString);

            // S'assurer que la référence est unique
            while (await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM RESERVATION WHERE REFERENCE = @Ref",
                new { Ref = r.Reference }) > 0)
            {
                rand = new Random().Next(1000, 9999);
                r.Reference = $"RES-{date}-{rand}";
            }

            var sql = @"
                INSERT INTO RESERVATION
                    (REFERENCE, ID_VOYAGE, ID_PASSAGER, NOM_CLIENT, PRENOM_CLIENT,
                     TELEPHONE_CLIENT, NUMERO_CNI_CLIENT, EMAIL_CLIENT, NUMERO_SIEGE, MONTANT,
                     STATUT_PAIEMENT, STATUT_RESERVATION)
                VALUES
                    (@Reference, @Id_Voyage, @Id_Passager, @Nom_Client, @Prenom_Client,
                     @Telephone_Client, @Numero_Cni_Client, @Email_Client, @Numero_Siege, @Montant,
                     @Statut_Paiement, @Statut_Reservation);
                SELECT LAST_INSERT_ID();";

            return await conn.ExecuteScalarAsync<int>(sql, r);
        }

        public async Task<bool> UpdateStatutPaiementAsync(int id, string statutPaiement, string? referencePaiement, string? provider)
        {
            using var conn = new MySqlConnection(_connectionString);
            var datePaiement = statutPaiement == "Payé" ? (DateTime?)DateTime.Now : null;
            var statutRes = statutPaiement == "Payé" ? "Confirmée" : "En attente";

            var rows = await conn.ExecuteAsync(@"
                UPDATE RESERVATION SET
                    STATUT_PAIEMENT    = @StatutP,
                    REFERENCE_PAIEMENT = @RefP,
                    PROVIDER_PAIEMENT  = @Provider,
                    DATE_PAIEMENT      = @DateP,
                    STATUT_RESERVATION = @StatutR
                WHERE ID_RESERVATION = @Id",
                new { StatutP = statutPaiement, RefP = referencePaiement, Provider = provider,
                      DateP = datePaiement, StatutR = statutRes, Id = id });
            return rows > 0;
        }

        public async Task<bool> SetPassagerAsync(int id, int idPassager)
        {
            using var conn = new MySqlConnection(_connectionString);
            var rows = await conn.ExecuteAsync(
                "UPDATE RESERVATION SET ID_PASSAGER = @IdPassager WHERE ID_RESERVATION = @Id",
                new { IdPassager = idPassager, Id = id });
            return rows > 0;
        }

        public async Task<bool> UpdateStatutReservationAsync(int id, string statut)
        {
            using var conn = new MySqlConnection(_connectionString);
            var rows = await conn.ExecuteAsync(
                "UPDATE RESERVATION SET STATUT_RESERVATION = @Statut WHERE ID_RESERVATION = @Id",
                new { Statut = statut, Id = id });
            return rows > 0;
        }

        public async Task<bool> ValiderAsync(int id, string valideePar)
        {
            using var conn = new MySqlConnection(_connectionString);
            var rows = await conn.ExecuteAsync(@"
                UPDATE RESERVATION SET
                    STATUT_RESERVATION = 'Utilisée',
                    VALIDEE_PAR        = @ValideePar,
                    DATE_VALIDATION    = @Now
                WHERE ID_RESERVATION = @Id AND STATUT_RESERVATION = 'Confirmée'",
                new { ValideePar = valideePar, Now = DateTime.Now, Id = id });
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            var rows = await conn.ExecuteAsync(
                "DELETE FROM RESERVATION WHERE ID_RESERVATION = @Id", new { Id = id });
            return rows > 0;
        }

        public async Task<bool> SiegeDisponibleAsync(int idVoyage, int numeroSiege, int? excludeId = null)
        {
            using var conn = new MySqlConnection(_connectionString);
            var count = await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM RESERVATION
                WHERE ID_VOYAGE = @IdVoyage
                  AND NUMERO_SIEGE = @Siege
                  AND STATUT_RESERVATION NOT IN ('Annulée')
                  AND (@ExcludeId IS NULL OR ID_RESERVATION <> @ExcludeId)",
                new { IdVoyage = idVoyage, Siege = numeroSiege, ExcludeId = excludeId });
            return count == 0;
        }

        public async Task AjouterLogAsync(PaiementLogModel log)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.ExecuteAsync(@"
                INSERT INTO PAIEMENT_LOG
                    (ID_RESERVATION, EVENEMENT, MONTANT, REFERENCE_EXTERNE, PAYLOAD_BRUT)
                VALUES
                    (@Id_Reservation, @Evenement, @Montant, @Reference_Externe, @Payload_Brut)",
                log);
        }
    }
}
