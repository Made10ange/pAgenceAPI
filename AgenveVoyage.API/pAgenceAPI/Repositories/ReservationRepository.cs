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
                r.ID_reservation    as Id_Reservation,
                r.REFERENCE         as Reference,
                r.ID_voyage         as Id_Voyage,
                r.ID_passager       as Id_Passager,
                r.NOM_CLIENT        as Nom_Client,
                r.PRENOM_CLIENT     as Prenom_Client,
                r.TELEPHONE_CLIENT  as Telephone_Client,
                r.NUMERO_CNI_CLIENT as Numero_Cni_Client,
                r.EMAIL_CLIENT      as Email_Client,
                r.NUMERO_SIEGE      as Numero_Siege,
                r.MONTANT           as Montant,
                r.STATUT_paiement   as Statut_Paiement,
                r.PROVIDER_paiement as Provider_Paiement,
                r.REFERENCE_paiement as Reference_Paiement,
                r.DATE_paiement     as Date_Paiement,
                r.STATUT_reservation as Statut_Reservation,
                r.VALIDEE_PAR       as Validee_Par,
                r.DATE_VALIDATION   as Date_Validation,
                r.DATE_CREATION     as Date_Creation,
                tv.POINT_DEPART     as Point_Depart,
                tv.POINT_ARRIVEE    as Point_Arrivee,
                v.DATE_DEPART       as Date_Depart,
                v.HEURE_DEPART      as Heure_Depart,
                vh.IMMATRICULATION  as Immatriculation,
                tv.LIBELLE_TYPE_VOYAGE as Libelle_Type_Voyage
            FROM reservation r
            LEFT JOIN voyage v       ON r.ID_voyage        = v.ID_voyage
            LEFT JOIN type_voyage tv ON v.ID_type_voyage   = tv.ID_type_voyage
            LEFT JOIN vehicule vh    ON v.ID_vehicule      = vh.ID_vehicule";

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
                BaseSelect + " WHERE r.ID_reservation = @Id", new { Id = id });
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
                BaseSelect + " WHERE r.ID_voyage = @Id ORDER BY r.NUMERO_SIEGE",
                new { Id = idVoyage });
            return result.ToList();
        }

        public async Task<List<ReservationModel>> GetPourEmbarquementAsync(int idVoyage)
        {
            // Toutes les réservations payées non utilisées du même type de voyage
            const string sql = @"
                SELECT r.ID_reservation    as Id_Reservation,
                       r.REFERENCE         as Reference,
                       r.ID_voyage         as Id_Voyage,
                       r.ID_passager       as Id_Passager,
                       r.NOM_CLIENT        as Nom_Client,
                       r.PRENOM_CLIENT     as Prenom_Client,
                       r.TELEPHONE_CLIENT  as Telephone_Client,
                       r.NUMERO_CNI_CLIENT as Numero_Cni_Client,
                       r.EMAIL_CLIENT      as Email_Client,
                       r.NUMERO_SIEGE      as Numero_Siege,
                       r.MONTANT           as Montant,
                       r.STATUT_paiement   as Statut_Paiement,
                       r.STATUT_reservation as Statut_Reservation,
                       tv.POINT_DEPART     as Point_Depart,
                       tv.POINT_ARRIVEE    as Point_Arrivee,
                       v.DATE_DEPART       as Date_Depart,
                       v.HEURE_DEPART      as Heure_Depart,
                       tv.LIBELLE_TYPE_VOYAGE as Libelle_Type_Voyage
                FROM reservation r
                JOIN voyage v_cible ON v_cible.Id_Voyage = @idVoyage
                JOIN voyage v       ON v.Id_Voyage       = r.ID_voyage
                JOIN type_voyage tv ON tv.Id_Type_Voyage = v.Id_Type_Voyage
                WHERE r.STATUT_paiement = 'Payé'
                  AND r.STATUT_reservation NOT IN ('Utilisée', 'Annulée')
                  AND v.Id_Type_Voyage = v_cible.Id_Type_Voyage
                  AND r.ID_passager IS NOT NULL
                ORDER BY r.DATE_CREATION ASC";

            using var conn = new MySqlConnection(_connectionString);
            var result = await conn.QueryAsync<ReservationModel>(sql, new { idVoyage });
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
                "SELECT COUNT(*) FROM reservation WHERE REFERENCE = @Ref",
                new { Ref = r.Reference }) > 0)
            {
                rand = new Random().Next(1000, 9999);
                r.Reference = $"RES-{date}-{rand}";
            }

            var sql = @"
                INSERT INTO reservation
                    (REFERENCE, ID_voyage, ID_passager, NOM_CLIENT, PRENOM_CLIENT,
                     TELEPHONE_CLIENT, NUMERO_CNI_CLIENT, EMAIL_CLIENT, NUMERO_SIEGE, MONTANT,
                     STATUT_paiement, STATUT_reservation)
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
                UPDATE reservation SET
                    STATUT_paiement    = @StatutP,
                    REFERENCE_paiement = @RefP,
                    PROVIDER_paiement  = @Provider,
                    DATE_paiement      = @DateP,
                    STATUT_reservation = @StatutR
                WHERE ID_reservation = @Id",
                new { StatutP = statutPaiement, RefP = referencePaiement, Provider = provider,
                      DateP = datePaiement, StatutR = statutRes, Id = id });

            // Quand le paiement est confirmé, créer le passager s'il n'existe pas encore
            if (statutPaiement == "Payé" && rows > 0)
            {
                try { await LierOuCreerPassagerAsync(conn, id); } catch { /* non bloquant */ }
            }

            return rows > 0;
        }

        private async Task LierOuCreerPassagerAsync(MySqlConnection conn, int idReservation)
        {
            // Récupérer les infos client + agence de la réservation
            var res = await conn.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT r.NOM_CLIENT, r.PRENOM_CLIENT, r.TELEPHONE_CLIENT,
                       r.NUMERO_CNI_CLIENT, r.EMAIL_CLIENT, r.ID_passager,
                       v.Id_Agence
                FROM reservation r
                LEFT JOIN voyage v ON r.ID_voyage = v.ID_voyage
                WHERE r.ID_reservation = @Id", new { Id = idReservation });

            if (res == null) return;

            // Déjà lié à un passager
            if (res.ID_passager != null && (int)res.ID_passager > 0) return;

            string? tel  = res.TELEPHONE_CLIENT;
            string? nom  = res.NOM_CLIENT;
            string? prenom = res.PRENOM_CLIENT;
            string? cni  = res.NUMERO_CNI_CLIENT;
            string? email = res.EMAIL_CLIENT;
            int? idAgence = (int?)res.Id_Agence;

            if (string.IsNullOrWhiteSpace(tel) && string.IsNullOrWhiteSpace(nom)) return;

            // Chercher un passager existant uniquement si le nom ET le prénom correspondent exactement
            // (évite de lier la réservation à un passager différent qui aurait le même téléphone)
            int? idPassager = null;
            if (!string.IsNullOrWhiteSpace(nom) && !string.IsNullOrWhiteSpace(prenom))
            {
                idPassager = await conn.ExecuteScalarAsync<int?>(@"
                    SELECT ID_passager FROM passager
                    WHERE LOWER(NOM) = LOWER(@Nom) AND LOWER(PRENOM) = LOWER(@Prenom)
                    LIMIT 1", new { Nom = nom, Prenom = prenom });
            }

            // Créer le passager s'il n'existe pas (ou si le nom ne correspond à aucun existant)
            if (idPassager == null || idPassager == 0)
            {
                idPassager = await conn.ExecuteScalarAsync<int>(@"
                    INSERT INTO passager
                        (NOM, PRENOM, TELEPHONE, NUMERO_PIECE, EMAIL, SEXE, Id_Agence)
                    VALUES
                        (@Nom, @Prenom, @Tel, @Cni, @Email, 'Non précisé', @IdAgence);
                    SELECT LAST_INSERT_ID();",
                    new { Nom = nom, Prenom = prenom, Tel = tel, Cni = cni,
                          Email = email, IdAgence = idAgence });
            }

            // Lier la réservation au passager
            if (idPassager > 0)
            {
                await conn.ExecuteAsync(
                    "UPDATE reservation SET ID_passager = @IdP WHERE ID_reservation = @Id",
                    new { IdP = idPassager, Id = idReservation });

                // Créer l'entrée d'embarquement si elle n'existe pas déjà
                var idVoyage = await conn.ExecuteScalarAsync<int?>(
                    "SELECT ID_voyage FROM reservation WHERE ID_reservation = @Id",
                    new { Id = idReservation });

                var numSiege = await conn.ExecuteScalarAsync<int?>(
                    "SELECT NUMERO_SIEGE FROM reservation WHERE ID_reservation = @Id",
                    new { Id = idReservation });

                if (idVoyage > 0)
                {
                    var existeEmbarquement = await conn.ExecuteScalarAsync<int>(@"
                        SELECT COUNT(*) FROM embarquement_voyage_passager
                        WHERE ID_voyage = @IdV AND ID_passager = @IdP",
                        new { IdV = idVoyage, IdP = idPassager });

                    if (existeEmbarquement == 0)
                    {
                        await conn.ExecuteAsync(@"
                            INSERT INTO embarquement_voyage_passager
                                (ID_voyage, ID_passager, STATUT_EMBARQUEMENT, NUMERO_SIEGE, DATE_ENREGISTREMENT)
                            VALUES
                                (@IdV, @IdP, 'En attente', @Siege, @Date)",
                            new { IdV = idVoyage, IdP = idPassager, Siege = numSiege, Date = DateTime.Now });
                    }
                }
            }
        }

        public async Task<bool> SetPassagerAsync(int id, int idPassager)
        {
            using var conn = new MySqlConnection(_connectionString);
            var rows = await conn.ExecuteAsync(
                "UPDATE reservation SET ID_passager = @IdPassager WHERE ID_reservation = @Id",
                new { IdPassager = idPassager, Id = id });
            return rows > 0;
        }

        public async Task<bool> UpdateStatutReservationAsync(int id, string statut)
        {
            using var conn = new MySqlConnection(_connectionString);
            var rows = await conn.ExecuteAsync(
                "UPDATE reservation SET STATUT_reservation = @Statut WHERE ID_reservation = @Id",
                new { Statut = statut, Id = id });
            return rows > 0;
        }

        public async Task<bool> ValiderAsync(int id, string valideePar)
        {
            using var conn = new MySqlConnection(_connectionString);
            var rows = await conn.ExecuteAsync(@"
                UPDATE reservation SET
                    STATUT_reservation = 'Utilisée',
                    VALIDEE_PAR        = @ValideePar,
                    DATE_VALIDATION    = @Now
                WHERE ID_reservation = @Id AND STATUT_reservation = 'Confirmée'",
                new { ValideePar = valideePar, Now = DateTime.Now, Id = id });
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            var rows = await conn.ExecuteAsync(
                "DELETE FROM reservation WHERE ID_reservation = @Id", new { Id = id });
            return rows > 0;
        }

        public async Task<bool> SiegeDisponibleAsync(int idVoyage, int numeroSiege, int? excludeId = null)
        {
            using var conn = new MySqlConnection(_connectionString);
            var count = await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM reservation
                WHERE ID_voyage = @IdVoyage
                  AND NUMERO_SIEGE = @Siege
                  AND STATUT_reservation NOT IN ('Annulée')
                  AND (@ExcludeId IS NULL OR ID_reservation <> @ExcludeId)",
                new { IdVoyage = idVoyage, Siege = numeroSiege, ExcludeId = excludeId });
            return count == 0;
        }

        public async Task AjouterLogAsync(PaiementLogModel log)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.ExecuteAsync(@"
                INSERT INTO paiement_log
                    (ID_reservation, EVENEMENT, MONTANT, REFERENCE_EXTERNE, PAYLOAD_BRUT)
                VALUES
                    (@Id_Reservation, @Evenement, @Montant, @Reference_Externe, @Payload_Brut)",
                log);
        }
    }
}
