using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.personnel;

[ApiController]
[Route("api/[controller]")]
public class FichePayeController : ControllerBase
{
    private readonly IFichePayeRepository _repo;
    private readonly IEcritureRepository _ecritureRepo;
    private readonly ILogger<FichePayeController> _logger;

    public FichePayeController(IFichePayeRepository repo, IEcritureRepository ecritureRepo,
        ILogger<FichePayeController> logger)
    {
        _repo = repo;
        _ecritureRepo = ecritureRepo;
        _logger = logger;
    }

    [HttpGet("liste")]
    public async Task<IActionResult> Liste([FromQuery] int? annee, [FromQuery] int? mois)
        => Ok(await _repo.GetAllAsync(annee, mois));

    [HttpGet("personnel/{idPersonnel}")]
    public async Task<IActionResult> ParPersonnel(int idPersonnel)
        => Ok(await _repo.GetByPersonnelAsync(idPersonnel));

    [HttpGet("{id}")]
    public async Task<IActionResult> Detail(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("ajouter")]
    public async Task<IActionResult> Ajouter([FromBody] FichePayeModel fiche)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var id = await _repo.AddAsync(fiche);
        return Ok(new { id });
    }

    [HttpPut("modifier")]
    public async Task<IActionResult> Modifier([FromBody] FichePayeModel fiche)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _repo.UpdateAsync(fiche) ? Ok() : NotFound();
    }

    [HttpPut("payer/{id}")]
    public async Task<IActionResult> MarquerPaye(int id)
    {
        var fiche = await _repo.GetByIdAsync(id);
        if (fiche is null) return NotFound();

        var ok = await _repo.MarquerPayeAsync(id);
        if (!ok) return NotFound();

        // Écriture comptable : Débit 6411 / Crédit caisse
        try
        {
            var numTransaction = $"SALAIRE-{id}-{DateTime.Now:yyyyMMddHHmm}";
            var nomEmploye = $"{fiche.NomPersonnel} {fiche.PrenomPersonnel}".Trim();
            await _ecritureRepo.EcritureSalaireAsync(numTransaction, nomEmploye, fiche.Net_A_Payer,
                idAgence: null, codeUser: null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Écriture salaire ignorée pour fiche {Id}", id);
        }

        return Ok();
    }

    [HttpDelete("supprimer/{id}")]
    public async Task<IActionResult> Supprimer(int id)
        => await _repo.DeleteAsync(id) ? Ok() : NotFound();

    [HttpPost("generer")]
    public async Task<IActionResult> Generer([FromQuery] int mois, [FromQuery] int annee)
    {
        if (mois < 1 || mois > 12 || annee < 2020) return BadRequest("Mois ou année invalide.");
        await _repo.GenererFichesAsync(mois, annee);
        return Ok(new { message = $"Fiches générées pour {mois}/{annee}" });
    }
}
