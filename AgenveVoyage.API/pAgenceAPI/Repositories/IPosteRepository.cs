using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public interface IPosteRepository
{
    Task<IEnumerable<PosteModel>> GetAllAsync();
    Task<PosteModel?> GetByIdAsync(int id);
    Task<int> AddAsync(PosteModel poste);
    Task<bool> UpdateAsync(PosteModel poste);
    Task<bool> DeleteAsync(int id);
}
