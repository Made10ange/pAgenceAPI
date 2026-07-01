#nullable enable
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public interface IPassagerRepository
    {
        Task<List<PassagerModel>> GetAllAsync(int? idAgence = null);
        Task<List<PassagerModel>> SearchAsync(string motCle, int? idAgence = null);
        Task<PassagerModel?> GetByIdAsync(int id);
        Task<int> AddAsync(PassagerModel passager);
        Task<string> UpdateAsync(PassagerModel passager);
        Task<string> DeleteAsync(int id);
    }
}
