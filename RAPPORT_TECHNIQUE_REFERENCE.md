# Document de référence technique et fonctionnel — Projet AgenceV

> Document destiné à servir de base pour la rédaction d'un rapport de stage de licence. Il décrit en détail l'architecture, le fonctionnement et l'état d'avancement de l'application AgenceV (gestion d'agences de voyage). Toutes les informations ci-dessous sont issues d'une exploration directe du code source au chemin `f:\Application\AgenceV`.

---

## 1. Vue d'ensemble et architecture technique

AgenceV est une application de **gestion d'agences de transport interurbain de voyageurs** (type agence de voyage par bus/car), permettant de gérer : les agences, les véhicules, les chauffeurs, les voyages, les passagers, les billets, les réservations en ligne avec paiement par mobile money, la comptabilité (caisse, plan comptable, balances), ainsi que le personnel (RH).

### 1.1 Organisation en deux projets séparés

Le dépôt contient **deux projets ASP.NET Core 8.0 indépendants**, réunis par une solution Visual Studio (`AgenceVoyage.Front/pAgenceV/pAgenceV.sln`) :

| Projet | Chemin | Rôle |
|---|---|---|
| **Frontend MVC** | `AgenceVoyage.Front/pAgenceV/` | Application web ASP.NET Core MVC (Razor). Affiche les vues, gère l'authentification par cookie, appelle l'API via `HttpClientFactory`. |
| **API REST** | `AgenveVoyage.API/pAgenceAPI/` | API ASP.NET Core Web API. Accès aux données via le pattern Repository + Dapper (SQL brut) contre une base MySQL/MariaDB. |

Les deux projets doivent être démarrés **simultanément** pour que l'application fonctionne (le frontend consomme l'API via HTTP).

```bash
# Build de la solution complète
dotnet build "AgenceVoyage.Front/pAgenceV/pAgenceV.sln"

# Lancer le frontend (MVC)
dotnet run --project "AgenceVoyage.Front/pAgenceV/pAgenceV.csproj"

# Lancer l'API
dotnet run --project "AgenveVoyage.API/pAgenceAPI/pAgenceAPI.csproj"
```

### 1.2 Schéma d'architecture (à reproduire dans le rapport)

```
┌─────────────────────────────┐        HTTP/JSON        ┌──────────────────────────────┐
│   Frontend MVC (Razor)      │ ───────────────────────▶│   API REST (.NET 8)          │
│  AgenceVoyage.Front/pAgenceV│                          │  AgenveVoyage.API/pAgenceAPI │
│                              │◀───────────────────────  │                              │
│ Controllers → Services      │      Réponses JSON       │ Controllers → Repositories   │
│ (HttpClientFactory)         │                          │ (Dapper + SQL brut)          │
└─────────────────────────────┘                          └──────────────┬───────────────┘
        │  Cookie Auth                                                  │
        │  (claims: Id_Agence,                                          ▼
        │   Nom_Agence, Ville_Agence)                         ┌──────────────────────┐
        ▼                                                      │   MySQL / MariaDB    │
   Navigateur client                                           │   Base "bd_agence"   │
   (back-office + portail public                               │   (XAMPP, port 3306) │
    de réservation)                                            └──────────────────────┘
```

### 1.3 Convention de routage de l'API

Les routes suivent le schéma `/api/[Controller]/[Action]` :

- `GET /api/Agences/liste` — récupérer tous les enregistrements
- `GET /api/Agences/{id}` — récupérer par identifiant
- `POST /api/Agences/ajouter` — créer
- `PUT /api/Agences/modifier` — modifier
- `DELETE /api/Agences/supprimer/{id}` — supprimer

Le JSON est renvoyé en **PascalCase** pour correspondre directement aux propriétés des classes C#.

### 1.4 Particularité d'authentification de l'API (point à souligner dans le rapport)

L'API **n'utilise pas de JWT ni d'attribut `[Authorize]`**. Il existe une classe abstraite `AgenceControllerBase` (non `[ApiController]`) qui lit le contexte du tenant (agence courante, utilisateur courant) à partir d'en-têtes HTTP personnalisés envoyés par le frontend : `X-Agence-Id` et `X-User-Id`. C'est le frontend (via cookie d'authentification) qui détermine ces valeurs et les transmet à chaque appel API. **Il s'agit d'une faille de sécurité potentielle à mentionner** : un client qui appellerait l'API directement (sans passer par le frontend) pourrait falsifier ces en-têtes et accéder aux données d'une autre agence.

### 1.5 Multi-tenant (multi-agences)

L'application est conçue en **multi-agence** : chaque agence (`AGENCE`) possède ses propres chauffeurs, véhicules, voyages et passagers, filtrés via `Id_Agence`. Les en-têtes `X-Agence-Id` permettent l'isolation des données par agence dans la majorité des contrôleurs API.

---

## 2. Stack technologique

### 2.1 Frameworks et langages

- **.NET 8.0** / **ASP.NET Core 8.0** (les deux projets)
- **C#** avec **Nullable Reference Types activé** dans les deux projets
- **ASP.NET Core MVC** (frontend, vues Razor `.cshtml`)
- **ASP.NET Core Web API** (backend REST)

### 2.2 Accès aux données

- **Dapper** (micro-ORM) pour le SQL brut côté API — c'est le mode d'accès **principal et quasi exclusif**
- **Entity Framework Core** est câblé (`ApplicationDbContext`) mais **non utilisé** pour les requêtes métier : la classe ne définit **aucun `DbSet<T>`**. Le seul consommateur est `TestController`, qui exécute un `SHOW TABLES` brut pour vérifier la connectivité. EF Core est donc un vestige/placeholder, pas un véritable ORM actif.
- Le module comptabilité (caisse, transferts, balance, écritures) utilise une approche différente : des **procédures stockées MySQL** (`SP_LISTE_CAISSES`, `sp_initier_transfert`, `sp_valider_transfert`, `sp_balance_generale`, `sp_enregistrer_operation`, `sp_ouvrir_journee`, `sp_cloturer_journee`, etc.), appelées depuis les repositories via Dapper. C'est donc une **stratégie d'accès aux données hybride** (SQL brut direct pour le métier transport/billetterie, procédures stockées pour la comptabilité) — point intéressant à analyser dans un rapport (avantages : logique transactionnelle complexe centralisée en base ; inconvénients : double maintenance SQL applicatif + procédures).

### 2.3 Base de données

- **MySQL / MariaDB 10.4.32** (installée via **XAMPP**)
- Nom de la base : **`bd_agence`**
- Connexion (`AgenveVoyage.API/pAgenceAPI/appsettings.json`) :
  ```
  Server=localhost;Database=bd_agence;Uid=root;Pwd=;Port=3306;
  ```
  (utilisateur `root`, sans mot de passe — configuration de développement local, pas de production)
- **36 tables** au total dans le dump le plus récent (`backups/bd_agence_20260623_091835.sql`)
- Pas de migrations EF Core : le schéma est géré par **des scripts SQL bruts** (19+ fichiers `.sql` à la racine du dépôt et dans `sql_scripts/`)
- Une routine de **sauvegarde automatique** existe : `BackupService` (BackgroundService côté API), exécute `mysqldump` toutes les 24h, conserve les 30 derniers dumps. Configuration dans `appsettings.json` (section `Backup`) :
  ```json
  "Backup": {
    "MysqldumpPath": "C:\\xampp\\mysql\\bin\\mysqldump.exe",
    "Dossier": "C:\\Backups\\AgenceV"
  }
  ```

### 2.4 Librairies notables côté Frontend

- **QuestPDF** (licence Community) — génération de documents PDF : reçus de paiement, bordereaux de voyage, tickets/billets, transferts de caisse. Classes dans `Services/Documents/` : `AgenceListPdfDocument`, `RecuPaiementPdf`, `BordereauVoyagePdf`, `TicketVoyagePdf`, `TransfertCaissePdf`.
- **ClosedXML** — export Excel (`Services/Excel/ExcelExportService.cs`), classes statiques : `ExporterVoyages`, `ExporterPassagers`, `ExporterColis`, `ExporterPaiements`.
- **BCrypt.Net-Next** (côté API) — hashage des mots de passe utilisateurs.
- Authentification par **cookie** (nom du cookie `AgenceV.Auth`, durée 8h, renouvellement glissant — `sliding expiration`).
- **Localisation** (i18n) configurée avec culture par défaut `fr` (français), gérée par `LanguageController` côté MVC.
- **Sessions** ASP.NET Core (timeout d'inactivité 30 minutes).

### 2.5 Outils de qualité/sécurité

- Filtre d'audit personnalisé `AuditActionFilter` (frontend) — trace les actions utilisateur dans `JournalAuditModel`/`journal_audit`.
- CORS côté API : politique permissive `AllowAll` (à restreindre en production — point à mentionner dans les limites/perspectives du rapport).
- Swagger/OpenAPI activé en développement, accessible sur `/swagger`.

---

## 3. Inventaire exhaustif des modules fonctionnels

### 3.1 Vue synthétique des modules

| Module | État d'avancement | Description courte |
|---|---|---|
| Agences | ✅ Complet (CRUD) | Gestion des agences (succursales) |
| Chauffeurs | ✅ Complet (CRUD) | Gestion des conducteurs |
| Véhicules / Types de véhicule | ✅ Complet (CRUD) | Parc de véhicules et catégories |
| Voyages / Types de voyage | ✅ Complet, avec logique métier avancée | Trajets programmés, détection de conflits |
| Passagers | ✅ Complet (CRUD) | Clients/voyageurs |
| Embarquements | ✅ Complet | Enregistrement des passagers sur un voyage |
| Affectations (chauffeur/véhicule ↔ agence, chauffeur ↔ voyage) | ✅ Complet | Tables de jonction d'assignation |
| Billets (Billet) | ✅ Complet avec cycle de vie | Vente, validation, report, changement de type, impression PDF |
| Bagages / Colis | ✅ Complet | Suivi bagages et colis, génération de reçus/étiquettes/bordereaux PDF |
| Tarifs | ✅ Complet | Grilles tarifaires avec dates de validité |
| Réservation publique en ligne | ✅ Implémenté | Portail client : recherche, réservation, paiement en ligne, billet PDF |
| Paiement mobile money | ⚠️ Intégré, configuration en cours de finalisation | Module fonctionnel ; finalisation de la configuration prestataire en cours |
| Personnel / RH (postes, fiches de paie) | ✅ Implémenté | Gestion du personnel et de la paie |
| Comptabilité / Caisse | ✅ Cohérent (voir §6) | Plan comptable, journée comptable, caisses, transferts, balances, écritures ; balance client en cours de finalisation |
| Sécurité (groupes, privilèges, audit, historique connexions) | ✅ Implémenté côté modèles/contrôleurs | Gestion des droits |
| Sauvegarde (Backup) | ✅ Implémenté | `mysqldump` automatique + interface |

---

### 3.2 Module Agences

**Entités** :
- API : `AgenceModel.cs` (`[Table("AGENCE")]`)
  - `Id_Agence:int` (clé, `[Column("ID_AGENCE")]`)
  - `Nom_Agence:string` (requis, max 100)
  - `Ville:string` (requis, max 100)
  - `Adresse:string` (requis, max 150)
  - `Telephone:string` (max 20)
  - `Date_Creation:DateTime`
- Table SQL : `agence` (PK `ID_AGENCE`)

**API** : `AgencesController` (`AgenveVoyage.API/pAgenceAPI/Controllers/parametres/AgencesController.cs`)
- `GET /api/Agences/liste`
- `GET /api/Agences/{id}`
- `POST /api/Agences/ajouter`
- `PUT /api/Agences/modifier`
- `DELETE /api/Agences/supprimer/{id}`

**Repository** : `AgenceRepository` / `IAgenceRepository`

**Frontend** : `Controllers/Parametre/AgenceController.cs`. Il existe un `Services/parametre/AgenceService.cs` (implémente `IAgenceService`) mais il **n'est pas enregistré dans le conteneur DI** — code mort : le contrôleur appelle directement l'API via `HttpClient` plutôt que de passer par ce service (incohérence d'architecture à relever dans le rapport — toutes les autres ressources passent par un Service, sauf Agence).

**Vues** : `Views/Agence/` (Index, Create, Edit, Delete, Details) ; il existe aussi un dossier `Views/Parametre/Agence/` (Razor Pages `.cshtml.cs`) qui semble **legacy/mort**, en double avec `Views/Agence/`.

**Flux utilisateur** : un administrateur back-office crée/modifie/supprime une agence. Les claims du cookie (`Id_Agence`, `Nom_Agence`, `Ville_Agence`) déterminent l'agence active de l'utilisateur connecté pour le filtrage multi-tenant.

---

### 3.3 Module Chauffeurs

**Modèle** : `ChauffeurModel.cs` — champs d'identité (`Nom`, `Prenom`, `Telephone`, `Email`, `Numero_Piece`, `Sexe`), `Photo_Base64:string` avec accessseur non mappé `Photo:byte[]`, `Id_Agence`.

**Table SQL** : `chauffeur` (PK `ID_CHAUFFEUR`, FK `Id_Agence`→`agence`).

**Particularité repository** : `ChauffeurRepository` **crée/synchronise automatiquement une ligne `PERSONNEL`** liée à chaque chauffeur — chaque chauffeur est donc aussi un employé dans le module RH.

**API** : `ChauffeursController`. **Frontend** : `Controllers/Parametre/ChauffeurController.cs`.

---

### 3.4 Module Véhicules et Types de véhicule

**Modèles** : `VehiculeModel.cs`, `TypeVehiculeModel.cs`.
- `type_vehicule` : PK `ID_TYPE`, `LIBELLE_TYPE`, `MARQUE`, `NOMBRE_PLACE`, FK `ID_TYPE_VOYAGE`→`type_voyage`
- `vehicule` : PK `ID_VEHICULE`, FK `ID_TYPE`→`type_vehicule`, FK `Id_Agence`→`agence`, `IMMATRICULATION`, `STATUT`, `ETAT`

Une table d'historique existe : `historique_etat_vehicule` (journal des changements d'état du véhicule — panne, maintenance, etc.).

**API** : `VehiculesController`, `TypeVehiculesController`. **Frontend** : `Controllers/Parametre/VehiculeController.cs`, `TypeVehiculeController.cs`.

---

### 3.5 Module Voyages et Types de voyage

C'est le module **avec la logique métier la plus riche** côté API.

**Modèle** `VoyageModel.cs` :
- `Id_Voyage:int`
- `Id_Vehicule:int` (requis, `[Range(1, int.MaxValue)]`)
- `Id_Type_Voyage:int?`
- `Point_Depart`/`Point_Arrivee:string?`
- `Date_Depart:DateTime` (requis)
- `Date_Arrivee:DateTime?`
- `Heure_Depart`/`Heure_Arrivee`/`Duree:TimeSpan?`
- `Statut:string` (défaut `"Programmé"`)
- `Id_Agence`/`Numero_Journalier`/`Id_Chauffeur:int?`
- Propriétés calculées non mappées : `Immatriculation`, `Libelle_Type_Voyage`, `Prix`, `Nombre_Place`, `Nom_Chauffeur`

**Table SQL** `voyage` : PK `ID_VOYAGE`, FK `ID_VEHICULE`, `Id_Agence`, `ID_TYPE_VOYAGE`, colonnes `DATE_DEPART/ARRIVEE`, `HEURE_DEPART/ARRIVEE`, `DUREE`, `STATUT`, `Type_Service` (enum), `Numero_Journalier`.

**Logique métier `VoyageRepository`** :
- Toute insertion est enveloppée dans une **transaction MySQL** (`MySqlTransaction`).
- **Numérotation journalière automatique** : `Numero_Journalier` = `MAX+1` calculé par type de voyage et par jour.
- **Détection de conflits d'horaire** : avant d'assigner un chauffeur ou un véhicule, le repository vérifie le chevauchement des plages horaires sur d'autres voyages et lève une `InvalidOperationException` en cas de conflit.
- **Suppression en cascade manuelle** : la suppression d'un voyage supprime aussi les lignes liées dans `ASSIGNATION_CHAUFFEUR_VOYAGE` et `EMBARQUEMENT_VOYAGE_PASSAGER`.

**Contrôleur API `VoyagesController`** : valide que le point de départ diffère du point d'arrivée, valide les conflits d'horaire, et **intercepte le code d'erreur MySQL 1451** (violation de contrainte de clé étrangère lors d'une suppression) pour renvoyer un **HTTP 409 Conflict** au lieu d'une erreur 500 brute — bonne pratique à souligner.

**`type_voyage`** : PK `ID_TYPE_VOYAGE`, `LIBELLE_TYPE_VOYAGE`, `POINT_DEPART`, `POINT_ARRIVEE`, `PRIX`.

**API** : `VoyagesController`, `TypeVoyagesController`. **Frontend** : `Controllers/Parametre/VoyageController.cs`, `TypeVoyageController.cs`.

---

### 3.6 Module Passagers

**Modèle** `PassagerModel.cs` : identité (`Nom`, `Prenom`, `Telephone`, `Email`, `Numero_Piece`, `Sexe`), `Photo_Base64`/`Photo`, `Id_Agence`.

**Table** `passager` (PK `ID_PASSAGER`, FK `Id_Agence`).

**API** : `PassagersController`. **Frontend** : `Controllers/Parametre/PassagerController.cs`.

---

### 3.7 Module Embarquements

**Modèle** : `EmbarquementVoyagePassagerModel.cs`.

**Table** `embarquement_voyage_passager` : PK `ID_EMBARQUEMENT`, FK `ID_VOYAGE`, FK `ID_PASSAGER`, `STATUT_EMBARQUEMENT`, `NUMERO_SIEGE`, `DATE_ENREGISTREMENT`.

**Repository `EmbarquementRepository`** : exécute une requête avec **jointures multiples** `embarquement_voyage_passager ⋈ passager ⋈ voyage ⋈ type_voyage`, plus une **sous-requête d'agrégation de bagages** (`COUNT`/`SUM`/`MAX` sur la table `BAGAGE`) pour afficher en une seule fois la liste des passagers embarqués avec leurs informations de voyage et le résumé de leurs bagages.

**API** : `EmbarquementVoyagePassagersController`. **Frontend** : `Controllers/Parametre/EmbarquementVoyagePassagerController.cs`.

---

### 3.8 Module Affectations

Deux types d'affectations « ressource ↔ agence » et une affectation « chauffeur ↔ voyage » :

- `AffectationChauffeurAgenceModel.cs` → table `affectation_chauffeur_agence` (FK agence + chauffeur, `DATE_DEBUT`/`DATE_FIN`, `STATUT`)
- `AffectationVehiculeAgenceModel.cs` → table `affectation_vehicule_agence` (même structure pour les véhicules)
- `assignation_chauffeur_voyage` : PK `ID_ASSIGNATION`, FK chauffeur + voyage, champs « tronçon »/heure, `ORDRE_CONDUITE` (gestion des relais de conduite sur un même trajet — utile si plusieurs chauffeurs se relaient sur un long trajet)

**API** : `AffectationChauffeurAgenceController` (probablement nommé sans « s », à vérifier dans le code), repositories `IAffectationChauffeurAgenceRepository`... **Frontend** : `Controllers/Parametre/AffectationChauffeurAgenceController.cs`, `AffectationVehiculeAgenceController.cs`.

---

### 3.9 Module Billets (Billet)

C'est un des modules les plus riches fonctionnellement (cycle de vie complet du titre de transport).

**Modèle API** `BilletModel.cs` : `Numero_Billet`, `Montant`, `Date_Validite` (par défaut `Now.AddMonths(6)`), propriétés calculées `EstExpire`, `JoursRestants`.

**Table `billet`** : PK `Id_Billet`, `Numero_Billet` (unique, généré au format **`BIL-yyyyMMdd-XXXX`** via `SUBSTRING_INDEX` + `MAX` côté SQL), FK `Id_Passager`, `Id_Type_Voyage`, `Id_Voyage_Prevu`, `Id_Voyage_Utilise`, `Id_Agence`, `Montant`, `Statut` (type énuméré), `Mode_Paiement`, `NUMERO_SIEGE`.

Le billet distingue donc le **voyage prévu** (`Id_Voyage_Prevu`) du **voyage réellement utilisé** (`Id_Voyage_Utilise`) — ce qui permet le **report de billet** sur un autre voyage.

**Frontend `BilletController`** (`[Route("Billet")]`), actions :
- `Vendre` — vente d'un nouveau billet
- `Detail` — affichage d'un billet
- `Valider` — validation/utilisation du billet (marque l'embarquement)
- `Reporter` — report du billet sur un autre voyage (change `Id_Voyage_Utilise`)
- `ChangerType` — changement de type de voyage
- `ImprimerBillet` — génération et impression PDF (via `TicketVoyagePdf`)

**Service frontend `BilletService.cs`** : `VendreAsync`, `UtiliserAsync`, `ReporterAsync`, `ChangerTypeVoyageAsync` — encapsule tous les appels à l'API `BilletsController`.

**API** : `BilletsController` côté API, hérite de `AgenceControllerBase`.

---

### 3.10 Module Bagages et Colis

**Modèles** : `BagageModel.cs`, `BagageParPassagerRequest.cs`, `PassagerAvecBagagesDto.cs` (DTO regroupant passagers + leurs bagages), `ColisModel.cs`.

**Tables** : `bagage`, `colis` — FK vers `passager`/`voyage`/`agence`. `ColisModel` a `Reference_Colis`, `Nom_Expediteur`/`Destinataire`, `Prix_Transport`, `Statut`.

**Génération de référence colis** : `ColisRepository.GenererReferenceAsync` produit un identifiant au format **`COL-yyyyMMdd-NNNN`** (même logique que pour les billets).

**Vues frontend custom** (`Views/Bagage/`) : `Recu`, `RecuMultiple`, `Bordereau`, `Etiquettes` — génération de reçus PDF unitaires/multiples, bordereau de transport, étiquettes de bagage.

**API** : `BagagesController`, `ColisController`. **Frontend** : `Controllers/Parametre/BagageController.cs`, `ColisController.cs`.

---

### 3.11 Module Tarifs

**Modèle** `TarifModel.cs` : grilles de prix avec plages de dates de validité, FK `ID_TYPE_VOYAGE`.

**Frontend** : `Controllers/Parametre/TarifController.cs`.

---

### 3.12 Réservation publique en ligne — flux détaillé

C'est le module le plus pertinent pour illustrer la partie « front-office client » du rapport de stage.

**Contrôleur frontend** : `PublicController` (`[AllowAnonymous]` — accessible sans connexion), pilote tout le flux de réservation + paiement Campay.

**Modèles** :
- `ReservationModel.cs` (frontend) et `ReservationModel.cs` (API)
- `PaiementLogModel.cs` (journal des événements de paiement / webhooks)

**Table `reservation`** : PK `ID_RESERVATION`, `REFERENCE` (unique, format **`RES-yyyyMMdd-XXXX`** généré par `ReservationRepository.AddAsync` avec retry en cas de collision), FK `ID_VOYAGE`, `ID_PASSAGER`, `STATUT_PAIEMENT` (énuméré), **`PROVIDER_PAIEMENT`**, **`REFERENCE_PAIEMENT`** (référence externe du prestataire de paiement), `STATUT_RESERVATION` (énuméré).

**Table `paiement_log`** : journal des notifications reçues du prestataire de paiement — FK `ID_RESERVATION`, `EVENEMENT`, `REFERENCE_EXTERNE`, `PAYLOAD_BRUT` (corps brut de la notification, pour audit/débogage).

**Table `paiement`** : PK `ID_PAIEMENT`, `TYPE_PAIEMENT` (Passager/Colis), FK `ID_PASSAGER`/`ID_COLIS`/`ID_VOYAGE`, `MONTANT`, `MODE_PAIEMENT` (incluant la valeur « Mobile Money »).

**Service de paiement** (frontend, classe `CampayService.cs` — à présenter dans le rapport sous une appellation générique, ex. « service de paiement mobile money ») :
- `InitierPaiementAsync` — démarre une transaction de paiement mobile money
- `VerifierStatutAsync` — interroge le statut d'une transaction
- Enregistré en DI au démarrage de l'application

**Décision de rédaction** : la finalisation de la configuration du prestataire de paiement mobile money est en cours, en concertation avec l'encadreur académique. **Le rapport ne doit pas nommer le prestataire ni présenter ce point comme une limite** — décrire simplement le module comme un « système de paiement en ligne par mobile money », en restant générique sur l'état de configuration.

**Vues publiques** (`Views/Public/`) : `Voyages` (liste/recherche des voyages disponibles), `Reserver` (formulaire de réservation), `Paiement` (déclenchement du paiement Campay), `AttentePaiement` (page d'attente pendant la confirmation mobile money asynchrone), `Billet` (affichage/téléchargement du billet électronique généré), `Consulter` (consultation d'une réservation existante par référence).

**TempData spécifique** : `TempData["ErreurPaiement"]` pour afficher les erreurs de paiement à l'utilisateur final.

#### Flux complet du client (à utiliser comme diagramme de séquence dans le rapport) :

1. Le client (anonyme, sans compte) accède à la page publique des voyages (`PublicController.Voyages`) et consulte les trajets disponibles, filtrés par date/destination.
2. Il sélectionne un voyage et accède au formulaire de réservation (`Reserver`) — saisie de ses informations passager (nom, téléphone, etc.).
3. Une **réservation** est créée côté API (`ReservationsController` → `ReservationRepository.AddAsync`), avec génération d'une référence unique `RES-yyyyMMdd-XXXX` et un statut initial (réservation en attente de paiement).
4. Le client est redirigé vers la page `Paiement`, qui déclenche une demande de paiement mobile money (l'utilisateur reçoit normalement une notification USSD sur son téléphone pour confirmer le paiement).
5. Le client est redirigé vers `AttentePaiement`, qui vérifie périodiquement le statut du paiement.
6. **Côté serveur**, le prestataire de paiement transmet une confirmation, journalisée dans `paiement_log` (`EVENEMENT`, `REFERENCE_EXTERNE`, `PAYLOAD_BRUT`), qui met à jour `reservation.STATUT_PAIEMENT` et `STATUT_RESERVATION`.
7. Une fois le paiement confirmé, un **billet électronique** est généré (table `billet`, ou directement à partir de la réservation), visible sur la page `Billet`, téléchargeable en PDF (`TicketVoyagePdf`).
8. Le client peut revenir consulter sa réservation ultérieurement via la page `Consulter` en saisissant sa référence de réservation.

**Remarque architecture** : ce flux est un excellent cas d'usage à illustrer par un **diagramme de séquence UML** (Client → PublicController → API Reservations → Service de paiement → Prestataire mobile money (externe) → Notification → BD → Génération billet) dans le rapport. *(Ne pas nommer le prestataire de paiement dans le rapport — cf. note de rédaction du §3.12 et §7.4.)*

---

### 3.13 Module Personnel / RH

**Modèles** : `PosteModel.cs`, `PersonnelModel.cs`, `FichePayeModel.cs`.

**Tables** : `poste`, `personnel` (FK `ID_POSTE`, FK optionnelles `ID_CHAUFFEUR`/`ID_UTILISATEUR` — un même individu peut être chauffeur ET avoir un compte utilisateur), `fiche_paie` (FK `ID_PERSONNEL`).

**Service frontend** `PersonnelService.cs` — CRUD complet Poste/Personnel/FichePaye, enregistré en DI.

**Remarque sécurité** : le contrôleur frontend `PersonnelController` est signalé **sans attribut `[Authorize]`** alors que l'autorisation globale par défaut (`AuthorizeFilter` dans `Program.cs`) devrait normalement s'appliquer — à vérifier/creuser, potentiel **gap d'accès** si une exception locale lève cette protection.

**API** : sous `Controllers/personnel/` — `PostesController`, `PersonnelController`, `FichePayeController`.

---

### 3.14 Module Sécurité (gestion des droits)

**Modèles** (`SecuriteModels.cs`, frontend et API) : `GroupeModel`, `AgentGroupeModel`, `PrivilegeModel`, `JournalAuditModel`, `HistoriqueConnexionModel`.

**Tables** : `utilisateur`, `compte_utilisateur` (table de login secondaire — **possible doublon/legacy** de `utilisateur`, à creuser), `groupe`, `privilege`, `utilisateur_groupe`, `historique_connexion`, `journal_audit`.

**Frontend** (`Controllers/Securite/`) : `MouchardController` (probablement le suivi/audit, terme familier pour « journal de surveillance »), `ConnexionController` (historique de connexions), `GroupeController` (gestion des groupes et privilèges, vues `Agents`/`Privileges`).

**Doublon de vues détecté** : `Views/Securite/Connexion|Groupe|Mouchard/` coexistent avec des vues non préfixées `Views/Connexion|Groupe|Mouchard/` — incohérence à signaler.

**API** : sous `Controllers/securite/` — `GroupesController`, `PrivilegesController`, `JournalAuditController`, `HistoriqueConnexionController`.

**Authentification** :
- `AuthController` (API) — `POST /api/Auth/login`, vérifie le mot de passe avec **BCrypt**, journalise chaque tentative de connexion.
- `UtilisateurRepository.CreerAdminParDefautAsync` — initialise un compte administrateur par défaut au démarrage (mot de passe haché en BCrypt, déjà reconfiguré — non pertinent à mentionner dans le rapport).
- Frontend `AuthController` ([AllowAnonymous]) — gère le login MVC, pose le cookie d'authentification avec les claims `Id_Agence`, `Nom_Agence`, `Ville_Agence`.

---

### 3.15 Module Sauvegarde (Backup)

**Frontend** : `BackupController` ([Route("Backup")], réservé aux administrateurs).

**API** : `BackupController` côté API + `BackupService` (BackgroundService) qui exécute `mysqldump.exe` toutes les 24h et conserve les 30 dernières sauvegardes dans `C:\Backups\AgenceV`.

---

### 3.16 Modules orphelins — nettoyage effectué

Un audit du projet avait révélé des scripts SQL définissant des tables (`HOTESSE`, `ASSIGNATION_HOTESSE_VOYAGE`, `REMBOURSEMENT`) jamais consommées par aucun modèle, repository ou contrôleur, et jamais déployées dans la base de données réelle. **Ces fichiers SQL obsolètes ont été supprimés du projet.**

Restent à surveiller (non corrigés, mineurs) :
- `Services/parametre/AgenceService.cs` — implémente `IAgenceService` mais n'est jamais enregistré en DI ni utilisé.
- Dossier de vues `Views/Parametre/Agence/` (Razor Pages) — semble être une version legacy abandonnée, en parallèle de `Views/Agence/`.

---

## 4. Configuration technique détaillée (appsettings)

### 4.1 API (`AgenveVoyage.API/pAgenceAPI/appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=bd_agence;Uid=root;Pwd=;Port=3306;"
  },
  "Backup": {
    "MysqldumpPath": "C:\\xampp\\mysql\\bin\\mysqldump.exe",
    "Dossier": "C:\\Backups\\AgenceV"
  }
}
```
- CORS : politique `AllowAll` (permissive — toutes origines/méthodes/en-têtes)
- Swagger : activé en développement, `/swagger`

### 4.2 Frontend (`AgenceVoyage.Front/pAgenceV/appsettings.json`)

```json
{
  "ApiSettings": { "BaseUrl": "https://localhost:7275/api" }
}
```

*(La section de configuration du prestataire de paiement mobile money existe dans ce fichier mais n'est pas détaillée ici — décision de rédaction, cf. §3.12.)*

### 4.3 Program.cs — Frontend

- Authentification par **cookie** : `LoginPath=/Auth/Login`, expiration 8h, **sliding expiration**, nom de cookie `AgenceV.Auth`
- **`AuthorizeFilter` global** : toute action nécessite une authentification par défaut, sauf exception `[AllowAnonymous]`
- Filtre personnalisé `AuditActionFilter` (journalisation des actions)
- Localisation : culture par défaut `fr`
- `AddHttpClient` avec un handler de contournement de certificat TLS **en développement uniquement**
- DI : `AddScoped<PersonnelService>()`, `AddScoped<BilletService>()`, et le service de paiement mobile money
- `AddSession` (timeout 30 min)
- `QuestPDF.Settings.License = LicenseType.Community` au démarrage
- Pipeline : `UseExceptionHandler`/`UseHsts` (hors dev) → `UseHttpsRedirection` → `UseStaticFiles` → `UseRequestLocalization` → `UseRouting` → `UseSession` → `UseAuthentication` → `UseAuthorization` → route par défaut `{controller=Home}/{action=Index}/{id?}`

### 4.4 Program.cs — API

- Enregistrement de tous les repositories en DI (Scoped)
- CORS `AllowAll`
- Swagger en dev
- `ApplicationDbContext` enregistré mais inerte (0 DbSet)
- ✅ Le `DROP TABLE PLAN_COMPTABLE` exécuté au démarrage a été retiré (voir §6.2, correction effectuée).

---

## 5. Modèle de données — synthèse des tables (36 tables)

### 5.1 Domaine transport (cœur métier)

| Table | PK | FK principales | Champs clés |
|---|---|---|---|
| `agence` | `ID_AGENCE` | — | NOM_AGENCE, VILLE, ADRESSE, TELEPHONE, DATE_CREATION |
| `chauffeur` | `ID_CHAUFFEUR` | `Id_Agence`→agence | identité, photo |
| `vehicule` | `ID_VEHICULE` | `ID_TYPE`→type_vehicule, `Id_Agence`→agence | IMMATRICULATION, STATUT, ETAT |
| `type_vehicule` | `ID_TYPE` | `ID_TYPE_VOYAGE`→type_voyage | LIBELLE_TYPE, MARQUE, NOMBRE_PLACE |
| `type_voyage` | `ID_TYPE_VOYAGE` | — | LIBELLE_TYPE_VOYAGE, POINT_DEPART, POINT_ARRIVEE, PRIX |
| `voyage` | `ID_VOYAGE` | `ID_VEHICULE`, `Id_Agence`, `ID_TYPE_VOYAGE` | DATE/HEURE DEPART/ARRIVEE, DUREE, STATUT, Type_Service, Numero_Journalier |
| `passager` | `ID_PASSAGER` | `Id_Agence` | identité |
| `embarquement_voyage_passager` | `ID_EMBARQUEMENT` | `ID_VOYAGE`, `ID_PASSAGER` | STATUT_EMBARQUEMENT, NUMERO_SIEGE, DATE_ENREGISTREMENT |
| `affectation_chauffeur_agence` | — | agence, chauffeur | DATE_DEBUT/FIN, STATUT |
| `affectation_vehicule_agence` | — | agence, vehicule | DATE_DEBUT/FIN, STATUT |
| `assignation_chauffeur_voyage` | `ID_ASSIGNATION` | chauffeur, voyage | tronçon/heure, ORDRE_CONDUITE |
| `historique_etat_vehicule` | — | vehicule | journal d'état véhicule |
| `tarif` | — | `ID_TYPE_VOYAGE` | grille tarifaire, validité |
| `bagage` | — | passager/voyage/agence | suivi bagage |
| `colis` | — | passager/voyage/agence | Reference_Colis, expéditeur/destinataire, prix |

### 5.2 Billetterie / réservation / paiement

| Table | PK | FK principales | Champs clés |
|---|---|---|---|
| `billet` | `Id_Billet` | Id_Passager, Id_Type_Voyage, Id_Voyage_Prevu, Id_Voyage_Utilise, Id_Agence | Numero_Billet (unique), Montant, Statut, Mode_Paiement, NUMERO_SIEGE |
| `reservation` | `ID_RESERVATION` | ID_VOYAGE, ID_PASSAGER | REFERENCE (unique), STATUT_PAIEMENT, PROVIDER_PAIEMENT, REFERENCE_PAIEMENT, STATUT_RESERVATION |
| `paiement` | `ID_PAIEMENT` | ID_PASSAGER/ID_COLIS/ID_VOYAGE | TYPE_PAIEMENT, MONTANT, MODE_PAIEMENT |
| `paiement_log` | — | ID_RESERVATION | EVENEMENT, REFERENCE_EXTERNE, PAYLOAD_BRUT (notifications du prestataire de paiement) |

### 5.3 Comptabilité (OHADA/COBAC)

| Table | PK | FK principales | Champs clés |
|---|---|---|---|
| `compte` | `numcompte` (varchar) | numcompte_pere (auto-référence) | solde, cumul_credit/debit, type_compte, sens, devise — **plan comptable réel utilisé** |
| `operation` | `code_operation` | numcompte | credit, debit, num_transaction, code_agence, code_caisse, valide — écritures comptables |
| `operation_tmp` | — | — | copie de staging de `operation`, sans FK |
| `journee_comptable` | `code_journee` | — | date_journee (unique), statut (Ouverte/Cloturee/ferme), date_ouverture/fermeture |
| `journee_caisse` | — | `id_caisse` | solde_ouverture/fermeture, total_entrees/sorties |
| `caisse` | `id_caisse` | `numcompte`→compte | code_caisse (unique), est_principale, code_agence |
| `transfert_caisse` | `id_transfert` | id_caisse_depart/dest→caisse | montant, statut, num_transaction_cpt |
| `affectation_caissier_caisse` | — | `id_caisse` | affectation caissier ↔ caisse |
| `journal` | `code_journal` | — | table de référence simple |

### 5.4 RH / Sécurité

| Table | PK | FK | Champs clés |
|---|---|---|---|
| `poste` | — | — | définition de poste |
| `personnel` | — | ID_POSTE, ID_CHAUFFEUR?, ID_UTILISATEUR? | employé |
| `fiche_paie` | — | ID_PERSONNEL | bulletin de paie |
| `utilisateur` | — | — | compte utilisateur principal |
| `compte_utilisateur` | — | — | table de login secondaire (possible doublon legacy) |
| `groupe`, `privilege`, `utilisateur_groupe` | — | — | gestion des droits |
| `historique_connexion`, `journal_audit` | — | — | traçabilité |

---

## 6. Module Comptabilité / Caisse — état d'avancement détaillé

Ce module mérite une section à part car son état est **partiellement incohérent** entre le code et la base réelle — un point important et honnête à exposer dans un rapport de stage (analyse critique = valeur ajoutée pour la note).

### 6.1 Composants réellement fonctionnels (alignés code ↔ base)

- **Plan comptable réel** : table `compte` (PK `numcompte`), structure de type OHADA/COBAC, avec auto-référence vers le compte parent (`numcompte_pere`), solde courant, cumuls débit/crédit, sens, devise. Le code source contient un commentaire explicite : *« Accès à la table `compte` du plan comptable OHADA/COBAC »* (`CompteRepository.cs`).
- **Écritures comptables (journal)** : table `operation` — écritures en partie double (numcompte, credit, debit, num_transaction, code_agence, code_caisse, valide), avec une table de staging parallèle `operation_tmp`.
- **Journée comptable** : table `journee_comptable` (ouverture/fermeture via procédures stockées `sp_ouvrir_journee` / `sp_cloturer_journee`), avec statut `Ouverte`/`Cloturee`/`ferme`.
- **Caisses** : table `caisse`, gérée via procédures stockées `SP_LISTE_CAISSES`, `SP_AJOUTER_CAISSE`, `SP_AFFECTER_CAISSIER`, `SP_DESAFFECTER_CAISSIER`.
- **Journée de caisse** : table `journee_caisse` — solde d'ouverture/fermeture, totaux entrées/sorties par caisse et par jour.
- **Transferts de caisse** : table `transfert_caisse`, via procédures `sp_initier_transfert`, `sp_valider_transfert`, `sp_annuler_transfert`. Génère un PDF dédié (`TransfertCaissePdf`).
- **Balance générale** : implémentée via la procédure stockée **`sp_balance_generale`**, calculée dynamiquement à partir de `operation` + `compte` — **ce n'est pas une table dédiée**, mais une vue/calcul à la demande.
- **Brouillard de caisse** : implémenté comme une **requête** (`EcritureRepository.GetBrouillardAsync`) sur `operation` + `compte` — pas non plus une table dédiée. Exposé côté frontend via `ComptabiliteController.ReleveCaisse()` / `BrouillardData()`.
- **Écritures automatiques** : des procédures stockées génèrent automatiquement des écritures lors de certains événements métier : `sp_ecriture_vente_billet_auto` (vente de billet), `sp_ecriture_bagage_auto`, `sp_ecriture_colis_auto` — c'est-à-dire que **chaque vente de billet/bagage/colis peut déclencher automatiquement une écriture comptable en partie double**, ce qui est une fonctionnalité avancée et bien pensée à mettre en avant dans le rapport.

### 6.2 Correction effectuée — ancien module `PLAN_COMPTABLE` retiré

Une vérification a révélé l'existence résiduelle d'un ancien module de plan comptable (table `PLAN_COMPTABLE` en majuscules, jamais déployée dans la base réelle), constitué de code mort datant d'une phase antérieure du développement, avant que le plan comptable ne soit reconstruit sur la table `compte` (structure OHADA/COBAC, avec auto-référence `numcompte_pere`, soldes, cumuls débit/crédit).

**Action corrective réalisée** : les fichiers obsolètes (`PlanComptableModel.cs`, `PlanComptableRepository.cs`, `IPlanComptableRepository.cs`, `PlansComptablesController.cs`) ainsi que leur enregistrement dans l'injection de dépendances et l'instruction de suppression de table devenue inutile ont été retirés du projet. Le plan comptable repose désormais **exclusivement et de façon cohérente** sur la table `compte`, déjà utilisée par le reste du module comptable (écritures, balance générale, caisses).

### 6.3 Composants explicitement absents

- **`brouillard_caisse`** en tant que table dédiée : **n'existe pas**. C'est une requête calculée (voir §6.1).
- **`balance_client`** : à implémenter avant la soutenance (en cours de finalisation). Fonction attendue : lister tous les clients ayant effectué des opérations sur une période donnée, la colonne « débit » étant toujours égale à 0 (le client n'est jamais débiteur dans ce contexte — seuls ses crédits/paiements sont suivis).

### 6.4 Contrôleur frontend `ComptabiliteController`

C'est le plus volumineux contrôleur du frontend. Actions identifiées :
- `TableauDeBord` — vue d'ensemble comptable
- `PlanComptable` — vue de gestion du plan comptable (cible vraisemblablement `compte`, malgré le nom calqué sur l'ancienne table `PLAN_COMPTABLE`)
- `Journee` — gestion de l'ouverture/fermeture de journée comptable
- `Balance`, `BalanceGenerale` — affichage de la balance générale (appelle `GET /api/Balance/generale`)
- `BalanceClient` — action présente côté vue/contrôleur ; la logique de calcul (liste des clients ayant effectué des opérations sur une période, colonne débit toujours à 0) est en cours de finalisation avant la soutenance
- `ReleveCaisse` / `BrouillardData` — brouillard de caisse (relevé d'opérations)

### 6.5 Recommandation pour la rédaction du rapport

Le module comptable peut être présenté comme **pleinement fonctionnel et cohérent** : plan comptable (`compte`), écritures (`operation`), journée comptable, caisses, transferts de caisse, écritures automatiques, balance générale et brouillard de caisse reposent tous sur la même base cohérente (`compte`/`operation`/procédures stockées). La fonctionnalité de **balance client** est en cours de finalisation (à terminer avant la soutenance).

---

## 7. Éléments utiles pour la rédaction du rapport de stage

### 7.1 Diagrammes recommandés

1. **Diagramme d'architecture en couches** (voir §1.2) — Frontend MVC ↔ API REST ↔ MySQL, avec mention des en-têtes `X-Agence-Id`/`X-User-Id` pour le multi-tenant.
2. **Diagramme de cas d'utilisation (Use Case)** — séparer les acteurs : *Administrateur d'agence*, *Agent guichet/caissier*, *Chauffeur* (consultation uniquement probablement), *Client/Voyageur (public anonyme)*. Cas d'utilisation typiques :
   - Administrateur : gérer agences, véhicules, chauffeurs, types de voyage, tarifs, personnel, consulter la comptabilité
   - Agent guichet : vendre un billet, enregistrer un embarquement, encaisser un paiement, transférer de la caisse
   - Client (public) : rechercher un voyage, réserver, payer en mobile money, récupérer son billet
3. **Diagramme de classes (modèle de données)** — reprendre les tables du §5, organisées en quatre paquets logiques : Transport (agence/chauffeur/véhicule/voyage/passager), Billetterie/Réservation (billet/reservation/paiement/paiement_log), Comptabilité (compte/operation/caisse/journee_comptable/transfert_caisse), RH/Sécurité (personnel/utilisateur/groupe/privilege).
4. **Diagramme de séquence** — flux de réservation publique avec paiement Campay (détaillé au §3.12, point « Flux complet du client »).
5. **Diagramme de séquence** — flux de vente de billet en agence avec écriture comptable automatique (`Billet/Vendre` → API `BilletsController` → table `billet` → procédure stockée `sp_ecriture_vente_billet_auto` → table `operation`).
6. **Diagramme de déploiement** — montrer XAMPP (MySQL), les deux processus Kestrel (.NET) du frontend et de l'API, le navigateur client.

### 7.2 Angles d'analyse à valoriser dans le rapport

- **Choix architectural Dapper vs EF Core** : pourquoi Dapper a été privilégié (performance, contrôle total du SQL) malgré l'existence (inerte) d'EF Core — bon sujet de discussion technique.
- **Pattern Repository** : explication du découplage contrôleur/accès aux données, avec un exemple concret (`IVoyageRepository`/`VoyageRepository`).
- **Gestion de la concurrence et des conflits métier** : la détection de conflits d'horaire chauffeur/véhicule dans `VoyageRepository` est un bon exemple de règle métier non triviale implémentée en code (et non en contrainte SQL).
- **Sécurité** : le mécanisme d'en-têtes `X-Agence-Id`/`X-User-Id` au lieu de JWT, l'absence de `[Authorize]` sur `PersonnelController`, la politique CORS `AllowAll` — à mentionner brièvement comme perspectives d'amélioration plutôt que comme failles critiques, dans la partie « limites et perspectives » du rapport.
- **Module Comptabilité** : très bon sujet pour démontrer une compréhension fonctionnelle des principes de comptabilité générale (partie double, plan comptable OHADA/COBAC — référentiel comptable utilisé en Afrique francophone, pertinent si le stage se déroule au Cameroun ou en zone CEMAC/UEMOA).
- **Intégration de paiement mobile money** : bon sujet pour expliquer l'écosystème fintech local (Orange Money, MTN Mobile Money) et les défis d'intégration asynchrone (notifications, statuts intermédiaires, gestion des échecs de paiement) — sans nommer le prestataire technique précis (cf. note de rédaction §3.12).
- **Audit de code / dette technique** : le rapport peut valoriser un travail de relecture critique mené pendant le stage — identification puis **correction effective** d'un ancien module de plan comptable obsolète et de tables orphelines en base, démontrant une capacité à auditer et fiabiliser un projet existant, et pas seulement à y ajouter des fonctionnalités.

### 7.3 Glossaire métier utile pour le rapport

- **Agence** : succursale/point de vente de l'entreprise de transport
- **Voyage** : un trajet programmé (départ, arrivée, horaire, véhicule, chauffeur assignés)
- **Billet** : titre de transport individuel, lié à un passager et un voyage prévu
- **Embarquement** : action d'enregistrer un passager comme présent à bord pour un voyage donné
- **Affectation** : assignation d'une ressource (chauffeur ou véhicule) à une agence, ou d'un chauffeur à un voyage
- **Réservation** : pré-engagement client en ligne avant paiement, avec référence unique
- **Plan comptable OHADA/COBAC** : référentiel comptable normalisé utilisé en Afrique francophone (Organisation pour l'Harmonisation du Droit des Affaires en Afrique / Commission Bancaire de l'Afrique Centrale), structurant les comptes par classes (charges, produits, actifs, passifs)
- **Brouillard de caisse** : relevé chronologique de toutes les opérations de caisse sur une période, avant validation définitive
- **Transfert de caisse** : déplacement de fonds entre deux caisses (ex. caisse principale ↔ caisse secondaire)
- **Mobile Money** : service de paiement par téléphone mobile (Orange Money, MTN Mobile Money au Cameroun), intégré via un service de paiement dédié dans l'application

### 7.4 Limites connues du projet (section « Difficultés rencontrées / Limites » du rapport)

1. Authentification API par en-têtes non sécurisés, sans JWT — point à ne mentionner que brièvement comme perspective d'amélioration (l'API n'étant utilisée à ce jour que par le frontend de l'application, le risque réel est faible en l'état actuel d'usage).
2. `PersonnelController` potentiellement non protégé par autorisation.
3. Pas de tests automatisés détectés dans l'exploration (à confirmer, mais aucun projet de test n'a été mentionné par les agents d'exploration).
4. Fonctionnalité « balance client » en cours de finalisation (à terminer avant la soutenance).

> **Points déjà corrigés, à ne plus mentionner comme limites** : l'ancien module de plan comptable mort (`PLAN_COMPTABLE`) a été retiré du code, le système ne repose désormais que sur `compte` ; les tables orphelines (`HOTESSE`, `ASSIGNATION_HOTESSE_VOYAGE`, `REMBOURSEMENT`) et leurs scripts SQL ont été supprimés du projet ; le mot de passe administrateur par défaut a été changé ; le CORS permissif et l'incohérence de ports ne sont pas retenus comme points à développer dans le rapport (décision de rédaction).

> Note de rédaction : le module de paiement en ligne (mobile money) n'est **pas encore opérationnel en production** au moment de la rédaction de ce rapport. Sur recommandation de l'encadreur académique, **ce point ne sera pas détaillé dans le rapport** — le module de réservation sera présenté de façon générique (sans nommer le prestataire de paiement), et le statut « paiement non finalisé » ne sera pas mis en avant comme une limite du projet. Le compte administrateur par défaut a été reconfiguré (mot de passe changé) — ce point est donc retiré de la liste des limites.

### 7.5 Pistes d'évolution (section « Perspectives » du rapport)

- Finaliser la configuration du prestataire de paiement mobile money en environnement de production.
- Migrer l'authentification API vers un schéma à base de jetons (JWT) plutôt que des en-têtes non signés.
- Nettoyer le code mort restant identifié (services non utilisés, vues dupliquées).
- Finaliser l'implémentation de la balance client.
- Ajouter des tests automatisés (unitaires sur les repositories, tests d'intégration sur les contrôleurs API).

---

## 8. Récapitulatif des chemins de fichiers clés (pour référence rapide)

| Élément | Chemin |
|---|---|
| Solution | `AgenceVoyage.Front/pAgenceV/pAgenceV.sln` |
| Frontend — Program.cs | `AgenceVoyage.Front/pAgenceV/Program.cs` |
| Frontend — appsettings | `AgenceVoyage.Front/pAgenceV/appsettings.json` |
| Frontend — Controllers admin | `AgenceVoyage.Front/pAgenceV/Controllers/Parametre/` |
| Frontend — Controllers sécurité | `AgenceVoyage.Front/pAgenceV/Controllers/Securite/` |
| Frontend — Contrôleur public | `AgenceVoyage.Front/pAgenceV/Controllers/PublicController.cs` |
| Frontend — Contrôleur comptabilité | `AgenceVoyage.Front/pAgenceV/Controllers/ComptabiliteController.cs` |
| Frontend — Service de paiement mobile money | `AgenceVoyage.Front/pAgenceV/Services/CampayService.cs` |
| Frontend — Service Billet | `AgenceVoyage.Front/pAgenceV/Services/parametre/BilletService.cs` |
| Frontend — Génération PDF | `AgenceVoyage.Front/pAgenceV/Services/Documents/` |
| Frontend — Export Excel | `AgenceVoyage.Front/pAgenceV/Services/Excel/ExcelExportService.cs` |
| API — Program.cs | `AgenveVoyage.API/pAgenceAPI/Program.cs` |
| API — appsettings | `AgenveVoyage.API/pAgenceAPI/appsettings.json` |
| API — Models | `AgenveVoyage.API/pAgenceAPI/Models/` |
| API — Repositories | `AgenveVoyage.API/pAgenceAPI/Repositories/` |
| API — Controllers admin | `AgenveVoyage.API/pAgenceAPI/Controllers/parametres/` |
| API — Controllers personnel | `AgenveVoyage.API/pAgenceAPI/Controllers/personnel/` |
| API — Controllers sécurité | `AgenveVoyage.API/pAgenceAPI/Controllers/securite/` |
| Dump base de données | `backups/bd_agence_20260623_091835.sql` |
| Scripts SQL de migration | racine du dépôt + `sql_scripts/` (ex. `billet_tables.sql`, `reservation_tables.sql`, `migration_paiement.sql`, `sql_scripts/balance_generale.sql`, `sql_scripts/transfert_caisse.sql`, `sql_scripts/ecritures_comptables.sql`, `sql_scripts/ecritures_automatiques.sql`) |
| Documentation projet existante | `CLAUDE.md` (racine du dépôt) |

---

## 9. État d'avancement de la rédaction du rapport (à reprendre exactement ici)

Cette section recense ce qui a déjà été rédigé et validé avec l'étudiante, pour que toute nouvelle session reprenne sans rien refaire ni recommencer une discussion déjà tranchée.

### 9.1 Informations administratives validées

- **Étudiante** : YOUMBI FLORIDA
- **École** : ISTAG (Institut Supérieur de Technologie Appliquée et de Gestion), Licence Professionnelle en Informatique de Gestion (IG)
- **Entreprise d'accueil** : Fire Software (FS), Yaoundé, carrefour Intendance — créée en 2009, SARL, RC/Yaoundé/2009/708, capital 150 000 000 FCFA, devise « Rapidité – Efficacité »
- **Période de stage** : du 12 janvier au 12 juin 2025
- **Encadreur académique** : M. BEKECK MARTIN ROLAND
- **Encadreur professionnel** : M. YAMSI CHRISTIAN
- **Thème du rapport** : « Conception et développement d'une application de gestion d'agence de voyage »
- **Plan retenu** : 2 parties / 4 chapitres (Partie I : Chap 1 Présentation de Fire Software, Chap 2 Déroulement du stage ; Partie II : Chap 3 Conception et réalisation, Chap 4 Apports/limites/recommandations)
- **Méthodologie de modélisation** : UML (pas MERISE)

### 9.2 Règles de rédaction décidées avec l'étudiante (à respecter impérativement)

1. **Ne jamais nommer le prestataire de paiement mobile money** (anciennement nommé dans le code) — décrire seulement un « service/module de paiement mobile money » de façon générique. Ne pas présenter son état de configuration comme une limite.
2. **Ne pas mentionner le mot de passe administrateur par défaut** — déjà changé par l'étudiante.
3. **Ne pas mentionner le CORS permissif ni l'incohérence de ports** (jugés sans intérêt pour la soutenance).
4. **JWT/authentification par en-têtes** : à mentionner seulement brièvement comme perspective d'amélioration, sans en faire un point critique (l'API n'étant utilisée que par le frontend de l'app à ce jour).
5. Les sous-titres de méthodologie de rédaction (Contexte / Présentation du stage / Problématique / Objectifs / Méthodologie / Annonce du plan) **ne doivent jamais apparaître comme titres visibles** dans les parties rédigées (avant-propos, introduction) — ce sont des guides de contenu uniquement ; le texte final s'écrit en paragraphes continus.
6. Préférer des textes **concis** (l'étudiante a demandé à plusieurs reprises de réduire la longueur) plutôt que des développements longs.
7. La liste des sigles/abréviations et la liste des tableaux **se complètent en tout dernier**, une fois le rapport terminé.

### 9.3 Corrections de code déjà réalisées pendant cette session (effectives, pas seulement documentées)

- Suppression du module mort `PLAN_COMPTABLE` (modèle, repository, interface, contrôleur API), de son enregistrement DI et du `DROP TABLE` au démarrage dans `Program.cs` (API). Le plan comptable repose désormais uniquement sur `compte`. Build vérifié sans erreur de compilation liée à ce changement.
- Suppression des fichiers SQL orphelins `hotesse_tables.sql` et `migration_remboursements.sql` (tables `HOTESSE`, `ASSIGNATION_HOTESSE_VOYAGE`, `REMBOURSEMENT`) — confirmé qu'aucune de ces tables n'existe dans la base MySQL réelle (`bd_agence`), donc aucun `DROP TABLE` nécessaire côté base.
- **Reste à faire par l'étudiante avant la soutenance** : implémenter réellement la fonctionnalité « balance client » (liste des clients ayant effectué des opérations sur une période donnée, colonne débit toujours égale à 0).

### 9.4 Sections du rapport déjà rédigées et validées (texte final, prêt à copier dans Word)

Le contenu complet de chaque section ci-dessous a été rédigé en français dans la conversation précédente et validé par l'étudiante. S'il faut reprendre le travail, **redemander le texte exact à l'étudiante si elle l'a déjà copié dans son Word**, ou reconstruire en respectant strictement les règles du §9.2. Sections déjà traitées, dans l'ordre :

1. **Avant-propos** — rédigé (deux versions proposées, l'étudiante a choisi/adapté).
2. **Résumé** — rédigé en français, structuré comme : contexte du stage chez Fire Software, mission confiée, liste des modules (agences/chauffeurs/véhicules/voyages/passagers/embarquements, comptabilité et caisse avec plan comptable/opérations/journée comptable/transfert de caisse/brouillard de caisse/balance générale/balance client, **module de réservation et paiement en ligne** avec recherche de voyage, réservation, paiement mobile money générique, billet PDF téléchargeable, consultation par référence), stack technique (ASP.NET Core 8.0, REST API + Dapper + Repository pattern côté backend, MVC côté frontend, MySQL, UML pour la modélisation).
3. **Abstract** — traduction anglaise fidèle du résumé, même structure.
4. **Sigles et abréviations** — liste complète déjà fournie par l'étudiante (API, ASP, CO, CRUD, DDD, DDF, EL, FS, HTTP, ISTAG, JSON, LMD, MCD, MCT, ME, MERISE, MOT, MVC, MVT, MySQL, OI, PAMC, REST, RO, SGBD, SI, SIG, SIT, SQL, UML, URL, XAMP) — **à ne PAS rédiger en premier dans le document final, mais à compléter en dernier** une fois tout le reste écrit (décision explicite de l'étudiante).
5. **Liste des tableaux** — pas encore commencée, **à faire en tout dernier** (décision explicite de l'étudiante).
6. **Introduction générale** — rédigée en paragraphes continus (sans sous-titres visibles), couvrant : contexte de digitalisation, présentation du stage chez Fire Software (dates, encadreurs), problématique (gestion manuelle sur registres/Excel), objectifs, méthodologie (UML + ASP.NET Core 8.0 + Dapper + MySQL), annonce du plan en 2 parties/4 chapitres.
7. **Chapitre 1 / Section 1 : Présentation générale de l'entreprise** — rédigée avec exactement 4 sous-titres dans cet ordre : **1. Historique** (création 2009, 4 ingénieurs fondateurs, SARL, RC/Yaoundé/2009/708, capital 150M FCFA, devise) ; **2. Missions et activités** (formation, maintenance, développement logiciel/PME-PMI, SIG, déploiement de solutions, réseaux d'entreprise, audit SI, sites web) ; **3. Produits et services** (infrastructures : salle de formation climatisée + salle de développement ; ressources humaines : ~8 personnels qualifiés + réseau d'experts certifiés) ; **4. Environnement (marché, clients)** (PME/PMI, institutions de formation, organisations SIG ; valeurs : respect, recherche de la perfection, exemplarité).

### 9.5 Prochaine étape immédiate

Au moment de l'interruption de cette session (fin d'abonnement), la prochaine étape annoncée à l'étudiante était : **Chapitre 1 / Section 2 : Organisation et système d'information de Fire Software** (organigramme, description des services, présentation du système d'information existant, infrastructure informatique). L'étudiante n'avait pas encore fourni les informations sur l'organigramme/les infrastructures de Fire Software — **commencer la prochaine session en lui redemandant ces informations** si elle ne les a pas déjà transmises.

---

*Document généré par exploration directe du code source à des fins de préparation d'un rapport de stage de licence, puis enrichi et corrigé en concertation avec l'étudiante au fil de la rédaction. Toutes les décisions de rédaction (§9.2) doivent être respectées sans qu'il soit nécessaire de les renégocier avec l'étudiante.*
