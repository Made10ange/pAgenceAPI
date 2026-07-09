-- Script : créer les entrées d'embarquement manquantes pour les réservations déjà payées
-- À exécuter une seule fois dans MySQL Workbench

INSERT INTO embarquement_voyage_passager (ID_voyage, ID_passager, STATUT_EMBARQUEMENT, NUMERO_SIEGE, DATE_ENREGISTREMENT)
SELECT
    r.ID_voyage,
    r.ID_passager,
    'En attente',
    r.NUMERO_SIEGE,
    NOW()
FROM reservation r
WHERE r.STATUT_paiement = 'Payé'
  AND r.ID_passager IS NOT NULL
  AND r.STATUT_reservation != 'Annulée'
  AND NOT EXISTS (
      SELECT 1 FROM embarquement_voyage_passager evp
      WHERE evp.ID_voyage = r.ID_voyage
        AND evp.ID_passager = r.ID_passager
  );

SELECT ROW_COUNT() AS 'Entrées embarquement créées';
