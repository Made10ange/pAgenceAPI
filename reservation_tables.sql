-- ============================================================
--  SYSTÈME DE RÉSERVATION EN LIGNE - bd_agence
--  À exécuter dans MySQL Workbench / phpMyAdmin / DBeaver
-- ============================================================

USE bd_agence;

-- ------------------------------------------------------------
-- TABLE 1 : RESERVATION
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS RESERVATION (
    ID_RESERVATION         INT AUTO_INCREMENT PRIMARY KEY,
    REFERENCE              VARCHAR(30)   NOT NULL UNIQUE,
    ID_VOYAGE              INT           NOT NULL,
    ID_PASSAGER            INT           NULL,
    NOM_CLIENT             VARCHAR(100)  NOT NULL,
    PRENOM_CLIENT          VARCHAR(100)  NOT NULL,
    TELEPHONE_CLIENT       VARCHAR(20)   NOT NULL,
    NUMERO_CNI_CLIENT      VARCHAR(30)   NOT NULL,
    EMAIL_CLIENT           VARCHAR(150)  NULL,
    NUMERO_SIEGE           INT           NULL,
    MONTANT                DECIMAL(10,2) NOT NULL,
    STATUT_PAIEMENT        ENUM('En attente','Payé','Échoué','Remboursé') NOT NULL DEFAULT 'En attente',
    PROVIDER_PAIEMENT      VARCHAR(50)   NULL,
    REFERENCE_PAIEMENT     VARCHAR(100)  NULL,
    DATE_PAIEMENT          DATETIME      NULL,
    STATUT_RESERVATION     ENUM('En attente','Confirmée','Annulée','Utilisée') NOT NULL DEFAULT 'En attente',
    VALIDEE_PAR            VARCHAR(100)  NULL,
    DATE_VALIDATION        DATETIME      NULL,
    DATE_CREATION          DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DATE_MODIFICATION      DATETIME      NULL ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_reservation_voyage
        FOREIGN KEY (ID_VOYAGE) REFERENCES VOYAGE(ID_VOYAGE)
        ON DELETE RESTRICT ON UPDATE CASCADE,

    CONSTRAINT fk_reservation_passager
        FOREIGN KEY (ID_PASSAGER) REFERENCES PASSAGER(ID_PASSAGER)
        ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ------------------------------------------------------------
-- TABLE 2 : PAIEMENT_LOG
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS PAIEMENT_LOG (
    ID_LOG                 INT AUTO_INCREMENT PRIMARY KEY,
    ID_RESERVATION         INT          NOT NULL,
    EVENEMENT              VARCHAR(50)  NOT NULL,
    MONTANT                DECIMAL(10,2) NULL,
    REFERENCE_EXTERNE      VARCHAR(100) NULL,
    PAYLOAD_BRUT           TEXT         NULL,
    DATE_EVENEMENT         DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_log_reservation
        FOREIGN KEY (ID_RESERVATION) REFERENCES RESERVATION(ID_RESERVATION)
        ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ------------------------------------------------------------
-- MISE A JOUR (si la table RESERVATION existe déjà sans cette colonne)
-- ------------------------------------------------------------
ALTER TABLE RESERVATION ADD COLUMN IF NOT EXISTS NUMERO_CNI_CLIENT VARCHAR(30) NOT NULL DEFAULT '' AFTER TELEPHONE_CLIENT;

-- ------------------------------------------------------------
-- INDEX
-- ------------------------------------------------------------
CREATE INDEX idx_reservation_reference  ON RESERVATION (REFERENCE);
CREATE INDEX idx_reservation_voyage     ON RESERVATION (ID_VOYAGE);
CREATE INDEX idx_reservation_telephone  ON RESERVATION (TELEPHONE_CLIENT);
CREATE INDEX idx_reservation_statut     ON RESERVATION (STATUT_RESERVATION, STATUT_PAIEMENT);

-- ------------------------------------------------------------
-- Vérification
-- ------------------------------------------------------------
SELECT 'Tables créées avec succès !' AS message;
SHOW TABLES LIKE '%RESERVATION%';
SHOW TABLES LIKE '%PAIEMENT%';
