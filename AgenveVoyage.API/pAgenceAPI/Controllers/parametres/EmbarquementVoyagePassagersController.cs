#nullable disable
using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmbarquementsController : ControllerBase
    {
        private readonly IEmbarquementRepository _repository;

        public EmbarquementsController(IEmbarquementRepository repository)
        {
            _repository = repository;
        }

        // ✅ GET: api/Embarquements/liste
        [HttpGet("liste")]
        public async Task<ActionResult<List<EmbarquementVoyagePassagerModel>>> GetAll()
        {
            try
            {
                var embarquements = await _repository.GetAllAsync();
                return Ok(embarquements);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Erreur lors de la récupération : {ex.Message}" });
            }
        }

        // ✅ GET: api/Embarquements/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<EmbarquementVoyagePassagerModel>> GetById(int id)
        {
            try
            {
                var embarquement = await _repository.GetByIdAsync(id);
                if (embarquement == null)
                {
                    return NotFound(new { message = $"Embarquement ID {id} non trouvé" });
                }
                return Ok(embarquement);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Erreur lors de la récupération : {ex.Message}" });
            }
        }

        // ✅ POST: api/Embarquements/ajouter
        [HttpPost("ajouter")]
        public async Task<ActionResult<string>> Create([FromBody] EmbarquementVoyagePassagerModel embarquement)
        {
            try
            {
                if (embarquement == null)
                {
                    return BadRequest(new { message = "Données invalides" });
                }

                // Vérifier que le voyage existe
                var voyageRepo = HttpContext.RequestServices.GetService<IVoyageRepository>();
                if (voyageRepo != null)
                {
                    var voyage = await voyageRepo.GetByIdAsync(embarquement.Id_Voyage);
                    if (voyage == null)
                    {
                        return BadRequest(new { message = $"Voyage ID {embarquement.Id_Voyage} non trouvé" });
                    }
                }

                // Vérifier que le passager existe
                var passagerRepo = HttpContext.RequestServices.GetService<IPassagerRepository>();
                if (passagerRepo != null)
                {
                    var passager = await passagerRepo.GetByIdAsync(embarquement.Id_Passager);
                    if (passager == null)
                    {
                        return BadRequest(new { message = $"Passager ID {embarquement.Id_Passager} non trouvé" });
                    }
                }

                // Si aucune date d'enregistrement n'est fournie, utiliser la date actuelle
                if (!embarquement.Date_Enregistrement.HasValue)
                {
                    embarquement.Date_Enregistrement = DateTime.Now;
                }

                var message = await _repository.AddAsync(embarquement);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Erreur lors de l'ajout : {ex.Message}" });
            }
        }

        // ✅ PUT: api/Embarquements/modifier/{id}
        [HttpPut("modifier/{id}")]
        public async Task<ActionResult<string>> Update(int id, [FromBody] EmbarquementVoyagePassagerModel embarquement)
        {
            try
            {
                if (id != embarquement.Id_Embarquement)
                {
                    return BadRequest(new { message = "ID incohérent" });
                }

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound(new { message = $"Embarquement ID {id} non trouvé" });
                }

                var message = await _repository.UpdateAsync(embarquement);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Erreur lors de la modification : {ex.Message}" });
            }
        }

        // ✅ DELETE: api/Embarquements/supprimer/{id}
        [HttpDelete("supprimer/{id}")]
        public async Task<ActionResult<string>> Delete(int id)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound(new { message = $"Embarquement ID {id} non trouvé" });
                }

                var message = await _repository.DeleteAsync(id);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Erreur lors de la suppression : {ex.Message}" });
            }
        }

        // ✅ GET: api/Embarquements/rechercher?motCle=xxx
        [HttpGet("rechercher")]
        public async Task<ActionResult<List<EmbarquementVoyagePassagerModel>>> Search([FromQuery] string motCle)
        {
            try
            {
                var embarquements = await _repository.GetAllAsync();

                if (!string.IsNullOrEmpty(motCle))
                {
                    embarquements = embarquements.Where(e =>
                        (!string.IsNullOrEmpty(e.Nom_Passager) && e.Nom_Passager.Contains(motCle, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(e.Prenom_Passager) && e.Prenom_Passager.Contains(motCle, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(e.Trajet) && e.Trajet.Contains(motCle, StringComparison.OrdinalIgnoreCase)) ||
                        (e.Numero_Siege.HasValue && e.Numero_Siege.Value.ToString().Contains(motCle))
                    ).ToList();
                }

                return Ok(embarquements);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Erreur lors de la recherche : {ex.Message}" });
            }
        }

        // ✅ GET: api/Embarquements/par-voyage/{idVoyage}
        [HttpGet("par-voyage/{idVoyage}")]
        public async Task<ActionResult<List<EmbarquementVoyagePassagerModel>>> GetByVoyage(int idVoyage)
        {
            try
            {
                var embarquements = await _repository.GetAllAsync();
                var filtered = embarquements.Where(e => e.Id_Voyage == idVoyage).ToList();
                return Ok(filtered);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Erreur : {ex.Message}" });
            }
        }

        // ✅ GET: api/Embarquements/par-passager/{idPassager}
        [HttpGet("par-passager/{idPassager}")]
        public async Task<ActionResult<List<EmbarquementVoyagePassagerModel>>> GetByPassager(int idPassager)
        {
            try
            {
                var embarquements = await _repository.GetAllAsync();
                var filtered = embarquements.Where(e => e.Id_Passager == idPassager).ToList();
                return Ok(filtered);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Erreur : {ex.Message}" });
            }
        }

        // ✅ GET: api/Embarquements/par-statut/{statut}
        [HttpGet("par-statut/{statut}")]
        public async Task<ActionResult<List<EmbarquementVoyagePassagerModel>>> GetByStatut(string statut)
        {
            try
            {
                var embarquements = await _repository.GetAllAsync();
                var filtered = embarquements.Where(e =>
                    e.Statut_Embarquement != null &&
                    e.Statut_Embarquement.Equals(statut, StringComparison.OrdinalIgnoreCase)
                ).ToList();
                return Ok(filtered);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Erreur : {ex.Message}" });
            }
        }

        // ✅ GET: api/Embarquements/confirme
        [HttpGet("confirme")]
        public async Task<ActionResult<List<EmbarquementVoyagePassagerModel>>> GetConfirmes()
        {
            try
            {
                var embarquements = await _repository.GetAllAsync();
                var confirmes = embarquements.Where(e =>
                    e.Statut_Embarquement == "Confirmé" ||
                    e.Statut_Embarquement == "Présent"
                ).ToList();
                return Ok(confirmes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Erreur : {ex.Message}" });
            }
        }

        // ✅ GET: api/Embarquements/verification-siege/{idVoyage}/{numeroSiege}
        [HttpGet("verification-siege/{idVoyage}/{numeroSiege}")]
        public async Task<ActionResult<bool>> VerifierDisponibiliteSiege(int idVoyage, int numeroSiege)
        {
            try
            {
                var embarquements = await _repository.GetAllAsync();
                var siegeOccupe = embarquements.Any(e =>
                    e.Id_Voyage == idVoyage &&
                    e.Numero_Siege == numeroSiege &&
                    (e.Statut_Embarquement == "Confirmé" || e.Statut_Embarquement == "Présent")
                );
                return Ok(!siegeOccupe); // true si libre, false si occupé
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Erreur : {ex.Message}" });
            }
        }
    }
}