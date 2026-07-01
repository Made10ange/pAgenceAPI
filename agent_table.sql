-- Table des agents (utilisateurs back-office)
-- Exécuter ce script dans MySQL, puis démarrer l'API :
-- l'admin par défaut sera créé automatiquement avec le mot de passe Admin@2025

CREATE TABLE IF NOT EXISTS AGENT (
    ID_AGENT        INT AUTO_INCREMENT PRIMARY KEY,
    Nom             VARCHAR(100) NOT NULL,
    Prenom          VARCHAR(100) NOT NULL,
    Login           VARCHAR(100) NOT NULL UNIQUE,
    MotDePasse      VARCHAR(255) NOT NULL,
    Role            VARCHAR(50)  NOT NULL DEFAULT 'Agent',
    Actif           TINYINT(1)   NOT NULL DEFAULT 1,
    Date_Creation   DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
);
