-- ============================================================
-- ÉCRITURES COMPTABLES — Procédures stockées
-- Utilise les tables existantes : operation, journee_comptable,
-- journal, compte
-- À exécuter dans bd_agence (MySQL/MariaDB)
-- ============================================================

-- ── Supprimer les tables temporaires créées par erreur ───────
DROP TABLE IF EXISTS LIGNE_ECRITURE;
DROP TABLE IF EXISTS ECRITURE_COMPTABLE;

-- ── Ajouter le journal des opérations manuelles si absent ────
INSERT IGNORE INTO journal (code_journal, libelle)
VALUES ('JMAN', 'Journal des opérations manuelles');

-- ============================================================
-- PROCÉDURE 1 : Enregistrer UNE ligne comptable dans `operation`
--   (une ligne = un compte + montant débit OU crédit)
--   Vérifie que la journée comptable est ouverte.
--   Met à jour le solde du compte.
-- ============================================================
DROP PROCEDURE IF EXISTS sp_enregistrer_operation;

DELIMITER $$
CREATE PROCEDURE sp_enregistrer_operation(
    IN  p_num_transaction VARCHAR(20),   -- identifiant groupant les lignes d'une même écriture
    IN  p_date            DATETIME,
    IN  p_numcompte       VARCHAR(30),   -- compte touché
    IN  p_debit           DECIMAL(15,2), -- montant débit (0 si crédit)
    IN  p_credit          DECIMAL(15,2), -- montant crédit (0 si débit)
    IN  p_libelle         VARCHAR(200),
    IN  p_code_journal    VARCHAR(10),   -- ex: 'JMAN', 'JDIV'
    IN  p_code_agence     INT,
    IN  p_code_user       INT,
    OUT p_code_operation  INT            -- id de la ligne insérée
)
BEGIN
    DECLARE v_code_journee INT DEFAULT NULL;

    -- ── 1. Vérifier qu'une journée comptable est ouverte aujourd'hui pour cette agence ──
    SELECT code_journee INTO v_code_journee
    FROM   journee_comptable
    WHERE  DATE(date_journee) = DATE(p_date)
      AND  code_agence = p_code_agence
      AND  statut = 'Ouverte'
    LIMIT  1;

    IF v_code_journee IS NULL THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Aucune journée comptable ouverte pour cette date dans cette agence. Ouvrez la journée avant de saisir des opérations.';
    END IF;

    -- ── 2. Insérer la ligne dans `operation` ──────────────────────────
    INSERT INTO operation
        (date_operation, numcompte, debit, credit, libelle,
         num_transaction, code_agence, code_user, valide)
    VALUES
        (p_date, p_numcompte, p_debit, p_credit, p_libelle,
         p_num_transaction, p_code_agence, p_code_user, 1);

    SET p_code_operation = LAST_INSERT_ID();

    -- ── 3. Mettre à jour le solde du compte ───────────────────────────
    UPDATE compte
    SET
        cumul_debit  = cumul_debit  + p_debit,
        cumul_credit = cumul_credit + p_credit,
        solde        = CASE
                           WHEN sens = 'C'  THEN solde + p_credit - p_debit
                           ELSE                  solde + p_debit  - p_credit
                       END
    WHERE numcompte = p_numcompte;
END$$
DELIMITER ;

-- ============================================================
-- PROCÉDURE 2 et 3 (sp_ouvrir_journee / sp_cloturer_journee) :
-- DÉPLACÉES vers sql_scripts/update_procedures_journee.sql, qui
-- ajoute le paramètre p_code_agence (une journée par agence).
-- Ne PAS les redéfinir ici — exécutez update_procedures_journee.sql
-- après ce script pour avoir la version à jour.
-- ============================================================
