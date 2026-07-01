using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using pAgenceAPI.Models;
using pAgenceAPI.Repositories;

namespace pAgenceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUtilisateurRepository _repo;
    private readonly IHistoriqueConnexionRepository _cnxRepo;
    private readonly IAgenceRepository _agenceRepo;

    public AuthController(IUtilisateurRepository repo, IHistoriqueConnexionRepository cnxRepo, IAgenceRepository agenceRepo)
    {
        _repo       = repo;
        _cnxRepo    = cnxRepo;
        _agenceRepo = agenceRepo;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.MotDePasse))
            return BadRequest(new LoginResponse { Succes = false, Message = "Login et mot de passe requis." });

        var ip        = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();
        var login     = request.Login.Trim().ToLower();

        // Pour l'admin : chercher sans filtre d'agence
        var agent = await _repo.GetByLoginAsync(login, null);

        if (agent == null || !BCrypt.Net.BCrypt.Verify(request.MotDePasse, agent.MotDePasse))
        {
            await _cnxRepo.EnregistrerAsync(new HistoriqueConnexionModel
            {
                Login_Tente = login,
                Statut      = "Échec",
                Motif_Echec = agent == null ? "Compte introuvable" : "Mot de passe incorrect",
                IP_Address  = ip,
                User_Agent  = userAgent
            });
            return Ok(new LoginResponse { Succes = false, Message = "Identifiants incorrects ou agence non autorisée." });
        }

        // Si non-Admin, vérifier que l'agence sélectionnée correspond à son agence
        if (agent.Role != "Admin" && agent.Id_Agence != request.Id_Agence)
        {
            await _cnxRepo.EnregistrerAsync(new HistoriqueConnexionModel
            {
                Login_Tente = login,
                Statut      = "Échec",
                Motif_Echec = "Agence non autorisée",
                IP_Address  = ip,
                User_Agent  = userAgent
            });
            return Ok(new LoginResponse { Succes = false, Message = "Identifiants incorrects ou agence non autorisée." });
        }

        // Si Admin, utiliser l'agence sélectionnée (pas celle liée à son compte)
        int? idAgenceSession = agent.Id_Agence;
        string? nomAgenceSession = agent.Nom_Agence;
        string? villeAgenceSession = agent.Ville_Agence;

        if (agent.Role == "Admin" && request.Id_Agence.HasValue)
        {
            var agenceSelectionnee = await _agenceRepo.GetByIdAsync(request.Id_Agence.Value);
            if (agenceSelectionnee != null)
            {
                idAgenceSession    = agenceSelectionnee.Id_Agence;
                nomAgenceSession   = agenceSelectionnee.Nom_Agence;
                villeAgenceSession = agenceSelectionnee.Ville;
            }
        }

        await _cnxRepo.EnregistrerAsync(new HistoriqueConnexionModel
        {
            Id_Utilisateur    = agent.Id_Utilisateur,
            Login_Tente = login,
            Nom_Agent   = $"{agent.Prenom} {agent.Nom}",
            Statut      = "Succès",
            IP_Address  = ip,
            User_Agent  = userAgent
        });

        return Ok(new LoginResponse
        {
            Succes       = true,
            Id_Utilisateur     = agent.Id_Utilisateur,
            Nom          = agent.Nom,
            Prenom       = agent.Prenom,
            Role         = agent.Role,
            Id_Agence    = idAgenceSession,
            Nom_Agence   = nomAgenceSession,
            Ville_Agence = villeAgenceSession
        });
    }
}

