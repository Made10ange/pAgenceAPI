using pAgenceAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pAgenceAPI.Repositories
{
    public interface IEmbarquementRepository
    {
        Task<List<EmbarquementVoyagePassagerModel>> GetAllAsync();
        Task<List<EmbarquementVoyagePassagerModel>> SearchAsync(string motCle);
        Task<List<EmbarquementVoyagePassagerModel>> GetByVoyageAsync(int idVoyage);
        Task<List<EmbarquementVoyagePassagerModel>> GetByPassagerAsync(int idPassager);
        Task<List<EmbarquementVoyagePassagerModel>> GetByStatutAsync(string statut);
        Task<EmbarquementVoyagePassagerModel?> GetByIdAsync(int id);
        Task<string> AddAsync(EmbarquementVoyagePassagerModel embarquement);
        Task<string> UpdateAsync(EmbarquementVoyagePassagerModel embarquement);
        Task<string> DeleteAsync(int id);
    }
}
