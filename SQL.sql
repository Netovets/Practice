
CREATE TABLE users (
    user_id    SERIAL       PRIMARY KEY,
    full_name  VARCHAR(100) NOT NULL,
    email      VARCHAR(100),
    phone      VARCHAR(20)
);

CREATE TABLE stores (
    store_id      SERIAL       PRIMARY KEY,
    store_name    VARCHAR(50)  NOT NULL,
    store_inn     BIGINT,                
    store_address VARCHAR(50),
    store_phone   VARCHAR(50),
    store_saleman BOOLEAN      NOT NULL DEFAULT FALSE,
    store_buyer   BOOLEAN      NOT NULL DEFAULT FALSE
);

CREATE TABLE specifications (
    specification_id       SERIAL       PRIMARY KEY,
    name_specification_id  VARCHAR(20)  NOT NULL,
    manufacturer           VARCHAR(20),
    materials              VARCHAR(20),
    unit_of_measurement    VARCHAR(50),
    quantity_specification INTEGER
);

CREATE TABLE products (
    product_id       SERIAL       PRIMARY KEY,
    specification_id INTEGER      NOT NULL REFERENCES specifications(specification_id),
    product_name     VARCHAR(50)  NOT NULL,
    nomenclature     VARCHAR(50),
    quantity_product INTEGER,
    unit_product     VARCHAR(50)
);

CREATE TABLE prices (
    price_id       SERIAL  PRIMARY KEY,
    product_id     INTEGER NOT NULL REFERENCES products(product_id),
    price_material INTEGER,
    price_product  INTEGER
);

CREATE TABLE cost_calculations (
    calc_id    SERIAL  PRIMARY KEY,
    product_id INTEGER NOT NULL REFERENCES products(product_id),
    price_id   INTEGER NOT NULL REFERENCES prices(price_id),
    quantity   INTEGER,
    total_cost INTEGER,
    unit_cost  INTEGER
);

CREATE TABLE orders (
    order_id        SERIAL         PRIMARY KEY,
    user_id         INTEGER        NOT NULL REFERENCES users(user_id),
    store_id        INTEGER        NOT NULL REFERENCES stores(store_id),
    order_date      DATE           NOT NULL DEFAULT CURRENT_DATE,
    unit_order      VARCHAR(50),
    executor        VARCHAR(50),
    price_order_pr  INTEGER,
    quantity_order  INTEGER,
    order_amount    NUMERIC(12, 2)        
);

CREATE TABLE product_orders (
    product_order_id SERIAL  PRIMARY KEY,
    product_id       INTEGER NOT NULL REFERENCES products(product_id),
    order_id         INTEGER NOT NULL REFERENCES orders(order_id),
    UNIQUE (product_id, order_id)
);

CREATE INDEX idx_products_specification_id  ON products(specification_id);
CREATE INDEX idx_prices_product_id          ON prices(product_id);
CREATE INDEX idx_cost_calc_product_id       ON cost_calculations(product_id);
CREATE INDEX idx_cost_calc_price_id         ON cost_calculations(price_id);
CREATE INDEX idx_orders_user_id             ON orders(user_id);
CREATE INDEX idx_orders_store_id            ON orders(store_id);
CREATE INDEX idx_product_orders_product_id  ON product_orders(product_id);
CREATE INDEX idx_product_orders_order_id    ON product_orders(order_id);