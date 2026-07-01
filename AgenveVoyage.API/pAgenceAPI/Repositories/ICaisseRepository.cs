using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public interface ICaisseRepository
{
    // Caisse
    Task<List<CaisseModel>> GetAllAsync(int? idAgence = null);
    Task<CaisseModel?> GetByIdAsync(int id);
    Task<int> AjouterAsync(CaisseModel model);
    Task<bool> ModifierAsync(CaisseModel model);
    Task<bool> SupprimerAsync(int id);

    // Affectations caissier ↔ caisse
    Task<List<AffectationCaissierModel>> GetAffectationsAsync(int? idCaisse = null);
    Task<bool> AffecterAsync(AffectationCaissierModel model);
    Task<bool> DesaffecterAsync(int idAffectation);
}
