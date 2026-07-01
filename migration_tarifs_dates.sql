-- ═══════════════════════════════════════════════════════════
--  MISE À JOUR TABLE TARIF — ajout périodes de validité
--  À exécuter dans la base bd_agence
-- ═══════════════════════════════════════════════════════════

ALTER TABLE TARIF
    ADD COLUMN DATE_DEBUT DATE NULL AFTER PRIX,
    ADD COLUMN DATE_FIN   DATE NULL AFTER DATE_DEBUT;

-- Les tarifs existants sans date = valables en permanence (NULL = pas de limite)
