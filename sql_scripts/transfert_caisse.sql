-- ============================================================
-- TRANSFERT DE CAISSE
-- ============================================================

-- Ajouter une caisse secondaire si elle n'existe pas
INSERT IGNORE INTO caisse (id_caisse, code_caisse, numcompte, libelle, est_principale, statut)
VALUES (2, '2', '5711', 'Caisse secondaire', 0, 'Active');

-- Table des transferts
CREATE TABLE IF NOT EXISTS transfert_caisse (
    id_transfert        INT(11)        NOT NULL AUTO_INCREMENT,
    id_caisse_depart    INT(11)        NOT NULL,
    id_caisse_dest      INT(11)        NOT NULL,
    montant             DECIMAL(15,2)  NOT NULL,
    date_transfert      DATE           NOT NULL,
    num_piece           VARCHAR(30)    DEFAULT NULL,
    motif               VARCHAR(200)   DEFAULT NULL,
    statut              VARCHAR(20)    NOT NULL DEFAULT 'En attente',
    code_user_init      INT(11)        DEFAULT NULL,
    code_user_valid     INT(11)        DEFAULT NULL,
    date_init           DATETIME       DEFAULT CURRENT_TIMESTAMP,
    date_validation     DATETIME       DEFAULT NULL,
    num_transaction_cpt BIGINT         DEFAULT NULL,
    PRIMARY KEY (id_transfert),
    KEY fk_tc_depart (id_caisse_depart),
    KEY fk_tc_dest   (id_caisse_dest),
    CONSTRAINT fk_tc_depart FOREIGN KEY (id_caisse_depart) REFERENCES caisse(id_caisse),
    CONSTRAINT fk_tc_dest   FOREIGN KEY (id_caisse_dest)   REFERENCES caisse(id_caisse)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ── SP : Initier un transfert ─────────────────────────────────
DROP PROCEDURE IF EXISTS sp_initier_transfert;

DELIMITER $$
CREATE PROCEDURE sp_initier_transfert(
    IN  p_id_caisse_depart INT,
    IN  p_id_caisse_dest   INT,
    IN  p_montant          DECIMAL(15,2),
    IN  p_date             DATE,
    IN  p_num_piece        VARCHAR(30),
    IN  p_motif            VARCHAR(200),
    IN  p_code_user        INT,
    OUT p_id_transfert     INT,
    OUT p_message          VARCHAR(200)
)
main_block: BEGIN
    DECLARE v_libelle_dep  VARCHAR(100);
    DECLARE v_libelle_dest VARCHAR(100);

    IF p_id_caisse_depart = p_id_caisse_dest THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Les caisses de départ et destination ne peuvent pas être identiques.';
    END IF;

    IF p_montant <= 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Le montant du transfert doit être supérieur à 0.';
    END IF;

    SELECT libelle INTO v_libelle_dep  FROM caisse WHERE id_caisse = p_id_caisse_depart LIMIT 1;
    SELECT libelle INTO v_libelle_dest FROM caisse WHERE id_caisse = p_id_caisse_dest   LIMIT 1;

    IF v_libelle_dep IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Caisse de départ introuvable.';
    END IF;
    IF v_libelle_dest IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Caisse destinataire introuvable.';
    END IF;

    INSERT INTO transfert_caisse
        (id_caisse_depart, id_caisse_dest, montant, date_transfert,
         num_piece, motif, statut, code_user_init, date_init)
    VALUES
        (p_id_caisse_depart, p_id_caisse_dest, p_montant, p_date,
         p_num_piece, p_motif, 'En attente', p_code_user, NOW());

    SET p_id_transfert = LAST_INSERT_ID();
    SET p_message = CONCAT('Transfert initié : ', v_libelle_dep, ' → ', v_libelle_dest,
                           ' — ', FORMAT(p_montant, 0), ' FCFA');
END$$
DELIMITER ;


-- ── SP : Valider un transfert ─────────────────────────────────
DROP PROCEDURE IF EXISTS sp_valider_transfert;

DELIMITER $$
CREATE PROCEDURE sp_valider_transfert(
    IN  p_id_transfert INT,
    IN  p_code_user    INT,
    OUT p_message      VARCHAR(200)
)
main_block: BEGIN
    DECLARE v_statut        VARCHAR(20);
    DECLARE v_montant       DECIMAL(15,2);
    DECLARE v_num_dep       VARCHAR(30);
    DECLARE v_num_dest      VARCHAR(30);
    DECLARE v_lib_dep       VARCHAR(100);
    DECLARE v_lib_dest      VARCHAR(100);
    DECLARE v_solde_dep     DECIMAL(15,2);
    DECLARE v_journee       INT DEFAULT NULL;
    DECLARE v_num_tx        BIGINT;

    -- Vérifier que le transfert existe et est en attente
    SELECT t.statut, t.montant,
           cd.numcompte, cd.libelle,
           ca.numcompte, ca.libelle
    INTO   v_statut, v_montant, v_num_dep, v_lib_dep, v_num_dest, v_lib_dest
    FROM   transfert_caisse t
    JOIN   caisse cd ON cd.id_caisse = t.id_caisse_depart
    JOIN   caisse ca ON ca.id_caisse = t.id_caisse_dest
    WHERE  t.id_transfert = p_id_transfert
    LIMIT  1;

    IF v_statut IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Transfert introuvable.';
    END IF;

    IF v_statut <> 'En attente' THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Ce transfert a déjà été validé ou annulé.';
    END IF;

    -- Vérifier que la caisse de départ a un solde suffisant
    SELECT solde INTO v_solde_dep FROM compte WHERE numcompte = v_num_dep LIMIT 1;
    IF v_solde_dep IS NULL THEN SET v_solde_dep = 0; END IF;

    IF v_solde_dep < v_montant THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Solde insuffisant dans la caisse de départ pour effectuer ce transfert.';
    END IF;

    -- La journée comptable du jour doit être ouverte pour pouvoir générer
    -- les écritures de ce transfert — sinon on bloque la validation au lieu
    -- de valider silencieusement sans écriture.
    SELECT code_journee INTO v_journee
    FROM   journee_comptable
    WHERE  DATE(date_journee) = CURDATE() AND statut = 'Ouverte'
    LIMIT  1;

    IF v_journee IS NULL THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Impossible de valider : aucune journée comptable ouverte aujourd''hui. Ouvrez la journée avant de valider ce transfert.';
    END IF;

    -- Valider le transfert
    UPDATE transfert_caisse
    SET statut          = 'Valide',
        code_user_valid = p_code_user,
        date_validation = NOW()
    WHERE id_transfert  = p_id_transfert;

    SET v_num_tx = UNIX_TIMESTAMP();

    -- DÉBIT caisse destinataire (reçoit l'argent)
    INSERT INTO operation
        (date_operation, numcompte, debit, credit, libelle,
         num_transaction, code_user, valide)
    VALUES (NOW(), v_num_dest, v_montant, 0,
            CONCAT('Transfert de ', v_lib_dep, ' vers ', v_lib_dest),
            v_num_tx, p_code_user, 1);

    UPDATE compte
    SET cumul_debit = cumul_debit + v_montant,
        solde       = solde + v_montant
    WHERE numcompte = v_num_dest;

    -- CRÉDIT caisse départ (perd l'argent)
    INSERT INTO operation
        (date_operation, numcompte, debit, credit, libelle,
         num_transaction, code_user, valide)
    VALUES (NOW(), v_num_dep, 0, v_montant,
            CONCAT('Transfert de ', v_lib_dep, ' vers ', v_lib_dest),
            v_num_tx, p_code_user, 1);

    UPDATE compte
    SET cumul_credit = cumul_credit + v_montant,
        solde        = solde - v_montant
    WHERE numcompte = v_num_dep;

    UPDATE transfert_caisse SET num_transaction_cpt = v_num_tx
    WHERE id_transfert = p_id_transfert;

    SET p_message = CONCAT('Transfert validé : ', v_lib_dep, ' → ', v_lib_dest,
                           ' — ', FORMAT(v_montant, 0), ' FCFA');
END$$
DELIMITER ;


-- ── SP : Annuler un transfert ─────────────────────────────────
DROP PROCEDURE IF EXISTS sp_annuler_transfert;

DELIMITER $$
CREATE PROCEDURE sp_annuler_transfert(
    IN  p_id_transfert INT,
    IN  p_code_user    INT,
    OUT p_message      VARCHAR(200)
)
BEGIN
    DECLARE v_statut VARCHAR(20);

    SELECT statut INTO v_statut FROM transfert_caisse WHERE id_transfert = p_id_transfert LIMIT 1;

    IF v_statut IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Transfert introuvable.';
    END IF;
    IF v_statut <> 'En attente' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Seuls les transferts en attente peuvent être annulés.';
    END IF;

    UPDATE transfert_caisse SET statut = 'Annule', code_user_valid = p_code_user, date_validation = NOW()
    WHERE id_transfert = p_id_transfert;

    SET p_message = 'Transfert annulé avec succès.';
END$$
DELIMITER ;
