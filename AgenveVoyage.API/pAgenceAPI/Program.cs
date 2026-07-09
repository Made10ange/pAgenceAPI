using Microsoft.EntityFrameworkCore;
using pAgenceAPI;
using pAgenceAPI.Repositories;
using Pomelo.EntityFrameworkCore.MySql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // ✅ Utiliser PascalCase pour correspondre aux modèles C#
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        // ✅ Ajouter conversor TimeSpan
        options.JsonSerializerOptions.Converters.Add(new pAgenceAPI.TimeSpanConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ... reste du code

// Configuration MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 32))
    )
);

// Configuration CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// 🔧 Enregistrer les repositories
builder.Services.AddScoped<IAgenceRepository, AgenceRepository>();
builder.Services.AddScoped<IChauffeurRepository, ChauffeurRepository>();
builder.Services.AddScoped<ITypeVehiculeRepository, TypeVehiculeRepository>();  
builder.Services.AddScoped<IVehiculeRepository, VehiculeRepository>();
builder.Services.AddScoped<IHistoriqueEtatVehiculeRepository, HistoriqueEtatVehiculeRepository>();
builder.Services.AddScoped<ITypeVoyageRepository, TypeVoyageRepository>();  
builder.Services.AddScoped<IPassagerRepository, PassagerRepository>();
builder.Services.AddScoped<IVoyageRepository, VoyageRepository>();  // ← AJOUTÉ ICI
builder.Services.AddScoped<IAffectationChauffeurAgenceRepository, AffectationChauffeurAgenceRepository>();
builder.Services.AddScoped<IAffectationVehiculeAgenceRepository, AffectationVehiculeAgenceRepository>();
builder.Services.AddScoped<IAssignationChauffeurVoyageRepository, AssignationChauffeurVoyageRepository>();
builder.Services.AddScoped<IEmbarquementRepository, EmbarquementRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IBagageRepository, BagageRepository>();
builder.Services.AddScoped<IColisRepository, ColisRepository>();
builder.Services.AddScoped<IPaiementRepository, PaiementRepository>();
builder.Services.AddScoped<IUtilisateurRepository, UtilisateurRepository>();
builder.Services.AddScoped<IGroupeRepository, GroupeRepository>();
builder.Services.AddScoped<IPrivilegeRepository, PrivilegeRepository>();
builder.Services.AddScoped<IJournalAuditRepository, JournalAuditRepository>();
builder.Services.AddScoped<IHistoriqueConnexionRepository, HistoriqueConnexionRepository>();
builder.Services.AddScoped<IPosteRepository, PosteRepository>();
builder.Services.AddScoped<IPersonnelRepository, PersonnelRepository>();
builder.Services.AddScoped<IFichePayeRepository, FichePayeRepository>();
builder.Services.AddScoped<ITarifRepository, TarifRepository>();
builder.Services.AddScoped<IBilletRepository, BilletRepository>();
builder.Services.AddScoped<ICompteRepository, CompteRepository>();
builder.Services.AddScoped<ICaisseRepository, CaisseRepository>();
builder.Services.AddScoped<ICaissierRepository, CaissierRepository>();
builder.Services.AddScoped<IEcritureRepository, EcritureRepository>();
builder.Services.AddScoped<ITransfertCaisseRepository, TransfertCaisseRepository>();
builder.Services.AddScoped<IBalanceRepository, BalanceRepository>();
builder.Services.AddSingleton<pAgenceAPI.Services.BackupService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<pAgenceAPI.Services.BackupService>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
// app.UseHttpsRedirection(); // désactivé : cause un 307 sans header CORS depuis le frontend HTTPS
app.UseAuthorization();
app.MapControllers();

// Rendre ID_VOYAGE nullable dans COLIS (migration one-shot)
try
{
    var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
    using var conn = new MySqlConnector.MySqlConnection(connStr);
    await conn.OpenAsync();
    await new MySqlConnector.MySqlCommand(
        "ALTER TABLE COLIS MODIFY COLUMN ID_VOYAGE INT NULL;", conn).ExecuteNonQueryAsync();
}
catch { /* déjà nullable ou table absente — pas grave */ }

// Renommer "En transit" → "En cours" dans BAGAGE
try
{
    var connStrB = builder.Configuration.GetConnectionString("DefaultConnection");
    using var connB = new MySqlConnector.MySqlConnection(connStrB);
    await connB.OpenAsync();
    await new MySqlConnector.MySqlCommand(
        "UPDATE BAGAGE SET STATUT = 'En cours' WHERE STATUT = 'En transit';", connB).ExecuteNonQueryAsync();
}
catch { }

// Rendre HEURE_DEPART et DATE_ARRIVEE nullables dans VOYAGE
try
{
    var connStrV = builder.Configuration.GetConnectionString("DefaultConnection");
    using var connV = new MySqlConnector.MySqlConnection(connStrV);
    await connV.OpenAsync();
    await new MySqlConnector.MySqlCommand(
        "ALTER TABLE VOYAGE MODIFY COLUMN HEURE_DEPART TIME NULL; ALTER TABLE VOYAGE MODIFY COLUMN DATE_ARRIVEE DATE NULL;",
        connV).ExecuteNonQueryAsync();
}
catch { /* déjà nullable — pas grave */ }

// Créer les tables groupe, utilisateur_groupe, privilege si elles n'existent pas
try
{
    var connStrG = builder.Configuration.GetConnectionString("DefaultConnection");
    using var connG = new MySqlConnector.MySqlConnection(connStrG);
    await connG.OpenAsync();
    await new MySqlConnector.MySqlCommand(@"
        CREATE TABLE IF NOT EXISTS groupe (
            ID_groupe INT AUTO_INCREMENT PRIMARY KEY,
            Libelle VARCHAR(100) NOT NULL,
            Description VARCHAR(255) NULL,
            Couleur VARCHAR(20) DEFAULT '#7C3AED',
            Actif TINYINT(1) DEFAULT 1,
            Date_Creation DATETIME DEFAULT CURRENT_TIMESTAMP
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

        CREATE TABLE IF NOT EXISTS utilisateur_groupe (
            Id_Utilisateur INT NOT NULL,
            ID_groupe INT NOT NULL,
            Date_Affectation DATETIME DEFAULT CURRENT_TIMESTAMP,
            PRIMARY KEY (Id_Utilisateur, ID_groupe)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

        CREATE TABLE IF NOT EXISTS privilege (
            ID_privilege INT AUTO_INCREMENT PRIMARY KEY,
            ID_groupe INT NOT NULL,
            Module VARCHAR(50) NOT NULL,
            Action VARCHAR(50) NOT NULL,
            Autorise TINYINT(1) DEFAULT 1
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;", connG).ExecuteNonQueryAsync();
}
catch { }

// Ajouter SEXE_CLIENT dans la table reservation (migration one-shot)
try
{
    var connStrSexe = builder.Configuration.GetConnectionString("DefaultConnection");
    using var connSexe = new MySqlConnector.MySqlConnection(connStrSexe);
    await connSexe.OpenAsync();
    await new MySqlConnector.MySqlCommand(
        "ALTER TABLE reservation ADD COLUMN SEXE_CLIENT VARCHAR(20) DEFAULT 'Non précisé';",
        connSexe).ExecuteNonQueryAsync();
}
catch { }

// Créer l'admin par défaut si aucun agent n'existe
using (var scope = app.Services.CreateScope())
{
    var utilisateurRepo = scope.ServiceProvider.GetRequiredService<IUtilisateurRepository>();
    try
    {
        if (!await utilisateurRepo.ExisteAsync())
            await utilisateurRepo.CreerAdminParDefautAsync();
    }
    catch { /* table pas encore créée — pas grave */ }
}

app.Run();

