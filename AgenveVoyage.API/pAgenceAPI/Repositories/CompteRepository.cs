using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

// Accès à la table `compte` du plan comptable OHADA/COBAC
public class CompteRepository : ICompteRepository
{
    private readonly string _cs;

    public CompteRepository(IConfiguration config)
    {
        _cs = config.GetConnectionString("DefaultConnection") ?? "";
    }

    public async Task<List<CompteModel>> GetAllAsync()
    {
        const string sql = @"
            SELECT c.*,
                   p.libelle_compte AS libelle_parent
            FROM   compte c
            LEFT JOIN compte p ON p.numcompte = c.numcompte_pere
            ORDER  BY c.numcompte";

        using var db = new MySqlConnection(_cs);
        return (await db.QueryAsync<CompteModel>(sql)).ToList();
    }

    public async Task<CompteModel?> GetByNumAsync(string numcompte)
    {
        const string sql = @"
            SELECT c.*,
                   p.libelle_compte AS libelle_parent
            FROM   compte c
            LEFT JOIN compte p ON p.numcompte = c.numcompte_pere
            WHERE  c.numcompte = @numcompte";

        using var db = new MySqlConnection(_cs);
        return await db.QueryFirstOrDefaultAsync<CompteModel>(sql, new { numcompte });
    }

    public async Task<bool> AddAsync(CompteModel compte)
    {
        using var db = new MySqlConnection(_cs);

        // Vérifier que le numéro n'existe pas déjà
        var existe = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM compte WHERE numcompte = @numcompte",
            new { compte.numcompte });

        if (existe > 0) return false;

        var rows = await db.ExecuteAsync(@"
            INSERT INTO compte
                (numcompte, numcompte_pere, libelle_compte, type_compte, sens, statut, devise, ferme, date_creation)
            VALUES
                (@numcompte, @numcompte_pere, @libelle_compte, @type_compte, @sens, @statut, @devise, @ferme, NOW())",
            compte);

        return rows > 0;
    }

    public async Task<bool> UpdateAsync(CompteModel compte)
    {
        using var db = new MySqlConnection(_cs);

        // Empêcher un compte d'être son propre parent
        if (compte.numcompte_pere == compte.numcompte) return false;

        var rows = await db.ExecuteAsync(@"
            UPDATE compte
            SET    numcompte_pere  = @numcompte_pere,
                   libelle_compte  = @libelle_compte,
                   type_compte     = @type_compte,
                   sens            = @sens,
                   statut          = @statut,
                   devise          = @devise
            WHERE  numcompte = @numcompte",
            compte);

        return rows > 0;
    }

    public async Task<bool> DeleteAsync(string numcompte)
    {
        using var db = new MySqlConnection(_cs);

        // Refuser si des comptes enfants existent
        var enfants = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM compte WHERE numcompte_pere = @numcompte",
            new { numcompte });

        if (enfants > 0) return false;

        return await db.ExecuteAsync(
            "DELETE FROM compte WHERE numcompte = @numcompte",
            new { numcompte }) > 0;
    }
}
