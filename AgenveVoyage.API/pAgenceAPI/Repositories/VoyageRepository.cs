using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using pAgenceAPI.Models;

namespace pAgenceAPI.Repositories
{
    public class VoyageRepository : IVoyageRepository
    {
        private readonly string? _connectionString;
        private readonly ILogger<VoyageRepository> _logger;

        public VoyageRepository(IConfiguration configuration, ILogger<VoyageRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        // ✅ LISTE TOUS LES VOYAGES
        public async Task<List<VoyageModel>> GetAllAsync()
        {
            using var connection = new MySqlConnection(_connectionString);
            try
            {
                return (await connection.QueryAsync<VoyageModel>(
                    @"SELECT 
                        v.ID_VOYAGE, v.ID_VEHICULE, v.ID_TYPE_VOYAGE, v.POINT_DEPART, v.POINT_ARRIVEE,
                        v.DATE_DEPART, v.DATE_ARRIVEE, v.HEURE_DEPART, v.HEURE_ARRIVEE, v.DUREE, v.STATUT,
                        COALESCE(vh.IMMATRICULATION, '') as Immatriculation,
                        COALESCE(tv.LIBELLE_TYPE_VOYAGE, '') as Libelle_Type_Voyage,
                        COALESCE(tv.PRIX, 0) as Prix
                      FROM VOYAGE v
                      LEFT JOIN VEHICULE vh ON v.ID_VEHICULE = vh.ID_VEHICULE
                      LEFT JOIN TYPE_VOYAGE tv ON v.ID_TYPE_VOYAGE = tv.ID_TYPE_VOYAGE
                      ORDER BY v.DATE_DEPART DESC"
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetAllAsync voyages");
                throw;
            }
        }

        // ✅ RÉCUPÈRE UN VOYAGE PAR ID
        public async Task<VoyageModel?> GetByIdAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            try
            {
                return await connection.QueryFirstOrDefaultAsync<VoyageModel>(
                    @"SELECT 
                        v.ID_VOYAGE, v.ID_VEHICULE, v.ID_TYPE_VOYAGE, v.POINT_DEPART, v.POINT_ARRIVEE,
                        v.DATE_DEPART, v.DATE_ARRIVEE, v.HEURE_DEPART, v.HEURE_ARRIVEE, v.DUREE, v.STATUT,
                        COALESCE(vh.IMMATRICULATION, '') as Immatriculation,
                        COALESCE(tv.LIBELLE_TYPE_VOYAGE, '') as Libelle_Type_Voyage,
                        COALESCE(tv.PRIX, 0) as Prix
                      FROM VOYAGE v
                      LEFT JOIN VEHICULE vh ON v.ID_VEHICULE = vh.ID_VEHICULE
                      LEFT JOIN TYPE_VOYAGE tv ON v.ID_TYPE_VOYAGE = tv.ID_TYPE_VOYAGE
                      WHERE v.ID_VOYAGE = @Id",
                    new { Id = id }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetByIdAsync voyage id={Id}", id);
                throw;
            }
        }

        // Helper : vérifie que le type de voyage existe ; retourne true si existe
        private async Task<bool> TypeVoyageExistsAsync(MySqlConnection connection, int idType)
        {
            var count = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM type_voyage WHERE ID_TYPE_VOYAGE = @Id",
                new { Id = idType }
            );
            return count > 0;
        }

        // ✅ AJOUTE UN NOUVEAU VOYAGE
        public async Task<string> AddAsync(VoyageModel voyage)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                int? idTypeVoyage = voyage.Id_Type_Voyage;

                // Vérifier la contrainte FK avant insertion
                if (idTypeVoyage.HasValue)
                {
                    var exists = await TypeVoyageExistsAsync(connection, idTypeVoyage.Value);
                    if (!exists)
                    {
                        _logger.LogWarning("Type de voyage {IdType} n'existe pas, mise à NULL", idTypeVoyage.Value);
                        idTypeVoyage = null;
                    }
                }

                _logger.LogInformation("Insertion voyage: Véhicule={IdVehicule}, Type={IdType}, Départ={PointDepart}-{PointArrivee}, DateDepart={DateDepart}, Heure={HeureDepart}",
                    voyage.Id_Vehicule, idTypeVoyage, voyage.Point_Depart, voyage.Point_Arrivee, voyage.Date_Depart, voyage.Heure_Depart);

                var rowsAffected = await connection.ExecuteAsync(
                    @"INSERT INTO VOYAGE (ID_VEHICULE, ID_TYPE_VOYAGE, POINT_DEPART, POINT_ARRIVEE, 
                                          DATE_DEPART, DATE_ARRIVEE, HEURE_DEPART, HEURE_ARRIVEE, DUREE, STATUT)
                      VALUES (@Id_Vehicule, @Id_Type_Voyage, @Point_Depart, @Point_Arrivee, 
                              @Date_Depart, @Date_Arrivee, @Heure_Depart, @Heure_Arrivee, @Duree, @Statut)",
                    new
                    {
                        Id_Vehicule = voyage.Id_Vehicule,
                        Id_Type_Voyage = idTypeVoyage,
                        Point_Depart = voyage.Point_Depart,
                        Point_Arrivee = voyage.Point_Arrivee,
                        Date_Depart = voyage.Date_Depart,
                        Date_Arrivee = voyage.Date_Arrivee,
                        Heure_Depart = voyage.Heure_Depart,
                        Heure_Arrivee = voyage.Heure_Arrivee,
                        Duree = voyage.Duree,
                        Statut = voyage.Statut ?? "Programmé"
                    }
                );

                _logger.LogInformation("Rows affectées: {RowsAffected}", rowsAffected);

                return idTypeVoyage == null && voyage.Id_Type_Voyage.HasValue
                    ? "Voyage ajouté (type invalide fourni — valeur enregistrée à NULL)."
                    : "Voyage ajouté avec succès !";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'ajout du voyage");
                throw;
            }
        }
        

        // ✅ MODIFIE UN VOYAGE EXISTANT
        public async Task<string> UpdateAsync(VoyageModel voyage)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            int? idTypeVoyage = voyage.Id_Type_Voyage;

            // Vérifier la contrainte FK avant update
            if (idTypeVoyage.HasValue)
            {
                var exists = await TypeVoyageExistsAsync(connection, idTypeVoyage.Value);
                if (!exists)
                {
                    idTypeVoyage = null;
                }
            }

            await connection.ExecuteAsync(
                @"UPDATE VOYAGE 
                  SET ID_VEHICULE = @Id_Vehicule, ID_TYPE_VOYAGE = @Id_Type_Voyage, 
                      POINT_DEPART = @Point_Depart, POINT_ARRIVEE = @Point_Arrivee, 
                      DATE_DEPART = @Date_Depart, DATE_ARRIVEE = @Date_Arrivee, 
                      HEURE_DEPART = @Heure_Depart, HEURE_ARRIVEE = @Heure_Arrivee, 
                      DUREE = @Duree, STATUT = @Statut
                  WHERE ID_VOYAGE = @Id",
                new
                {
                    Id = voyage.Id_Voyage,
                    Id_Vehicule = voyage.Id_Vehicule,
                    Id_Type_Voyage = idTypeVoyage,
                    Point_Depart = voyage.Point_Depart,
                    Point_Arrivee = voyage.Point_Arrivee,
                    Date_Depart = voyage.Date_Depart,
                    Date_Arrivee = voyage.Date_Arrivee,
                    Heure_Depart = voyage.Heure_Depart,
                    Heure_Arrivee = voyage.Heure_Arrivee,
                    Duree = voyage.Duree,
                    Statut = voyage.Statut ?? "Programmé"
                }
            );

            return idTypeVoyage == null && voyage.Id_Type_Voyage.HasValue
                ? "Voyage modifié (type invalide fourni — valeur enregistrée à NULL)."
                : "Voyage modifié avec succès !";
        }

        // ✅ SUPPRIME UN VOYAGE
        public async Task<string> DeleteAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await connection.ExecuteAsync(
                "DELETE FROM VOYAGE WHERE ID_VOYAGE = @Id",
                new { Id = id }
            );
            return "Voyage supprimé avec succès !";
        }
    }
}