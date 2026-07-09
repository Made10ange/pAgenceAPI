-- ============================================================
-- Script : Lier les réservations payées sans passager
-- À exécuter dans MySQL Workbench ou via WAMP
-- ============================================================

-- ÉTAPE 1 : Créer les passagers manquants (un par téléphone unique)
INSERT INTO passager (NOM, PRENOM, TELEPHONE, NUMERO_PIECE, EMAIL, SEXE, Id_Agence)
SELECT DISTINCT
    r.NOM_CLIENT,
    r.PRENOM_CLIENT,
    r.TELEPHONE_CLIENT,
    r.NUMERO_CNI_CLIENT,
    r.EMAIL_CLIENT,
    'Non précisé',
    v.Id_Agence
FROM reservation r
LEFT JOIN voyage v ON r.ID_voyage = v.ID_voyage
WHERE r.STATUT_paiement = 'Payé'
  AND (r.ID_passager IS NULL OR r.ID_passager = 0)
  AND r.TELEPHONE_CLIENT IS NOT NULL
  AND r.TELEPHONE_CLIENT <> ''
  -- Ne pas insérer si un passager avec ce téléphone existe déjà
  AND NOT EXISTS (
    SELECT 1 FROM passager p WHERE p.TELEPHONE = r.TELEPHONE_CLIENT
  );

-- ÉTAPE 2 : Lier chaque réservation payée à son passager (par téléphone)
UPDATE reservation r
JOIN voyage v ON r.ID_voyage = v.ID_voyage
JOIN passager p ON p.TELEPHONE = r.TELEPHONE_CLIENT
SET r.ID_passager = p.ID_passager
WHERE r.STATUT_paiement = 'Payé'
  AND (r.ID_passager IS NULL OR r.ID_passager = 0)
  AND r.TELEPHONE_CLIENT IS NOT NULL
  AND r.TELEPHONE_CLIENT <> '';

-- ÉTAPE 3 : Vérification — doit retourner 0 ligne si tout est lié
SELECT COUNT(*) AS reservations_non_liees
FROM reservation
WHERE STATUT_paiement = 'Payé'
  AND (ID_passager IS NULL OR ID_passager = 0);
