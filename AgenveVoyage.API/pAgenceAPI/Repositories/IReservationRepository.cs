using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public interface IReservationRepository
    {
        Task<List<ReservationModel>> GetAllAsync();
        Task<List<ReservationModel>> SearchAsync(string motCle);
        Task<ReservationModel?> GetByIdAsync(int id);
        Task<ReservationModel?> GetByReferenceAsync(string reference);
        Task<List<ReservationModel>> GetByVoyageAsync(int idVoyage);
        Task<List<ReservationModel>> GetPourEmbarquementAsync(int idVoyage);
        Task<int> AddAsync(ReservationModel reservation);
        Task<bool> UpdateStatutPaiementAsync(int id, string statutPaiement, string? referencePaiement, string? provider);
        Task<bool> SetPassagerAsync(int id, int idPassager);
        Task<bool> UpdateStatutReservationAsync(int id, string statut);
        Task<bool> ValiderAsync(int id, string valideepar);
        Task<bool> DeleteAsync(int id);
        Task<bool> SiegeDisponibleAsync(int idVoyage, int numeroSiege, int? excludeId = null);
        Task AjouterLogAsync(PaiementLogModel log);
    }
}
