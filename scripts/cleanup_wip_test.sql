-- cleanup_wip_test.sql
-- Apaga tudo o que foi criado pelo seed_wip_state.py (encomenda "ORD-WIP-TEST"),
-- respeitando a ordem das foreign keys (filhos antes dos pais).
-- NÃO toca nos produtos sintéticos (VIN-SEED-*) nem em mais nenhum dado.

BEGIN;

CREATE TEMP TABLE wip_test_products AS
SELECT p.id
FROM product p
JOIN manufacturing_order mo ON mo.id = p.manufacturing_order_id
JOIN client_order co ON co.id = mo.client_order_id
WHERE co.order_number = 'ORD-WIP-TEST';

DELETE FROM alert WHERE product_id IN (SELECT id FROM wip_test_products);
DELETE FROM product_phase WHERE product_id IN (SELECT id FROM wip_test_products);
DELETE FROM supported_product WHERE product_id IN (SELECT id FROM wip_test_products);
DELETE FROM product_config WHERE product_id IN (SELECT id FROM wip_test_products);
DELETE FROM product WHERE id IN (SELECT id FROM wip_test_products);

DELETE FROM manufacturing_order
WHERE client_order_id IN (SELECT id FROM client_order WHERE order_number = 'ORD-WIP-TEST');

DELETE FROM client_order WHERE order_number = 'ORD-WIP-TEST';

-- Limpar localizações "ativas" deixadas para trás pelos suportes usados no teste
-- (senão os skids ficam a aparecer como "ocupados" para sempre nos ecrãs reais).
UPDATE localization_history
SET datetime_end = now(), status = 'closed_by_cleanup'
WHERE status = 'active' AND datetime_end IS NULL;

DROP TABLE wip_test_products;

COMMIT;