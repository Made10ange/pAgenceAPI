using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public interface IAffectationVehiculeAgenceRepository
    {
        Task<List<AffectationVehiculeAgenceModel>> GetAllAsync();
        Task<List<AffectationVehiculeAgenceModel>> SearchAsync(string motCle);
        Task<AffectationVehiculeAgenceModel?> GetByIdAsync(int id);
        Task<string> AddAsync(AffectationVehiculeAgenceModel affectation);
        Task<string> UpdateAsync(AffectationVehiculeAgenceModel affectation);
        Task<string> DeleteAsync(int id);
    }
}