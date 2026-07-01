using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public interface IGroupeRepository
{
    Task<IEnumerable<GroupeModel>> GetAllAsync();
    Task<GroupeModel?> GetByIdAsync(int id);
    Task<int> AddAsync(GroupeModel groupe);
    Task<bool> UpdateAsync(int id, GroupeModel groupe);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<AgentGroupeModel>> GetAgentsAsync(int idGroupe);
    Task<bool> AffecterAgentAsync(int idGroupe, int idAgent);
    Task<bool> RetirerAgentAsync(int idGroupe, int idAgent);
}
