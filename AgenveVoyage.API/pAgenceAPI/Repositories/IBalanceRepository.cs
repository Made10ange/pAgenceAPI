using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public interface IBalanceRepository
{
    Task<List<BalanceGeneraleModel>> GetBalanceGeneraleAsync(
        DateTime dateDebut, DateTime dateFin, int? idAgence);
}
