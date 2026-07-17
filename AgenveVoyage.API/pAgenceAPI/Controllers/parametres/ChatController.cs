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
    }
}
