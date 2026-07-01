-- Ajout colonnes Sexe/Telephone sur utilisateur
ALTER TABLE utilisateur
  ADD COLUMN IF NOT EXISTS Sexe VARCHAR(1) NULL AFTER Prenom,
  ADD COLUMN IF NOT EXISTS Telephone VARCHAR(20) NULL AFTER Sexe;

DELIMITER $

DROP PROCEDURE IF EXISTS SP_LISTE_CAISSIERS$
CREATE PROCEDURE SP_LISTE_CAISSIERS()
BEGIN
    SELECT u.Id_Utilisateur, u.Nom, u.Prenom, u.Sexe, u.Telephone, u.Login, u.Role, u.Actif,
           u.Date_Creation, u.Id_Agence,
           a.Nom_Agence,
           ug.Id_Groupe,
           g.Libelle AS Nom_Groupe,
           COUNT(DISTINCT acc.id_affectation) AS nb_caisses_actives
    FROM   utilisateur u
    LEFT JOIN agence a ON a.Id_Agence = u.Id_Agence
    LEFT JOIN utilisateur_groupe ug ON ug.Id_Utilisateur = u.Id_Utilisateur
    LEFT JOIN groupe g ON g.Id_Groupe = ug.Id_Groupe
    LEFT JOIN affectation_caissier_caisse acc
           ON acc.id_utilisateur = u.Id_Utilisateur AND acc.statut = 'Active'
    WHERE  u.Role = 'Caissier'
    GROUP  BY u.Id_Utilisateur
    ORDER  BY u.Nom, u.Prenom;
END$

DROP PROCEDURE IF EXISTS SP_GET_CAISSIER$
CREATE PROCEDURE SP_GET_CAISSIER(IN p_id INT)
BEGIN
    SELECT u.*, a.Nom_Agence, ug.Id_Groupe, g.Libelle AS Nom_Groupe
    FROM   utilisateur u
    LEFT JOIN agence a ON a.Id_Agence = u.Id_Agence
    LEFT JOIN utilisateur_groupe ug ON ug.Id_Utilisateur = u.Id_Utilisateur
    LEFT JOIN groupe g ON g.Id_Groupe = ug.Id_Groupe
    WHERE  u.Id_Utilisateur = p_id AND u.Role = 'Caissier';
END$

DROP PROCEDURE IF EXISTS SP_AJOUTER_CAISSIER$
CREATE PROCEDURE SP_AJOUTER_CAISSIER(
    IN p_nom        VARCHAR(100),
    IN p_prenom     VARCHAR(100),
    IN p_sexe       VARCHAR(1),
    IN p_telephone  VARCHAR(20),
    IN p_login      VARCHAR(100),
    IN p_motdepasse VARCHAR(255),
    IN p_id_agence  INT,
    IN p_id_groupe  INT,
    OUT p_id        INT
)
BEGIN
    INSERT INTO utilisateur (Nom, Prenom, Sexe, Telephone, Login, MotDePasse, Role, Actif, Id_Agence)
    VALUES (p_nom, p_prenom, p_sexe, p_telephone, p_login, p_motdepasse, 'Caissier', 1, p_id_agence);

    SET p_id = LAST_INSERT_ID();

    IF p_id_groupe IS NOT NULL THEN
        INSERT INTO utilisateur_groupe (Id_Utilisateur, Id_Groupe) VALUES (p_id, p_id_groupe);
    END IF;
END$

DROP PROCEDURE IF EXISTS SP_MODIFIER_CAISSIER$
CREATE PROCEDURE SP_MODIFIER_CAISSIER(
    IN p_id        INT,
    IN p_nom       VARCHAR(100),
    IN p_prenom    VARCHAR(100),
    IN p_sexe      VARCHAR(1),
    IN p_telephone VARCHAR(20),
    IN p_login     VARCHAR(100),
    IN p_id_agence INT,
    IN p_actif     TINYINT,
    IN p_id_groupe INT
)
BEGIN
    UPDATE utilisateur
    SET    Nom       = p_nom,
           Prenom    = p_prenom,
           Sexe      = p_sexe,
           Telephone = p_telephone,
           Login     = p_login,
           Id_Agence = p_id_agence,
           Actif     = p_actif
    WHERE  Id_Utilisateur = p_id AND Role = 'Caissier';

    DELETE FROM utilisateur_groupe WHERE Id_Utilisateur = p_id;

    IF p_id_groupe IS NOT NULL THEN
        INSERT INTO utilisateur_groupe (Id_Utilisateur, Id_Groupe) VALUES (p_id, p_id_groupe);
    END IF;
END$

DELIMITER ;
