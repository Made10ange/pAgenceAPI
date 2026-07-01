using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public interface IFichePayeRepository
{
    Task<IEnumerable<FichePayeModel>> GetAllAsync(int? annee = null, int? mois = null);
    Task<IEnumerable<FichePayeModel>> GetByPersonnelAsync(int idPersonnel);
    Task<FichePayeModel?> GetByIdAsync(int id);
    Task<int> AddAsync(FichePayeModel fiche);
    Task<bool> UpdateAsync(FichePayeModel fiche);
    Task<bool> MarquerPayeAsync(int id);
    Task<bool> DeleteAsync(int id);
    Task GenererFichesAsync(int mois, int annee);
}
