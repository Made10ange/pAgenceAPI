using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public interface IVoyageRepository
    {
        Task<List<VoyageModel>> GetAllAsync();
        Task<VoyageModel> GetByIdAsync(int id);
        Task<string> AddAsync(VoyageModel voyage);
        Task<string> UpdateAsync(VoyageModel voyage);
        Task<string> DeleteAsync(int id);
    }
}