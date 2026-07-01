using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public class FichePayeRepository : IFichePayeRepository
{
    private readonly string _connectionString;

    public FichePayeRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string manquante.");
    }

    private const string SelectJoin = @"
        SELECT f.*, p.Nom AS NomPersonnel, p.Prenom AS PrenomPersonnel, po.Libelle AS LibellePoste
        FROM FICHE_PAIE f
        JOIN PERSONNEL p ON f.ID_PERSONNEL = p.ID_PERSONNEL
        LEFT JOIN POSTE po ON p.ID_POSTE = po.ID_POSTE";

    public async Task<IEnumerable<FichePayeModel>> GetAllAsync(int? annee = null, int? mois = null)
    {
        using var connection = new MySqlConnection(_connectionString);
        var where = new List<string>();
        if (annee.HasValue) where.Add("f.Annee = @Annee");
        if (mois.HasValue) where.Add("f.Mois = @Mois");
        var sql = SelectJoin + (where.Count > 0 ? " WHERE " + string.Join(" AND ", where) : "")
                + " ORDER BY f.Annee DESC, f.Mois DESC, p.Nom";
        return await connection.QueryAsync<FichePayeModel>(sql, new { Annee = annee, Mois = mois });
    }

    public async Task<IEnumerable<FichePayeModel>> GetByPersonnelAsync(int idPersonnel)
    {
        using var connection = new MySqlConnection(_connectionString);
        return await connection.QueryAsync<FichePayeModel>(
            SelectJoin + " WHERE f.ID_PERSONNEL = @Id ORDER BY f.Annee DESC, f.Mois DESC",
            new { Id = idPersonnel });
    }

    public async Task<FichePayeModel?> GetByIdAsync(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<FichePayeModel>(
            SelectJoin + " WHERE f.ID_FICHE = @Id", new { Id = id });
    }

    public async Task<int> AddAsync(FichePayeModel fiche)
    {
        fiche.Net_A_Payer = fiche.Salaire_Base + fiche.Primes - fiche.Deductions;
        using var connection = new MySqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(@"
            INSERT INTO FICHE_PAIE (ID_PERSONNEL, Mois, Annee, Salaire_Base, Primes, Deductions, Net_A_Payer, Statut, Note)
            VALUES (@ID_PERSONNEL, @Mois, @Annee, @Salaire_Base, @Primes, @Deductions, @Net_A_Payer, @Statut, @Note);
            SELECT LAST_INSERT_ID();",
            new { fiche.ID_PERSONNEL, fiche.Mois, fiche.Annee, fiche.Salaire_Base,
                  fiche.Primes, fiche.Deductions, fiche.Net_A_Payer, fiche.Statut, fiche.Note });
    }

    public async Task<bool> UpdateAsync(FichePayeModel fiche)
    {
        fiche.Net_A_Payer = fiche.Salaire_Base + fiche.Primes - fiche.Deductions;
        using var connection = new MySqlConnection(_connectionString);
        var rows = await connection.ExecuteAsync(@"
            UPDATE FICHE_PAIE SET Salaire_Base=@Salaire_Base, Primes=@Primes, Deductions=@Deductions,
            Net_A_Payer=@Net_A_Payer, Statut=@Statut, Note=@Note
            WHERE ID_FICHE=@ID_FICHE",
            new { fiche.Salaire_Base, fiche.Primes, fiche.Deductions, fiche.Net_A_Payer,
                  fiche.Statut, fiche.Note, fiche.ID_FICHE });
        return rows > 0;
    }

    public async Task<bool> MarquerPayeAsync(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        var rows = await connection.ExecuteAsync(@"
            UPDATE FICHE_PAIE SET Statut='Payé', Date_Paiement=NOW() WHERE ID_FICHE=@Id",
            new { Id = id });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        var rows = await connection.ExecuteAsync("DELETE FROM FICHE_PAIE WHERE ID_FICHE=@Id", new { Id = id });
        return rows > 0;
    }

    // Génère automatiquement une fiche pour chaque personnel actif pour le mois/année donné
    public async Task GenererFichesAsync(int mois, int annee)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(@"
            INSERT IGNORE INTO FICHE_PAIE (ID_PERSONNEL, Mois, Annee, Salaire_Base, Primes, Deductions, Net_A_Payer, Statut)
            SELECT ID_PERSONNEL, @Mois, @Annee, Salaire_Base, 0, 0, Salaire_Base, 'En attente'
            FROM PERSONNEL WHERE Statut = 'Actif'",
            new { Mois = mois, Annee = annee });
    }
}
