using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public interface IVehiculeRepository
    {
        Task<List<VehiculeModel>> GetAllAsync();
        Task<VehiculeModel?> GetByIdAsync(int id);
        Task<string> AddAsync(VehiculeModel vehicule);
        Task<string> UpdateAsync(VehiculeModel vehicule);
        Task<string> DeleteAsync(int id);

        // ✅ AJOUTE CETTE MÉTHODE
        Task<List<VehiculeModel>> GetByStatutAsync(string statut);
    }
}