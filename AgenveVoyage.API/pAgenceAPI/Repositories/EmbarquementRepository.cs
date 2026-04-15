#nullable disable
using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using pAgenceAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pAgenceAPI.Repositories
{
    public class EmbarquementRepository : IEmbarquementRepository
    {
        private readonly string _connectionString;

        public EmbarquementRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration), "Connection string is missing");
        }

        public async Task<List<EmbarquementVoyagePassagerModel>> GetAllAsync()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = @"
            SELECT 
                evp.ID_EMBARQUEMENT AS Id_Embarquement,
                evp.ID_VOYAGE AS Id_Voyage,
                evp.ID_PASSAGER AS Id_Passager,
                evp.STATUT_EMBARQUEMENT AS Statut_Embarquement,
                evp.NUMERO_SIEGE AS Numero_Siege,
                evp.DATE_ENREGISTREMENT AS Date_Enregistrement,
                CONCAT(p.nom, ' ', p.prenom) AS Nom_Passager,
                p.prenom AS Prenom_Passager,
                CONCAT(v.point_depart, ' → ', v.point_arrivee) AS Trajet
            FROM embarquement_voyage_passager evp
            LEFT JOIN passager p ON evp.ID_PASSAGER = p.ID_PASSAGER
            LEFT JOIN voyage v ON evp.ID_VOYAGE = v.ID_VOYAGE
            ORDER BY evp.ID_EMBARQUEMENT DESC";

                var result = await connection.QueryAsync<EmbarquementVoyagePassagerModel>(sql);
                return result.ToList();
            }
        }

        public async Task<EmbarquementVoyagePassagerModel> GetByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = @"
            SELECT 
                evp.ID_EMBARQUEMENT AS Id_Embarquement,
                evp.ID_VOYAGE AS Id_Voyage,
                evp.ID_PASSAGER AS Id_Passager,
                evp.STATUT_EMBARQUEMENT AS Statut_Embarquement,
                evp.NUMERO_SIEGE AS Numero_Siege,
                evp.DATE_ENREGISTREMENT AS Date_Enregistrement,
                CONCAT(p.nom, ' ', p.prenom) AS Nom_Passager,
                p.prenom AS Prenom_Passager,
                CONCAT(v.point_depart, ' → ', v.point_arrivee) AS Trajet
            FROM embarquement_voyage_passager evp
            LEFT JOIN passager p ON evp.ID_PASSAGER = p.ID_PASSAGER
            LEFT JOIN voyage v ON evp.ID_VOYAGE = v.ID_VOYAGE
            WHERE evp.ID_EMBARQUEMENT = @Id";

                return await connection.QueryFirstOrDefaultAsync<EmbarquementVoyagePassagerModel>(sql, new { Id = id });
            }
        }

        public async Task<string> AddAsync(EmbarquementVoyagePassagerModel embarquement)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = @"
                    INSERT INTO EMBARQUEMENT_VOYAGE_PASSAGER 
                    (ID_VOYAGE, ID_PASSAGER, STATUT_EMBARQUEMENT, NUMERO_SIEGE, DATE_ENREGISTREMENT) 
                    VALUES (@Id_Voyage, @Id_Passager, @Statut_Embarquement, @Numero_Siege, @Date_Enregistrement)";

                await connection.ExecuteAsync(sql, new
                {
                    Id_Voyage = embarquement.Id_Voyage,
                    Id_Passager = embarquement.Id_Passager,
                    Statut_Embarquement = embarquement.Statut_Embarquement ?? "Confirmé",
                    Numero_Siege = embarquement.Numero_Siege,
                    Date_Enregistrement = embarquement.Date_Enregistrement ?? DateTime.Now
                });

                return "Embarquement enregistré avec succès !";
            }
        }

        public async Task<string> UpdateAsync(EmbarquementVoyagePassagerModel embarquement)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = @"
                    UPDATE EMBARQUEMENT_VOYAGE_PASSAGER 
                    SET ID_VOYAGE = @Id_Voyage, 
                        ID_PASSAGER = @Id_Passager, 
                        STATUT_EMBARQUEMENT = @Statut_Embarquement, 
                        NUMERO_SIEGE = @Numero_Siege, 
                        DATE_ENREGISTREMENT = @Date_Enregistrement 
                    WHERE ID_EMBARQUEMENT = @Id_Embarquement";

                await connection.ExecuteAsync(sql, new
                {
                    Id_Embarquement = embarquement.Id_Embarquement,
                    Id_Voyage = embarquement.Id_Voyage,
                    Id_Passager = embarquement.Id_Passager,
                    Statut_Embarquement = embarquement.Statut_Embarquement ?? "Confirmé",
                    Numero_Siege = embarquement.Numero_Siege,
                    Date_Enregistrement = embarquement.Date_Enregistrement
                });

                return "Embarquement modifié avec succès !";
            }
        }

        public async Task<string> DeleteAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "DELETE FROM EMBARQUEMENT_VOYAGE_PASSAGER WHERE ID_EMBARQUEMENT = @Id";
                await connection.ExecuteAsync(sql, new { Id = id });
                return "Embarquement supprimé avec succès !";
            }
        }
    }
}