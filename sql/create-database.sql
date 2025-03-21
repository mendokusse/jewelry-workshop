CREATE TABLE roles (
    role_id             INTEGER GENERATED ALWAYS AS IDENTITY    PRIMARY KEY,
    role_name           VARCHAR(30)    NOT NULL
);

CREATE TABLE users (
    user_id             INTEGER GENERATED ALWAYS AS IDENTITY    PRIMARY KEY,
    role_id             INT             NOT NULL,
    user_name           VARCHAR(50)    NOT NULL,
    user_password_hash  VARCHAR(50)    NOT NULL,
    user_phone          VARCHAR(15),
    user_email          VARCHAR(50),
    FOREIGN KEY (role_id) REFERENCES roles(role_id) ON DELETE CASCADE
);

CREATE TABLE statuses (
    status_id           INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    status_name         VARCHAR(30)    NOT NULL
);

CREATE TABLE product_types (
    product_type_id     INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    product_type_name   VARCHAR(30)    NOT NULL
);

CREATE TABLE orders (
    order_id            INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    product_type_id     INT     NOT NULL,
    customer_user_id    INT,
    manager_user_id     INT,
    jeweler_user_id     INT,
    status_id           INT     NOT NULL,
    order_comment       TEXT,
    order_price         DECIMAL,
    order_date          TIMESTAMP,
    order_update_date   TIMESTAMP,
    FOREIGN KEY (product_type_id) REFERENCES product_types(product_type_id) ON DELETE CASCADE,
    FOREIGN KEY (customer_user_id) REFERENCES users(user_id) ON DELETE SET NULL,
    FOREIGN KEY (manager_user_id) REFERENCES users(user_id) ON DELETE SET NULL,
    FOREIGN KEY (jeweler_user_id) REFERENCES users(user_id) ON DELETE SET NULL,
    FOREIGN KEY (status_id) REFERENCES statuses(status_id) ON DELETE CASCADE
);

CREATE TABLE units (
    unit_id             INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    unit_short_name     VARCHAR(5)     NOT NULL,
    unit_full_name      VARCHAR(30)    NOT NULL
);

CREATE TABLE materials (
    material_id         INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    unit_id             INT             NOT NULL,
    material_price      DECIMAL         NOT NULL,
    material_name       VARCHAR(30)    NOT NULL,
    material_quantity   FLOAT           NOT NULL,
    FOREIGN KEY (unit_id) REFERENCES units(unit_id) ON DELETE CASCADE
);

CREATE TABLE order_details (
    details_list_id     INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    order_id                INT     NOT NULL,
    material_id             INT     NOT NULL,
    order_material_weight   FLOAT   NOT NULL,
    FOREIGN KEY (order_id) REFERENCES orders(order_id) ON DELETE CASCADE,
    FOREIGN KEY (material_id) REFERENCES materials(material_id) ON DELETE CASCADE
);