\c drivolution;

CREATE TABLE IF NOT EXISTS production_line (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    location VARCHAR(100),
    status VARCHAR(50),
    capacity INTEGER
);

CREATE TABLE IF NOT EXISTS resource (
    id SERIAL PRIMARY KEY,
    is_human BOOLEAN NOT NULL,
    status VARCHAR(50)
);

CREATE TABLE IF NOT EXISTS model (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    version VARCHAR(50),
    type VARCHAR(100)
);

CREATE TABLE IF NOT EXISTS client_order (
    id SERIAL PRIMARY KEY,
    order_number VARCHAR(100) NOT NULL,
    order_date TIMESTAMP NOT NULL,
    customer_name VARCHAR(150) NOT NULL,
    quantity INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS manufacturing_phase (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    estimated_duration INTEGER,
    max_acceptable_severity VARCHAR(20),
    rework_severity VARCHAR(20),
    time_threshold_pct INTEGER NOT NULL DEFAULT 150
);

CREATE TABLE IF NOT EXISTS material (
    id SERIAL PRIMARY KEY,
    item VARCHAR(100) NOT NULL,
    part_number VARCHAR(100)
);

CREATE TABLE IF NOT EXISTS workstation (
    id SERIAL PRIMARY KEY,
    production_line_id INTEGER NOT NULL,
    type VARCHAR(100),
    kind VARCHAR(20),
    manufacturing_phase_id INTEGER,

    CONSTRAINT chk_workstation_kind CHECK (kind IN ('human', 'hybrid', 'machine')),

    CONSTRAINT fk_workstation_production_line
        FOREIGN KEY (production_line_id)
        REFERENCES production_line(id),

    CONSTRAINT fk_workstation_manufacturing_phase
        FOREIGN KEY (manufacturing_phase_id)
        REFERENCES manufacturing_phase(id)
        ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS support (
    id SERIAL PRIMARY KEY,
    production_line_id INTEGER NOT NULL,
    rfid_tag VARCHAR(100),
    type VARCHAR(100),

    CONSTRAINT fk_support_production_line
        FOREIGN KEY (production_line_id)
        REFERENCES production_line(id)
);

CREATE TABLE IF NOT EXISTS manufacturing_order (
    id SERIAL PRIMARY KEY,
    client_order_id INTEGER NOT NULL,
    manufacturing_order_number VARCHAR(100) NOT NULL,
    start_date TIMESTAMP NOT NULL,
    end_date TIMESTAMP,
    status VARCHAR(50),

    CONSTRAINT fk_manufacturing_order_client_order
        FOREIGN KEY (client_order_id)
        REFERENCES client_order(id)
);

CREATE TABLE IF NOT EXISTS product (
    id SERIAL PRIMARY KEY,
    manufacturing_order_id INTEGER NOT NULL,
    model_id INTEGER NOT NULL,
    serial_number VARCHAR(100),
    lot_number VARCHAR(100),
    color_code VARCHAR(50),
    production_date TIMESTAMP,

    CONSTRAINT fk_product_manufacturing_order
        FOREIGN KEY (manufacturing_order_id)
        REFERENCES manufacturing_order(id),

    CONSTRAINT fk_product_model
        FOREIGN KEY (model_id)
        REFERENCES model(id)
);

CREATE TABLE IF NOT EXISTS workstation_status (
    id SERIAL PRIMARY KEY,
    workstation_id INTEGER NOT NULL,
    status VARCHAR(50) NOT NULL,
    timestamp TIMESTAMP NOT NULL,

    CONSTRAINT fk_workstation_status_workstation
        FOREIGN KEY (workstation_id)
        REFERENCES workstation(id)
);

CREATE TABLE IF NOT EXISTS workstation_allocation (
    id SERIAL PRIMARY KEY,
    resource_id INTEGER NOT NULL,
    workstation_id INTEGER NOT NULL,
    status VARCHAR(50),
    start_date TIMESTAMP NOT NULL,
    end_date TIMESTAMP,

    CONSTRAINT fk_workstation_allocation_resource
        FOREIGN KEY (resource_id)
        REFERENCES resource(id),

    CONSTRAINT fk_workstation_allocation_workstation
        FOREIGN KEY (workstation_id)
        REFERENCES workstation(id)
);

CREATE TABLE IF NOT EXISTS localization_history (
    id SERIAL PRIMARY KEY,
    support_id INTEGER NOT NULL,
    workstation_id INTEGER NOT NULL,
    datetime_ini TIMESTAMP NOT NULL,
    datetime_end TIMESTAMP,
    status VARCHAR(50),

    CONSTRAINT fk_localization_history_support
        FOREIGN KEY (support_id)
        REFERENCES support(id),

    CONSTRAINT fk_localization_history_workstation
        FOREIGN KEY (workstation_id)
        REFERENCES workstation(id)
);

CREATE TABLE IF NOT EXISTS supported_product (
    id SERIAL PRIMARY KEY,
    support_id INTEGER NOT NULL,
    product_id INTEGER,
    datetime_ini TIMESTAMP NOT NULL,
    datetime_end TIMESTAMP,

    CONSTRAINT fk_supported_product_support
        FOREIGN KEY (support_id)
        REFERENCES support(id),

    CONSTRAINT fk_supported_product_product
        FOREIGN KEY (product_id)
        REFERENCES product(id)
);

CREATE TABLE IF NOT EXISTS phase_sequence (
    id SERIAL PRIMARY KEY,
    "order" INTEGER NOT NULL,
    manufacturing_phase_id INTEGER NOT NULL,
    model_id INTEGER NOT NULL,

    CONSTRAINT fk_phase_sequence_manufacturing_phase
        FOREIGN KEY (manufacturing_phase_id)
        REFERENCES manufacturing_phase(id),

    CONSTRAINT fk_phase_sequence_model
        FOREIGN KEY (model_id)
        REFERENCES model(id)
);

CREATE TABLE IF NOT EXISTS product_phase (
    id SERIAL PRIMARY KEY,
    notes TEXT,
    result VARCHAR(100),
    datetime_ini TIMESTAMP NOT NULL,
    datetime_end TIMESTAMP,
    manufacturing_phase_id INTEGER NOT NULL,
    product_id INTEGER NOT NULL,
    workstation_id INTEGER NOT NULL,
    quality_id INTEGER,

    CONSTRAINT fk_product_phase_manufacturing_phase
        FOREIGN KEY (manufacturing_phase_id)
        REFERENCES manufacturing_phase(id),

    CONSTRAINT fk_product_phase_product
        FOREIGN KEY (product_id)
        REFERENCES product(id),

    CONSTRAINT fk_product_phase_workstation
        FOREIGN KEY (workstation_id)
        REFERENCES workstation(id)
);

CREATE TABLE IF NOT EXISTS quality_check (
    id SERIAL PRIMARY KEY,
    product_id INTEGER NOT NULL,
    manufacturing_phase_id INTEGER NOT NULL,
    severity VARCHAR(20) NOT NULL,
    notes TEXT,
    status VARCHAR(50),

    CONSTRAINT fk_quality_check_product
        FOREIGN KEY (product_id)
        REFERENCES product(id),

    CONSTRAINT fk_quality_check_manufacturing_phase
        FOREIGN KEY (manufacturing_phase_id)
        REFERENCES manufacturing_phase(id)
);

ALTER TABLE product_phase
ADD CONSTRAINT fk_product_phase_quality_check
FOREIGN KEY (quality_id)
REFERENCES quality_check(id);

CREATE TABLE IF NOT EXISTS model_material (
    id SERIAL PRIMARY KEY,
    model_id INTEGER NOT NULL,
    material_id INTEGER NOT NULL,
    manufacturing_phase_id INTEGER NOT NULL,
    quantity NUMERIC(10,2) NOT NULL,
    unit VARCHAR(50),

    CONSTRAINT fk_model_material_model
        FOREIGN KEY (model_id)
        REFERENCES model(id),

    CONSTRAINT fk_model_material_material
        FOREIGN KEY (material_id)
        REFERENCES material(id),

    CONSTRAINT fk_model_material_manufacturing_phase
        FOREIGN KEY (manufacturing_phase_id)
        REFERENCES manufacturing_phase(id)
);

CREATE TABLE IF NOT EXISTS config (
    id SERIAL PRIMARY KEY,
    model_id INTEGER NOT NULL,
    item VARCHAR(100) NOT NULL,
    allow_multiple BOOLEAN NOT NULL DEFAULT FALSE,

    CONSTRAINT fk_config_model
        FOREIGN KEY (model_id)
        REFERENCES model(id)
);

CREATE TABLE IF NOT EXISTS config_option (
    id SERIAL PRIMARY KEY,
    config_id INTEGER NOT NULL,
    value VARCHAR(255) NOT NULL,
    is_default BOOLEAN DEFAULT FALSE,

    CONSTRAINT fk_config_option_config
        FOREIGN KEY (config_id)
        REFERENCES config(id)
);

CREATE TABLE IF NOT EXISTS product_config (
    id SERIAL PRIMARY KEY,
    product_id INTEGER NOT NULL,
    config_option_id INTEGER NOT NULL,

    CONSTRAINT fk_product_config_product
        FOREIGN KEY (product_id)
        REFERENCES product(id),

    CONSTRAINT fk_product_config_option
        FOREIGN KEY (config_option_id)
        REFERENCES config_option(id),

    CONSTRAINT uq_product_config
        UNIQUE (product_id, config_option_id)
);

CREATE TABLE IF NOT EXISTS alert (
    id SERIAL PRIMARY KEY,
    type VARCHAR(50) NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'open',
    product_id INTEGER NOT NULL,
    product_phase_id INTEGER NOT NULL,
    triggered_at TIMESTAMP NOT NULL,
    acknowledged_at TIMESTAMP,
    resolved_at TIMESTAMP,
    notes TEXT,
    product_serial VARCHAR(100) NOT NULL,
    phase_name VARCHAR(100) NOT NULL,
    threshold_pct INTEGER,
    estimated_duration INTEGER,
    order_from INTEGER,
    order_to INTEGER,

    CONSTRAINT fk_alert_product
        FOREIGN KEY (product_id)
        REFERENCES product(id),

    CONSTRAINT fk_alert_product_phase
        FOREIGN KEY (product_phase_id)
        REFERENCES product_phase(id)
);

CREATE TABLE IF NOT EXISTS phase_time_coefficient (
    id SERIAL PRIMARY KEY,
    manufacturing_phase_id INTEGER NOT NULL,
    config_option_id INTEGER,
    production_line_id INTEGER,
    model_id INTEGER,
    weight_seconds NUMERIC(10,2) NOT NULL,
    trained_at TIMESTAMP NOT NULL,

    CONSTRAINT fk_phase_time_coefficient_phase
        FOREIGN KEY (manufacturing_phase_id)
        REFERENCES manufacturing_phase(id),

    CONSTRAINT fk_phase_time_coefficient_option
        FOREIGN KEY (config_option_id)
        REFERENCES config_option(id),

    CONSTRAINT fk_phase_time_coefficient_line
        FOREIGN KEY (production_line_id)
        REFERENCES production_line(id),

    CONSTRAINT fk_phase_time_coefficient_model
        FOREIGN KEY (model_id)
        REFERENCES model(id)
);

CREATE TABLE IF NOT EXISTS app_user (
    id              SERIAL PRIMARY KEY,
    name            VARCHAR(100) NOT NULL,
    email           VARCHAR(150) NOT NULL UNIQUE,
    password_hash   VARCHAR(255) NOT NULL,
    role            VARCHAR(20)  NOT NULL DEFAULT 'operator',
    status          VARCHAR(20)  NOT NULL DEFAULT 'active',
    must_change_password BOOLEAN NOT NULL DEFAULT true,
    created_at      TIMESTAMP    NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_app_user_role   CHECK (role   IN ('admin', 'operator', 'client', 'manager')),
    CONSTRAINT chk_app_user_status CHECK (status IN ('active', 'inactive'))
);

CREATE TABLE IF NOT EXISTS workstation_presence (
    id               SERIAL PRIMARY KEY,
    app_user_id      INTEGER NOT NULL,
    workstation_id   INTEGER NOT NULL,
    checked_in_at    TIMESTAMP NOT NULL DEFAULT NOW(),
    checked_out_at   TIMESTAMP,

    CONSTRAINT fk_wp_app_user
        FOREIGN KEY (app_user_id)
        REFERENCES app_user(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_wp_workstation
        FOREIGN KEY (workstation_id)
        REFERENCES workstation(id)
        ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS audit_log (
    id           SERIAL PRIMARY KEY,
    user_id      INTEGER NOT NULL,
    user_name    VARCHAR(150) NOT NULL,
    action       VARCHAR(20) NOT NULL,
    entity       VARCHAR(50) NOT NULL,
    entity_id    INTEGER NOT NULL,
    entity_label VARCHAR(255),
    created_at   TIMESTAMP NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_audit_log_action CHECK (action IN ('created', 'updated', 'deleted')),
    CONSTRAINT chk_audit_log_entity CHECK (entity IN ('car_model', 'phase', 'phase_sequence', 'production_line', 'workstation', 'support', 'order', 'user', 'config', 'config_option'))
);

CREATE INDEX IF NOT EXISTS idx_audit_log_user     ON audit_log(user_id);
CREATE INDEX IF NOT EXISTS idx_audit_log_entity   ON audit_log(entity, entity_id);
CREATE INDEX IF NOT EXISTS idx_audit_log_created  ON audit_log(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_wp_workstation ON workstation_presence(workstation_id);
CREATE INDEX IF NOT EXISTS idx_wp_user        ON workstation_presence(app_user_id);
CREATE INDEX IF NOT EXISTS idx_wp_active      ON workstation_presence(app_user_id, workstation_id)
    WHERE checked_out_at IS NULL;