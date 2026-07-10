using Dapper;
using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres
{
    [ApiController]
    [Route("api/[controller]")]
    public class BilletsController : AgenceControllerBase
    {
        private readonly IBilletRepository _repo;
        private readonly IEcritureRepository _ecriture;
        private readonly IReservationRepository _reservationRepo;
        private readonly ILogger<BilletsController> _log;

        public BilletsController(IBilletRepository repo, IEcritureRepository ecriture,
                                 IReservationRepository reservationRepo,
                                 ILogger<BilletsController> log)
        {
            _repo            = repo;
            _ecriture        = ecriture;
            _reservationRepo = reservationRepo;
            _log             = log;
        }

        [HttpGet("liste")]
        public async Task<IActionResult> Liste()
        {
            try   { return Ok(await _repo.GetAllAsync()); }
            catch (Exception ex) { _log.LogError(ex, "liste billets"); return StatusCode(500, ex.Message); }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var b = await _repo.GetByIdAsync(id);
            return b is null ? NotFound() : Ok(b);
        }

        [HttpGet("numero/{numero}")]
        public async Task<IActionResult> GetByNumero(string numero)
        {
            var b = await _repo.GetByNumeroAsync(numero);
            return b is null ? NotFound() : Ok(b);
        }

        [HttpGet("passager/{idPassager:int}")]
        public async Task<IActionResult> ParPassager(int idPassager)
        {
            try   { return Ok(await _repo.GetByPassagerAsync(idPassager)); }
            catch (Exception ex) { _log.LogError(ex, "billets passager"); return StatusCode(500, ex.Message); }
        }

        [HttpGet("par-voyage/{idVoyage:int}")]
        public async Task<IActionResult> ParVoyage(int idVoyage)
        {
            try   { return Ok(await _repo.GetByVoyageAsync(idVoyage)); }
            catch (Exception ex) { _log.LogError(ex, "billets par voyage {id}", idVoyage); return StatusCode(500, ex.Message); }
        }

        [HttpGet("par-voyage-etendu/{idVoyage:int}")]
        public async Task<IActionResult> ParVoyageEtendu(int idVoyage)
        {
            try   { return Ok(await _repo.GetByVoyageEtenduAsync(idVoyage)); }
            catch (Exception ex) { _log.LogError(ex, "billets etendu voyage {id}", idVoyage); return StatusCode(500, ex.Message); }
        }

        [HttpGet("pour-embarquement/{idVoyage:int}")]
        public async Task<IActionResult> PourEmbarquement(int idVoyage)
        {
            try   { return Ok(await _repo.GetPourEmbarquementAsync(idVoyage)); }
            catch (Exception ex) { _log.LogError(ex, "billets embarquement voyage {id}", idVoyage); return StatusCode(500, ex.Message); }
        }

        // DEBUG : voir les données brutes pour diagnostiquer l'embarquement
        [HttpGet("debug-embarquement/{idVoyage:int}")]
        public async Task<IActionResult> DebugEmbarquement(int idVoyage)
        {
            try
            {
                var cs = HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection")!;
                using var db = new MySqlConnector.MySqlConnection(cs);

                var voyage = await db.QueryFirstOrDefaultAsync(
                    @"SELECT v.Id_Voyage, v.Id_Type_Voyage, tv.Libelle_Type_Voyage,
                             tv.Point_Depart as TV_Depart, tv.Point_Arrivee as TV_Arrivee
                      FROM voyage v LEFT JOIN type_voyage tv ON tv.Id_Type_Voyage = v.Id_Type_Voyage
                      WHERE v.Id_Voyage = @id", new { id = idVoyage });

                var billets = await db.QueryAsync(
                    @"SELECT b.Id_Billet, b.Numero_Billet, b.Id_Passager, b.Id_Type_Voyage,
                             b.Point_Depart as B_Depart, b.Point_Arrivee as B_Arrivee,
                             b.Id_Voyage_Prevu, b.Statut,
                             tv.Libelle_Type_Voyage as TV_Libelle,
                             p.Nom, p.Prenom
                      FROM billet b
                      LEFT JOIN type_voyage tv ON tv.Id_Type_Voyage = b.Id_Type_Voyage
                      LEFT JOIN passager p ON p.Id_Passager = b.Id_Passager
                      WHERE b.Statut IN ('Valide','Reporté')
                      ORDER BY b.Date_Achat DESC LIMIT 20");

                return Ok(new { voyage, billets });
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("sieges-occupes/{idVoyage:int}")]
        public async Task<IActionResult> SiegesOccupes(int idVoyage)
        {
            try   { return Ok(await _repo.GetSiegesOccupesAsync(idVoyage)); }
            catch (Exception ex) { _log.LogError(ex, "sieges occupes voyage {id}", idVoyage); return StatusCode(500, ex.Message); }
        }

        [HttpGet("rechercher")]
        public async Task<IActionResult> Rechercher(
            [FromQuery] string?   keyword,
            [FromQuery] string?   statut,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin)
        {
            try   { return Ok(await _repo.SearchAsync(keyword, statut, dateDebut, dateFin)); }
            catch (Exception ex) { _log.LogError(ex, "recherche billets"); return StatusCode(500, ex.Message); }
        }

        [HttpPost("vendre")]
        public async Task<IActionResult> Vendre([FromBody] BilletModel billet)
        {
            try
            {
                var dateJournee = await _ecriture.GetDateJourneeOuverteAsync(AgenceId);
                if (dateJournee is null)
                    return BadRequest(new { message = "Aucune journée comptable ouverte pour votre agence. Impossible de vendre un billet." });

                var id      = await _repo.AjouterAsync(billet);
                var created = await _repo.GetByIdAsync(id);

                if (created is not null && created.Montant > 0)
                {
                    var numTx = $"BILT-{created.Numero_Billet}";
                    var trajet = $"{created.Point_Depart} - {created.Point_Arrivee}";
                    await _ecriture.EcritureVenteBilletAsync(
                        numTx, created.Numero_Billet, created.Montant,
                        created.Libelle_Type_Voyage, trajet, AgenceId, UserId);
                }

                // Marquer les réservations en ligne du passager comme "Utilisée" (le billet les remplace)
                if (billet.Id_Passager > 0)
                {
                    var reservations = await _reservationRepo.GetAllAsync();
                    foreach (var r in reservations.Where(r =>
                        r.Id_Passager == billet.Id_Passager &&
                        r.Statut_Paiement == "Payé" &&
                        r.Statut_Reservation != "Utilisée" &&
                        r.Statut_Reservation != "Annulée"))
                    {
                        await _reservationRepo.UpdateStatutReservationAsync(r.Id_Reservation, "Utilisée");
                    }
                }

                return CreatedAtAction(nameof(GetById), new { id }, created);
            }
            catch (Exception ex) { _log.LogError(ex, "vente billet"); return StatusCode(500, ex.Message); }
        }

        [HttpPut("{id:int}/utiliser")]
        public async Task<IActionResult> Utiliser(int id, [FromQuery] int idVoyage)
        {
            var ok = await _repo.UtiliserAsync(id, idVoyage);
            return ok ? Ok(new { message = "Billet validé." })
                      : BadRequest(new { message = "Billet invalide, déjà utilisé ou expiré." });
        }

        [HttpPut("{id:int}/reporter")]
        public async Task<IActionResult> Reporter(int id, [FromQuery] int idNouveauVoyage)
        {
            var ok = await _repo.ReporterAsync(id, idNouveauVoyage);
            return ok ? Ok(new { message = "Billet reporté." })
                      : BadRequest(new { message = "Impossible de reporter ce billet." });
        }

        [HttpPut("{id:int}/changer-type")]

        public async Task<IActionResult> ChangerType(int id, [FromQuery] int idTypeVoyage, [FromQuery] int? idVoyagePrevu, [FromQuery] decimal montant)
        {
            var ok = await _repo.ChangerTypeVoyageAsync(id, idTypeVoyage, idVoyagePrevu, montant);
            return ok ? Ok(new { message = "Type de voyage mis à jour." })
                      : BadRequest(new { message = "Impossible de modifier ce billet." });
        }


        [HttpPost("expirer")]
        public async Task<IActionResult> Expirer()
        {
            var n = await _repo.ExpirerAsync();
            return Ok(new { message = $"{n} billet(s) marqué(s) expiré(s)." });
        }
    }
}
