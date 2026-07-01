using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres;

[Route("api/[controller]")]
[ApiController]
public class BalanceController : AgenceControllerBase
{
    private readonly IBalanceRepository _repo;
    private readonly ILogger<BalanceController> _logger;

    public BalanceController(IBalanceRepository repo, ILogger<BalanceController> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    // GET api/Balance/generale?dateDebut=2026-06-01&dateFin=2026-06-16
    [HttpGet("generale")]
    public async Task<IActionResult> Generale(
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin)
    {
        try
        {
            var d1   = dateDebut ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var d2   = dateFin   ?? DateTime.Today;
            var rows = await _repo.GetBalanceGeneraleAsync(d1, d2, AgenceId);
            return Ok(rows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur Balance Générale");
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }
}
