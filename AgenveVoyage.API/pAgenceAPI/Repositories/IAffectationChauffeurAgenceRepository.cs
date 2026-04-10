using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public interface IAffectationChauffeurAgenceRepository
    {
        Task<List<AffectationChauffeurAgenceModel>> GetAllAsync();
        Task<AffectationChauffeurAgenceModel> GetByIdAsync(int id);
        Task<string> AddAsync(AffectationChauffeurAgenceModel affectation);
        Task<string> UpdateAsync(AffectationChauffeurAgenceModel affectation);
        Task<string> DeleteAsync(int id);
    }
}