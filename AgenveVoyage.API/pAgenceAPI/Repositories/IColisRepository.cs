using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public interface IColisRepository
    {
        Task<List<ColisModel>> GetAllAsync(int? idAgence = null);
        Task<ColisModel?> GetByIdAsync(int id);
        Task<ColisModel?> GetByReferenceAsync(string reference);
        Task<List<ColisModel>> GetByVoyageAsync(int idVoyage);
        Task<List<ColisModel>> GetByTrajetVoyageAsync(int idVoyage);
        Task<List<ColisModel>> GetByStatutAsync(string statut);
        Task<List<ColisModel>> SearchAsync(string motCle);
        Task<int> AddAsync(ColisModel colis);
        Task<string> UpdateAsync(ColisModel colis);
        Task<string> UpdateStatutAsync(int id, string statut);
        Task<int> UpdateStatutByVoyageAsync(int idVoyage, string statutActuel, string nouveauStatut);
        Task LivrerParVoyageAsync(int idVoyage);
        Task<string> DeleteAsync(int id);
        Task<string> GenererReferenceAsync();
    }
}
