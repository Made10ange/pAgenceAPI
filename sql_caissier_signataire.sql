ALTER TABLE utilisateur
  ADD COLUMN IF NOT EXISTS Signataire VARCHAR(100) NULL AFTER Lieu_Delivrance;

DELIMITER $

DROP PROCEDURE IF EXISTS SP_AJOUTER_CAISSIER$
CREATE PROCEDURE SP_AJOUTER_CAISSIER(
    IN p_nom             VARCHAR(100),
    IN p_prenom          VARCHAR(100),
    IN p_sexe            VARCHAR(1),
    IN p_telephone       VARCHAR(20),
    IN p_email           VARCHAR(100),
    IN p_date_naissance  DATE,
    IN p_lieu_naissance  VARCHAR(150),
    IN p_nationalite     VARCHAR(50),
    IN p_profession      VARCHAR(100),
    IN p_type_piece      VARCHAR(50),
    IN p_numero_piece    VARCHAR(100),
    IN p_date_delivrance DATE,
    IN p_lieu_delivrance VARCHAR(150),
    IN p_signataire      VARCHAR(100),
    IN p_date_expiration DATE,
    IN p_photo           BLOB,
    IN p_login           VARCHAR(100),
    IN p_motdepasse      VARCHAR(255),
    IN p_id_agence       INT,
    IN p_id_groupe       INT,
    OUT p_id             INT
)
BEGIN
    INSERT INTO utilisateur (Nom, Prenom, Sexe, Telephone, Email, Date_Naissance, Lieu_Naissance,
                              Nationalite, Profession, Type_Piece, Numero_Piece, Date_Delivrance,
                              Lieu_Delivrance, Signataire, Date_Expiration, Photo,
                              Login, MotDePasse, Role, Actif, Id_Agence)
    VALUES (p_nom, p_prenom, p_sexe, p_telephone, p_email, p_date_naissance, p_lieu_naissance,
            p_nationalite, p_profession, p_type_piece, p_numero_piece, p_date_delivrance,
            p_lieu_delivrance, p_signataire, p_date_expiration, p_photo,
            p_login, p_motdepasse, 'Caissier', 1, p_id_agence);

    SET p_id = LAST_INSERT_ID();

    IF p_id_groupe IS NOT NULL THEN
        INSERT INTO utilisateur_groupe (Id_Utilisateur, Id_Groupe) VALUES (p_id, p_id_groupe);
    END IF;
END$

DROP PROCEDURE IF EXISTS SP_MODIFIER_CAISSIER$
CREATE PROCEDURE SP_MODIFIER_CAISSIER(
    IN p_id              INT,
    IN p_nom             VARCHAR(100),
    IN p_prenom          VARCHAR(100),
    IN p_sexe            VARCHAR(1),
    IN p_telephone       VARCHAR(20),
    IN p_email           VARCHAR(100),
    IN p_date_naissance  DATE,
    IN p_lieu_naissance  VARCHAR(150),
    IN p_nationalite     VARCHAR(50),
    IN p_profession      VARCHAR(100),
    IN p_type_piece      VARCHAR(50),
    IN p_numero_piece    VARCHAR(100),
    IN p_date_delivrance DATE,
    IN p_lieu_delivrance VARCHAR(150),
    IN p_signataire      VARCHAR(100),
    IN p_date_expiration DATE,
    IN p_photo           BLOB,
    IN p_maj_photo       TINYINT,
    IN p_login           VARCHAR(100),
    IN p_id_agence       INT,
    IN p_actif           TINYINT,
    IN p_id_groupe       INT
)
BEGIN
    UPDATE utilisateur
    SET    Nom             = p_nom,
           Prenom          = p_prenom,
           Sexe            = p_sexe,
           Telephone       = p_telephone,
           Email           = p_email,
           Date_Naissance  = p_date_naissance,
           Lieu_Naissance  = p_lieu_naissance,
           Nationalite     = p_nationalite,
           Profession      = p_profession,
           Type_Piece      = p_type_piece,
           Numero_Piece    = p_numero_piece,
           Date_Delivrance = p_date_delivrance,
           Lieu_Delivrance = p_lieu_delivrance,
           Signataire      = p_signataire,
           Date_Expiration = p_date_expiration,
           Login           = p_login,
           Id_Agence       = p_id_agence,
           Actif           = p_actif,
           Photo           = IF(p_maj_photo = 1, p_photo, Photo)
    WHERE  Id_Utilisateur = p_id AND Role = 'Caissier';

    DELETE FROM utilisateur_groupe WHERE Id_Utilisateur = p_id;

    IF p_id_groupe IS NOT NULL THEN
        INSERT INTO utilisateur_groupe (Id_Utilisateur, Id_Groupe) VALUES (p_id, p_id_groupe);
    END IF;
END$

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
           ON acc.id_utilisateur = u.Id_Utilisateur AND acc.statut = 'Active'
    WHERE  u.Role = 'Caissier'
    GROUP  BY u.Id_Utilisateur
    ORDER  BY u.Nom, u.Prenom;
END$

DELIMITER ;
