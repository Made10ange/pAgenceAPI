using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public interface IPassagerRepository
    {
        Task<List<PassagerModel>> GetAllAsync();
        Task<PassagerModel> GetByIdAsync(int id);
        Task<string> AddAsync(PassagerModel passager);
        Task<string> UpdateAsync(PassagerModel passager);
        Task<string> DeleteAsync(int id);
    }
}