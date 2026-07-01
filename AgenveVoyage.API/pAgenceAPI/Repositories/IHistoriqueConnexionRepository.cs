using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public interface IHistoriqueConnexionRepository
{
    Task<IEnumerable<HistoriqueConnexionModel>> GetAllAsync(int page, int pageSize);
    Task<IEnumerable<HistoriqueConnexionModel>> GetEchecsRecentsAsync(int minutes);
    Task EnregistrerAsync(HistoriqueConnexionModel entry);
}
