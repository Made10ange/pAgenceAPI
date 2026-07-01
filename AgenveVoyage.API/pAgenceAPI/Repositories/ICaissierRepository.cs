using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public interface ICaissierRepository
{
    Task<List<CaissierModel>> GetAllAsync(int? idAgence = null);
    Task<CaissierModel?> GetByIdAsync(int id);
    Task<int> AjouterAsync(CaissierModel model);
    Task<bool> ModifierAsync(CaissierModel model);
    Task<bool> ToggleActifAsync(int id);
    Task<bool> SupprimerAsync(int id);
}
