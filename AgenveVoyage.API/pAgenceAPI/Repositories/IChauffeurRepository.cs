#nullable enable
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public interface IChauffeurRepository
    {
        Task<List<ChauffeurModel>> GetAllAsync(int? idAgence = null);
        Task<List<ChauffeurModel>> GetDisponiblesAsync(int? idAgence = null);
        Task<List<ChauffeurModel>> SearchAsync(string motCle);
        Task<ChauffeurModel?> GetByIdAsync(int id);
        Task<string> AddAsync(ChauffeurModel chauffeur);
        Task<string> UpdateAsync(ChauffeurModel chauffeur);
        Task<string> DeleteAsync(int id);
    }
}
