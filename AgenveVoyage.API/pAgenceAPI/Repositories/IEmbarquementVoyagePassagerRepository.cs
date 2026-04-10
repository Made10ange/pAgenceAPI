using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public interface IEmbarquementVoyagePassagerRepository
    {
        Task<List<EmbarquementVoyagePassagerModel>> GetAllAsync();
        Task<EmbarquementVoyagePassagerModel> GetByIdAsync(int id);
        Task<string> AddAsync(EmbarquementVoyagePassagerModel embarquement);
        Task<string> UpdateAsync(EmbarquementVoyagePassagerModel embarquement);
        Task<string> DeleteAsync(int id);
    }
}