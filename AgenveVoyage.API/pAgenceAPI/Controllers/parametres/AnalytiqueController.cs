using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using pAgenceAPI.Controllers;

namespace pAgenceAPI.Controllers.parametres;

[ApiController]
[Route("api/[controller]")]
public class AnalytiqueController : AgenceControllerBase
{
    private readonly string _cs;
    public AnalytiqueController(IConfiguration config)
        => _cs = config.GetConnectionString("DefaultConnection")!;

    // GET /api/Analytique/resume?annee=2026
    [HttpGet("resume")]
    public async Task<IActionResult> Resume([FromQuery] int? annee = null)
    {
        var an = annee ?? DateTime.Today.Year;
        var idAgence = AgenceId;

        using var db = new MySqlConnection(_cs);

        // ── 1. Recettes mensuelles (billets + colis payés) ──────────────────
        var agenceWhere = idAgence.HasValue ? "AND (v.Id_Agence = @idAgence OR v.Id_Agence IS NULL)" : "";
        var recettes = (await db.QueryAsync<RecetteMois>($@"
            SELECT MONTH(b.Date_Achat) AS Mois, SUM(b.Montant) AS Total
            FROM BILLET b
            LEFT JOIN TYPE_VOYAGE tv ON b.Id_Type_Voyage = tv.ID_TYPE_VOYAGE
            LEFT JOIN VOYAGE v       ON b.Id_Voyage_Utilise = v.ID_VOYAGE
            WHERE YEAR(b.Date_Achat) = @an
              AND b.Statut IN ('Valide','Utilisé')
              {agenceWhere}
            GROUP BY MONTH(b.Date_Achat)
            ORDER BY Mois",
            new { an, idAgence })).ToList();

        // ── 2. Taux de remplissage des 10 derniers voyages ──────────────────
        var agenceWhereV = idAgence.HasValue ? "WHERE (v.Id_Agence = @idAgence OR v.Id_Agence IS NULL)" : "";
        var remplissage = (await db.QueryAsync<TauxRemplissage>($@"
            SELECT v.ID_VOYAGE AS IdVoyage,
                   CONCAT(tv.POINT_DEPART,' → ',tv.POINT_ARRIVEE) AS Trajet,
                   DATE_FORMAT(v.DATE_DEPART,'%d/%m/%Y') AS DateDepart,
                   IFNULL(vh.NOMBRE_PLACE, 50) AS Capacite,
                   COUNT(e.ID_EMBARQUEMENT) AS NbPassagers,
                   ROUND(COUNT(e.ID_EMBARQUEMENT) * 100.0 / IFNULL(NULLIF(vh.NOMBRE_PLACE,0),50), 1) AS TauxPct
            FROM VOYAGE v
            LEFT JOIN TYPE_VOYAGE tv  ON v.ID_TYPE_VOYAGE = tv.ID_TYPE_VOYAGE
            LEFT JOIN VEHICULE vh     ON v.ID_VEHICULE    = vh.ID_VEHICULE
            LEFT JOIN EMBARQUEMENT_VOYAGE_PASSAGER e ON e.ID_VOYAGE = v.ID_VOYAGE
            {agenceWhereV}
            GROUP BY v.ID_VOYAGE, tv.POINT_DEPART, tv.POINT_ARRIVEE, v.DATE_DEPART, vh.NOMBRE_PLACE
            HAVING NbPassagers > 0
            ORDER BY v.DATE_DEPART DESC
            LIMIT 12",
            new { idAgence })).ToList();

        // ── 3. Évolution mensuelle des passagers embarqués ──────────────────
        var agenceWhereE = idAgence.HasValue
            ? "LEFT JOIN VOYAGE vv ON e.ID_VOYAGE = vv.ID_VOYAGE WHERE YEAR(e.DATE_ENREGISTREMENT) = @an AND (vv.Id_Agence = @idAgence OR vv.Id_Agence IS NULL)"
            : "WHERE YEAR(e.DATE_ENREGISTREMENT) = @an";
        var passagers = (await db.QueryAsync<PassagersMois>($@"
            SELECT MONTH(e.DATE_ENREGISTREMENT) AS Mois, COUNT(*) AS Total
            FROM EMBARQUEMENT_VOYAGE_PASSAGER e
            {agenceWhereE}
            GROUP BY MONTH(e.DATE_ENREGISTREMENT)
            ORDER BY Mois",
            new { an, idAgence })).ToList();

        // ── 4. Top 5 trajets par recette ────────────────────────────────────
        var topTrajets = (await db.QueryAsync<TopTrajet>($@"
            SELECT CONCAT(tv.POINT_DEPART,' → ',tv.POINT_ARRIVEE) AS Trajet,
                   COUNT(*) AS NbBillets,
                   SUM(b.Montant) AS TotalRecette
            FROM BILLET b
            JOIN TYPE_VOYAGE tv ON b.Id_Type_Voyage = tv.ID_TYPE_VOYAGE
            WHERE YEAR(b.Date_Achat) = @an AND b.Statut IN ('Valide','Utilisé')
            GROUP BY tv.POINT_DEPART, tv.POINT_ARRIVEE
            ORDER BY TotalRecette DESC
            LIMIT 5",
            new { an })).ToList();

        return Ok(new { recettes, remplissage, passagers, topTrajets, annee = an });
    }

    record RecetteMois(int Mois, decimal Total);
    record TauxRemplissage(int IdVoyage, string Trajet, string DateDepart, int Capacite, int NbPassagers, double TauxPct);
    record PassagersMois(int Mois, int Total);
    record TopTrajet(string Trajet, int NbBillets, decimal TotalRecette);
}
