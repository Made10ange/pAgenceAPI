DROP PROCEDURE IF EXISTS sp_ouvrir_journee;
DROP PROCEDURE IF EXISTS sp_cloturer_journee;

DELIMITER $$

-- Chaque agence a désormais sa propre journée comptable, indépendante des autres.
CREATE PROCEDURE sp_ouvrir_journee(
    IN  p_code_user   INT,
    IN  p_code_agence INT,
    IN  p_date        DATE,
    OUT p_message     VARCHAR(200)
)
BEGIN
    DECLARE v_existante INT DEFAULT NULL;

    SELECT code_journee INTO v_existante
    FROM   journee_comptable
    WHERE  DATE(date_journee) = p_date
      AND  code_agence = p_code_agence
    LIMIT  1;

    IF v_existante IS NOT NULL THEN
        SET p_message = 'Une journee est deja ouverte ou cloturee pour cette date dans cette agence.';
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Une journée est déjà ouverte ou clôturée pour cette date dans cette agence.';
    ELSE
        INSERT INTO journee_comptable (date_journee, code_agence, statut, date_ouverture, code_user)
        VALUES (p_date, p_code_agence, 'Ouverte', NOW(), p_code_user);
        SET p_message = 'Journee ouverte avec succes.';
    END IF;
END$$

CREATE PROCEDURE sp_cloturer_journee(
    IN  p_code_user   INT,
    IN  p_code_agence INT,
    IN  p_date        DATE,
    OUT p_message     VARCHAR(200)
)
BEGIN
    DECLARE v_code_journee INT DEFAULT NULL;

    SELECT code_journee INTO v_code_journee
    FROM   journee_comptable
    WHERE  DATE(date_journee) = p_date
      AND  code_agence = p_code_agence
      AND  statut = 'Ouverte'
    LIMIT  1;

    IF v_code_journee IS NULL THEN
        SET p_message = 'Aucune journee ouverte a cloturer pour cette date dans cette agence.';
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Aucune journée ouverte à clôturer pour cette date dans cette agence.';
    ELSE
        UPDATE journee_comptable
        SET    statut = 'Cloturee', date_fermeture = NOW(), code_user = p_code_user
        WHERE  code_journee = v_code_journee;
        SET p_message = 'Journee cloturee avec succes.';
    END IF;
END$$

DELIMITER ;
