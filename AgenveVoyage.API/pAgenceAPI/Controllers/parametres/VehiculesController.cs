using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiculesController : AgenceControllerBase
    {
        private readonly IVehiculeRepository _repository;
        private readonly IHistoriqueEtatVehiculeRepository _historiqueRepo;

        public VehiculesController(IVehiculeRepository repository,
                                   IHistoriqueEtatVehiculeRepository historiqueRepo)
        {
            _repository    = repository;
            _historiqueRepo = historiqueRepo;
        }

        // ✅ GET: api/Vehicules/liste
        [HttpGet("liste")]
        public async Task<ActionResult<List<VehiculeModel>>> GetAll()
        {
            var vehicules = await _repository.GetAllAsync(AgenceId);
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
            vehicule.Id_Agence = AgenceId;
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

        // POST: api/Vehicules/changer-etat/{id}
        [HttpPost("changer-etat/{id}")]
        public async Task<ActionResult> ChangerEtat(int id, [FromBody] ChangerEtatRequest req)
        {
            var vehicule = await _repository.GetByIdAsync(id);
            if (vehicule == null) return NotFound();

            var historique = new HistoriqueEtatVehiculeModel
            {
                Id_Vehicule      = id,
                Ancien_Etat      = vehicule.Etat,
                Nouvel_Etat      = req.Nouvel_Etat,
                Ancien_Type      = vehicule.Id_Type,
                Nouveau_Type     = req.Nouveau_Type ?? vehicule.Id_Type,
                Motif            = req.Motif,
                Date_Changement  = DateTime.Now,
                Modifie_Par      = req.Modifie_Par
            };

            vehicule.Etat = req.Nouvel_Etat;
            if (req.Nouveau_Type.HasValue)
                vehicule.Id_Type = req.Nouveau_Type.Value;

            await _repository.UpdateAsync(vehicule);
            await _historiqueRepo.AddAsync(historique);

            return Ok(new { message = "État mis à jour avec succès." });
        }

        // GET: api/Vehicules/historique/{id}
        [HttpGet("historique/{id}")]
        public async Task<ActionResult<List<HistoriqueEtatVehiculeModel>>> GetHistorique(int id)
        {
            var historique = await _historiqueRepo.GetByVehiculeAsync(id);
            return Ok(historique);
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