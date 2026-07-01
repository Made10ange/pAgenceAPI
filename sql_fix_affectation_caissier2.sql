DELIMITER $

DROP PROCEDURE IF EXISTS SP_LISTE_AFFECTATIONS$
CREATE PROCEDURE SP_LISTE_AFFECTATIONS(IN p_id_caisse INT)
BEGIN
    SELECT a.id_affectation,
           a.id_caisse,
           a.code_user              AS id_utilisateur,
           a.date_debut,
           a.date_fin,
           a.statut,
           a.code_user_createur     AS id_utilisateur_createur,
           a.date_creation,
           c.libelle                AS libelle_caisse,
           u.Nom                    AS nom_caissier,
           u.Prenom                 AS prenom_caissier,
           u.Login                  AS login_caissier
    FROM   affectation_caissier_caisse a
    JOIN   caisse      c ON c.id_caisse       = a.id_caisse
    JOIN   utilisateur u ON u.Id_Utilisateur  = a.code_user
    WHERE  p_id_caisse IS NULL OR a.id_caisse = p_id_caisse
    ORDER  BY a.date_debut DESC;
END$

DELIMITER ;
