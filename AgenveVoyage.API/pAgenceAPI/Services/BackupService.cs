using System.Diagnostics;

namespace pAgenceAPI.Services
{
    public class BackupService : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<BackupService> _logger;

        public BackupService(IConfiguration config, ILogger<BackupService> logger)
        {
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Sauvegarde au démarrage
            await FaireBackupAsync();

            // Ensuite toutes les 24h
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                await FaireBackupAsync();
            }
        }

        public async Task<(bool ok, string message, string? fichier)> FaireBackupAsync()
        {
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection") ?? "";
                var (server, db, user, pwd, port) = ParseConnString(connStr);

                var dossier = _config["Backup:Dossier"] ?? Path.Combine(AppContext.BaseDirectory, "Backups");
                Directory.CreateDirectory(dossier);

                // Garder seulement les 30 derniers fichiers
                var anciens = Directory.GetFiles(dossier, "*.sql")
                    .OrderByDescending(f => f)
                    .Skip(30).ToList();
                anciens.ForEach(File.Delete);

                var nomFichier = $"bd_agence_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
                var chemin     = Path.Combine(dossier, nomFichier);

                var mysqldump = _config["Backup:MysqldumpPath"] ?? @"C:\xampp\mysql\bin\mysqldump.exe";

                var args = $"-h {server} -P {port} -u {user}";
                if (!string.IsNullOrEmpty(pwd)) args += $" -p{pwd}";
                args += $" --single-transaction --routines --triggers {db}";

                var psi = new ProcessStartInfo
                {
                    FileName               = mysqldump,
                    Arguments              = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true
                };

                using var process = Process.Start(psi)!;
                var output = await process.StandardOutput.ReadToEndAsync();
                var error  = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    _logger.LogError("mysqldump erreur : {err}", error);
                    return (false, $"Erreur mysqldump : {error}", null);
                }

                await File.WriteAllTextAsync(chemin, output);
                _logger.LogInformation("Sauvegarde créée : {f}", chemin);
                return (true, "Sauvegarde réussie.", nomFichier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur sauvegarde");
                return (false, ex.Message, null);
            }
        }

        private (string server, string db, string user, string pwd, string port) ParseConnString(string cs)
        {
            var dict = cs.Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Split('=', 2))
                .Where(p => p.Length == 2)
                .ToDictionary(p => p[0].Trim().ToLower(), p => p[1].Trim());

            return (
                dict.GetValueOrDefault("server") ?? "localhost",
                dict.GetValueOrDefault("database") ?? "bd_agence",
                dict.GetValueOrDefault("uid") ?? "root",
                dict.GetValueOrDefault("pwd") ?? "",
                dict.GetValueOrDefault("port") ?? "3306"
            );
        }
    }
}
