-- Massive demo seed for Business Dashboard
-- Target: PostgreSQL 16+ (compatible with the EF Core Npgsql model in this repo)
-- Usage in dBeaver:
-- 1. Open your target database
-- 2. Run this file as a script
-- 3. It will DELETE existing data from all business tables and reload them with demo data

BEGIN;

CREATE EXTENSION IF NOT EXISTS pgcrypto;

TRUNCATE TABLE
  "SaleItems",
  "Sales",
  "Notifications",
  "InventoryMovements",
  "Costs",
  "Customers",
  "Products"
RESTART IDENTITY;

DROP TABLE IF EXISTS tmp_customers;
DROP TABLE IF EXISTS tmp_products;
DROP TABLE IF EXISTS tmp_sales_base;
DROP TABLE IF EXISTS tmp_sale_items;
DROP TABLE IF EXISTS tmp_sales;

CREATE TEMP TABLE tmp_customers AS
SELECT
  gen_random_uuid() AS "Id",
  gs AS "SeedNo",
  format('Cliente %s', lpad(gs::text, 3, '0')) AS "Name",
  format('cliente%s@demo.local', lpad(gs::text, 3, '0')) AS "Email",
  format('+598 9%s', lpad(((gs * 713) % 10000000)::text, 7, '0')) AS "Phone",
  CASE
    WHEN gs % 6 = 0 THEN NULL
    ELSE (
      date_trunc('day', now())
      - make_interval(years => 20 + (gs % 35), days => (gs * 17) % 365)
    )
  END AS "BirthDate",
  (gs % 11 <> 0) AS "IsActive",
  now() - make_interval(days => 120 + ((gs * 5) % 320)) AS "LastPurchaseDate",
  0::integer AS "TotalPurchases",
  0::numeric(18,2) AS "TotalLifetimeValue"
FROM generate_series(1, 80) AS gs;

INSERT INTO "Customers" (
  "Id",
  "Name",
  "Email",
  "Phone",
  "BirthDate",
  "IsActive",
  "LastPurchaseDate",
  "TotalPurchases",
  "TotalLifetimeValue"
)
SELECT
  "Id",
  "Name",
  "Email",
  "Phone",
  "BirthDate",
  "IsActive",
  "LastPurchaseDate",
  "TotalPurchases",
  "TotalLifetimeValue"
FROM tmp_customers;

CREATE TEMP TABLE tmp_products AS
SELECT
  gen_random_uuid() AS "Id",
  gs AS "SeedNo",
  format(
    '%s %s',
    (ARRAY[
      'Auriculares',
      'Mouse',
      'Teclado',
      'Monitor',
      'Lampara',
      'Mochila',
      'Botella',
      'Agenda',
      'Cargador',
      'Soporte'
    ])[1 + ((gs - 1) % 10)],
    lpad(gs::text, 3, '0')
  ) AS "Name",
  format(
    'Producto demo %s para pruebas de dashboard, ventas, stock y reportes.',
    gs
  ) AS "Description",
  round((12 + (gs * 4.35) + ((gs % 7) * 2.15))::numeric, 2) AS "Price",
  0::integer AS "Stock",
  (gs % 14 <> 0) AS "IsActive"
FROM generate_series(1, 48) AS gs;

INSERT INTO "Products" (
  "Id",
  "Name",
  "Description",
  "Price",
  "Stock",
  "IsActive"
)
SELECT
  "Id",
  "Name",
  "Description",
  "Price",
  "Stock",
  "IsActive"
FROM tmp_products;

CREATE TEMP TABLE tmp_sales_base AS
WITH seeded_sales AS (
  SELECT
    gs,
    (ARRAY[2026, 2025, 2024, 2023])[1 + ((gs - 1) / 120)] AS "SaleYear",
    1 + ((gs - 1) % 120) AS "YearSeed"
  FROM generate_series(1, 480) AS gs
)
SELECT
  gen_random_uuid() AS "Id",
  ss.gs AS "SeedNo",
  CASE
    WHEN ss.gs % 5 = 0 THEN NULL
    ELSE (
      SELECT c."Id"
      FROM tmp_customers AS c
      ORDER BY md5(c."Id"::text || '-' || ss.gs::text)
      LIMIT 1
    )
  END AS "CustomerId",
  CASE
    WHEN ss.gs % 6 = 0 THEN 'Transfer'
    WHEN ss.gs % 5 = 0 THEN 'Mercado Pago'
    WHEN ss.gs % 2 = 0 THEN 'Card'
    ELSE 'Cash'
  END AS "PaymentMethod",
  (ss.gs % 7 = 0) AS "IsDebt",
  CASE
    WHEN ss.gs % 7 = 0 THEN format('Venta fiada #%s', lpad(ss.gs::text, 4, '0'))
    WHEN ss.gs % 9 = 0 THEN 'Cliente frecuente'
    WHEN ss.gs % 13 = 0 THEN 'Entrega parcial'
    ELSE NULL
  END AS "Notes",
  make_timestamptz(
    ss."SaleYear",
    1 + ((ss."YearSeed" * 5) % 12),
    1,
    8 + ((ss."YearSeed" * 3) % 10),
    (ss."YearSeed" * 11) % 60,
    0,
    'UTC'
  )
  + make_interval(days => (ss."YearSeed" * 7) % 28) AS "CreatedAt"
FROM seeded_sales AS ss;

CREATE TEMP TABLE tmp_sale_items AS
SELECT
  gen_random_uuid() AS "Id",
  s."Id" AS "SaleId",
  p."Id" AS "ProductId",
  (1 + ((s."SeedNo" + item_no) % 5))::integer AS "Quantity",
  p."Price" AS "UnitPrice",
  CASE
    WHEN (s."SeedNo" + item_no) % 6 = 0
      THEN round((p."Price" * (0.82 + (((s."SeedNo" + item_no) % 5) * 0.03)))::numeric, 2)
    ELSE NULL
  END AS "SpecialPrice"
FROM tmp_sales_base AS s
CROSS JOIN LATERAL generate_series(1, 1 + (s."SeedNo" % 4)) AS item_no
CROSS JOIN LATERAL (
  SELECT p2."Id", p2."Price"
  FROM tmp_products AS p2
  ORDER BY md5(p2."Id"::text || '-' || s."SeedNo"::text || '-' || item_no::text)
  LIMIT 1
) AS p;

CREATE TEMP TABLE tmp_sales AS
SELECT
  s."Id",
  s."CreatedAt",
  round(sum(si."Quantity" * COALESCE(si."SpecialPrice", si."UnitPrice"))::numeric, 2) AS "Total",
  s."CustomerId",
  s."PaymentMethod",
  s."Notes",
  s."IsDebt"
FROM tmp_sales_base AS s
JOIN tmp_sale_items AS si
  ON si."SaleId" = s."Id"
GROUP BY
  s."Id",
  s."CreatedAt",
  s."CustomerId",
  s."PaymentMethod",
  s."Notes",
  s."IsDebt";

INSERT INTO "Sales" (
  "Id",
  "CreatedAt",
  "Total",
  "CustomerId",
  "PaymentMethod",
  "Notes",
  "IsDebt"
)
SELECT
  "Id",
  "CreatedAt",
  "Total",
  "CustomerId",
  "PaymentMethod",
  "Notes",
  "IsDebt"
FROM tmp_sales;

INSERT INTO "SaleItems" (
  "Id",
  "ProductId",
  "Quantity",
  "SaleId",
  "UnitPrice",
  "SpecialPrice"
)
SELECT
  "Id",
  "ProductId",
  "Quantity",
  "SaleId",
  "UnitPrice",
  "SpecialPrice"
FROM tmp_sale_items;

INSERT INTO "Costs" (
  "Id",
  "Name",
  "Amount",
  "DateIncurred",
  "Description"
)
WITH seeded_costs AS (
  SELECT
    gs,
    (ARRAY[2026, 2025, 2024, 2023])[1 + ((gs - 1) / 60)] AS "CostYear",
    1 + ((gs - 1) % 60) AS "YearSeed"
  FROM generate_series(1, 240) AS gs
)
SELECT
  gen_random_uuid() AS "Id",
  format(
    '%s %s',
    (ARRAY[
      'Alquiler',
      'Sueldos',
      'Marketing',
      'Servicios',
      'Logistica',
      'Impuestos',
      'Mantenimiento',
      'Software'
    ])[1 + ((sc.gs - 1) % 8)],
    lpad(sc.gs::text, 3, '0')
  ) AS "Name",
  round((65 + (sc.gs * 8.90) + ((sc.gs % 11) * 13.75))::numeric, 2) AS "Amount",
  make_timestamptz(
    sc."CostYear",
    1 + ((sc."YearSeed" * 4) % 12),
    1,
    7 + ((sc."YearSeed" * 5) % 8),
    (sc."YearSeed" * 9) % 60,
    0,
    'UTC'
  )
  + make_interval(days => (sc."YearSeed" * 9) % 28) AS "DateIncurred",
  format('Costo demo %s para pruebas de reportes y periodos.', sc.gs) AS "Description"
FROM seeded_costs AS sc;

INSERT INTO "InventoryMovements" (
  "Id",
  "ProductId",
  "Type",
  "Reason",
  "Quantity",
  "CreatedAt"
)
SELECT
  gen_random_uuid(),
  p."Id",
  0,
  1,
  90 + ((p."SeedNo" * 7) % 70),
  now() - make_interval(days => 600 - (p."SeedNo" * 3))
FROM tmp_products AS p;

INSERT INTO "InventoryMovements" (
  "Id",
  "ProductId",
  "Type",
  "Reason",
  "Quantity",
  "CreatedAt"
)
SELECT
  gen_random_uuid(),
  si."ProductId",
  1,
  0,
  si."Quantity",
  s."CreatedAt"
FROM "SaleItems" AS si
JOIN "Sales" AS s
  ON s."Id" = si."SaleId";

INSERT INTO "InventoryMovements" (
  "Id",
  "ProductId",
  "Type",
  "Reason",
  "Quantity",
  "CreatedAt"
)
SELECT
  gen_random_uuid(),
  p."Id",
  CASE WHEN p."SeedNo" % 2 = 0 THEN 0 ELSE 1 END,
  2,
  1 + (p."SeedNo" % 8),
  now() - make_interval(days => 30 + (p."SeedNo" % 120))
FROM tmp_products AS p
WHERE p."SeedNo" % 3 = 0;

WITH stock_by_product AS (
  SELECT
    im."ProductId",
    GREATEST(
      SUM(
        CASE
          WHEN im."Type" = 0 THEN im."Quantity"
          ELSE -im."Quantity"
        END
      ),
      0
    )::integer AS "Stock"
  FROM "InventoryMovements" AS im
  GROUP BY im."ProductId"
)
UPDATE "Products" AS p
SET "Stock" = sbp."Stock"
FROM stock_by_product AS sbp
WHERE p."Id" = sbp."ProductId";

WITH customer_rollup AS (
  SELECT
    s."CustomerId",
    COUNT(*)::integer AS "TotalPurchases",
    round(SUM(s."Total")::numeric, 2) AS "TotalLifetimeValue",
    MAX(s."CreatedAt") AS "LastPurchaseDate"
  FROM "Sales" AS s
  WHERE s."CustomerId" IS NOT NULL
  GROUP BY s."CustomerId"
)
UPDATE "Customers" AS c
SET
  "TotalPurchases" = cr."TotalPurchases",
  "TotalLifetimeValue" = cr."TotalLifetimeValue",
  "LastPurchaseDate" = cr."LastPurchaseDate"
FROM customer_rollup AS cr
WHERE c."Id" = cr."CustomerId";

INSERT INTO "Notifications" (
  "Id",
  "Title",
  "Date",
  "IsSeen",
  "CustomerId"
)
SELECT
  gen_random_uuid(),
  format('Cumpleanos cercano de %s', c."Name"),
  now() + make_interval(days => (c."SeedNo" % 45)),
  (c."SeedNo" % 4 = 0),
  c."Id"
FROM tmp_customers AS c
WHERE c."BirthDate" IS NOT NULL
  AND c."SeedNo" <= 50;

INSERT INTO "Notifications" (
  "Id",
  "Title",
  "Date",
  "IsSeen",
  "CustomerId"
)
SELECT
  gen_random_uuid(),
  format(
    'Recordatorio de cobro: %s por USD %s',
    COALESCE(c."Name", 'cliente ocasional'),
    to_char(s."Total", 'FM999999990.00')
  ),
  s."CreatedAt" + interval '7 days',
  (row_number() OVER (ORDER BY s."CreatedAt")) % 3 = 0,
  s."CustomerId"
FROM "Sales" AS s
LEFT JOIN "Customers" AS c
  ON c."Id" = s."CustomerId"
WHERE s."IsDebt" = true;

COMMIT;

SELECT 'Customers' AS "Table", COUNT(*) AS "Rows" FROM "Customers"
UNION ALL
SELECT 'Products', COUNT(*) FROM "Products"
UNION ALL
SELECT 'Sales', COUNT(*) FROM "Sales"
UNION ALL
SELECT 'SaleItems', COUNT(*) FROM "SaleItems"
UNION ALL
SELECT 'Costs', COUNT(*) FROM "Costs"
UNION ALL
SELECT 'InventoryMovements', COUNT(*) FROM "InventoryMovements"
UNION ALL
SELECT 'Notifications', COUNT(*) FROM "Notifications"
ORDER BY "Table";
