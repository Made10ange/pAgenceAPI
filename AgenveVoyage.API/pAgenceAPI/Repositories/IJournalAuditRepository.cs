using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories;

public interface IJournalAuditRepository
{
    Task<IEnumerable<JournalAuditModel>> GetAllAsync(int page, int pageSize);
    Task<IEnumerable<JournalAuditModel>> RechercherAsync(string? module, string? login, DateTime? dateDebut, DateTime? dateFin);
    Task EnregistrerAsync(JournalAuditModel entry);
}
