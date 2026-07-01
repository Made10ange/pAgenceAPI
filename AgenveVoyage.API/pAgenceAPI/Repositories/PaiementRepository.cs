using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class PaiementRepository : IPaiementRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<PaiementRepository> _logger;

        private const string BaseSelectSql =
            @"SELECT
                p.ID_PAIEMENT       AS Id_Paiement,
                p.TYPE_PAIEMENT     AS Type_Paiement,
                p.ID_PASSAGER       AS Id_Passager,
                p.ID_COLIS          AS Id_Colis,
                p.ID_VOYAGE         AS Id_Voyage,
                p.MONTANT           AS Montant,
                p.MODE_PAIEMENT     AS Mode_Paiement,
                p.STATUT            AS Statut,
                p.DATE_PAIEMENT     AS Date_Paiement,
                p.NOTES             AS Notes,
                CONCAT(IFNULL(pa.NOM,''), ' ', IFNULL(pa.PRENOM,'')) AS Nom_Passager,
                c.REFERENCE_COLIS   AS Reference_Colis,
                CONCAT(tv.POINT_DEPART, ' -> ', tv.POINT_ARRIVEE) AS Trajet_Voyage
              FROM PAIEMENT p
              LEFT JOIN PASSAGER pa    ON p.ID_PASSAGER    = pa.ID_PASSAGER
              LEFT JOIN COLIS c        ON p.ID_COLIS       = c.ID_COLIS
              LEFT JOIN VOYAGE v       ON p.ID_VOYAGE      = v.ID_VOYAGE
              LEFT JOIN TYPE_VOYAGE tv ON v.ID_TYPE_VOYAGE  = tv.ID_TYPE_VOYAGE";

        public PaiementRepository(IConfiguration configuration, ILogger<PaiementRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string manquante.");
            _logger = logger;
        }

        private MySqlConnection CreateConnection() => new MySqlConnection(_connectionString);

        public async Task<List<PaiementModel>> GetAllAsync(int? idAgence = null)
        {
            using var conn = CreateConnection();
            // Un paiement appartient à l'agence du voyage ou du colis concerné.
            // Les paiements non rattachés à une agence (voyage/colis sans Id_Agence) restent visibles partout.
            var where = idAgence.HasValue
                ? "WHERE (v.Id_Agence IS NULL OR v.Id_Agence = @idAgence) AND (c.Id_Agence IS NULL OR c.Id_Agence = @idAgence)"
                : "";
            var result = await conn.QueryAsync<PaiementModel>(
                $"{BaseSelectSql} {where} ORDER BY p.DATE_PAIEMENT DESC", new { idAgence });
            return result.ToList();
        }

        public async Task<PaiementModel?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<PaiementModel>(
                $"{BaseSelectSql} WHERE p.ID_PAIEMENT = @id", new { id });
        }

        public async Task<List<PaiementModel>> GetByPassagerAsync(int idPassager)
        {
            using var conn = CreateConnection();
            var result = await conn.QueryAsync<PaiementModel>(
                $"{BaseSelectSql} WHERE p.ID_PASSAGER = @idPassager ORDER BY p.DATE_PAIEMENT DESC",
                new { idPassager });
            return result.ToList();
        }

        public async Task<List<PaiementModel>> GetByColisAsync(int idColis)
        {
            using var conn = CreateConnection();
            var result = await conn.QueryAsync<PaiementModel>(
                $"{BaseSelectSql} WHERE p.ID_COLIS = @idColis ORDER BY p.DATE_PAIEMENT DESC",
                new { idColis });
            return result.ToList();
        }

        public async Task<List<PaiementModel>> GetByVoyageAsync(int idVoyage)
        {
            using var conn = CreateConnection();
            var result = await conn.QueryAsync<PaiementModel>(
                $"{BaseSelectSql} WHERE p.ID_VOYAGE = @idVoyage ORDER BY p.DATE_PAIEMENT DESC",
                new { idVoyage });
            return result.ToList();
        }

        public async Task<List<PaiementModel>> GetByPeriodeAsync(DateTime dateDebut, DateTime dateFin)
        {
            using var conn = CreateConnection();
            var result = await conn.QueryAsync<PaiementModel>(
                $"{BaseSelectSql} WHERE DATE(p.DATE_PAIEMENT) BETWEEN @dateDebut AND @dateFin ORDER BY p.DATE_PAIEMENT DESC",
                new { dateDebut = dateDebut.Date, dateFin = dateFin.Date });
            return result.ToList();
        }

        public async Task<List<PaiementModel>> SearchAsync(string motCle)
        {
            using var conn = CreateConnection();
            var result = await conn.QueryAsync<PaiementModel>(
                $@"{BaseSelectSql}
                   WHERE pa.NOM LIKE @motCle OR pa.PRENOM LIKE @motCle
                      OR c.REFERENCE_COLIS LIKE @motCle
                      OR p.MODE_PAIEMENT LIKE @motCle
                      OR p.STATUT LIKE @motCle
                   ORDER BY p.DATE_PAIEMENT DESC",
                new { motCle = $"%{motCle}%" });
            return result.ToList();
        }

        public async Task<string> AddAsync(PaiementModel p)
        {
            const string sql = @"
                INSERT INTO PAIEMENT
                    (TYPE_PAIEMENT, ID_PASSAGER, ID_COLIS, ID_VOYAGE, MONTANT,
                     MODE_PAIEMENT, STATUT, DATE_PAIEMENT, NOTES)
                VALUES
                    (@Type_Paiement, @Id_Passager, @Id_Colis, @Id_Voyage, @Montant,
                     @Mode_Paiement, @Statut, @Date_Paiement, @Notes)";
            using var conn = CreateConnection();
            await conn.ExecuteAsync(sql, p);
            return "Paiement enregistré avec succès.";
        }

        public async Task<string> UpdateAsync(PaiementModel p)
        {
            const string sql = @"
                UPDATE PAIEMENT SET
                    TYPE_PAIEMENT = @Type_Paiement,
                    ID_PASSAGER   = @Id_Passager,
                    ID_COLIS      = @Id_Colis,
                    ID_VOYAGE     = @Id_Voyage,
                    MONTANT       = @Montant,
                    MODE_PAIEMENT = @Mode_Paiement,
                    STATUT        = @Statut,
                    DATE_PAIEMENT = @Date_Paiement,
                    NOTES         = @Notes
                WHERE ID_PAIEMENT = @Id_Paiement";
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync(sql, p);
            return rows > 0 ? "Paiement modifié avec succès." : "Paiement introuvable.";
        }

        public async Task<string> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync("DELETE FROM PAIEMENT WHERE ID_PAIEMENT = @id", new { id });
            return rows > 0 ? "Paiement supprimé avec succès." : "Paiement introuvable.";
        }

        public async Task<decimal> GetTotalByPeriodeAsync(DateTime dateDebut, DateTime dateFin)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<decimal>(
                @"SELECT IFNULL(SUM(MONTANT),0) FROM PAIEMENT
                  WHERE STATUT = 'Payé' AND DATE(DATE_PAIEMENT) BETWEEN @dateDebut AND @dateFin",
                new { dateDebut = dateDebut.Date, dateFin = dateFin.Date });
        }

        public async Task<List<(int Mois, decimal Total)>> GetChiffreAffaireMensuelAsync(int annee, int? idAgence = null)
        {
            using var conn = CreateConnection();
            var where = idAgence.HasValue
                ? "AND (v.Id_Agence IS NULL OR v.Id_Agence = @idAgence) AND (c.Id_Agence IS NULL OR c.Id_Agence = @idAgence)"
                : "";
            var rows = await conn.QueryAsync<(int Mois, decimal Total)>(
                $@"SELECT MONTH(p.DATE_PAIEMENT) AS Mois, SUM(p.MONTANT) AS Total
                   FROM PAIEMENT p
                   LEFT JOIN VOYAGE v ON p.ID_VOYAGE = v.ID_VOYAGE
                   LEFT JOIN COLIS c  ON p.ID_COLIS  = c.ID_COLIS
                   WHERE p.STATUT = 'Payé' AND YEAR(p.DATE_PAIEMENT) = @annee {where}
                   GROUP BY MONTH(p.DATE_PAIEMENT)",
                new { annee, idAgence });

            var parMois = rows.ToDictionary(r => r.Mois, r => r.Total);
            return Enumerable.Range(1, 12)
                .Select(m => (Mois: m, Total: parMois.TryGetValue(m, out var t) ? t : 0m))
                .ToList();
        }
    }
}
