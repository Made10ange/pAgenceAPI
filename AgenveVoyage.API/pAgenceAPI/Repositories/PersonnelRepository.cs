using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public class PersonnelRepository : IPersonnelRepository
{
    private readonly string _connectionString;

    public PersonnelRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string manquante.");
    }

    private const string SelectJoin = @"
        SELECT p.*, po.Libelle AS LibellePoste, a.NOM_AGENCE AS Nom_Agence
        FROM PERSONNEL p
        LEFT JOIN POSTE po ON p.ID_POSTE = po.ID_POSTE
        LEFT JOIN agence a ON p.Id_Agence = a.ID_AGENCE";

    public async Task<IEnumerable<PersonnelModel>> GetAllAsync(int? idAgence = null)
    {
        using var connection = new MySqlConnection(_connectionString);
        // Le personnel non encore rattaché à une agence (Id_Agence NULL) reste visible
        // partout jusqu'à ce qu'il soit affecté manuellement.
        var where = idAgence.HasValue ? " WHERE p.Id_Agence = @idAgence OR p.Id_Agence IS NULL" : "";
        return await connection.QueryAsync<PersonnelModel>(
            SelectJoin + where + " ORDER BY p.Nom, p.Prenom", new { idAgence });
    }

    public async Task<PersonnelModel?> GetByIdAsync(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<PersonnelModel>(
            SelectJoin + " WHERE p.ID_PERSONNEL = @Id", new { Id = id });
    }

    public async Task<int> AddAsync(PersonnelModel personnel)
    {
        using var connection = new MySqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(@"
            INSERT INTO PERSONNEL (Nom, Prenom, Telephone, Email, ID_POSTE, Type_Contrat, Salaire_Base, Date_Embauche, Statut, Notes, Id_Agence)
            VALUES (@Nom, @Prenom, @Telephone, @Email, @ID_POSTE, @Type_Contrat, @Salaire_Base, @Date_Embauche, @Statut, @Notes, @Id_Agence);
            SELECT LAST_INSERT_ID();",
            new { personnel.Nom, personnel.Prenom, personnel.Telephone, personnel.Email,
                  personnel.ID_POSTE, personnel.Type_Contrat, personnel.Salaire_Base,
                  personnel.Date_Embauche, personnel.Statut, personnel.Notes, personnel.Id_Agence });
    }

    public async Task<bool> UpdateAsync(PersonnelModel personnel)
    {
        using var connection = new MySqlConnection(_connectionString);
        var rows = await connection.ExecuteAsync(@"
            UPDATE PERSONNEL SET Nom=@Nom, Prenom=@Prenom, Telephone=@Telephone, Email=@Email,
            ID_POSTE=@ID_POSTE, Type_Contrat=@Type_Contrat, Salaire_Base=@Salaire_Base,
            Date_Embauche=@Date_Embauche, Statut=@Statut, Notes=@Notes, Id_Agence=@Id_Agence
            WHERE ID_PERSONNEL=@ID_PERSONNEL",
            new { personnel.Nom, personnel.Prenom, personnel.Telephone, personnel.Email,
                  personnel.ID_POSTE, personnel.Type_Contrat, personnel.Salaire_Base,
                  personnel.Date_Embauche, personnel.Statut, personnel.Notes, personnel.Id_Agence, personnel.ID_PERSONNEL });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        var rows = await connection.ExecuteAsync("DELETE FROM PERSONNEL WHERE ID_PERSONNEL=@Id", new { Id = id });
        return rows > 0;
    }
}
