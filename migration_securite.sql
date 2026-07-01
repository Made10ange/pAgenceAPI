-- =====================================================
--  MODULE SÉCURITÉ — pAgenceV
--  Tables : GROUPE, AGENT_GROUPE, PRIVILEGE,
--           JOURNAL_AUDIT, HISTORIQUE_CONNEXION
-- =====================================================

-- 1. Groupes d'utilisateurs
CREATE TABLE IF NOT EXISTS GROUPE (
    Id_Groupe       INT AUTO_INCREMENT PRIMARY KEY,
    Libelle         VARCHAR(100) NOT NULL,
    Description     VARCHAR(255),
    Couleur         VARCHAR(7) DEFAULT '#7C3AED',
    Actif           TINYINT(1) NOT NULL DEFAULT 1,
    Date_Creation   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2. Affectation agents ↔ groupes
CREATE TABLE IF NOT EXISTS AGENT_GROUPE (
    Id_Agent        INT NOT NULL,
    Id_Groupe       INT NOT NULL,
    Date_Affectation DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (Id_Agent, Id_Groupe),
    FOREIGN KEY (Id_Agent)  REFERENCES AGENT(Id_Agent)  ON DELETE CASCADE,
    FOREIGN KEY (Id_Groupe) REFERENCES GROUPE(Id_Groupe) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3. Privilèges par groupe (Module × Action)
CREATE TABLE IF NOT EXISTS PRIVILEGE (
    Id_Privilege    INT AUTO_INCREMENT PRIMARY KEY,
    Id_Groupe       INT NOT NULL,
    Module          VARCHAR(100) NOT NULL,   -- ex: Voyage, Passager, Colis...
    Action          VARCHAR(50)  NOT NULL,   -- Liste | Ajouter | Modifier | Supprimer | Exporter
    Autorise        TINYINT(1) NOT NULL DEFAULT 1,
    FOREIGN KEY (Id_Groupe) REFERENCES GROUPE(Id_Groupe) ON DELETE CASCADE,
    UNIQUE KEY uq_privilege (Id_Groupe, Module, Action)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 4. Journal d'audit (mouchard)
CREATE TABLE IF NOT EXISTS JOURNAL_AUDIT (
    Id_Journal      INT AUTO_INCREMENT PRIMARY KEY,
    Id_Agent        INT,
    Login_Agent     VARCHAR(100),
    Nom_Agent       VARCHAR(200),
    Module          VARCHAR(100),
    Action          VARCHAR(100),
    Details         TEXT,
    Ancienne_Valeur TEXT,
    Nouvelle_Valeur TEXT,
    IP_Address      VARCHAR(45),
    User_Agent      VARCHAR(500),
    Statut          VARCHAR(20) DEFAULT 'Succès',   -- Succès | Échec
    Date_Action     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_journal_agent  (Id_Agent),
    INDEX idx_journal_date   (Date_Action),
    INDEX idx_journal_module (Module)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5. Historique des connexions
CREATE TABLE IF NOT EXISTS HISTORIQUE_CONNEXION (
    Id_Connexion    INT AUTO_INCREMENT PRIMARY KEY,
    Id_Agent        INT,
    Login_Tente     VARCHAR(100) NOT NULL,
    Nom_Agent       VARCHAR(200),
    Statut          VARCHAR(20) NOT NULL DEFAULT 'Succès',  -- Succès | Échec
    Motif_Echec     VARCHAR(255),
    IP_Address      VARCHAR(45),
    User_Agent      VARCHAR(500),
    Date_Connexion  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_cnx_agent  (Id_Agent),
    INDEX idx_cnx_date   (Date_Connexion),
    INDEX idx_cnx_statut (Statut)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ── Données initiales ──────────────────────────────
INSERT IGNORE INTO GROUPE (Libelle, Description, Couleur)
VALUES
  ('Administrateurs', 'Accès complet à toutes les fonctionnalités', '#4C1D95'),
  ('Caissiers',       'Gestion des paiements et rapports financiers',  '#0D9488'),
  ('Agents',          'Gestion des opérations courantes',              '#6B7280');
