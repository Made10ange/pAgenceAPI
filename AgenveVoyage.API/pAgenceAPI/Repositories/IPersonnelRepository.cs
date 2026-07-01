using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public interface IPersonnelRepository
{
    Task<IEnumerable<PersonnelModel>> GetAllAsync(int? idAgence = null);
    Task<PersonnelModel?> GetByIdAsync(int id);
    Task<int> AddAsync(PersonnelModel personnel);
    Task<bool> UpdateAsync(PersonnelModel personnel);
    Task<bool> DeleteAsync(int id);
}
