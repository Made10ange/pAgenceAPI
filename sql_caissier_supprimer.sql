DELIMITER $

DROP PROCEDURE IF EXISTS SP_SUPPRIMER_CAISSIER$
CREATE PROCEDURE SP_SUPPRIMER_CAISSIER(IN p_id INT)
BEGIN
    DELETE FROM affectation_caissier_caisse WHERE code_user = p_id;
    DELETE FROM utilisateur_groupe WHERE Id_Utilisateur = p_id;
    DELETE FROM utilisateur WHERE Id_Utilisateur = p_id AND Role = 'Caissier';
END$

DELIMITER ;
