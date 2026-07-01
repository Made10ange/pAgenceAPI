-- ═══════════════════════════════════════
--  MODULE GESTION DU PERSONNEL
-- ═══════════════════════════════════════

-- Table des postes / fonctions
CREATE TABLE IF NOT EXISTS POSTE (
    ID_POSTE    INT AUTO_INCREMENT PRIMARY KEY,
    Libelle     VARCHAR(100) NOT NULL,
    Description VARCHAR(255)
);

INSERT INTO POSTE (Libelle) VALUES
('Chauffeur'), ('Caissière'), ('Contrôleur de billets'),
('Gestionnaire'), ('Agent d''accueil'), ('Manutentionnaire'), ('Autre');

-- Table principale du personnel
CREATE TABLE IF NOT EXISTS PERSONNEL (
    ID_PERSONNEL    INT AUTO_INCREMENT PRIMARY KEY,
    Nom             VARCHAR(100) NOT NULL,
    Prenom          VARCHAR(100) NOT NULL,
    Telephone       VARCHAR(20),
    Email           VARCHAR(150),
    ID_POSTE        INT NOT NULL,
    Type_Contrat    ENUM('CDI','CDD','Journalier') NOT NULL DEFAULT 'CDI',
    Salaire_Base    DECIMAL(12,2) NOT NULL DEFAULT 0,
    Date_Embauche   DATE NOT NULL,
    Statut          ENUM('Actif','Inactif','Suspendu') NOT NULL DEFAULT 'Actif',
    Notes           TEXT,
    Date_Creation   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ID_POSTE) REFERENCES POSTE(ID_POSTE)
);

-- Fiches de paie mensuelles
CREATE TABLE IF NOT EXISTS FICHE_PAIE (
    ID_FICHE        INT AUTO_INCREMENT PRIMARY KEY,
    ID_PERSONNEL    INT NOT NULL,
    Mois            TINYINT NOT NULL,   -- 1 à 12
    Annee           SMALLINT NOT NULL,
    Salaire_Base    DECIMAL(12,2) NOT NULL,
    Primes          DECIMAL(12,2) NOT NULL DEFAULT 0,
    Deductions      DECIMAL(12,2) NOT NULL DEFAULT 0,
    Net_A_Payer     DECIMAL(12,2) NOT NULL,
    Statut          ENUM('En attente','Payé') NOT NULL DEFAULT 'En attente',
    Date_Paiement   DATETIME,
    Note            VARCHAR(255),
    Date_Creation   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ID_PERSONNEL) REFERENCES PERSONNEL(ID_PERSONNEL),
    UNIQUE KEY uq_fiche (ID_PERSONNEL, Mois, Annee)
);

-- Dépenses de l'entreprise
CREATE TABLE IF NOT EXISTS DEPENSE (
    ID_DEPENSE      INT AUTO_INCREMENT PRIMARY KEY,
    Categorie       VARCHAR(100) NOT NULL,  -- 'Salaires', 'Carburant', 'Loyer'...
    Description     VARCHAR(255) NOT NULL,
    Montant         DECIMAL(12,2) NOT NULL,
    Date_Depense    DATE NOT NULL,
    ID_FICHE        INT,                    -- lien optionnel vers fiche de paie
    Date_Creation   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ID_FICHE) REFERENCES FICHE_PAIE(ID_FICHE)
);
