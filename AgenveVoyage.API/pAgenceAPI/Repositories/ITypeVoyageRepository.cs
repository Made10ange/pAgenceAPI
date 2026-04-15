using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public interface ITypeVoyageRepository
    {
        Task<List<TypeVoyageModel>> GetAllAsync();
        Task<TypeVoyageModel?> GetByIdAsync(int id);
        Task<string> AddAsync(TypeVoyageModel type);
        Task<string> UpdateAsync(TypeVoyageModel type);
        Task<string> DeleteAsync(int id);
    }
}