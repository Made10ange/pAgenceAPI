using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public interface IAssignationChauffeurVoyageRepository
    {
        Task<List<AssignationChauffeurVoyageModel>> GetAllAsync();
        Task<AssignationChauffeurVoyageModel?> GetByIdAsync(int id);
        Task<string> AddAsync(AssignationChauffeurVoyageModel assignation);
        Task<string> UpdateAsync(AssignationChauffeurVoyageModel assignation);
        Task<string> DeleteAsync(int id);
    }
}