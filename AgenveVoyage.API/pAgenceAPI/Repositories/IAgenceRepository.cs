
using pAgenceAPI.Models;
public interface IAgenceRepository
{
    Task<List<AgenceModel>> GetAllAsync();
    Task<AgenceModel?> GetByIdAsync(int id);  // ← Ajoute ?
    Task<string> AddAsync(AgenceModel agence);
    Task<string> UpdateAsync(AgenceModel agence);
    Task<string> DeleteAsync(int id);
}