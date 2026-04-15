#nullable disable
using pAgenceAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pAgenceAPI.Repositories
{
    public interface IEmbarquementRepository
    {
        Task<List<EmbarquementVoyagePassagerModel>> GetAllAsync();
        Task<EmbarquementVoyagePassagerModel> GetByIdAsync(int id);
        Task<string> AddAsync(EmbarquementVoyagePassagerModel embarquement);
        Task<string> UpdateAsync(EmbarquementVoyagePassagerModel embarquement);
        Task<string> DeleteAsync(int id);
    }
}