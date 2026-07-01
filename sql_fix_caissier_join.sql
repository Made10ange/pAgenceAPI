DELIMITER $

DROP PROCEDURE IF EXISTS SP_LISTE_CAISSIERS$
CREATE PROCEDURE SP_LISTE_CAISSIERS()
BEGIN
    SELECT u.Id_Utilisateur, u.Nom, u.Prenom, u.Sexe, u.Telephone, u.Email,
           u.Date_Naissance, u.Lieu_Naissance, u.Nationalite, u.Profession,
           u.Type_Piece, u.Numero_Piece, u.Date_Delivrance, u.Lieu_Delivrance, u.Signataire, u.Date_Expiration,
           u.Login, u.Role, u.Actif, u.Date_Creation, u.Id_Agence,
           a.Nom_Agence,
           ug.Id_Groupe,
           g.Libelle AS Nom_Groupe,
           COUNT(DISTINCT acc.id_affectation) AS nb_caisses_actives
    FROM   utilisateur u
    LEFT JOIN agence a ON a.Id_Agence = u.Id_Agence
    LEFT JOIN utilisateur_groupe ug ON ug.Id_Utilisateur = u.Id_Utilisateur
    LEFT JOIN groupe g ON g.Id_Groupe = ug.Id_Groupe
    LEFT JOIN affectation_caissier_caisse acc
           ON acc.code_user = u.Id_Utilisateur AND acc.statut = 'Active'
    WHERE  u.Role = 'Caissier'
    GROUP  BY u.Id_Utilisateur
    ORDER  BY u.Nom, u.Prenom;
END$

DELIMITER ;
