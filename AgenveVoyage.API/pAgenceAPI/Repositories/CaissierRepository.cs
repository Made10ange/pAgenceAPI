using System.Data;
using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;
using BCrypt.Net;

namespace pAgenceAPI.Repositories;

// Gestion des caissiers via procédures stockées
public class CaissierRepository : ICaissierRepository
{
    private readonly string _cs;

    public CaissierRepository(IConfiguration config)
        => _cs = config.GetConnectionString("DefaultConnection")!;

    public async Task<List<CaissierModel>> GetAllAsync(int? idAgence = null)
    {
        using var db = new MySqlConnection(_cs);
        var result = (await db.QueryAsync<CaissierModel>("CALL SP_LISTE_CAISSIERS()")).ToList();

        // Un caissier non encore rattaché à une agence (Id_Agence NULL) reste visible
        // partout jusqu'à ce qu'il soit affecté manuellement (même logique que Caisses/Personnel).
        if (idAgence.HasValue)
            result = result.Where(c => c.Id_Agence is null || c.Id_Agence == idAgence).ToList();

        return result;
    }

    public async Task<CaissierModel?> GetByIdAsync(int id)
    {
        using var db = new MySqlConnection(_cs);
        return await db.QueryFirstOrDefaultAsync<CaissierModel>(
            "CALL SP_GET_CAISSIER(@p_id)", new { p_id = id });
    }

    public async Task<int> AjouterAsync(CaissierModel model)
    {
        // Hacher le mot de passe avant insertion
        var hash = BCrypt.Net.BCrypt.HashPassword(model.MotDePasse);

        using var db = new MySqlConnection(_cs);
        var p = new DynamicParameters();
        p.Add("p_nom",             model.Nom);
        p.Add("p_prenom",          model.Prenom);
        p.Add("p_sexe",            model.Sexe);
        p.Add("p_telephone",       model.Telephone);
        p.Add("p_email",           model.Email);
        p.Add("p_date_naissance",  model.Date_Naissance);
        p.Add("p_lieu_naissance",  model.Lieu_Naissance);
        p.Add("p_nationalite",     model.Nationalite);
        p.Add("p_profession",      model.Profession);
        p.Add("p_type_piece",      model.Type_Piece);
        p.Add("p_numero_piece",    model.Numero_Piece);
        p.Add("p_date_delivrance", model.Date_Delivrance);
        p.Add("p_lieu_delivrance", model.Lieu_Delivrance);
        p.Add("p_signataire",      model.Signataire);
        p.Add("p_date_expiration", model.Date_Expiration);
        p.Add("p_photo",           model.Photo);
        p.Add("p_login",      model.Login);
        p.Add("p_motdepasse", hash);
        p.Add("p_id_agence",  model.Id_Agence);
        p.Add("p_id_groupe",  model.Id_Groupe);
        p.Add("p_id", dbType: System.Data.DbType.Int32,
              direction: System.Data.ParameterDirection.Output);

        await db.ExecuteAsync("SP_AJOUTER_CAISSIER", p, commandType: CommandType.StoredProcedure);
        return p.Get<int>("p_id");
    }

    public async Task<bool> ModifierAsync(CaissierModel model)
    {
        using var db = new MySqlConnection(_cs);
        var p = new DynamicParameters();
        p.Add("p_id",              model.Id_Utilisateur);
        p.Add("p_nom",             model.Nom);
        p.Add("p_prenom",          model.Prenom);
        p.Add("p_sexe",            model.Sexe);
        p.Add("p_telephone",       model.Telephone);
        p.Add("p_email",           model.Email);
        p.Add("p_date_naissance",  model.Date_Naissance);
        p.Add("p_lieu_naissance",  model.Lieu_Naissance);
        p.Add("p_nationalite",     model.Nationalite);
        p.Add("p_profession",      model.Profession);
        p.Add("p_type_piece",      model.Type_Piece);
        p.Add("p_numero_piece",    model.Numero_Piece);
        p.Add("p_date_delivrance", model.Date_Delivrance);
        p.Add("p_lieu_delivrance", model.Lieu_Delivrance);
        p.Add("p_signataire",      model.Signataire);
        p.Add("p_date_expiration", model.Date_Expiration);
        p.Add("p_photo",           model.Photo);
        p.Add("p_maj_photo",       string.IsNullOrEmpty(model.Photo_Base64) ? 0 : 1);
        p.Add("p_login",     model.Login);
        p.Add("p_id_agence", model.Id_Agence);
        p.Add("p_actif",     model.Actif ? 1 : 0);
        p.Add("p_id_groupe", model.Id_Groupe);

        await db.ExecuteAsync("SP_MODIFIER_CAISSIER", p, commandType: CommandType.StoredProcedure);
        return true;
    }

    public async Task<bool> ToggleActifAsync(int id)
    {
        using var db = new MySqlConnection(_cs);
        var p = new DynamicParameters();
        p.Add("p_id", id);
        p.Add("p_actif", dbType: System.Data.DbType.Boolean,
              direction: System.Data.ParameterDirection.Output);

        await db.ExecuteAsync("SP_TOGGLE_CAISSIER", p, commandType: CommandType.StoredProcedure);
        return true;
    }

    public async Task<bool> SupprimerAsync(int id)
    {
        using var db = new MySqlConnection(_cs);
        await db.ExecuteAsync("CALL SP_SUPPRIMER_CAISSIER(@p_id)", new { p_id = id });
        return true;
    }
}
