using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public interface ITransfertCaisseRepository
{
    Task<List<TransfertCaisseModel>> GetEnAttenteAsync(int? idAgence = null);
    Task<List<TransfertCaisseModel>> GetHistoriqueAsync(DateTime? dateDebut = null, DateTime? dateFin = null, int? idAgence = null);
    Task<TransfertCaisseModel?> GetByIdAsync(int id);
    Task<int> InitierAsync(TransfertCaisseModel model, int? codeUser);
    Task ValiderAsync(int idTransfert, int? codeUser);
    Task AnnulerAsync(int idTransfert, int? codeUser);
}
