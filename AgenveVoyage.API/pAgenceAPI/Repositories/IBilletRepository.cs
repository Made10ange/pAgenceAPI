using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public interface IBilletRepository
    {
        Task<IEnumerable<BilletModel>> GetAllAsync();
        Task<BilletModel?>            GetByIdAsync(int id);
        Task<BilletModel?>            GetByNumeroAsync(string numero);
        Task<IEnumerable<BilletModel>> GetByPassagerAsync(int idPassager);
        Task<IEnumerable<BilletModel>> GetByStatutAsync(string statut);
        Task<IEnumerable<BilletModel>> SearchAsync(string? keyword, string? statut, DateTime? dateDebut, DateTime? dateFin);
        Task<IEnumerable<BilletModel>> GetByVoyageAsync(int idVoyage);
        Task<IEnumerable<BilletModel>> GetByVoyageEtenduAsync(int idVoyage);
        Task<IEnumerable<BilletModel>> GetPourEmbarquementAsync(int idVoyage);
        Task<List<int>>                GetSiegesOccupesAsync(int idVoyage);
        Task<int>                      AjouterAsync(BilletModel billet);
        Task<bool>                     UtiliserAsync(int id, int idVoyage);
        Task<bool>                     ReporterAsync(int id, int idNouveauVoyage);
        Task<bool>                     ChangerTypeVoyageAsync(int id, int idTypeVoyage, int? idVoyagePrevu, decimal montant);
        Task<bool>                     UtiliserParPassagerAsync(int idPassager, int idVoyage);
        Task<int>                      ExpirerAsync();   // marque "Expiré" tous les billets dépassés
        Task<string>                   GenererNumeroAsync();
    }
}
