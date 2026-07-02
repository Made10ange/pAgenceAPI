using Dapper;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class HistoriqueEtatVehiculeRepository : IHistoriqueEtatVehiculeRepository
    {
        private readonly string _connectionString;

        public HistoriqueEtatVehiculeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<List<HistoriqueEtatVehiculeModel>> GetByVehiculeAsync(int idVehicule)
        {
            using var connection = new MySqlConnection(_connectionString);
            return (await connection.QueryAsync<HistoriqueEtatVehiculeModel>(
                @"SELECT h.*,
                         v.IMMATRICULATION,
                         ta.LIBELLE_TYPE as Libelle_Ancien_Type,
                         tn.LIBELLE_TYPE as Libelle_Nouveau_Type
                  FROM historique_etat_vehicule h
                  LEFT JOIN vehicule v       ON h.ID_vehicule  = v.ID_vehicule
                  LEFT JOIN type_vehicule ta ON h.ANCIEN_TYPE  = ta.ID_TYPE
                  LEFT JOIN type_vehicule tn ON h.NOUVEAU_TYPE = tn.ID_TYPE
                  WHERE h.ID_vehicule = @IdVehicule
                  ORDER BY h.DATE_CHANGEMENT DESC",
                new { IdVehicule = idVehicule }
            )).ToList();
        }

        public async Task AddAsync(HistoriqueEtatVehiculeModel historique)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.ExecuteAsync(
                @"INSERT INTO historique_etat_vehicule
                    (ID_vehicule, ANCIEN_ETAT, NOUVEL_ETAT, ANCIEN_TYPE, NOUVEAU_TYPE, MOTIF, DATE_CHANGEMENT, MODIFIE_PAR)
                  VALUES
                    (@Id_Vehicule, @Ancien_Etat, @Nouvel_Etat, @Ancien_Type, @Nouveau_Type, @Motif, @Date_Changement, @Modifie_Par)",
                historique
            );
        }
    }
}
