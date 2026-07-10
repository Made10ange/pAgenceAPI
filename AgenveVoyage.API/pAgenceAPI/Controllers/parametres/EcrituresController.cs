using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres;

[Route("api/[controller]")]
[ApiController]
public class EcrituresController : AgenceControllerBase
{
    private readonly IEcritureRepository _repo;
    private readonly ILogger<EcrituresController> _logger;

    public EcrituresController(IEcritureRepository repo, ILogger<EcrituresController> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    // GET api/Ecritures/du-jour?date=2026-06-16
    [HttpGet("du-jour")]
    public async Task<IActionResult> DuJour([FromQuery] DateTime? date = null)
    {
        try
        {
            var d = date ?? DateTime.Today;
            var ops = await _repo.GetByDateAsync(d, AgenceId);
            return Ok(ops);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur DuJour");
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }

    // GET api/Ecritures/brouillard?dateDebut=&dateFin=&idCaisse=
    [HttpGet("brouillard")]
    public async Task<IActionResult> Brouillard(
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin,
        [FromQuery] int? idCaisse)
    {
        try
        {
            var d1 = dateDebut ?? DateTime.Today;
            var d2 = dateFin   ?? DateTime.Today;
            var ops = await _repo.GetBrouillardAsync(d1, d2, idCaisse);
            return Ok(ops);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur Brouillard");
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }

    // GET api/Ecritures/journee-ouverte
    [HttpGet("journee-ouverte")]
    public async Task<IActionResult> JourneeOuverte([FromQuery] DateTime? date = null)
    {
        // Si une date précise est demandée, on vérifie cette date exacte
        if (date.HasValue)
        {
            var ok = await _repo.JourneeOuverteAsync(date.Value, AgenceId);
            return Ok(new { ouverte = ok, date = date.Value.ToString("yyyy-MM-dd") });
        }
        // Sinon : chercher n'importe quelle journée ouverte pour l'agence
        var dateOuverte = await _repo.GetDateJourneeOuverteAsync(AgenceId);
        return Ok(new { ouverte = dateOuverte.HasValue, date = (dateOuverte ?? DateTime.Today).ToString("yyyy-MM-dd") });
    }

    // GET api/Ecritures/prochaine-journee
    // Retourne la date à proposer par défaut : le jour suivant la dernière journée
    // si celle-ci est clôturée, sinon la date de la dernière journée (encore ouverte),
    // ou la date du jour s'il n'existe aucune journée en base.
    [HttpGet("prochaine-journee")]
    public async Task<IActionResult> ProchaineJournee()
    {
        var derniere = await _repo.GetDerniereJourneeAsync(AgenceId);
        var date = derniere is null
            ? DateTime.Today
            : derniere.Value.Statut == "Cloturee"
                ? derniere.Value.Date.AddDays(1)
                : derniere.Value.Date;
        return Ok(new { date = date.ToString("yyyy-MM-dd") });
    }

    // POST api/Ecritures/ouvrir-journee?date=2026-06-16
    [HttpPost("ouvrir-journee")]
    public async Task<IActionResult> OuvrirJournee([FromQuery] DateTime? date = null)
    {
        try
        {
            if (AgenceId is null) return BadRequest(new { message = "Agence inconnue." });
            if (!await _repo.PeutGererJourneeAsync(UserId))
                return StatusCode(403, new { message = "Seuls l'administrateur ou le caissier principal peuvent ouvrir/clôturer la journée." });

            var d = date ?? DateTime.Today;

            // Bloquer la réouverture d'une journée déjà clôturée
            var derniere = await _repo.GetDerniereJourneeAsync(AgenceId);
            if (derniere.HasValue && derniere.Value.Date.Date == d.Date && derniere.Value.Statut == "Clôturée")
                return BadRequest(new { message = $"La journée du {d:dd/MM/yyyy} est déjà clôturée et ne peut plus être réouverte." });

            await _repo.OuvrirJourneeAsync(UserId ?? 0, AgenceId.Value, d);
            return Ok(new { message = $"Journée du {d:dd/MM/yyyy} ouverte avec succès." });
        }
        catch (MySqlConnector.MySqlException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur OuvrirJournee");
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }

    // POST api/Ecritures/cloturer-journee?date=2026-06-16
    [HttpPost("cloturer-journee")]
    public async Task<IActionResult> CloturerJournee([FromQuery] DateTime? date = null)
    {
        try
        {
            if (AgenceId is null) return BadRequest(new { message = "Agence inconnue." });
            if (!await _repo.PeutGererJourneeAsync(UserId))
                return StatusCode(403, new { message = "Seuls l'administrateur ou le caissier principal peuvent ouvrir/clôturer la journée." });

            var d = date ?? DateTime.Today;
            await _repo.CloturerJourneeAsync(UserId ?? 0, AgenceId.Value, d);
            return Ok(new { message = $"Journée du {d:dd/MM/yyyy} clôturée avec succès." });
        }
        catch (MySqlConnector.MySqlException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur CloturerJournee");
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }

    // POST api/Ecritures/enregistrer
    [HttpPost("enregistrer")]
    public async Task<IActionResult> Enregistrer([FromBody] EcritureComptableModel ecriture)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (ecriture.Lignes == null || ecriture.Lignes.Count == 0)
                return BadRequest(new { message = "L'écriture doit contenir au moins une ligne." });

            // Vérifier l'équilibre débit = crédit
            var totalDebit  = ecriture.Lignes.Sum(l => l.debit);
            var totalCredit = ecriture.Lignes.Sum(l => l.credit);
            if (Math.Abs(totalDebit - totalCredit) > 0.01m)
                return BadRequest(new
                {
                    message = $"L'écriture n'est pas équilibrée : Débit = {totalDebit:N0} ≠ Crédit = {totalCredit:N0} FCFA."
                });

            ecriture.id_agence = AgenceId;
            await _repo.EnregistrerAsync(ecriture);
            return Ok(new { message = "Écriture enregistrée avec succès !", num_transaction = ecriture.num_transaction });
        }
        catch (MySqlConnector.MySqlException ex)
        {
            // Erreur levée par la SP (ex: journée non ouverte)
            _logger.LogWarning("SP refus: {Msg}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur Enregistrer écriture");
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }
}
