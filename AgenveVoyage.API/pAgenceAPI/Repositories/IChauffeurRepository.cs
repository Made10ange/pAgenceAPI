using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public interface IChauffeurRepository
    {
        Task<List<ChauffeurModel>> GetAllAsync();
        Task<ChauffeurModel?> GetByIdAsync(int id);
        Task<string> AddAsync(ChauffeurModel chauffeur);
        Task<string> UpdateAsync(ChauffeurModel chauffeur);
        Task<string> DeleteAsync(int id);
    }
}
