DELIMITER $

DROP PROCEDURE IF EXISTS SP_AFFECTER_CAISSIER$
CREATE PROCEDURE SP_AFFECTER_CAISSIER(
    IN p_id_caisse             INT,
    IN p_id_utilisateur        INT,
    IN p_date_debut            DATE,
    IN p_date_fin              DATE,
    IN p_id_utilisateur_createur INT,
    OUT p_ok TINYINT
)
BEGIN
    INSERT INTO affectation_caissier_caisse
        (id_caisse, code_user, date_debut, date_fin, statut, code_user_createur)
    VALUES
        (p_id_caisse, p_id_utilisateur, p_date_debut, p_date_fin, 'Active', p_id_utilisateur_createur);
    SET p_ok = 1;
END$

DROP PROCEDURE IF EXISTS SP_LISTE_AFFECTATIONS$
CREATE PROCEDURE SP_LISTE_AFFECTATIONS(IN p_id_caisse INT)
BEGIN
    SELECT a.*,
           c.libelle                         AS libelle_caisse,
           u.Nom                             AS nom_caissier,
           u.Prenom                          AS prenom_caissier,
           u.Login                           AS login_caissier
    FROM   affectation_caissier_caisse a
    JOIN   caisse      c ON c.id_caisse       = a.id_caisse
    JOIN   utilisateur u ON u.Id_Utilisateur  = a.code_user
    WHERE  p_id_caisse IS NULL OR a.id_caisse = p_id_caisse
    ORDER  BY a.date_debut DESC;
END$

DELIMITER ;
