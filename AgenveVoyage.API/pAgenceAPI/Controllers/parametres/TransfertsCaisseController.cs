using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Controllers;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres;

[Route("api/[controller]")]
[ApiController]
public class TransfertsCaisseController : AgenceControllerBase
{
    private readonly ITransfertCaisseRepository _repo;
    private readonly IEcritureRepository _ecriture;
    private readonly ILogger<TransfertsCaisseController> _logger;

    public TransfertsCaisseController(ITransfertCaisseRepository repo,
                                      IEcritureRepository ecriture,
                                      ILogger<TransfertsCaisseController> logger)
    {
        _repo     = repo;
        _ecriture = ecriture;
        _logger   = logger;
    }

    // GET api/TransfertsCaisse/en-attente
    [HttpGet("en-attente")]
    public async Task<IActionResult> EnAttente()
    {
        try   { return Ok(await _repo.GetEnAttenteAsync(AgenceId)); }
        catch (Exception ex) { _logger.LogError(ex, "EnAttente"); return Problem(detail: ex.Message, statusCode: 500); }
    }

    // GET api/TransfertsCaisse/historique?dateDebut=&dateFin=
    [HttpGet("historique")]
    public async Task<IActionResult> Historique([FromQuery] DateTime? dateDebut, [FromQuery] DateTime? dateFin)
    {
        try   { return Ok(await _repo.GetHistoriqueAsync(dateDebut, dateFin, AgenceId)); }
        catch (Exception ex) { _logger.LogError(ex, "Historique transferts"); return Problem(detail: ex.Message, statusCode: 500); }
    }

    // GET api/TransfertsCaisse/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var t = await _repo.GetByIdAsync(id);
        return t is null ? NotFound() : Ok(t);
    }

    // POST api/TransfertsCaisse/initier
    [HttpPost("initier")]
    public async Task<IActionResult> Initier([FromBody] TransfertCaisseModel model)
    {
        try
        {
            if (!await _ecriture.JourneeOuverteAsync(DateTime.Today, AgenceId))
                return BadRequest(new { message = "Aucune journée comptable ouverte pour votre agence. Impossible d'effectuer un transfert de caisse." });

            var id = await _repo.InitierAsync(model, UserId);
            var created = await _repo.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id }, created);
        }
        catch (MySqlConnector.MySqlException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Initier transfert");
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }

    // POST api/TransfertsCaisse/{id}/valider
    [HttpPost("{id:int}/valider")]
    public async Task<IActionResult> Valider(int id)
    {
        try
        {
            await _repo.ValiderAsync(id, UserId);
            return Ok(new { message = "Transfert validé avec succès." });
        }
        catch (MySqlConnector.MySqlException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Valider transfert {Id}", id);
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }

    // POST api/TransfertsCaisse/{id}/annuler
    [HttpPost("{id:int}/annuler")]
    public async Task<IActionResult> Annuler(int id)
    {
        try
        {
            await _repo.AnnulerAsync(id, UserId);
            return Ok(new { message = "Transfert annulé." });
        }
        catch (MySqlConnector.MySqlException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Annuler transfert {Id}", id);
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }
}
