using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public interface IPrivilegeRepository
{
    Task<IEnumerable<PrivilegeModel>> GetByGroupeAsync(int idGroupe);
    Task SauvegarderAsync(int idGroupe, IEnumerable<PrivilegeModel> privileges);
}
