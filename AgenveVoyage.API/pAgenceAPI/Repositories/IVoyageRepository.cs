#nullable enable
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public interface IVoyageRepository
    {
        Task<List<VoyageModel>> GetAllAsync(int? idAgence = null);
        Task<List<VoyageModel>> GetByStatutAsync(string statut, int? idAgence = null);
        Task<List<VoyageModel>> GetByVehiculeAsync(int idVehicule);
        Task<List<VoyageModel>> SearchAsync(string motCle, int? idAgence = null);
        Task<VoyageModel?> GetByIdAsync(int id);
        Task<bool> AddAsync(VoyageModel voyage);
        Task<bool> UpdateAsync(VoyageModel voyage);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateStatutAsync(int id, string statut);
        Task<bool> HasScheduleConflictAsync(
            int idVehicule,
            DateTime dateDepart,
            DateTime dateArrivee,
            TimeSpan heureDepart,
            TimeSpan heureArrivee,
            int? excludeVoyageId = null);
    }
}
