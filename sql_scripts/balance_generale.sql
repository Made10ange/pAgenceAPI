-- ============================================================
-- Balance Générale (format classique avec soldes initiaux)
-- ============================================================

DROP PROCEDURE IF EXISTS sp_balance_generale;

DELIMITER $

CREATE PROCEDURE sp_balance_generale(
    IN  p_date_debut DATE,
    IN  p_date_fin   DATE,
    IN  p_id_agence  INT
)
BEGIN
    SELECT
        c.numcompte,
        c.libelle_compte,

        -- ── Soldes initiaux (tout ce qui précède p_date_debut) ──────────
        GREATEST(0,
            COALESCE(SUM(CASE WHEN DATE(o.date_operation) < p_date_debut THEN o.debit  ELSE 0 END), 0) -
            COALESCE(SUM(CASE WHEN DATE(o.date_operation) < p_date_debut THEN o.credit ELSE 0 END), 0)
        ) AS solde_debiteur_initial,

        GREATEST(0,
            COALESCE(SUM(CASE WHEN DATE(o.date_operation) < p_date_debut THEN o.credit ELSE 0 END), 0) -
            COALESCE(SUM(CASE WHEN DATE(o.date_operation) < p_date_debut THEN o.debit  ELSE 0 END), 0)
        ) AS solde_crediteur_initial,

        -- ── Mouvements de la période ─────────────────────────────────────
        COALESCE(SUM(CASE WHEN DATE(o.date_operation) BETWEEN p_date_debut AND p_date_fin THEN o.debit  ELSE 0 END), 0) AS mvt_debit,
        COALESCE(SUM(CASE WHEN DATE(o.date_operation) BETWEEN p_date_debut AND p_date_fin THEN o.credit ELSE 0 END), 0) AS mvt_credit,

        -- ── Soldes finaux = initial + mouvements ─────────────────────────
        GREATEST(0,
            (
                COALESCE(SUM(CASE WHEN DATE(o.date_operation) < p_date_debut THEN o.debit  ELSE 0 END), 0) +
                COALESCE(SUM(CASE WHEN DATE(o.date_operation) BETWEEN p_date_debut AND p_date_fin THEN o.debit  ELSE 0 END), 0)
            ) - (
                COALESCE(SUM(CASE WHEN DATE(o.date_operation) < p_date_debut THEN o.credit ELSE 0 END), 0) +
                COALESCE(SUM(CASE WHEN DATE(o.date_operation) BETWEEN p_date_debut AND p_date_fin THEN o.credit ELSE 0 END), 0)
            )
        ) AS solde_debiteur_final,

        GREATEST(0,
            (
                COALESCE(SUM(CASE WHEN DATE(o.date_operation) < p_date_debut THEN o.credit ELSE 0 END), 0) +
                COALESCE(SUM(CASE WHEN DATE(o.date_operation) BETWEEN p_date_debut AND p_date_fin THEN o.credit ELSE 0 END), 0)
            ) - (
                COALESCE(SUM(CASE WHEN DATE(o.date_operation) < p_date_debut THEN o.debit  ELSE 0 END), 0) +
                COALESCE(SUM(CASE WHEN DATE(o.date_operation) BETWEEN p_date_debut AND p_date_fin THEN o.debit  ELSE 0 END), 0)
            )
        ) AS solde_crediteur_final

    FROM   compte c
    LEFT   JOIN operation o
           ON  o.numcompte = c.numcompte
           AND (p_id_agence IS NULL OR o.code_agence = p_id_agence)
    GROUP  BY c.numcompte, c.libelle_compte
    ORDER  BY c.numcompte;
END$

DELIMITER ;
