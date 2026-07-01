using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.securite;

[ApiController]
[Route("api/[controller]")]
public class PrivilegesController : ControllerBase
{
    private readonly IPrivilegeRepository _repo;
    public PrivilegesController(IPrivilegeRepository repo) => _repo = repo;

    [HttpGet("par-groupe/{id}")]
    public async Task<IActionResult> ParGroupe(int id) =>
        Ok(await _repo.GetByGroupeAsync(id));

    [HttpPost("sauvegarder/{id}")]
    public async Task<IActionResult> Sauvegarder(int id, [FromBody] IEnumerable<PrivilegeModel> privileges)
    {
        await _repo.SauvegarderAsync(id, privileges);
        return Ok();
    }
}
