-- ═══════════════════════════════════════════════════════════
--  GESTION DES TARIFS
--  À exécuter dans la base bd_agence
-- ═══════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS TARIF (
    ID_TARIF        INT AUTO_INCREMENT PRIMARY KEY,
    LIBELLE         VARCHAR(150) NOT NULL,
    ID_TYPE_VOYAGE  INT NULL,
    POINT_DEPART    VARCHAR(150) NULL,
    POINT_ARRIVEE   VARCHAR(150) NULL,
    TYPE_PASSAGER   VARCHAR(50)  NOT NULL DEFAULT 'Adulte', -- Adulte | Enfant | VIP | Étudiant
    PRIX            DECIMAL(10,2) NOT NULL DEFAULT 0,
    ACTIF           TINYINT(1) NOT NULL DEFAULT 1,
    Date_Creation   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ID_TYPE_VOYAGE) REFERENCES TYPE_VOYAGE(ID_TYPE_VOYAGE) ON DELETE SET NULL
);

-- Quelques tarifs d'exemple
INSERT IGNORE INTO TARIF (LIBELLE, TYPE_PASSAGER, PRIX) VALUES
('Tarif standard adulte', 'Adulte',  5000),
('Tarif enfant (-12 ans)', 'Enfant', 2500),
('Tarif VIP',              'VIP',   12000),
('Tarif étudiant',         'Étudiant', 3500);
