using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class BilletRepository : IBilletRepository
    {
        private readonly string _conn;
        private readonly ILogger<BilletRepository> _log;

        public BilletRepository(IConfiguration cfg, ILogger<BilletRepository> log)
        {
            _conn = cfg.GetConnectionString("DefaultConnection")!;
            _log  = log;
        }

        private const string BaseSelect = @"
            SELECT b.*,
                   p.Nom       AS Nom_Passager,
                   p.Prenom    AS Prenom_Passager,
                   p.Telephone AS Telephone_Passager,
                   tv.Libelle_Type_Voyage,
                   CONCAT(tvp.Point_Depart,' → ',tvp.Point_Arrivee) AS Libelle_Voyage_Prevu,
                   CONCAT(tvu.Point_Depart,' → ',tvu.Point_Arrivee) AS Libelle_Voyage_Utilise
            FROM   billet b
            LEFT JOIN passager    p   ON p.Id_Passager      = b.Id_Passager
            LEFT JOIN type_voyage tv  ON tv.Id_Type_Voyage  = b.Id_Type_Voyage
            LEFT JOIN voyage      vp  ON vp.Id_Voyage       = b.Id_Voyage_Prevu
            LEFT JOIN type_voyage tvp ON tvp.Id_Type_Voyage = vp.Id_Type_Voyage
            LEFT JOIN voyage      vu  ON vu.Id_Voyage       = b.Id_Voyage_Utilise
            LEFT JOIN type_voyage tvu ON tvu.Id_Type_Voyage = vu.Id_Type_Voyage ";

        public async Task<IEnumerable<BilletModel>> GetAllAsync()
        {
            using var db = new MySqlConnection(_conn);
            return await db.QueryAsync<BilletModel>(BaseSelect + "ORDER BY b.Date_Achat DESC");
        }

        public async Task<BilletModel?> GetByIdAsync(int id)
        {
            using var db = new MySqlConnection(_conn);
            return await db.QueryFirstOrDefaultAsync<BilletModel>(
                BaseSelect + "WHERE b.Id_Billet = @id", new { id });
        }

        public async Task<BilletModel?> GetByNumeroAsync(string numero)
        {
            using var db = new MySqlConnection(_conn);
            return await db.QueryFirstOrDefaultAsync<BilletModel>(
                BaseSelect + "WHERE b.Numero_Billet = @numero", new { numero });
        }

        public async Task<IEnumerable<BilletModel>> GetByPassagerAsync(int idPassager)
        {
            using var db = new MySqlConnection(_conn);
            return await db.QueryAsync<BilletModel>(
                BaseSelect + "WHERE b.Id_Passager = @idPassager ORDER BY b.Date_Achat DESC",
                new { idPassager });
        }

        public async Task<IEnumerable<BilletModel>> GetByStatutAsync(string statut)
        {
            using var db = new MySqlConnection(_conn);
            return await db.QueryAsync<BilletModel>(
                BaseSelect + "WHERE b.Statut = @statut ORDER BY b.Date_Achat DESC",
                new { statut });
        }

        public async Task<IEnumerable<BilletModel>> SearchAsync(
            string? keyword, string? statut, DateTime? dateDebut, DateTime? dateFin)
        {
            var where = new List<string>();
            if (!string.IsNullOrWhiteSpace(keyword))
                where.Add("(b.Numero_Billet LIKE @kw OR p.Nom LIKE @kw OR p.Prenom LIKE @kw OR b.Point_Depart LIKE @kw OR b.Point_Arrivee LIKE @kw)");
            if (!string.IsNullOrWhiteSpace(statut))
                where.Add("b.Statut = @statut");
            if (dateDebut.HasValue)
                where.Add("b.Date_Achat >= @dateDebut");
            if (dateFin.HasValue)
                where.Add("b.Date_Achat <= @dateFin");

            var sql = BaseSelect + (where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "")
                    + " ORDER BY b.Date_Achat DESC";

            using var db = new MySqlConnection(_conn);
            return await db.QueryAsync<BilletModel>(sql, new
            {
                kw        = $"%{keyword}%",
                statut,
                dateDebut,
                dateFin   = dateFin.HasValue ? dateFin.Value.Date.AddDays(1) : (DateTime?)null
            });
        }

        public async Task<IEnumerable<BilletModel>> GetByVoyageAsync(int idVoyage)
        {
            using var db = new MySqlConnection(_conn);
            return await db.QueryAsync<BilletModel>(
                BaseSelect + "WHERE b.Id_Voyage_Prevu = @idVoyage AND b.Statut = 'Valide' ORDER BY b.Numero_Siege",
                new { idVoyage });
        }

        public async Task<IEnumerable<BilletModel>> GetByVoyageEtenduAsync(int idVoyage)
        {
            const string sql = @"
                SELECT b.*,
                       p.Nom       AS Nom_Passager,
                       p.Prenom    AS Prenom_Passager,
                       p.Telephone AS Telephone_Passager,
                       tv.Libelle_Type_Voyage,
                       CONCAT(tvp.Point_Depart,' → ',tvp.Point_Arrivee) AS Libelle_Voyage_Prevu,
                       CONCAT(tvu.Point_Depart,' → ',tvu.Point_Arrivee) AS Libelle_Voyage_Utilise
                FROM   billet b
                LEFT JOIN passager    p   ON p.Id_Passager      = b.Id_Passager
                LEFT JOIN type_voyage tv  ON tv.Id_Type_Voyage  = b.Id_Type_Voyage
                LEFT JOIN voyage      vp  ON vp.Id_Voyage       = b.Id_Voyage_Prevu
                LEFT JOIN type_voyage tvp ON tvp.Id_Type_Voyage = vp.Id_Type_Voyage
                LEFT JOIN voyage      vu  ON vu.Id_Voyage       = b.Id_Voyage_Utilise
                LEFT JOIN type_voyage tvu ON tvu.Id_Type_Voyage = vu.Id_Type_Voyage
                JOIN  voyage      v   ON v.Id_Voyage        = @idVoyage
                JOIN  type_voyage tv0 ON tv0.Id_Type_Voyage = v.Id_Type_Voyage
                WHERE b.Statut IN ('Valide', 'Reporté')
                  AND (
                        b.Id_Voyage_Prevu IN (
                            SELECT v2.Id_Voyage FROM voyage v2
                            JOIN type_voyage tv2 ON tv2.Id_Type_Voyage = v2.Id_Type_Voyage
                            WHERE tv2.Id_Type_Voyage = tv0.Id_Type_Voyage
                              AND DATE(v2.Date_Depart) = DATE(v.Date_Depart)
                        )
                        OR (b.Id_Voyage_Prevu IS NULL
                            AND LOWER(TRIM(b.Point_Depart))  = LOWER(TRIM(tv0.Point_Depart))
                            AND LOWER(TRIM(b.Point_Arrivee)) = LOWER(TRIM(tv0.Point_Arrivee)))
                        OR b.Id_Passager IN (
                            SELECT e.Id_Passager FROM embarquement_voyage_passager e
                            JOIN voyage ev ON ev.Id_Voyage = e.Id_Voyage
                            JOIN type_voyage tev ON tev.Id_Type_Voyage = ev.Id_Type_Voyage
                            WHERE e.Statut_Embarquement = 'Absent'
                              AND tev.Id_Type_Voyage = tv0.Id_Type_Voyage
                        )
                      )
                ORDER BY b.Numero_Siege";
            using var db = new MySqlConnection(_conn);
            return await db.QueryAsync<BilletModel>(sql, new { idVoyage });
        }

        public async Task<IEnumerable<BilletModel>> GetPourEmbarquementAsync(int idVoyage)
        {
            const string sql = @"
                SELECT b.*,
                       p.Nom       AS Nom_Passager,
                       p.Prenom    AS Prenom_Passager,
                       p.Telephone AS Telephone_Passager,
                       tv.Libelle_Type_Voyage,
                       CONCAT(tvp.Point_Depart,' → ',tvp.Point_Arrivee) AS Libelle_Voyage_Prevu,
                       CONCAT(tvu.Point_Depart,' → ',tvu.Point_Arrivee) AS Libelle_Voyage_Utilise
                FROM billet b
                LEFT JOIN passager    p   ON p.Id_Passager      = b.Id_Passager
                LEFT JOIN type_voyage tv  ON tv.Id_Type_Voyage  = b.Id_Type_Voyage
                LEFT JOIN voyage      vp  ON vp.Id_Voyage       = b.Id_Voyage_Prevu
                LEFT JOIN type_voyage tvp ON tvp.Id_Type_Voyage = vp.Id_Type_Voyage
                LEFT JOIN voyage      vu  ON vu.Id_Voyage       = b.Id_Voyage_Utilise
                LEFT JOIN type_voyage tvu ON tvu.Id_Type_Voyage = vu.Id_Type_Voyage
                JOIN  voyage          v0   ON v0.Id_Voyage       = @idVoyage
                LEFT JOIN type_voyage tv0  ON tv0.Id_Type_Voyage = v0.Id_Type_Voyage
                WHERE b.Statut IN ('Valide', 'Reporté')
                  AND (
                        b.Id_Voyage_Prevu = @idVoyage
                        OR b.Id_Voyage_Prevu IN (
                            SELECT Id_Voyage FROM voyage
                            WHERE Id_Type_Voyage = v0.Id_Type_Voyage
                        )
                        OR (b.Id_Voyage_Prevu IS NULL
                            AND b.Id_Type_Voyage = v0.Id_Type_Voyage)
                        OR (LOWER(b.Point_Depart)  = LOWER(tv0.Point_Depart)
                            AND LOWER(b.Point_Arrivee) = LOWER(tv0.Point_Arrivee))
                      )
                ORDER BY b.Date_Achat ASC";

            using var db = new MySqlConnection(_conn);
            return await db.QueryAsync<BilletModel>(sql, new { idVoyage });
        }

        public async Task<List<int>> GetSiegesOccupesAsync(int idVoyage)
        {
            using var db = new MySqlConnection(_conn);
            var sieges = await db.QueryAsync<int?>(
                @"SELECT Numero_Siege FROM billet
                  WHERE Id_Voyage_Prevu = @idVoyage
                    AND Numero_Siege IS NOT NULL
                    AND Statut NOT IN ('Annulé')",
                new { idVoyage });
            return sieges.Where(s => s.HasValue).Select(s => s!.Value).ToList();
        }

        public async Task<int> AjouterAsync(BilletModel b)
        {
            b.Numero_Billet = await GenererNumeroAsync();
            b.Date_Validite = b.Date_Achat.AddMonths(6);

            const string sql = @"
                INSERT INTO billet
                    (Numero_Billet, Id_Passager, Point_Depart, Point_Arrivee,
                     Id_Type_Voyage, Montant, Date_Achat, Date_Validite, Statut,
                     Id_Voyage_Prevu, Numero_Siege, Mode_Paiement, Vendu_Par)
                VALUES
                    (@Numero_Billet, @Id_Passager, @Point_Depart, @Point_Arrivee,
                     @Id_Type_Voyage, @Montant, @Date_Achat, @Date_Validite, 'Valide',
                     @Id_Voyage_Prevu, @Numero_Siege, @Mode_Paiement, @Vendu_Par);
                SELECT LAST_INSERT_ID();";

            using var db = new MySqlConnection(_conn);
            return await db.ExecuteScalarAsync<int>(sql, b);
        }

        public async Task<bool> UtiliserAsync(int id, int idVoyage)
        {
            const string sql = @"
                UPDATE billet
                SET    Statut = 'Utilisé', Id_Voyage_Utilise = @idVoyage
                WHERE  Id_Billet = @id AND Statut = 'Valide' AND Date_Validite >= NOW()";
            using var db = new MySqlConnection(_conn);
            return await db.ExecuteAsync(sql, new { id, idVoyage }) > 0;
        }

        public async Task<bool> ReporterAsync(int id, int idNouveauVoyage)
        {
            const string sql = @"
                UPDATE billet
                SET    Statut = 'Reporté', Id_Voyage_Prevu = @idNouveauVoyage
                WHERE  Id_Billet = @id AND Statut IN ('Valide','Reporté') AND Date_Validite >= NOW()";
            using var db = new MySqlConnection(_conn);
            return await db.ExecuteAsync(sql, new { id, idNouveauVoyage }) > 0;
        }

        public async Task<bool> UtiliserParPassagerAsync(int idPassager, int idVoyage)
        {
            const string sql = @"
                UPDATE billet
                SET    Statut = 'Utilisé', Id_Voyage_Utilise = @idVoyage
                WHERE  Id_Passager = @idPassager
                  AND  Statut IN ('Valide', 'Reporté')
                  AND  Date_Validite >= NOW()
                ORDER BY Date_Achat DESC
                LIMIT 1";
            using var db = new MySqlConnection(_conn);
            return await db.ExecuteAsync(sql, new { idPassager, idVoyage }) > 0;
        }

        public async Task<bool> ChangerTypeVoyageAsync(int id, int idTypeVoyage, int? idVoyagePrevu, decimal montant)
        {
            const string sql = @"
                UPDATE billet
                SET    Id_Type_Voyage  = @idTypeVoyage,
                       Id_Voyage_Prevu = @idVoyagePrevu,
                       Montant         = @montant,
                       Statut          = 'Valide'
                WHERE  Id_Billet = @id AND Statut IN ('Valide','Reporté')";
            using var db = new MySqlConnection(_conn);
            return await db.ExecuteAsync(sql, new { id, idTypeVoyage, idVoyagePrevu, montant }) > 0;
        }


        public async Task<int> ExpirerAsync()
        {
            const string sql = @"
                UPDATE billet SET Statut = 'Expiré'
                WHERE  Statut = 'Valide' AND Date_Validite < NOW()";
            using var db = new MySqlConnection(_conn);
            return await db.ExecuteAsync(sql);
        }

        public async Task<string> GenererNumeroAsync()
        {
            var prefix = $"BIL-{DateTime.Now:yyyyMMdd}-";
            const string sql = @"
                SELECT IFNULL(MAX(CAST(SUBSTRING_INDEX(Numero_Billet,'-',-1) AS UNSIGNED)),0) + 1
                FROM   billet WHERE Numero_Billet LIKE @prefix";
            using var db = new MySqlConnection(_conn);
            var seq = await db.ExecuteScalarAsync<int>(sql, new { prefix = prefix + "%" });
            return $"{prefix}{seq:D4}";
        }
    }
}
