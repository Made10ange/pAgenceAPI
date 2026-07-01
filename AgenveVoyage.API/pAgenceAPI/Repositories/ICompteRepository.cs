using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public interface ICompteRepository
{
    Task<List<CompteModel>> GetAllAsync();
    Task<CompteModel?> GetByNumAsync(string numcompte);
    Task<bool> AddAsync(CompteModel compte);
    Task<bool> UpdateAsync(CompteModel compte);
    Task<bool> DeleteAsync(string numcompte);
}
