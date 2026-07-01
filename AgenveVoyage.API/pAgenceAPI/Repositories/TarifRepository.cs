using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class TarifRepository : ITarifRepository
    {
        private readonly string _cs;

        public TarifRepository(IConfiguration config)
        {
            _cs = config.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<List<TarifModel>> GetAllAsync()
        {
            using var db = new MySqlConnection(_cs);
            return (await db.QueryAsync<TarifModel>(
                @"SELECT t.*, tv.LIBELLE_TYPE_VOYAGE
                  FROM TARIF t
                  LEFT JOIN TYPE_VOYAGE tv ON t.ID_TYPE_VOYAGE = tv.ID_TYPE_VOYAGE
                  ORDER BY t.POINT_DEPART, t.POINT_ARRIVEE, t.TYPE_PASSAGER, t.DATE_DEBUT DESC")).ToList();
        }

        public async Task<TarifModel?> GetByIdAsync(int id)
        {
            using var db = new MySqlConnection(_cs);
            return await db.QueryFirstOrDefaultAsync<TarifModel>(
                @"SELECT t.*, tv.LIBELLE_TYPE_VOYAGE
                  FROM TARIF t
                  LEFT JOIN TYPE_VOYAGE tv ON t.ID_TYPE_VOYAGE = tv.ID_TYPE_VOYAGE
                  WHERE t.ID_TARIF = @id", new { id });
        }

        public async Task<List<TarifModel>> RechercherAsync(int? idTypeVoyage, string? depart, string? arrivee, string? typePassager)
        {
            using var db = new MySqlConnection(_cs);

            // Filtre de base : actif + période valide aujourd'hui
            var sql = @"SELECT t.*, tv.LIBELLE_TYPE_VOYAGE
                        FROM TARIF t
                        LEFT JOIN TYPE_VOYAGE tv ON t.ID_TYPE_VOYAGE = tv.ID_TYPE_VOYAGE
                        WHERE t.ACTIF = 1
                          AND (t.DATE_DEBUT IS NULL OR t.DATE_DEBUT <= CURDATE())
                          AND (t.DATE_FIN   IS NULL OR t.DATE_FIN   >= CURDATE())";

            var p = new DynamicParameters();
            if (idTypeVoyage.HasValue)       { sql += " AND t.ID_TYPE_VOYAGE = @idTypeVoyage"; p.Add("idTypeVoyage", idTypeVoyage); }
            if (!string.IsNullOrEmpty(depart))       { sql += " AND (t.POINT_DEPART IS NULL OR t.POINT_DEPART LIKE @depart)"; p.Add("depart", $"%{depart}%"); }
            if (!string.IsNullOrEmpty(arrivee))      { sql += " AND (t.POINT_ARRIVEE IS NULL OR t.POINT_ARRIVEE LIKE @arrivee)"; p.Add("arrivee", $"%{arrivee}%"); }
            if (!string.IsNullOrEmpty(typePassager)) { sql += " AND t.TYPE_PASSAGER = @typePassager"; p.Add("typePassager", typePassager); }

            // Le plus précis d'abord : tarif avec trajet défini avant tarif général
            sql += " ORDER BY (t.POINT_DEPART IS NOT NULL) DESC, (t.POINT_ARRIVEE IS NOT NULL) DESC, t.PRIX";

            return (await db.QueryAsync<TarifModel>(sql, p)).ToList();
        }

        public async Task<bool> AddAsync(TarifModel t)
        {
            using var db = new MySqlConnection(_cs);
            var rows = await db.ExecuteAsync(
                @"INSERT INTO TARIF (LIBELLE, ID_TYPE_VOYAGE, POINT_DEPART, POINT_ARRIVEE, TYPE_PASSAGER, PRIX, DATE_DEBUT, DATE_FIN, ACTIF)
                  VALUES (@Libelle, @Id_Type_Voyage, @Point_Depart, @Point_Arrivee, @Type_Passager, @Prix, @Date_Debut, @Date_Fin, @Actif)", t);
            return rows > 0;
        }

        public async Task<bool> UpdateAsync(TarifModel t)
        {
            using var db = new MySqlConnection(_cs);
            var rows = await db.ExecuteAsync(
                @"UPDATE TARIF SET LIBELLE=@Libelle, ID_TYPE_VOYAGE=@Id_Type_Voyage,
                    POINT_DEPART=@Point_Depart, POINT_ARRIVEE=@Point_Arrivee,
                    TYPE_PASSAGER=@Type_Passager, PRIX=@Prix,
                    DATE_DEBUT=@Date_Debut, DATE_FIN=@Date_Fin, ACTIF=@Actif
                  WHERE ID_TARIF=@Id_Tarif", t);
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var db = new MySqlConnection(_cs);
            return await db.ExecuteAsync("DELETE FROM TARIF WHERE ID_TARIF=@id", new { id }) > 0;
        }
    }
}
