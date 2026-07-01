#nullable enable
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public interface IUtilisateurRepository
{
    Task<UtilisateurModel?> GetByLoginAsync(string login, int? idAgence = null);
    Task<bool> ExisteAsync();
    Task CreerAdminParDefautAsync();
    Task<IEnumerable<UtilisateurModel>> GetAllAsync();
    Task<UtilisateurModel?> GetByIdAsync(int id);
    Task<int> AddAsync(UtilisateurModel agent);
    Task<bool> UpdateAsync(UtilisateurModel agent);
    Task<bool> UpdatePasswordAsync(int id, string hashedPassword);
    Task<bool> DeleteAsync(int id);
}

