using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public interface IHistoriqueEtatVehiculeRepository
    {
        Task<List<HistoriqueEtatVehiculeModel>> GetByVehiculeAsync(int idVehicule);
        Task AddAsync(HistoriqueEtatVehiculeModel historique);
    }
}
