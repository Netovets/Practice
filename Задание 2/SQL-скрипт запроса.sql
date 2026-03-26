INSERT INTO counterparts (name, inn, address, phone, is_salesman, is_buyer)
SELECT
    rec->>'name',
    NULLIF(TRIM(rec->>'inn'), ''),
    rec->>'addres',
    rec->>'phone',
    (rec->>'salesman')::BOOLEAN,
    (rec->>'buyer')::BOOLEAN
FROM jsonb_array_elements('[
    {"id":"000000001","name":"ООО \"Поставка\"","inn":"","addres":"г.Пятигорск","phone":"+79198634592","salesman":true,"buyer":true},
    {"id":"000000002","name":"ООО \"Кинотеатр Квант\"","inn":"26320045123","addres":"г. Железноводск, ул. Мира, 123","phone":"+79884581555","salesman":true,"buyer":false},
    {"id":"000000008","name":"ООО \"Новый JDTO\"","inn":"26320045111","addres":"г. Железноводсу","phone":"+79884581555","salesman":true,"buyer":false},
    {"id":"000000003","name":"ООО \"Ромашка\"","inn":"4140784214","addres":"г. Омск, ул. Строителей, 294","phone":"+79882584546","salesman":false,"buyer":true},
    {"id":"000000009","name":"ООО \"Ипподром\"","inn":"5874045632","addres":"г. Уфа, ул. Набережная, 37","phone":"+79627486389","salesman":true,"buyer":true},
    {"id":"000000010","name":"ООО \"Ассоль\"","inn":"2629011278","addres":"г. Калуга, ул. Пушкина, 94","phone":"+79184572398","salesman":false,"buyer":true}
]'::jsonb) AS rec
ON CONFLICT (inn) DO UPDATE
    SET name        = EXCLUDED.name,
        address     = EXCLUDED.address,
        phone       = EXCLUDED.phone,
        is_salesman = EXCLUDED.is_salesman,
        is_buyer    = EXCLUDED.is_buyer;

SELECT id, name, inn, address, phone, is_salesman, is_buyer
FROM counterparts
ORDER BY id;


WITH
actual_material_price AS (
    SELECT DISTINCT ON (material_id)
        material_id,
        price
    FROM prices
    WHERE material_id IS NOT NULL
      AND valid_from <= CURRENT_DATE
    ORDER BY material_id, valid_from DESC
),

unit_material_cost AS (
    SELECT
        s.product_id,
        SUM(s.quantity * amp.price)  AS cost_per_unit
    FROM specifications s
    JOIN actual_material_price amp ON amp.material_id = s.material_id
    GROUP BY s.product_id
),

order_lines AS (
    SELECT
        co.id                                       AS order_id,
        co.order_date,
        buyer.name                                  AS buyer_name,
        seller.name                                 AS salesman_name,
        pr.name                                     AS product_name,
        pr.code                                     AS product_code,
        coi.quantity,
        coi.unit,
        coi.price                                   AS sale_price,
       
        coi.quantity * coi.price                    AS line_sale_amount,
        COALESCE(umc.cost_per_unit, 0)              AS unit_material_cost,

        coi.quantity * COALESCE(umc.cost_per_unit, 0) AS line_material_amount
    FROM customer_orders co
    JOIN counterparts buyer  ON buyer.id  = co.buyer_id
    JOIN counterparts seller ON seller.id = co.salesman_id
    JOIN customer_order_items coi ON coi.order_id   = co.id
    JOIN products pr              ON pr.id           = coi.product_id
    LEFT JOIN unit_material_cost umc ON umc.product_id = coi.product_id
)

SELECT
    order_id                                        AS "№ заказа",
    order_date                                      AS "Дата",
    buyer_name                                      AS "Покупатель",
    salesman_name                                   AS "Исполнитель",
    product_name                                    AS "Продукция",
    product_code                                    AS "Код",
    quantity                                        AS "Кол-во",
    unit                                            AS "Ед.изм.",
    sale_price                                      AS "Цена продажи",
    line_sale_amount                                AS "Сумма продажи",
    ROUND(unit_material_cost::NUMERIC, 2)           AS "Себест. матер./ед.",
    ROUND(line_material_amount::NUMERIC, 2)         AS "Стоим. матер. по строке",

    SUM(line_sale_amount)
        OVER (PARTITION BY order_id)                AS "Итого по заказу (продажа)",
    ROUND(SUM(line_material_amount)
        OVER (PARTITION BY order_id)::NUMERIC, 2)  AS "Итого по заказу (матер.)"
FROM order_lines
ORDER BY order_id, product_name;