namespace pAgenceAPI.Models
{
    public class AgenceModel
    {
        public int Id_Agence { get; set; }

        // ✅ Strings OBLIGATOIRES (avec valeur par défaut)
        public string Nom_Agence { get; set; } = string.Empty;
        public string Ville { get; set; } = string.Empty;
        public string Adresse { get; set; } = string.Empty;

        // ✅ Strings OPTIONNELS (nullable avec ?)
        public string? Telephone { get; set; }
        public string? Email { get; set; }

        // ✅ DateTime optionnel
        public DateTime? Date_Creation { get; set; }
    }
}