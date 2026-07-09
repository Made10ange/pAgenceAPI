using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace pAgenceAPI.Controllers.parametres
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly string _conn;

        public AdminController(IConfiguration configuration)
        {
            _conn = configuration.GetConnectionString("DefaultConnection")!;
        }

        // Appeler UNE SEULE FOIS pour corriger les reservations dupliquees
        [HttpPost("fix-reservations-utilisees")]
        public async Task<IActionResult> FixReservationsUtilisees()
        {
            const string sql = @"
                UPDATE reservation r
                JOIN billet b ON b.Id_Passager = r.ID_passager
                SET r.STATUT_RESERVATION = 'Utilisée'
                WHERE r.STATUT_PAIEMENT = 'Payé'
                  AND r.STATUT_RESERVATION NOT IN ('Utilisée', 'Annulée')
                  AND b.Statut = 'Valide'";

            using var db = new MySqlConnection(_conn);
            var rows = await db.ExecuteAsync(sql);
            return Ok(new { message = $"{rows} réservation(s) marquée(s) comme Utilisée." });
        }
    }
}
