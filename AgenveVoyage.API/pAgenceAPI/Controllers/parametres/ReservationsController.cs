using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers.parametres
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationRepository _repo;
        private readonly IPassagerRepository _passagerRepo;
        private readonly IEmbarquementRepository _embarquementRepo;
        private readonly IVoyageRepository _voyageRepo;
        private readonly IAgenceRepository _agenceRepo;
        private readonly IEcritureRepository _ecritureRepo;
        private readonly IBilletRepository _billetRepo;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(
            IReservationRepository repo,
            IPassagerRepository passagerRepo,
            IEmbarquementRepository embarquementRepo,
            IVoyageRepository voyageRepo,
            IAgenceRepository agenceRepo,
            IEcritureRepository ecritureRepo,
            IBilletRepository billetRepo,
            ILogger<ReservationsController> logger)
        {
            _repo = repo;
            _passagerRepo = passagerRepo;
            _embarquementRepo = embarquementRepo;
            _voyageRepo = voyageRepo;
            _agenceRepo = agenceRepo;
            _ecritureRepo = ecritureRepo;
            _billetRepo = billetRepo;
            _logger = logger;
        }

        // GET api/Reservations/liste
        [HttpGet("liste")]
        public async Task<IActionResult> Liste([FromQuery] string? recherche)
        {
            try
            {
                var data = string.IsNullOrWhiteSpace(recherche)
                    ? await _repo.GetAllAsync()
                    : await _repo.SearchAsync(recherche);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur liste réservations");
                return StatusCode(500, "Erreur serveur");
            }
        }

        // GET api/Reservations/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var r = await _repo.GetByIdAsync(id);
            return r is null ? NotFound() : Ok(r);
        }

        // GET api/Reservations/reference/{ref}
        [HttpGet("reference/{reference}")]
        public async Task<IActionResult> GetByReference(string reference)
        {
            var r = await _repo.GetByReferenceAsync(reference);
            return r is null ? NotFound(new { message = "Réservation introuvable" }) : Ok(r);
        }

        // GET api/Reservations/voyage/{idVoyage}
        [HttpGet("voyage/{idVoyage}")]
        public async Task<IActionResult> GetByVoyage(int idVoyage)
        {
            var data = await _repo.GetByVoyageAsync(idVoyage);
            return Ok(data);
        }

        // POST api/Reservations/creer
        [HttpPost("creer")]
        public async Task<IActionResult> Creer([FromBody] ReservationModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Vérifier disponibilité du siège
                if (model.Numero_Siege.HasValue)
                {
                    var dispo = await _repo.SiegeDisponibleAsync(model.Id_Voyage, model.Numero_Siege.Value);
                    if (!dispo)
                        return BadRequest(new { message = $"Le siège {model.Numero_Siege} est déjà réservé." });
                }

                var id = await _repo.AddAsync(model);

                try
                {
                    await _repo.AjouterLogAsync(new PaiementLogModel
                    {
                        Id_Reservation = id,
                        Evenement = "RESERVATION_CREEE",
                        Montant = model.Montant
                    });
                }
                catch { /* log optionnel, ne bloque pas */ }

                var created = await _repo.GetByIdAsync(id);
                return Ok(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur création réservation");
                return StatusCode(500, "Erreur serveur");
            }
        }

        // PUT api/Reservations/confirmer-paiement/{id}
        [HttpPut("confirmer-paiement/{id}")]
        public async Task<IActionResult> ConfirmerPaiement(int id, [FromBody] ConfirmerPaiementDto dto)
        {
            try
            {
                var ok = await _repo.UpdateStatutPaiementAsync(id, dto.StatutPaiement, dto.ReferencePaiement, dto.Provider);
                if (!ok) return NotFound();

                try
                {
                    await _repo.AjouterLogAsync(new PaiementLogModel
                    {
                        Id_Reservation = id,
                        Evenement = dto.StatutPaiement == "Payé" ? "PAIEMENT_CONFIRME" : "PAIEMENT_ECHOUE",
                        Reference_Externe = dto.ReferencePaiement,
                        Payload_Brut = dto.PayloadBrut
                    });
                }
                catch { /* log optionnel */ }

                // Paiement confirmé : écriture comptable + passager + embarquement
                if (dto.StatutPaiement == "Payé")
                {
                    try
                    {
                        var reservation = await _repo.GetByIdAsync(id);
                        if (reservation != null)
                        {
                            var voyage  = await _voyageRepo.GetByIdAsync(reservation.Id_Voyage);
                            var agences = await _agenceRepo.GetAllAsync();
                            var agenceDepart = agences.FirstOrDefault(a =>
                                string.Equals(a.Ville?.Trim(), reservation.Point_Depart?.Trim(), StringComparison.OrdinalIgnoreCase));
                            var idAgenceVente = agenceDepart?.Id_Agence ?? voyage?.Id_Agence;

                            // Créer le passager, le billet et l'embarquement seulement s'il n'est pas encore lié
                            if (reservation.Id_Passager == null)
                            {
                                var idPassager = await _passagerRepo.AddAsync(new PassagerModel
                                {
                                    Nom          = reservation.Nom_Client,
                                    Prenom       = reservation.Prenom_Client,
                                    Telephone    = reservation.Telephone_Client,
                                    Email        = reservation.Email_Client,
                                    Type_Piece   = "CNI",
                                    Numero_Piece = reservation.Numero_Cni_Client,
                                    Id_Agence    = idAgenceVente
                                });

                                await _repo.SetPassagerAsync(id, idPassager);

                                // Créer un billet officiel pour le passager en ligne
                                await _billetRepo.AjouterAsync(new BilletModel
                                {
                                    Id_Passager    = idPassager,
                                    Point_Depart   = reservation.Point_Depart ?? "",
                                    Point_Arrivee  = reservation.Point_Arrivee ?? "",
                                    Id_Type_Voyage = voyage?.Id_Type_Voyage,
                                    Montant        = reservation.Montant,
                                    Date_Achat     = DateTime.Now,
                                    Id_Voyage_Prevu = reservation.Id_Voyage,
                                    Numero_Siege   = reservation.Numero_Siege,
                                    Mode_Paiement  = dto.Provider ?? "Mobile Money",
                                    Vendu_Par      = "En ligne",
                                    Statut         = "Valide"
                                });

                                await _embarquementRepo.AddAsync(new EmbarquementVoyagePassagerModel
                                {
                                    Id_Voyage           = reservation.Id_Voyage,
                                    Id_Passager         = idPassager,
                                    Statut_Embarquement = "En attente",
                                    Numero_Siege        = reservation.Numero_Siege,
                                    Date_Enregistrement = DateTime.Now
                                });
                            }

                            // Écriture comptable — toujours générée si journée ouverte, quelle que soit la date
                            if (reservation.Montant > 0)
                            {
                                var trajet = $"{reservation.Point_Depart} - {reservation.Point_Arrivee}";
                                await _ecritureRepo.EcritureVenteBilletAsync(
                                    $"RES-{reservation.Reference}", reservation.Reference, reservation.Montant,
                                    "En ligne", trajet, idAgenceVente, null);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erreur écriture/passager après paiement, reservation id={Id}", id);
                    }
                }

                return Ok(new { message = "Statut mis à jour" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur confirmation paiement");
                return StatusCode(500, "Erreur serveur");
            }
        }

        // PUT api/Reservations/valider/{id}
        [HttpPut("valider/{id}")]
        public async Task<IActionResult> Valider(int id, [FromBody] ValiderDto dto)
        {
            var ok = await _repo.ValiderAsync(id, dto.ValideePar);
            if (!ok) return BadRequest(new { message = "Réservation déjà utilisée ou non confirmée." });
            return Ok(new { message = "Réservation validée avec succès" });
        }

        // DELETE api/Reservations/supprimer/{id}
        [HttpDelete("supprimer/{id}")]
        public async Task<IActionResult> Supprimer(int id)
        {
            var ok = await _repo.DeleteAsync(id);
            return ok ? Ok() : NotFound();
        }
    }

    public class ConfirmerPaiementDto
    {
        public string StatutPaiement { get; set; } = string.Empty;
        public string? ReferencePaiement { get; set; }
        public string? Provider { get; set; }
        public string? PayloadBrut { get; set; }
    }

    public class ValiderDto
    {
        public string ValideePar { get; set; } = "Agent";
    }
}
