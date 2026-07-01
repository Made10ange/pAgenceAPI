using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public interface ITarifRepository
    {
        Task<List<TarifModel>> GetAllAsync();
        Task<TarifModel?> GetByIdAsync(int id);
        Task<List<TarifModel>> RechercherAsync(int? idTypeVoyage, string? depart, string? arrivee, string? typePassager);
        Task<bool> AddAsync(TarifModel tarif);
        Task<bool> UpdateAsync(TarifModel tarif);
        Task<bool> DeleteAsync(int id);
    }
}
