-- ============================================================
-- ÉCRITURES AUTOMATIQUES
-- p_code_user → SP helper récupère le numcompte de sa caisse
-- Fallback : caisse principale (est_principale=1), puis '5710'
-- ============================================================

DROP PROCEDURE IF EXISTS sp_get_numcompte_caisse_user;
DELIMITER $
CREATE PROCEDURE sp_get_numcompte_caisse_user(
    IN  p_code_user INT,
    OUT p_numcompte VARCHAR(30)
)
BEGIN
    SELECT c.numcompte INTO p_numcompte
    FROM   affectation_caissier_caisse acc
    INNER  JOIN caisse c ON c.id_caisse = acc.id_caisse
    WHERE  acc.code_user = p_code_user
      AND  acc.statut    = 'Active'
      AND  (acc.date_fin IS NULL OR acc.date_fin >= CURDATE())
    ORDER  BY acc.date_debut DESC
    LIMIT  1;

    IF p_numcompte IS NULL THEN
        SELECT numcompte INTO p_numcompte FROM caisse WHERE est_principale = 1 LIMIT 1;
    END IF;
    IF p_numcompte IS NULL THEN
        SET p_numcompte = '5710';
    END IF;
END$
DELIMITER ;

-- ── VENTE DE BILLET ──────────────────────────────────────────
DROP PROCEDURE IF EXISTS sp_ecriture_vente_billet_auto;
DELIMITER $
CREATE PROCEDURE sp_ecriture_vente_billet_auto(
    IN  p_num_transaction VARCHAR(30), IN  p_num_billet   VARCHAR(30),
    IN  p_montant         DECIMAL(15,2), IN  p_libelle_type VARCHAR(50),
    IN  p_trajet          VARCHAR(100),  IN  p_id_agence   INT,
    IN  p_code_user       INT,           OUT p_ok          TINYINT
)
main_block: BEGIN
    DECLARE v_journee      INT DEFAULT NULL;
    DECLARE v_compte_vente VARCHAR(10);
    DECLARE v_caisse       VARCHAR(30);
    DECLARE v_libelle      VARCHAR(200);

    SELECT code_journee INTO v_journee FROM journee_comptable
    WHERE DATE(date_journee) = CURDATE() AND code_agence = p_id_agence AND statut = 'Ouverte' LIMIT 1;
    IF v_journee IS NULL THEN SET p_ok = 0; LEAVE main_block; END IF;

    CALL sp_get_numcompte_caisse_user(p_code_user, v_caisse);

    SET v_compte_vente = CASE
        WHEN p_libelle_type LIKE '%VIP%'    THEN '7102'
        WHEN p_libelle_type LIKE '%Report%' THEN '7103'
        ELSE '7101' END;

    SET v_libelle = CONCAT('Vente billet ', p_num_billet,
        CASE WHEN p_trajet IS NOT NULL THEN CONCAT(' - ', p_trajet) ELSE '' END);

    INSERT INTO operation (date_operation, numcompte, debit, credit, libelle, num_transaction, code_agence, code_user, valide)
    VALUES (NOW(), v_caisse, p_montant, 0, v_libelle, p_num_transaction, p_id_agence, p_code_user, 1);
    UPDATE compte SET cumul_debit = cumul_debit + p_montant, solde = solde + p_montant WHERE numcompte = v_caisse;

    INSERT INTO operation (date_operation, numcompte, debit, credit, libelle, num_transaction, code_agence, code_user, valide)
    VALUES (NOW(), v_compte_vente, 0, p_montant, v_libelle, p_num_transaction, p_id_agence, p_code_user, 1);
    UPDATE compte SET cumul_credit = cumul_credit + p_montant, solde = solde - p_montant WHERE numcompte = v_compte_vente;

    SET p_ok = 1;
END$
DELIMITER ;

-- ── FRAIS DE COLIS ───────────────────────────────────────────
DROP PROCEDURE IF EXISTS sp_ecriture_colis_auto;
DELIMITER $
CREATE PROCEDURE sp_ecriture_colis_auto(
    IN  p_num_transaction VARCHAR(30), IN  p_ref_colis    VARCHAR(30),
    IN  p_montant         DECIMAL(15,2), IN  p_expediteur  VARCHAR(100),
    IN  p_destinataire    VARCHAR(100),  IN  p_id_agence   INT,
    IN  p_code_user       INT,           OUT p_ok          TINYINT
)
main_block: BEGIN
    DECLARE v_journee INT DEFAULT NULL;
    DECLARE v_caisse  VARCHAR(30);
    DECLARE v_libelle VARCHAR(200);

    SELECT code_journee INTO v_journee FROM journee_comptable
    WHERE DATE(date_journee) = CURDATE() AND code_agence = p_id_agence AND statut = 'Ouverte' LIMIT 1;
    IF v_journee IS NULL THEN SET p_ok = 0; LEAVE main_block; END IF;

    CALL sp_get_numcompte_caisse_user(p_code_user, v_caisse);

    SET v_libelle = CONCAT('Colis ', COALESCE(p_ref_colis,''), ' - ',
                           COALESCE(p_expediteur,'?'), ' vers ', COALESCE(p_destinataire,'?'));

    INSERT INTO operation (date_operation, numcompte, debit, credit, libelle, num_transaction, code_agence, code_user, valide)
    VALUES (NOW(), v_caisse, p_montant, 0, v_libelle, p_num_transaction, p_id_agence, p_code_user, 1);
    UPDATE compte SET cumul_debit = cumul_debit + p_montant, solde = solde + p_montant WHERE numcompte = v_caisse;

    INSERT INTO operation (date_operation, numcompte, debit, credit, libelle, num_transaction, code_agence, code_user, valide)
    VALUES (NOW(), '7301', 0, p_montant, v_libelle, p_num_transaction, p_id_agence, p_code_user, 1);
    UPDATE compte SET cumul_credit = cumul_credit + p_montant, solde = solde - p_montant WHERE numcompte = '7301';

    SET p_ok = 1;
END$
DELIMITER ;

-- ── FRAIS DE BAGAGE ──────────────────────────────────────────
DROP PROCEDURE IF EXISTS sp_ecriture_bagage_auto;
DELIMITER $
CREATE PROCEDURE sp_ecriture_bagage_auto(
    IN  p_num_transaction VARCHAR(30), IN  p_ref_bagage  VARCHAR(30),
    IN  p_montant         DECIMAL(15,2), IN  p_passager   VARCHAR(100),
    IN  p_id_agence       INT,           IN  p_code_user  INT,
    OUT p_ok              TINYINT
)
main_block: BEGIN
    DECLARE v_journee INT DEFAULT NULL;
    DECLARE v_caisse  VARCHAR(30);
    DECLARE v_libelle VARCHAR(200);

    SELECT code_journee INTO v_journee FROM journee_comptable
    WHERE DATE(date_journee) = CURDATE() AND code_agence = p_id_agence AND statut = 'Ouverte' LIMIT 1;
    IF v_journee IS NULL THEN SET p_ok = 0; LEAVE main_block; END IF;

    CALL sp_get_numcompte_caisse_user(p_code_user, v_caisse);

    SET v_libelle = CONCAT('Bagage ', COALESCE(p_ref_bagage,''),
        CASE WHEN p_passager IS NOT NULL THEN CONCAT(' - ', p_passager) ELSE '' END);

    INSERT INTO operation (date_operation, numcompte, debit, credit, libelle, num_transaction, code_agence, code_user, valide)
    VALUES (NOW(), v_caisse, p_montant, 0, v_libelle, p_num_transaction, p_id_agence, p_code_user, 1);
    UPDATE compte SET cumul_debit = cumul_debit + p_montant, solde = solde + p_montant WHERE numcompte = v_caisse;

    INSERT INTO operation (date_operation, numcompte, debit, credit, libelle, num_transaction, code_agence, code_user, valide)
    VALUES (NOW(), '7201', 0, p_montant, v_libelle, p_num_transaction, p_id_agence, p_code_user, 1);
    UPDATE compte SET cumul_credit = cumul_credit + p_montant, solde = solde - p_montant WHERE numcompte = '7201';

    SET p_ok = 1;
END$
DELIMITER ;
