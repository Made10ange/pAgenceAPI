using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoyagesController : ControllerBase
    {
        private readonly IVoyageRepository _repository;
        private readonly ILogger<VoyagesController> _logger;

        public VoyagesController(IVoyageRepository repository, ILogger<VoyagesController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // LISTE DES VOYAGES
        [HttpGet("liste")]
        public async Task<ActionResult<List<VoyageModel>>> GetAll()
        {
            try
            {
                var voyages = await _repository.GetAllAsync();
                return Ok(voyages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des voyages");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors du chargement des voyages");
            }
        }

        // RECHERCHE (corrigée pour correspondre à VoyageModel)
        [HttpGet("rechercher")]
        public async Task<ActionResult<List<VoyageModel>>> Search([FromQuery] string motCle)
        {
            try
            {
                var voyages = await _repository.GetAllAsync();

                if (!string.IsNullOrEmpty(motCle))
                {
                    var mc = motCle.Trim();
                    voyages = voyages.Where(v =>
                        // recherche sur l'ID (converti en string), sur les points de départ/arrivée et le statut
                        v.Id_Voyage.ToString().Contains(mc, StringComparison.OrdinalIgnoreCase) ||
                        (v.Point_Depart ?? string.Empty).Contains(mc, StringComparison.OrdinalIgnoreCase) ||
                        (v.Point_Arrivee ?? string.Empty).Contains(mc, StringComparison.OrdinalIgnoreCase) ||
                        (v.Statut ?? string.Empty).Contains(mc, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                return Ok(voyages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Search voyages");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la recherche des voyages");
            }
        }

        // AJOUTER
        [HttpPost("ajouter")]
        public async Task<ActionResult<string>> Create([FromBody] VoyageModel voyage)
        {
            try
            {
                _logger.LogInformation("Ajout voyage : {@Voyage}", voyage);
                
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState invalide : {@Errors}", ModelState.Values.SelectMany(m => m.Errors));
                    return BadRequest(ModelState);
                }

                var message = await _repository.AddAsync(voyage);
                _logger.LogInformation("Voyage ajouté avec succès: {@Message}", message);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Create voyage: {@Voyage}", voyage);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de l'ajout du voyage");
            }
        }

        // MODIFIER
        [HttpPut("modifier/{id}")]
        public async Task<ActionResult<string>> Update(int id, [FromBody] VoyageModel voyage)
        {
            try
            {
                if (id != voyage.Id_Voyage) return BadRequest("ID incohérent");
                var message = await _repository.UpdateAsync(voyage);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Update voyage id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la modification du voyage");
            }
        }

        // SUPPRIMER
        [HttpDelete("supprimer/{id}")]
        public async Task<ActionResult<string>> Delete(int id)
        {
            try
            {
                var message = await _repository.DeleteAsync(id);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Delete voyage id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la suppression du voyage");
            }
        }

        // ROUTES STANDARDS REST
        [HttpGet]
        public async Task<ActionResult<List<VoyageModel>>> GetAllStandard()
            => await GetAll();

        [HttpGet("{id}")]
        public async Task<ActionResult<VoyageModel>> GetById(int id)
        {
            var voyage = await _repository.GetByIdAsync(id);
            if (voyage == null) return NotFound();
            return Ok(voyage);
        }
    }
}