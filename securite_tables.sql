-- ═══════════════════════════════════════
--  MODULE SÉCURITÉ
-- ═══════════════════════════════════════

-- Table des groupes / profils d'accès
CREATE TABLE IF NOT EXISTS GROUPE (
    ID_GROUPE       INT AUTO_INCREMENT PRIMARY KEY,
    Libelle         VARCHAR(100) NOT NULL,
    Description     VARCHAR(255),
    Couleur         VARCHAR(20)  NOT NULL DEFAULT '#7C3AED',
    Actif           TINYINT(1)   NOT NULL DEFAULT 1,
    Date_Creation   DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Agents affectés à un groupe
CREATE TABLE IF NOT EXISTS AGENT_GROUPE (
    ID_AGENT        INT NOT NULL,
    ID_GROUPE       INT NOT NULL,
    Date_Affectation DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (ID_AGENT, ID_GROUPE),
    FOREIGN KEY (ID_AGENT)  REFERENCES AGENT(Id_Agent)  ON DELETE CASCADE,
    FOREIGN KEY (ID_GROUPE) REFERENCES GROUPE(ID_GROUPE) ON DELETE CASCADE
);

-- Privilèges par groupe (module × action)
CREATE TABLE IF NOT EXISTS PRIVILEGE (
    ID_PRIVILEGE    INT AUTO_INCREMENT PRIMARY KEY,
    ID_GROUPE       INT NOT NULL,
    Module          VARCHAR(100) NOT NULL,
    Action          VARCHAR(50)  NOT NULL,
    Autorise        TINYINT(1)   NOT NULL DEFAULT 1,
    UNIQUE KEY uq_priv (ID_GROUPE, Module, Action),
    FOREIGN KEY (ID_GROUPE) REFERENCES GROUPE(ID_GROUPE) ON DELETE CASCADE
);

-- Journal d'audit (mouchard) — toutes les actions du système
CREATE TABLE IF NOT EXISTS JOURNAL_AUDIT (
    ID_JOURNAL      INT AUTO_INCREMENT PRIMARY KEY,
    ID_AGENT        INT,
    Login_Agent     VARCHAR(100),
    Nom_Agent       VARCHAR(200),
    Module          VARCHAR(100),
    Action          VARCHAR(100),
    Details         TEXT,
    Ancienne_Valeur TEXT,
    Nouvelle_Valeur TEXT,
    IP_Address      VARCHAR(45),
    User_Agent      VARCHAR(500),
    Statut          ENUM('Succès','Échec') NOT NULL DEFAULT 'Succès',
    Date_Action     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ID_AGENT) REFERENCES AGENT(Id_Agent) ON DELETE SET NULL
);

-- Historique des connexions
CREATE TABLE IF NOT EXISTS HISTORIQUE_CONNEXION (
    ID_CONNEXION    INT AUTO_INCREMENT PRIMARY KEY,
    ID_AGENT        INT,
    Login_Tente     VARCHAR(100) NOT NULL,
    Nom_Agent       VARCHAR(200),
    Statut          ENUM('Succès','Échec') NOT NULL DEFAULT 'Succès',
    Motif_Echec     VARCHAR(255),
    IP_Address      VARCHAR(45),
    User_Agent      VARCHAR(500),
    Date_Connexion  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ID_AGENT) REFERENCES AGENT(Id_Agent) ON DELETE SET NULL
);

-- Données initiales : groupe Admin par défaut
INSERT IGNORE INTO GROUPE (ID_GROUPE, Libelle, Description, Couleur) VALUES
(1, 'Administrateur', 'Accès total au système',  '#4C1D95'),
(2, 'Caissière',      'Gestion des paiements et opérations', '#0D9488'),
(3, 'Superviseur',    'Consultation et rapports uniquement', '#1D4ED8');
