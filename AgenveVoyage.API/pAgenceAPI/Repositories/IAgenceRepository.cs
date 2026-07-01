#nullable enable
using pAgenceAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pAgenceAPI.Repositories
{
    public interface IAgenceRepository
    {
        Task<List<AgenceModel>> GetAllAsync();
        Task<List<AgenceModel>> SearchAsync(string motCle);
        Task<AgenceModel?> GetByIdAsync(int id);
        Task<string> AddAsync(AgenceModel agence);
        Task<string> UpdateAsync(AgenceModel agence);
        Task<string> DeleteAsync(int id);
    }
}
