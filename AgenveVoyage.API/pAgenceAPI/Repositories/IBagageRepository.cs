using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public interface IBagageRepository
    {
        Task<List<BagageModel>> GetAllAsync(int? idAgence = null);
        Task<BagageModel?> GetByIdAsync(int id);
        Task<List<BagageModel>> GetByPassagerAsync(int idPassager);
        Task<List<BagageModel>> GetByVoyageAsync(int idVoyage);
        Task<List<BagageModel>> SearchAsync(string motCle);
        Task<int> AddAsync(BagageModel bagage);
        Task<string> UpdateAsync(BagageModel bagage);
        Task<string> UpdateStatutAsync(int id, string statut);
        Task<string> DeleteAsync(int id);

        // Enregistre tous les bagages d'un passager pour un voyage en une seule transaction
        Task EnregistrerParPassagerAsync(int idPassager, int idVoyage, decimal? montantTotal, List<BagageLigneRequest> bagages);

        // Récupère les passagers d'un voyage avec leurs bagages
        Task<List<PassagerAvecBagagesDto>> GetPassagersAvecBagagesAsync(int idVoyage);

        // Récupère les bagages en attente de chargement (pas encore assignés à un voyage)
        Task<List<BagageModel>> GetEnAttenteAsync();

        // Assigne un bagage à un voyage (chargement sur le bus)
        Task<string> AssignerVoyageAsync(int idBagage, int idVoyage);

        // Passe tous les bagages d'un voyage de l'ancien statut au nouveau
        Task LivrerParVoyageAsync(int idVoyage);

        // Retourne tous les bagages livrés (archives)
        Task<List<BagageModel>> GetArchivesAsync(int? idAgence = null);

        // Retourne les bagages des passagers embarqués sur un voyage
        Task<List<BagageModel>> GetByPassagersEmbarquesAsync(int idVoyage);

        // Retourne les bagages En attente pour une liste de passager IDs
        Task<List<BagageModel>> GetByPassagerIdsAsync(List<int> passagerIds);
    }
}
