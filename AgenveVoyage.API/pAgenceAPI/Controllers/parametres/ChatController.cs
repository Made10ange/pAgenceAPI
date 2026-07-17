using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Repositories;
using pAgenceAPI.Services;

namespace pAgenceAPI.Controllers.parametres
{
    public class ChatDemandeDto
    {
        public string Message { get; set; } = string.Empty;
    }

    public class ChatReponseDto
    {
        public string Reponse { get; set; } = string.Empty;
        public object? Donnees { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : AgenceControllerBase
    {
        private readonly GeminiService _gemini;
        private readonly IChauffeurRepository _chauffeurs;
        private readonly IVoyageRepository _voyages;
        private readonly IVehiculeRepository _vehicules;
        private readonly IAgenceRepository _agences;
        private readonly IBilletRepository _billets;

        public ChatController(
            GeminiService gemini,
            IChauffeurRepository chauffeurs,
            IVoyageRepository voyages,
            IVehiculeRepository vehicules,
            IAgenceRepository agences,
            IBilletRepository billets)
        {
            _gemini = gemini;
            _chauffeurs = chauffeurs;
            _voyages = voyages;
            _vehicules = vehicules;
            _agences = agences;
            _billets = billets;
        }

        [HttpPost("demander")]
        public async Task<ActionResult<ChatReponseDto>> Demander([FromBody] ChatDemandeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest("Message vide.");

            var intention = await _gemini.InterpreterAsync(dto.Message);

            try
            {
            switch (intention.Action)
            {
                case "liste_chauffeurs":
                {
                    var liste = await _chauffeurs.GetAllAsync(AgenceId);
                    return Ok(new ChatReponseDto
                    {
                        Reponse = $"Voici {liste.Count} chauffeur(s) trouvé(s) :",
                        Donnees = liste.Select(c => new { c.Id_Chauffeur, c.Nom, c.Prenom, c.Telephone })
                    });
                }
                case "liste_voyages":
                {
                    var liste = await _voyages.GetAllAsync(AgenceId);
                    return Ok(new ChatReponseDto
                    {
                        Reponse = $"Voici {liste.Count} voyage(s) trouvé(s) :",
                        Donnees = liste.Select(v => new { v.Id_Voyage, v.Point_Depart, v.Point_Arrivee, v.Date_Depart, v.Statut })
                    });
                }
                case "liste_voyages_en_cours":
                {
                    var liste = await _voyages.GetByStatutAsync("En cours", AgenceId);
                    return Ok(new ChatReponseDto
                    {
                        Reponse = $"Voici {liste.Count} voyage(s) en cours :",
                        Donnees = liste.Select(v => new { v.Id_Voyage, v.Point_Depart, v.Point_Arrivee, v.Date_Depart, v.Statut })
                    });
                }
                case "liste_voyages_programmes":
                {
                    var liste = await _voyages.GetByStatutAsync("Programmé", AgenceId);
                    return Ok(new ChatReponseDto
                    {
                        Reponse = $"Voici {liste.Count} voyage(s) programmé(s) :",
                        Donnees = liste.Select(v => new { v.Id_Voyage, v.Point_Depart, v.Point_Arrivee, v.Date_Depart, v.Statut })
                    });
                }
                case "liste_voyages_termines":
                {
                    var liste = await _voyages.GetByStatutAsync("Terminé", AgenceId);
                    return Ok(new ChatReponseDto
                    {
                        Reponse = $"Voici {liste.Count} voyage(s) terminé(s) :",
                        Donnees = liste.Select(v => new { v.Id_Voyage, v.Point_Depart, v.Point_Arrivee, v.Date_Depart, v.Statut })
                    });
                }
                case "liste_vehicules":
                {
                    var liste = await _vehicules.GetAllAsync(AgenceId);
                    return Ok(new ChatReponseDto
                    {
                        Reponse = $"Voici {liste.Count} véhicule(s) trouvé(s) :",
                        Donnees = liste.Select(v => new { v.Id_Vehicule, v.Immatriculation, v.Libelle_Type, v.Statut })
                    });
                }
                case "liste_agences":
                {
                    var liste = await _agences.GetAllAsync();
                    return Ok(new ChatReponseDto
                    {
                        Reponse = $"Voici {liste.Count} agence(s) trouvée(s) :",
                        Donnees = liste.Select(a => new { a.Id_Agence, a.Nom_Agence, a.Ville })
                    });
                }
                case "liste_billets":
                {
                    var liste = (await _billets.GetAllAsync()).ToList();
                    return Ok(new ChatReponseDto
                    {
                        Reponse = $"Voici {liste.Count} billet(s) trouvé(s) :",
                        Donnees = liste.Select(b => new { b.Id_Billet, b.Numero_Billet, b.Point_Depart, b.Point_Arrivee, b.Statut })
                    });
                }
                case "voyages_aujourdhui":
                {
                    var liste = (await _voyages.GetAllAsync(AgenceId))
                        .Where(v => v.Date_Depart.Date == DateTime.Today).ToList();
                    return Ok(new ChatReponseDto
                    {
                        Reponse = liste.Count == 0
                            ? "Aucun voyage prévu aujourd'hui."
                            : $"Il y a {liste.Count} voyage(s) prévu(s) aujourd'hui :",
                        Donnees = liste.Select(v => new { v.Id_Voyage, v.Point_Depart, v.Point_Arrivee, v.Heure_Depart, v.Statut })
                    });
                }
                case "chauffeur_plus_actif":
                {
                    var voyagesTous = await _voyages.GetAllAsync(AgenceId);
                    var top = voyagesTous
                        .Where(v => !string.IsNullOrEmpty(v.Nom_Chauffeur))
                        .GroupBy(v => v.Nom_Chauffeur)
                        .Select(g => new { Nom = g.Key, NombreVoyages = g.Count() })
                        .OrderByDescending(x => x.NombreVoyages)
                        .FirstOrDefault();

                    return Ok(new ChatReponseDto
                    {
                        Reponse = top == null
                            ? "Aucune donnée de voyage disponible pour déterminer le chauffeur le plus actif."
                            : $"Le chauffeur le plus actif est {top.Nom} avec {top.NombreVoyages} voyage(s).",
                        Donnees = top == null ? null : new[] { top }
                    });
                }
                case "chercher_chauffeur":
                {
                    if (string.IsNullOrWhiteSpace(intention.MotCle))
                        return Ok(new ChatReponseDto { Reponse = "Précisez le nom du chauffeur que vous cherchez." });

                    var liste = await _chauffeurs.SearchAsync(intention.MotCle);
                    return Ok(new ChatReponseDto
                    {
                        Reponse = liste.Count == 0
                            ? $"Aucun chauffeur trouvé pour « {intention.MotCle} »."
                            : $"{liste.Count} chauffeur(s) correspondant à « {intention.MotCle} » :",
                        Donnees = liste.Select(c => new { c.Id_Chauffeur, c.Nom, c.Prenom, c.Telephone })
                    });
                }
                case "erreur_ia":
                    return Ok(new ChatReponseDto { Reponse = "Le service IA est momentanément indisponible, réessayez dans un instant." });
                default:
                    return Ok(new ChatReponseDto { Reponse = "Je n'ai pas compris votre demande. Vous pouvez me demander la liste des chauffeurs, des voyages, des véhicules, des agences ou des billets." });
            }
            }
            catch (Exception)
            {
                return Ok(new ChatReponseDto { Reponse = "Impossible d'accéder à la base de données pour le moment. Vérifiez que le serveur MySQL est démarré." });
            }
        }

        public class RechercheVoyageReponseDto
        {
            public string Reponse { get; set; } = string.Empty;
            public object? Voyages { get; set; }
            public bool EstSuggestion { get; set; } = false;
        }

        /// <summary>
        /// Recherche de voyage en langage naturel pour le portail public de réservation (sans authentification).
        /// Si aucun résultat exact, propose des alternatives (même trajet à une autre date, ou trajets proches).
        /// </summary>
        [HttpPost("recherche-voyage")]
        public async Task<ActionResult<RechercheVoyageReponseDto>> RechercheVoyage([FromBody] ChatDemandeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest("Message vide.");

            try
            {
                var criteres = await _gemini.ExtraireRechercheVoyageAsync(dto.Message);

                var tous = (await _voyages.GetAllAsync(null))
                    .Where(v => v.Statut == "Programmé")
                    .ToList();

                var resultats = tous.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(criteres.Depart))
                    resultats = resultats.Where(v => v.Point_Depart != null && v.Point_Depart.Contains(criteres.Depart, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(criteres.Arrivee))
                    resultats = resultats.Where(v => v.Point_Arrivee != null && v.Point_Arrivee.Contains(criteres.Arrivee, StringComparison.OrdinalIgnoreCase));
                if (criteres.Date.HasValue)
                    resultats = resultats.Where(v => v.Date_Depart.Date == criteres.Date.Value.Date);

                var listeExacte = resultats.OrderBy(v => v.Date_Depart).ToList();

                if (listeExacte.Count > 0)
                {
                    return Ok(new RechercheVoyageReponseDto
                    {
                        Reponse = $"J'ai trouvé {listeExacte.Count} voyage(s) correspondant à votre recherche :",
                        Voyages = ProjeterVoyages(listeExacte)
                    });
                }

                // Aucun résultat exact : on cherche des alternatives
                var alternatives = tous.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(criteres.Depart))
                    alternatives = alternatives.Where(v => v.Point_Depart != null && v.Point_Depart.Contains(criteres.Depart, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(criteres.Arrivee))
                    alternatives = alternatives.Where(v => v.Point_Arrivee != null && v.Point_Arrivee.Contains(criteres.Arrivee, StringComparison.OrdinalIgnoreCase));

                var listeAlternative = alternatives.OrderBy(v => v.Date_Depart).Take(5).ToList();

                if (listeAlternative.Count > 0)
                {
                    return Ok(new RechercheVoyageReponseDto
                    {
                        Reponse = "Aucun voyage exactement à cette date, mais voici les prochains disponibles sur ce trajet :",
                        Voyages = ProjeterVoyages(listeAlternative),
                        EstSuggestion = true
                    });
                }

                return Ok(new RechercheVoyageReponseDto
                {
                    Reponse = "Aucun voyage trouvé pour ce trajet. Essayez avec d'autres villes ou consultez la liste complète des voyages disponibles.",
                    Voyages = null
                });
            }
            catch (Exception)
            {
                return Ok(new RechercheVoyageReponseDto { Reponse = "Le service de recherche est momentanément indisponible. Utilisez le formulaire de recherche classique ci-dessous." });
            }
        }

        private static object ProjeterVoyages(IEnumerable<pAgenceAPI.Models.VoyageModel> liste) =>
            liste.Select(v => new
            {
                v.Id_Voyage, v.Point_Depart, v.Point_Arrivee, v.Date_Depart, v.Heure_Depart,
                v.Prix, v.Libelle_Type_Voyage, v.Nom_Agence
            });
    }
}
