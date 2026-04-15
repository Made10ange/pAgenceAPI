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
builder.Services.AddScoped<ITypeVoyageRepository, TypeVoyageRepository>();  
builder.Services.AddScoped<IPassagerRepository, PassagerRepository>();
builder.Services.AddScoped<IVoyageRepository, VoyageRepository>();  // ← AJOUTÉ ICI
builder.Services.AddScoped<IAffectationChauffeurAgenceRepository, AffectationChauffeurAgenceRepository>();
builder.Services.AddScoped<IAffectationVehiculeAgenceRepository, AffectationVehiculeAgenceRepository>();
builder.Services.AddScoped<IAssignationChauffeurVoyageRepository, AssignationChauffeurVoyageRepository>();
builder.Services.AddScoped<IEmbarquementRepository, EmbarquementRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();