using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public interface ITypeVehiculeRepository
    {
        Task<List<TypeVehiculeModel>> GetAllAsync();
        Task<TypeVehiculeModel> GetByIdAsync(int id);
        Task<string> AddAsync(TypeVehiculeModel type);
        Task<string> UpdateAsync(TypeVehiculeModel type);
        Task<string> DeleteAsync(int id);
    }
}