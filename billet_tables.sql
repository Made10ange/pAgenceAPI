-- ============================================================
-- MODULE BILLET — Titre de transport avec validité 6 mois
-- À exécuter dans la base bd_agence
-- ============================================================

CREATE TABLE IF NOT EXISTS BILLET (
    Id_Billet           INT          NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Numero_Billet       VARCHAR(20)  NOT NULL UNIQUE,          -- Ex: BIL-20260427-0001
    Id_Passager         INT          NOT NULL,
    Point_Depart        VARCHAR(150) NOT NULL,
    Point_Arrivee       VARCHAR(150) NOT NULL,
    Id_Type_Voyage      INT          NULL,                     -- Filtre compatibilité voyages
    Montant             DECIMAL(12,2) NOT NULL DEFAULT 0,
    Date_Achat          DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Date_Validite       DATETIME     NOT NULL,                 -- Date_Achat + 6 mois
    Statut              ENUM('Valide','Utilisé','Expiré','Reporté','Annulé') NOT NULL DEFAULT 'Valide',
    Id_Voyage_Prevu     INT          NULL,                     -- Voyage souhaité à l'achat
    Id_Voyage_Utilise   INT          NULL,                     -- Voyage sur lequel il a embarqué
    Mode_Paiement       VARCHAR(50)  NOT NULL DEFAULT 'Espèces',
    Vendu_Par           VARCHAR(100) NULL,                     -- Nom de l'agent caissier
    Notes               TEXT         NULL,
    Date_Creation       DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Date_Modification   DATETIME     NULL ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_billet_passager    FOREIGN KEY (Id_Passager)       REFERENCES PASSAGER(Id_Passager),
    CONSTRAINT fk_billet_typevoyage  FOREIGN KEY (Id_Type_Voyage)    REFERENCES TYPE_VOYAGE(Id_Type_Voyage),
    CONSTRAINT fk_billet_voyage_prev FOREIGN KEY (Id_Voyage_Prevu)   REFERENCES VOYAGE(Id_Voyage),
    CONSTRAINT fk_billet_voyage_util FOREIGN KEY (Id_Voyage_Utilise) REFERENCES VOYAGE(Id_Voyage)
);

-- Index pour les recherches fréquentes
CREATE INDEX IF NOT EXISTS idx_billet_passager  ON BILLET (Id_Passager);
CREATE INDEX IF NOT EXISTS idx_billet_statut    ON BILLET (Statut);
CREATE INDEX IF NOT EXISTS idx_billet_validite  ON BILLET (Date_Validite);
CREATE INDEX IF NOT EXISTS idx_billet_numero    ON BILLET (Numero_Billet);

-- ============================================================
-- Procédure : expirer automatiquement les billets dépassés
-- Appeler via un event scheduler ou manuellement
-- ============================================================
DROP PROCEDURE IF EXISTS ExpirerBillets;

DELIMITER //
CREATE PROCEDURE ExpirerBillets()
BEGIN
    UPDATE BILLET
    SET    Statut = 'Expiré'
    WHERE  Statut = 'Valide'
      AND  Date_Validite < NOW();
    SELECT ROW_COUNT() AS BilletsExpires;
END //
DELIMITER ;

-- Pour activer l'event scheduler (une seule fois sur le serveur) :
-- SET GLOBAL event_scheduler = ON;

-- Event automatique chaque nuit à minuit :
DROP EVENT IF EXISTS evt_expirer_billets;
CREATE EVENT evt_expirer_billets
    ON SCHEDULE EVERY 1 DAY STARTS (CURRENT_DATE + INTERVAL 1 DAY)
    DO CALL ExpirerBillets();
