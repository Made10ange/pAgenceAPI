#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pAgenceAPI.Models
{
    [Table("EMBARQUEMENT_VOYAGE_PASSAGER")]
    public class EmbarquementVoyagePassagerModel
    {
        // ✅ CLÉ PRIMAIRE
        [Key]
        [Column("ID_EMBARQUEMENT")]
        public int Id_Embarquement { get; set; }

        // ✅ CLÉS ÉTRANGÈRES
        [Column("ID_VOYAGE")]
        public int Id_Voyage { get; set; }

        [Column("ID_PASSAGER")]
        public int Id_Passager { get; set; }

        // ✅ CHAMPS DE LA TABLE
        [Column("STATUT_EMBARQUEMENT")]
        [StringLength(50)]
        public string Statut_Embarquement { get; set; }

        [Column("NUMERO_SIEGE")]
        public int? Numero_Siege { get; set; }  // ✅ CORRIGÉ : int? au lieu de int

        [Column("DATE_ENREGISTREMENT")]
        public DateTime? Date_Enregistrement { get; set; }  // ✅ CORRIGÉ : DateTime? au lieu de DateTime

        // ✅ PROPRIÉTÉS POUR L'AFFICHAGE (JOINTURES) - [NotMapped]
        [NotMapped] public string? Nom_Passager { get; set; }
        [NotMapped] public string? Prenom_Passager { get; set; }
        [NotMapped] public string? Trajet { get; set; }
        [NotMapped] public string? Telephone { get; set; }
        [NotMapped] public string? Sexe { get; set; }
        [NotMapped] public int Nb_Bagages { get; set; }
        [NotMapped] public decimal? Poids_Total_Bagages { get; set; }
        [NotMapped] public decimal? Montant_Bagages { get; set; }
    }
}