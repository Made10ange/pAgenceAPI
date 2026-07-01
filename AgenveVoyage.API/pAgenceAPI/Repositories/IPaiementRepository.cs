using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public interface IPaiementRepository
    {
        Task<List<PaiementModel>> GetAllAsync(int? idAgence = null);
        Task<PaiementModel?> GetByIdAsync(int id);
        Task<List<PaiementModel>> GetByPassagerAsync(int idPassager);
        Task<List<PaiementModel>> GetByColisAsync(int idColis);
        Task<List<PaiementModel>> GetByVoyageAsync(int idVoyage);
        Task<List<PaiementModel>> GetByPeriodeAsync(DateTime dateDebut, DateTime dateFin);
        Task<List<PaiementModel>> SearchAsync(string motCle);
        Task<string> AddAsync(PaiementModel paiement);
        Task<string> UpdateAsync(PaiementModel paiement);
        Task<string> DeleteAsync(int id);
        Task<decimal> GetTotalByPeriodeAsync(DateTime dateDebut, DateTime dateFin);
        Task<List<(int Mois, decimal Total)>> GetChiffreAffaireMensuelAsync(int annee, int? idAgence = null);
    }
}
