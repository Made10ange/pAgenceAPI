using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public class PosteRepository : IPosteRepository
{
    private readonly string _connectionString;

    public PosteRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string manquante.");
    }

    public async Task<IEnumerable<PosteModel>> GetAllAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        return await connection.QueryAsync<PosteModel>("SELECT * FROM poste ORDER BY Libelle");
    }

    public async Task<PosteModel?> GetByIdAsync(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<PosteModel>(
            "SELECT * FROM poste WHERE ID_poste = @Id", new { Id = id });
    }

    public async Task<int> AddAsync(PosteModel poste)
    {
        using var connection = new MySqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(
            "INSERT INTO poste (Libelle, Description) VALUES (@Libelle, @Description); SELECT LAST_INSERT_ID();",
            new { poste.Libelle, poste.Description });
    }

    public async Task<bool> UpdateAsync(PosteModel poste)
    {
        using var connection = new MySqlConnection(_connectionString);
        var rows = await connection.ExecuteAsync(
            "UPDATE poste SET Libelle=@Libelle, Description=@Description WHERE ID_poste=@ID_poste",
            new { poste.Libelle, poste.Description, poste.ID_poste });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        var rows = await connection.ExecuteAsync("DELETE FROM poste WHERE ID_poste=@Id", new { Id = id });
        return rows > 0;
    }
}
