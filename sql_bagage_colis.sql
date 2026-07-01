-- ═══════════════════════════════════════════════════════
--  pAgenceV — Tables BAGAGE & COLIS
--  À exécuter dans la base bd_agence
-- ═══════════════════════════════════════════════════════

-- ── TABLE BAGAGE ──────────────────────────────────────
CREATE TABLE IF NOT EXISTS BAGAGE (
    ID_BAGAGE            INT          NOT NULL AUTO_INCREMENT,
    ID_PASSAGER          INT          NOT NULL,
    ID_VOYAGE_PASSAGER   INT          NOT NULL,          -- bus du passager
    ID_VOYAGE_BAGAGE     INT          NOT NULL,          -- bus du bagage (peut être différent)
    DESCRIPTION          VARCHAR(255) NULL,
    POIDS                DECIMAL(8,2) NULL,              -- en kg
    STATUT               VARCHAR(50)  NOT NULL DEFAULT 'En attente',
                                                        -- En attente | Chargé | En transit | Livré | Perdu
    DATE_ENREGISTREMENT  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (ID_BAGAGE),
    CONSTRAINT fk_bagage_passager        FOREIGN KEY (ID_PASSAGER)        REFERENCES PASSAGER (ID_PASSAGER),
    CONSTRAINT fk_bagage_voyage_passager FOREIGN KEY (ID_VOYAGE_PASSAGER) REFERENCES VOYAGE   (ID_VOYAGE),
    CONSTRAINT fk_bagage_voyage_bagage   FOREIGN KEY (ID_VOYAGE_BAGAGE)   REFERENCES VOYAGE   (ID_VOYAGE)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ── TABLE COLIS ───────────────────────────────────────
CREATE TABLE IF NOT EXISTS COLIS (
    ID_COLIS                  INT           NOT NULL AUTO_INCREMENT,
    REFERENCE_COLIS           VARCHAR(30)   NOT NULL UNIQUE,   -- ex: COL-20260421-0001
    ID_VOYAGE                 INT           NOT NULL,
    ID_AGENCE                 INT           NULL,
    NOM_EXPEDITEUR            VARCHAR(150)  NOT NULL,
    TEL_EXPEDITEUR            VARCHAR(20)   NULL,
    NOM_DESTINATAIRE          VARCHAR(150)  NOT NULL,
    TEL_DESTINATAIRE          VARCHAR(20)   NULL,
    VILLE_DEPART              VARCHAR(100)  NOT NULL,
    VILLE_ARRIVEE             VARCHAR(100)  NOT NULL,
    DESCRIPTION               VARCHAR(255)  NULL,
    POIDS                     DECIMAL(8,2)  NULL,
    VALEUR_DECLAREE           DECIMAL(12,2) NULL DEFAULT 0,
    PRIX_TRANSPORT            DECIMAL(12,2) NULL DEFAULT 0,
    STATUT                    VARCHAR(50)   NOT NULL DEFAULT 'En attente',
                                                              -- En attente | En transit | Livré | Retourné
    DATE_ENVOI                DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DATE_LIVRAISON_PREVUE     DATE          NULL,
    DATE_LIVRAISON_EFFECTIVE  DATE          NULL,
    PRIMARY KEY (ID_COLIS),
    CONSTRAINT fk_colis_voyage FOREIGN KEY (ID_VOYAGE) REFERENCES VOYAGE (ID_VOYAGE),
    CONSTRAINT fk_colis_agence FOREIGN KEY (ID_AGENCE) REFERENCES AGENCE (ID_AGENCE)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
