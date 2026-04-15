using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;
using System;
using System.Linq;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiculesController : ControllerBase
    {
        private readonly IVehiculeRepository _repository;

        public VehiculesController(IVehiculeRepository repository)
        {
            _repository = repository;
        }

        // ✅ GET: api/Vehicules/liste
        [HttpGet("liste")]
        public async Task<ActionResult<List<VehiculeModel>>> GetAll()
        {
            var vehicules = await _repository.GetAllAsync();
            return Ok(vehicules);
        }

        // ✅ GET: api/Vehicules/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<VehiculeModel>> GetById(int id)
        {
            var vehicule = await _repository.GetByIdAsync(id);
            if (vehicule == null) return NotFound();
            return Ok(vehicule);
        }

        // ✅ POST: api/Vehicules/ajouter
        [HttpPost("ajouter")]
        public async Task<ActionResult<string>> Create([FromBody] VehiculeModel vehicule)
        {
            var message = await _repository.AddAsync(vehicule);
            return Ok(new { message });
        }

        // ✅ PUT: api/Vehicules/modifier/{id}
        [HttpPut("modifier/{id}")]
        public async Task<ActionResult<string>> Update(int id, [FromBody] VehiculeModel vehicule)
        {
            if (id != vehicule.Id_Vehicule) return BadRequest("ID incohérent");
            var message = await _repository.UpdateAsync(vehicule);
            return Ok(new { message });
        }

        // ✅ DELETE: api/Vehicules/supprimer/{id}
        [HttpDelete("supprimer/{id}")]
        public async Task<ActionResult<string>> Delete(int id)
        {
            var message = await _repository.DeleteAsync(id);
            return Ok(new { message });
        }

        // ✅ GET: api/Vehicules/par-statut?statut=Disponible
        [HttpGet("par-statut")]
        public async Task<ActionResult<List<VehiculeModel>>> GetByStatut([FromQuery] string statut)
        {
            var vehicules = await _repository.GetByStatutAsync(statut);
            return Ok(vehicules);
        }

        // ✅ GET: api/Vehicules/rechercher?motCle=xxx
        [HttpGet("rechercher")]
        public async Task<ActionResult<List<VehiculeModel>>> Search([FromQuery] string motCle)
        {
            var vehicules = await _repository.GetAllAsync();

            if (!string.IsNullOrEmpty(motCle))
            {
                vehicules = vehicules.Where(v =>
                    v.Immatriculation.Contains(motCle, StringComparison.OrdinalIgnoreCase) ||
                    (v.Libelle_Type != null && v.Libelle_Type.Contains(motCle, StringComparison.OrdinalIgnoreCase)) ||
                    (v.Marque != null && v.Marque.Contains(motCle, StringComparison.OrdinalIgnoreCase)) ||
                    v.Statut.Contains(motCle, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return Ok(vehicules);
        }
    }
}