using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoyagesController : AgenceControllerBase
    {
        private readonly IVoyageRepository _repository;
        private readonly IVehiculeRepository _vehiculeRepository;
        private readonly ITypeVoyageRepository _typeVoyageRepository;
        private readonly IColisRepository _colisRepository;
        private readonly IBagageRepository _bagageRepository;
        private readonly ILogger<VoyagesController> _logger;

        private static readonly HashSet<string> AllowedStatuts = new(StringComparer.OrdinalIgnoreCase)
        {
            "Programmé",
            "Ouvert",
            "En cours",
            "Terminé",
            "Annulé"
        };

        public VoyagesController(
            IVoyageRepository repository,
            IVehiculeRepository vehiculeRepository,
            ITypeVoyageRepository typeVoyageRepository,
            IColisRepository colisRepository,
            IBagageRepository bagageRepository,
            ILogger<VoyagesController> logger)
        {
            _repository = repository;
            _vehiculeRepository = vehiculeRepository;
            _typeVoyageRepository = typeVoyageRepository;
            _colisRepository = colisRepository;
            _bagageRepository = bagageRepository;
            _logger = logger;
        }

        [HttpGet]
        [HttpGet("liste")]
        public async Task<ActionResult<List<VoyageModel>>> GetAll()
        {
            try
            {
                var voyages = await _repository.GetAllAsync(AgenceId);
                return Ok(voyages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des voyages");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors du chargement des voyages");
            }
        }

        [HttpGet("rechercher")]
        public async Task<ActionResult<List<VoyageModel>>> Search([FromQuery] string motCle)
        {
            try
            {
                var voyages = string.IsNullOrWhiteSpace(motCle)
                    ? await _repository.GetAllAsync(AgenceId)
                    : await _repository.SearchAsync(motCle, AgenceId);

                return Ok(voyages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Search voyages");
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la recherche des voyages");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VoyageModel>> GetById(int id)
        {
            try
            {
                var voyage = await _repository.GetByIdAsync(id);
                if (voyage == null) return NotFound();
                return Ok(voyage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetById voyage id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la récupération du voyage");
            }
        }

        [HttpGet("par-statut/{statut}")]
        public async Task<ActionResult<List<VoyageModel>>> GetByStatut(string statut)
        {
            try
            {
                var voyages = await _repository.GetByStatutAsync(statut);
                return Ok(voyages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetByStatut statut={Statut}", statut);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors du filtre par statut");
            }
        }

        [HttpGet("par-vehicule/{idVehicule}")]
        public async Task<ActionResult<List<VoyageModel>>> GetByVehicule(int idVehicule)
        {
            try
            {
                var voyages = await _repository.GetByVehiculeAsync(idVehicule);
                return Ok(voyages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetByVehicule idVehicule={IdVehicule}", idVehicule);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors du filtre par véhicule");
            }
        }

        [HttpPost("ajouter")]
        public async Task<ActionResult<string>> Create([FromBody] VoyageModel voyage)
        {
            try
            {
                _logger.LogInformation("Ajout voyage : {@Voyage}", voyage);

                voyage.Id_Agence = AgenceId;
                var validationResult = await ValidateVoyageAsync(voyage);
                if (validationResult != null) return validationResult;

                var success = await _repository.AddAsync(voyage);
                if (!success)
                    return Problem(detail: "Aucune ligne insérée", statusCode: 500, title: "Erreur lors de l'ajout du voyage");

                _logger.LogInformation("Voyage ajouté avec succès");
                return Ok(new { message = "Voyage ajouté avec succès !" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Create voyage: {@Voyage}", voyage);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de l'ajout du voyage");
            }
        }

        [HttpPut("modifier/{id}")]
        public async Task<ActionResult<string>> Update(int id, [FromBody] VoyageModel voyage)
        {
            try
            {
                if (id != voyage.Id_Voyage) return BadRequest("ID incohérent");

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { message = $"Voyage ID {id} non trouvé" });

                var validationResult = await ValidateVoyageAsync(voyage, id);
                if (validationResult != null) return validationResult;

                var success = await _repository.UpdateAsync(voyage);
                if (!success)
                    return Problem(detail: "Aucune ligne modifiée", statusCode: 500, title: "Erreur lors de la modification du voyage");

                // Synchroniser le statut des colis et bagages si le voyage vient de changer de statut
                if (!string.Equals(existing.Statut, voyage.Statut, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(voyage.Statut, "En cours", StringComparison.OrdinalIgnoreCase))
                        await _colisRepository.UpdateStatutByVoyageAsync(id, "En attente", "En cours");
                    else if (string.Equals(voyage.Statut, "Terminé", StringComparison.OrdinalIgnoreCase))
                    {
                        await _colisRepository.LivrerParVoyageAsync(id);
                        await _bagageRepository.LivrerParVoyageAsync(id);
                    }
                }

                return Ok(new { message = "Voyage modifié avec succès !" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Update voyage id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la modification du voyage");
            }
        }

        [HttpDelete("supprimer/{id}")]
        public async Task<ActionResult<string>> Delete(int id)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { message = $"Voyage ID {id} non trouvé" });

                var success = await _repository.DeleteAsync(id);
                if (!success)
                    return Problem(detail: "Aucune ligne supprimée", statusCode: 500, title: "Erreur lors de la suppression du voyage");

                return Ok(new { message = "Voyage supprimé avec succès !" });
            }
            catch (MySqlConnector.MySqlException ex) when (ex.Number == 1451)
            {
                _logger.LogError(ex, "Contrainte d'intégrité Delete voyage id={Id}", id);
                return Problem(
                    detail: "Ce voyage est lié à des billets, réservations ou paiements existants et ne peut pas être supprimé.",
                    statusCode: 409,
                    title: "Suppression impossible");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Delete voyage id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la suppression du voyage");
            }
        }

        [HttpPatch("modifier-statut/{id}")]
        public async Task<ActionResult<string>> UpdateStatus(int id, [FromBody] string statut)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(statut))
                    return BadRequest(new { message = "Le statut est obligatoire" });

                var voyage = await _repository.GetByIdAsync(id);
                if (voyage == null)
                    return NotFound(new { message = $"Voyage ID {id} non trouvé" });

                var normalizedStatut = NormalizeStatut(statut);
                if (normalizedStatut == null)
                {
                    return BadRequest(new
                    {
                        message = "Statut invalide",
                        valeursAutorisees = AllowedStatuts
                    });
                }

                var success = await _repository.UpdateStatutAsync(id, normalizedStatut);
                if (!success)
                    return Problem(detail: "Aucune ligne modifiée", statusCode: 500, title: "Erreur lors de la mise à jour du statut");

                // Synchroniser le statut des colis et bagages selon la transition du voyage
                if (normalizedStatut == "En cours")
                    await _colisRepository.UpdateStatutByVoyageAsync(id, "En attente", "En cours");
                else if (normalizedStatut == "Terminé")
                {
                    await _colisRepository.UpdateStatutByVoyageAsync(id, "En cours", "Livré");
                    await _bagageRepository.LivrerParVoyageAsync(id);
                }

                return Ok(new { message = "Statut mis à jour avec succès !" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur UpdateStatus voyage id={Id}", id);
                return Problem(detail: ex.Message, statusCode: 500, title: "Erreur lors de la mise à jour du statut");
            }
        }

        private async Task<ActionResult?> ValidateVoyageAsync(VoyageModel voyage, int? voyageId = null)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState invalide : {@Errors}", ModelState.Values.SelectMany(m => m.Errors));
                return BadRequest(ModelState);
            }

            // Si Point_Depart/Arrivee sont vides, les récupérer depuis le TypeVoyage
            if ((string.IsNullOrWhiteSpace(voyage.Point_Depart) || string.IsNullOrWhiteSpace(voyage.Point_Arrivee))
                && voyage.Id_Type_Voyage.HasValue)
            {
                var tv = await _typeVoyageRepository.GetByIdAsync(voyage.Id_Type_Voyage.Value);
                if (tv == null)
                    return BadRequest(new { message = $"Type de voyage ID {voyage.Id_Type_Voyage.Value} non trouvé" });
                voyage.Point_Depart  = tv.Point_Depart;
                voyage.Point_Arrivee = tv.Point_Arrivee;
            }

            if (string.IsNullOrWhiteSpace(voyage.Point_Depart) || string.IsNullOrWhiteSpace(voyage.Point_Arrivee))
                return BadRequest(new { message = "Les points de départ et d'arrivée sont obligatoires. Sélectionnez un type de voyage." });

            if (string.Equals(voyage.Point_Depart.Trim(), voyage.Point_Arrivee.Trim(), StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Le point de départ doit être différent du point d'arrivée" });

            var depart  = voyage.Date_Depart.Date.Add(voyage.Heure_Depart ?? TimeSpan.Zero);
            var dateArrivee = voyage.Date_Arrivee ?? voyage.Date_Depart;
            var arrivee = voyage.Heure_Arrivee.HasValue
                ? dateArrivee.Date.Add(voyage.Heure_Arrivee.Value)
                : dateArrivee.Date.AddHours(23).AddMinutes(59);
            if (voyage.Heure_Arrivee.HasValue && arrivee <= depart)
                return BadRequest(new { message = "La date et l'heure d'arrivée doivent être postérieures au départ" });

            if (voyage.Duree.HasValue && voyage.Duree.Value <= TimeSpan.Zero)
                return BadRequest(new { message = "La durée doit être positive lorsqu'elle est renseignée" });

            voyage.Duree = arrivee - depart;

            var normalizedStatut = NormalizeStatut(voyage.Statut);
            if (normalizedStatut == null)
            {
                return BadRequest(new
                {
                    message = "Statut invalide",
                    valeursAutorisees = AllowedStatuts
                });
            }

            voyage.Statut = normalizedStatut;
            voyage.Point_Depart = voyage.Point_Depart.Trim();
            voyage.Point_Arrivee = voyage.Point_Arrivee.Trim();

            var vehicule = await _vehiculeRepository.GetByIdAsync(voyage.Id_Vehicule);
            if (vehicule == null)
                return BadRequest(new { message = $"Véhicule ID {voyage.Id_Vehicule} non trouvé" });

            if (!string.Equals(vehicule.Statut, "Disponible", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(vehicule.Statut, "Affecté", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = $"Le véhicule {vehicule.Immatriculation} n'est pas disponible pour un voyage",
                    statutVehicule = vehicule.Statut
                });
            }

            // Vérifier la compatibilité type véhicule / type voyage
            if (vehicule.Id_Type_Voyage.HasValue && voyage.Id_Type_Voyage.HasValue
                && vehicule.Id_Type_Voyage.Value != voyage.Id_Type_Voyage.Value)
            {
                var typeVoyageDemande = await _typeVoyageRepository.GetByIdAsync(voyage.Id_Type_Voyage.Value);
                return BadRequest(new
                {
                    message = $"Le véhicule {vehicule.Immatriculation} est réservé au type « {vehicule.Libelle_Type_Voyage} » et ne peut pas effectuer un voyage de type « {typeVoyageDemande?.Libelle_Type_Voyage ?? "autre"} »."
                });
            }

            var hasConflict = await _repository.HasScheduleConflictAsync(
                voyage.Id_Vehicule,
                voyage.Date_Depart,
                voyage.Date_Arrivee ?? voyage.Date_Depart,
                voyage.Heure_Depart ?? TimeSpan.Zero,
                voyage.Heure_Arrivee ?? TimeSpan.Zero,
                voyageId);

            if (hasConflict)
            {
                return Conflict(new
                {
                    message = "Ce véhicule est déjà planifié sur un autre voyage pendant cette période"
                });
            }

            return null;
        }

        private static string? NormalizeStatut(string? statut)
        {
            if (string.IsNullOrWhiteSpace(statut))
                return "Programmé";

            return AllowedStatuts.FirstOrDefault(value =>
                string.Equals(value, statut.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
