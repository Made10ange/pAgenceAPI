using System.Text;
using System.Text.Json;

namespace pAgenceAPI.Services
{
    public class GeminiIntentResult
    {
        public string Action { get; set; } = "inconnu";
        public string? MotCle { get; set; }
        public bool ViaSecours { get; set; } = false;
    }

    public class RechercheVoyageResult
    {
        public string? Depart { get; set; }
        public string? Arrivee { get; set; }
        public DateTime? Date { get; set; }
    }

    public class GeminiService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _model;

        private const string ActionsDisponibles = @"
- liste_chauffeurs : lister tous les chauffeurs
- liste_voyages : lister tous les voyages, sans filtre de statut
- liste_voyages_en_cours : lister uniquement les voyages dont le statut est ""En cours""
- liste_voyages_programmes : lister uniquement les voyages dont le statut est ""Programmé""
- liste_voyages_termines : lister uniquement les voyages dont le statut est ""Terminé""
- liste_vehicules : lister tous les véhicules
- liste_agences : lister toutes les agences
- liste_billets : lister tous les billets
- voyages_aujourdhui : lister uniquement les voyages dont la date de départ est aujourd'hui
- chauffeur_plus_actif : trouver le chauffeur ayant le plus de voyages assignés
- chauffeurs_sans_voyage : lister les chauffeurs non affectés à un voyage en cours ou programmé (chauffeurs disponibles/libres/inactifs)
- chercher_chauffeur : rechercher un chauffeur précis par son nom (mets le nom dans ""mot_cle"")
- aide : l'utilisateur demande comment utiliser l'application (""comment faire X"", ""où se trouve Y"", tutoriel/aide) plutôt que des données réelles
- inconnu : si la question ne correspond à aucune action ci-dessus

Choisis liste_voyages_en_cours / liste_voyages_programmes / liste_voyages_termines seulement si l'utilisateur précise explicitement ce statut (""en cours"", ""programmés"", ""terminés"", ""à venir""...). Sinon utilise liste_voyages.
Choisis chercher_chauffeur seulement si l'utilisateur cite un nom de personne précis. Choisis liste_chauffeurs s'il veut la liste complète.";

        public GeminiService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _http = httpClientFactory.CreateClient();
            _http.Timeout = TimeSpan.FromSeconds(8);
            _apiKey = config["Gemini:ApiKey"] ?? throw new InvalidOperationException("Clé API Gemini manquante dans appsettings.Development.json");
            _model = config["Gemini:Model"] ?? "gemini-flash-latest";
        }

        public async Task<GeminiIntentResult> InterpreterAsync(string question)
        {
            // 1) On tente l'IA (avec une nouvelle tentative en cas de surcharge passagère du service)
            for (int tentative = 1; tentative <= 2; tentative++)
            {
                try
                {
                    var resultat = await AppellerGeminiAsync(question);
                    if (resultat != null)
                        return resultat;
                }
                catch
                {
                    // on retente une fois, puis on bascule sur le secours
                }

                if (tentative == 1)
                    await Task.Delay(600);
            }

            // 2) Filet de sécurité : reconnaissance par mots-clés, toujours disponible hors-ligne
            return ReconnaissanceParMotsCles(question);
        }

        private async Task<GeminiIntentResult?> AppellerGeminiAsync(string question)
        {
            var systemPrompt = $@"Tu es l'assistant vocal de l'application AgenceV, un logiciel de gestion d'agence de voyage.
Analyse la question de l'utilisateur et détermine quelle action exécuter parmi la liste suivante :
{ActionsDisponibles}

Si l'utilisateur cherche un élément précis (ex: un nom de chauffeur), extrait le mot-clé dans ""mot_cle"".
Réponds UNIQUEMENT avec un objet JSON strict de la forme : {{""action"": ""...""; ""mot_cle"": ""...ou null""}}
Ne mets aucun texte autour du JSON, pas de balises markdown.

Question de l'utilisateur : ""{question}""";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            var body = new
            {
                contents = new object[]
                {
                    new { role = "user", parts = new object[] { new { text = systemPrompt } } }
                },
                generationConfig = new { temperature = 0.1 }
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
                return null; // déclenche la nouvelle tentative / le secours

            var raw = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(raw);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "{}";

            text = text.Trim().Trim('`');
            if (text.StartsWith("json")) text = text[4..].Trim();

            using var parsed = JsonDocument.Parse(text);
            var action = parsed.RootElement.TryGetProperty("action", out var a) ? a.GetString() : "inconnu";
            string? motCle = parsed.RootElement.TryGetProperty("mot_cle", out var m) && m.ValueKind != JsonValueKind.Null
                ? m.GetString()
                : null;

            return new GeminiIntentResult { Action = action ?? "inconnu", MotCle = motCle };
        }

        /// <summary>
        /// Reconnaissance locale par mots-clés, utilisée si le service Gemini est indisponible
        /// (quota dépassé, surcharge, coupure réseau). Garantit que le chatbot répond toujours.
        /// </summary>
        private GeminiIntentResult ReconnaissanceParMotsCles(string question)
        {
            var q = question.ToLowerInvariant();

            if (q.Contains("chauffeur le plus actif") || q.Contains("chauffeur plus actif") || q.Contains("meilleur chauffeur"))
                return new GeminiIntentResult { Action = "chauffeur_plus_actif", ViaSecours = true };
            if (q.Contains("sans voyage") || q.Contains("disponible") || q.Contains("libre") || q.Contains("inactif"))
                return new GeminiIntentResult { Action = "chauffeurs_sans_voyage", ViaSecours = true };
            if (q.Contains("chauffeur"))
                return new GeminiIntentResult { Action = "liste_chauffeurs", ViaSecours = true };
            if (q.Contains("voyage") || q.Contains("trajet"))
            {
                if (q.Contains("aujourd'hui") || q.Contains("aujourdhui") || q.Contains("aujourd’hui"))
                    return new GeminiIntentResult { Action = "voyages_aujourdhui", ViaSecours = true };
                if (q.Contains("en cours"))
                    return new GeminiIntentResult { Action = "liste_voyages_en_cours", ViaSecours = true };
                if (q.Contains("programm"))
                    return new GeminiIntentResult { Action = "liste_voyages_programmes", ViaSecours = true };
                if (q.Contains("termin"))
                    return new GeminiIntentResult { Action = "liste_voyages_termines", ViaSecours = true };
                return new GeminiIntentResult { Action = "liste_voyages", ViaSecours = true };
            }
            if (q.Contains("véhicule") || q.Contains("vehicule") || q.Contains("bus") || q.Contains("car"))
                return new GeminiIntentResult { Action = "liste_vehicules", ViaSecours = true };
            if (q.Contains("agence"))
                return new GeminiIntentResult { Action = "liste_agences", ViaSecours = true };
            if (q.Contains("billet"))
                return new GeminiIntentResult { Action = "liste_billets", ViaSecours = true };
            if (q.Contains("comment") || q.Contains("où") || q.Contains("ou se trouve") || q.Contains("tutoriel"))
                return new GeminiIntentResult { Action = "aide", ViaSecours = true };

            return new GeminiIntentResult { Action = "inconnu", ViaSecours = true };
        }

        private const string BaseConnaissanceAide = @"
[Voyages] Ajouter un voyage : Opérations > Voyages > Nouveau voyage, choisir type de voyage, véhicule, chauffeur, date et heure, puis valider.
[Voyages] Modifier/supprimer un voyage : Opérations > Voyages, boutons crayon (modifier) ou corbeille (supprimer).
[Voyages] Voir les voyages archivés : onglet Archives de la page Voyages.
[Passagers] Vendre un billet : Passagers > Ajouter, choisir le type de voyage puis le trajet, remplir les infos du passager et le mode de paiement, cliquer sur VALIDER.
[Passagers] Retrouver un passager déjà enregistré : taper son nom dans le champ Nom du Guichet passagers, une liste de suggestions apparaît.
[Embarquements] Embarquer des passagers : Opérations > Embarquements, sélectionner le voyage, cocher les passagers présents, Valider l'embarquement.
[Embarquements] Bordereau de voyage : généré automatiquement après validation de l'embarquement, s'ouvre dans un nouvel onglet.
[Bagages/Colis] Enregistrer un bagage ou un colis : Opérations > Bagages ou Colis > Nouveau, renseigner passager/expéditeur, description et trajet.
[Paiements] Voir les paiements reçus : menu Paiements.
[Comptabilité] Rapport de caisse : Opérations > Rapport de caisse.
[Comptabilité] Affecter un caissier à une caisse : Comptabilité > Affectation caissiers.
[Paramètres] Ajouter un chauffeur ou véhicule : Paramètres > Chauffeurs ou Véhicules > Ajouter.
[Interface] Changer la langue : boutons FR/EN en haut de page.
[Interface] Mode sombre : icône lune/soleil en haut de page.
[Interface] Se déconnecter : bouton Déconnexion en haut à droite.";

        /// <summary>
        /// Répond aux questions "comment faire X" en s'appuyant sur la base de connaissance de l'application
        /// (fusion de la FAQ statique avec l'IA générative).
        /// </summary>
        public async Task<string> RepondreAideAsync(string question)
        {
            for (int tentative = 1; tentative <= 2; tentative++)
            {
                try
                {
                    var reponse = await AppellerAideAsync(question);
                    if (reponse != null) return reponse;
                }
                catch { }
                if (tentative == 1) await Task.Delay(600);
            }

            return RechercheAideSecours(question);
        }

        private async Task<string?> AppellerAideAsync(string question)
        {
            var prompt = $@"Tu es l'assistant d'aide de l'application AgenceV (logiciel de gestion d'agence de voyage).
Voici la documentation d'utilisation disponible :
{BaseConnaissanceAide}

Réponds à la question de l'utilisateur UNIQUEMENT à partir de cette documentation, en français, en 2-3 phrases claires et directes, sans formatage markdown.
Si aucune information pertinente n'existe dans la documentation, réponds exactement : ""Je n'ai pas d'information à ce sujet dans mon guide d'utilisation.""

Question : ""{question}""";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
            var body = new
            {
                contents = new object[] { new { role = "user", parts = new object[] { new { text = prompt } } } },
                generationConfig = new { temperature = 0.2 }
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(url, content);
            if (!response.IsSuccessStatusCode) return null;

            var raw = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(raw);
            return doc.RootElement.GetProperty("candidates")[0].GetProperty("content")
                .GetProperty("parts")[0].GetProperty("text").GetString()?.Trim();
        }

        private string RechercheAideSecours(string question)
        {
            var q = question.ToLowerInvariant();
            var lignes = BaseConnaissanceAide.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var motsClesQuestion = q.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(m => m.Length > 3).ToList();

            var meilleure = lignes
                .Select(l => new { Ligne = l, Score = motsClesQuestion.Count(m => l.ToLowerInvariant().Contains(m)) })
                .OrderByDescending(x => x.Score)
                .FirstOrDefault();

            return (meilleure != null && meilleure.Score > 0)
                ? meilleure.Ligne.Trim()
                : "Je n'ai pas trouvé de réponse précise. Consultez le centre d'aide (icône ?) pour la liste complète des questions fréquentes.";
        }

        /// <summary>
        /// Extrait ville de départ / arrivée / date d'une phrase libre pour la recherche de voyage
        /// (utilisé par l'assistant de réservation en ligne, public).
        /// </summary>
        public async Task<RechercheVoyageResult> ExtraireRechercheVoyageAsync(string question)
        {
            for (int tentative = 1; tentative <= 2; tentative++)
            {
                try
                {
                    var resultat = await AppellerExtractionAsync(question);
                    if (resultat != null)
                        return resultat;
                }
                catch { }

                if (tentative == 1)
                    await Task.Delay(600);
            }

            // Secours : extraction naïve par mots-clés / villes connues
            return ExtractionParMotsClesSecours(question);
        }

        private async Task<RechercheVoyageResult?> AppellerExtractionAsync(string question)
        {
            var aujourdhui = DateTime.Today.ToString("yyyy-MM-dd");
            var prompt = $@"Tu es l'assistant de réservation de l'application AgenceV (transport interurbain de voyageurs).
Aujourd'hui nous sommes le {aujourdhui}.
Analyse la phrase de l'utilisateur et extrait : la ville de départ, la ville d'arrivée, et la date du voyage (au format YYYY-MM-DD).
Résous les dates relatives (""demain"", ""après-demain"", ""ce weekend"") par rapport à aujourd'hui.
Si une information est absente, mets null.

Réponds UNIQUEMENT avec un JSON strict : {{""depart"": ""...ou null"", ""arrivee"": ""...ou null"", ""date"": ""YYYY-MM-DDou null""}}
Aucun texte autour, pas de markdown.

Phrase de l'utilisateur : ""{question}""";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
            var body = new
            {
                contents = new object[] { new { role = "user", parts = new object[] { new { text = prompt } } } },
                generationConfig = new { temperature = 0.1 }
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(url, content);
            if (!response.IsSuccessStatusCode) return null;

            var raw = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(raw);
            var text = doc.RootElement.GetProperty("candidates")[0].GetProperty("content")
                .GetProperty("parts")[0].GetProperty("text").GetString() ?? "{}";
            text = text.Trim().Trim('`');
            if (text.StartsWith("json")) text = text[4..].Trim();

            using var parsed = JsonDocument.Parse(text);
            string? Lire(string prop) =>
                parsed.RootElement.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

            DateTime? date = null;
            if (DateTime.TryParse(Lire("date"), out var d)) date = d;

            return new RechercheVoyageResult { Depart = Lire("depart"), Arrivee = Lire("arrivee"), Date = date };
        }

        private RechercheVoyageResult ExtractionParMotsClesSecours(string question)
        {
            var q = question.ToLowerInvariant();
            var resultat = new RechercheVoyageResult();

            if (q.Contains("demain"))
                resultat.Date = DateTime.Today.AddDays(1);
            else if (q.Contains("après-demain") || q.Contains("apres-demain"))
                resultat.Date = DateTime.Today.AddDays(2);
            else if (q.Contains("aujourd'hui") || q.Contains("aujourdhui"))
                resultat.Date = DateTime.Today;

            // Villes camerounaises courantes — complétez selon vos agences
            var villes = new[] { "yaoundé", "yaounde", "douala", "bafoussam", "bamenda", "garoua", "maroua", "ngaoundéré", "ngaoundere", "bertoua", "ebolowa", "buea", "limbe", "kribi", "dschang" };
            var trouvees = villes.Where(v => q.Contains(v)).ToList();
            if (trouvees.Count >= 1) resultat.Depart = trouvees[0];
            if (trouvees.Count >= 2) resultat.Arrivee = trouvees[1];

            return resultat;
        }
    }
}
