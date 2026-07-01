using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.securite;

[ApiController]
[Route("api/[controller]")]
public class HistoriqueConnexionController : ControllerBase
{
    private readonly IHistoriqueConnexionRepository _repo;
    public HistoriqueConnexionController(IHistoriqueConnexionRepository repo) => _repo = repo;

    [HttpGet("liste")]
    public async Task<IActionResult> Liste([FromQuery] int page = 1, [FromQuery] int pageSize = 50) =>
        Ok(await _repo.GetAllAsync(page, pageSize));

    [HttpGet("echecs-recents")]
    public async Task<IActionResult> EchecsRecents([FromQuery] int minutes = 30) =>
        Ok(await _repo.GetEchecsRecentsAsync(minutes));

    [HttpPost("enregistrer")]
    public async Task<IActionResult> Enregistrer([FromBody] HistoriqueConnexionModel entry)
    {
        await _repo.EnregistrerAsync(entry);
        return Ok();
    }
}
