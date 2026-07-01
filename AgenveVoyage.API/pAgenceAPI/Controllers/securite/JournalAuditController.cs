using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.securite;

[ApiController]
[Route("api/[controller]")]
public class JournalAuditController : ControllerBase
{
    private readonly IJournalAuditRepository _repo;
    public JournalAuditController(IJournalAuditRepository repo) => _repo = repo;

    [HttpGet("liste")]
    public async Task<IActionResult> Liste([FromQuery] int page = 1, [FromQuery] int pageSize = 50) =>
        Ok(await _repo.GetAllAsync(page, pageSize));

    [HttpGet("rechercher")]
    public async Task<IActionResult> Rechercher(
        [FromQuery] string? module,
        [FromQuery] string? login,
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin) =>
        Ok(await _repo.RechercherAsync(module, login, dateDebut, dateFin));

    [HttpPost("enregistrer")]
    public async Task<IActionResult> Enregistrer([FromBody] JournalAuditModel entry)
    {
        await _repo.EnregistrerAsync(entry);
        return Ok();
    }
}
