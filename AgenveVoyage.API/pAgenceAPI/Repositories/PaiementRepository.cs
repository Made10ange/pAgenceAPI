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
                p.ID_paiement       AS Id_Paiement,
                p.TYPE_paiement     AS Type_Paiement,
                p.ID_passager       AS Id_Passager,
                p.ID_COLIS          AS Id_Colis,
                p.ID_voyage         AS Id_Voyage,
                p.MONTANT           AS Montant,
                p.MODE_paiement     AS Mode_Paiement,
                p.STATUT            AS Statut,
                p.DATE_paiement     AS Date_Paiement,
                p.NOTES             AS Notes,
                CONCAT(IFNULL(pa.NOM,''), ' ', IFNULL(pa.PRENOM,'')) AS Nom_Passager,
                c.REFERENCE_COLIS   AS Reference_Colis,
                CONCAT(tv.POINT_DEPART, ' -> ', tv.POINT_ARRIVEE) AS Trajet_Voyage
              FROM paiement p
              LEFT JOIN passager pa    ON p.ID_passager    = pa.ID_passager
              LEFT JOIN colis c        ON p.ID_COLIS       = c.ID_COLIS
              LEFT JOIN voyage v       ON p.ID_voyage      = v.ID_voyage
              LEFT JOIN type_voyage tv ON v.ID_type_voyage  = tv.ID_type_voyage";

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
                $"{BaseSelectSql} {where} ORDER BY p.DATE_paiement DESC", new { idAgence });
            return result.ToList();
        }

        public async Task<PaiementModel?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<PaiementModel>(
                $"{BaseSelectSql} WHERE p.ID_paiement = @id", new { id });
        }

        public async Task<List<PaiementModel>> GetByPassagerAsync(int idPassager)
        {
            using var conn = CreateConnection();
            var result = await conn.QueryAsync<PaiementModel>(
                $"{BaseSelectSql} WHERE p.ID_passager = @idPassager ORDER BY p.DATE_paiement DESC",
                new { idPassager });
            return result.ToList();
        }

        public async Task<List<PaiementModel>> GetByColisAsync(int idColis)
        {
            using var conn = CreateConnection();
            var result = await conn.QueryAsync<PaiementModel>(
                $"{BaseSelectSql} WHERE p.ID_COLIS = @idColis ORDER BY p.DATE_paiement DESC",
                new { idColis });
            return result.ToList();
        }

        public async Task<List<PaiementModel>> GetByVoyageAsync(int idVoyage)
        {
            using var conn = CreateConnection();
            var result = await conn.QueryAsync<PaiementModel>(
                $"{BaseSelectSql} WHERE p.ID_voyage = @idVoyage ORDER BY p.DATE_paiement DESC",
                new { idVoyage });
            return result.ToList();
        }

        public async Task<List<PaiementModel>> GetByPeriodeAsync(DateTime dateDebut, DateTime dateFin)
        {
            using var conn = CreateConnection();
            var result = await conn.QueryAsync<PaiementModel>(
                $"{BaseSelectSql} WHERE DATE(p.DATE_paiement) BETWEEN @dateDebut AND @dateFin ORDER BY p.DATE_paiement DESC",
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
                      OR p.MODE_paiement LIKE @motCle
                      OR p.STATUT LIKE @motCle
                   ORDER BY p.DATE_paiement DESC",
                new { motCle = $"%{motCle}%" });
            return result.ToList();
        }

        public async Task<string> AddAsync(PaiementModel p)
        {
            const string sql = @"
                INSERT INTO paiement
                    (TYPE_paiement, ID_passager, ID_COLIS, ID_voyage, MONTANT,
                     MODE_paiement, STATUT, DATE_paiement, NOTES)
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
                UPDATE paiement SET
                    TYPE_paiement = @Type_Paiement,
                    ID_passager   = @Id_Passager,
                    ID_COLIS      = @Id_Colis,
                    ID_voyage     = @Id_Voyage,
                    MONTANT       = @Montant,
                    MODE_paiement = @Mode_Paiement,
                    STATUT        = @Statut,
                    DATE_paiement = @Date_Paiement,
                    NOTES         = @Notes
                WHERE ID_paiement = @Id_Paiement";
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync(sql, p);
            return rows > 0 ? "Paiement modifié avec succès." : "Paiement introuvable.";
        }

        public async Task<string> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync("DELETE FROM paiement WHERE ID_paiement = @id", new { id });
            return rows > 0 ? "Paiement supprimé avec succès." : "Paiement introuvable.";
        }

        public async Task<decimal> GetTotalByPeriodeAsync(DateTime dateDebut, DateTime dateFin)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<decimal>(
                @"SELECT IFNULL(SUM(MONTANT),0) FROM paiement
                  WHERE STATUT = 'Payé' AND DATE(DATE_paiement) BETWEEN @dateDebut AND @dateFin",
                new { dateDebut = dateDebut.Date, dateFin = dateFin.Date });
        }

        public async Task<List<(int Mois, decimal Total)>> GetChiffreAffaireMensuelAsync(int annee, int? idAgence = null)
        {
            using var conn = CreateConnection();
            var where = idAgence.HasValue
                ? "AND (v.Id_Agence IS NULL OR v.Id_Agence = @idAgence) AND (c.Id_Agence IS NULL OR c.Id_Agence = @idAgence)"
                : "";
            var rows = await conn.QueryAsync<(int Mois, decimal Total)>(
                $@"SELECT MONTH(p.DATE_paiement) AS Mois, SUM(p.MONTANT) AS Total
                   FROM paiement p
                   LEFT JOIN voyage v ON p.ID_voyage = v.ID_voyage
                   LEFT JOIN colis c  ON p.ID_COLIS  = c.ID_COLIS
                   WHERE p.STATUT = 'Payé' AND YEAR(p.DATE_paiement) = @annee {where}
                   GROUP BY MONTH(p.DATE_paiement)",
                new { annee, idAgence });

            var parMois = rows.ToDictionary(r => r.Mois, r => r.Total);
            return Enumerable.Range(1, 12)
                .Select(m => (Mois: m, Total: parMois.TryGetValue(m, out var t) ? t : 0m))
                .ToList();
        }
    }
}
