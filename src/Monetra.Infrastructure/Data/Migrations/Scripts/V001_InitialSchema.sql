-- =============================================
-- Monetra - Schema Inicial
-- Versão: 1.0.0
-- =============================================

-- Extensão para UUID
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- =============================================
-- Tabela: users
-- =============================================
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(200) NOT NULL,
    email VARCHAR(200) NOT NULL,
    password_hash VARCHAR(300) NOT NULL,

    -- Autenticação
    email_verified_at TIMESTAMPTZ,
    last_login_at TIMESTAMPTZ,
    last_password_change_at TIMESTAMPTZ,
    failed_login_attempts INTEGER DEFAULT 0,
    locked_until TIMESTAMPTZ,
    two_factor_enabled BOOLEAN DEFAULT false,
    two_factor_secret VARCHAR(100),

    -- Role
    role VARCHAR(30) DEFAULT 'user',

    -- Status
    is_active BOOLEAN DEFAULT true,
    is_premium BOOLEAN DEFAULT false,
    premium_until TIMESTAMPTZ,

    -- Preferências
    currency VARCHAR(3) DEFAULT 'BRL',
    fiscal_year_start INTEGER DEFAULT 1,

    -- Timestamps
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    deleted_at TIMESTAMPTZ
);

-- Índices users
CREATE UNIQUE INDEX idx_users_email ON users(email) WHERE deleted_at IS NULL;
CREATE INDEX idx_users_email_login ON users(email) INCLUDE (id, password_hash, is_active, locked_until);
CREATE INDEX idx_users_premium ON users(is_premium, premium_until);
CREATE INDEX idx_users_deleted ON users(deleted_at) WHERE deleted_at IS NOT NULL;

-- =============================================
-- Tabela: persons
-- =============================================
CREATE TABLE persons (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,

    -- Dados pessoais
    phone VARCHAR(20),
    birth_date DATE,
    occupation VARCHAR(100),
    monthly_income_range VARCHAR(30),

    -- Endereço
    city VARCHAR(100),
    state VARCHAR(2),
    country VARCHAR(100) DEFAULT 'Brasil',

    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE UNIQUE INDEX idx_persons_user ON persons(user_id);

-- =============================================
-- Tabela: bank_accounts
-- =============================================
CREATE TABLE bank_accounts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,

    name VARCHAR(100) NOT NULL,
    account_type VARCHAR(30) NOT NULL,
    bank_name VARCHAR(100),
    bank_code VARCHAR(10),
    agency VARCHAR(20),
    account_number VARCHAR(30),
    account_digit VARCHAR(5),

    -- Saldo
    balance DECIMAL(15,2) DEFAULT 0,
    initial_balance DECIMAL(15,2) DEFAULT 0,
    balance_date DATE,

    -- Aparência
    color VARCHAR(7) DEFAULT '#10B981',
    icon VARCHAR(50) DEFAULT 'account_balance',

    -- Status
    is_active BOOLEAN DEFAULT true,
    is_archived BOOLEAN DEFAULT false,
    include_in_totals BOOLEAN DEFAULT true,
    display_order INTEGER DEFAULT 0,

    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_bank_accounts_user ON bank_accounts(user_id);
CREATE INDEX idx_bank_accounts_active ON bank_accounts(user_id, is_active) WHERE is_active = true;

-- =============================================
-- Tabela: bank_account_balances
-- =============================================
CREATE TABLE bank_account_balances (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    bank_account_id UUID NOT NULL REFERENCES bank_accounts(id) ON DELETE CASCADE,

    balance DECIMAL(15,2) NOT NULL,
    balance_date DATE NOT NULL,

    created_at TIMESTAMPTZ DEFAULT NOW(),

    UNIQUE(bank_account_id, balance_date)
);

CREATE INDEX idx_balance_account ON bank_account_balances(bank_account_id, balance_date DESC);

-- =============================================
-- Tabela: transaction_categories
-- =============================================
CREATE TABLE transaction_categories (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,

    name VARCHAR(100) NOT NULL,
    description VARCHAR(300),
    icon VARCHAR(50) DEFAULT 'category',
    color VARCHAR(7) DEFAULT '#6B7280',

    transaction_type VARCHAR(10) NOT NULL,

    -- Hierarquia
    parent_id UUID REFERENCES transaction_categories(id) ON DELETE SET NULL,
    level INTEGER DEFAULT 0,

    -- Orçamento
    monthly_budget_limit DECIMAL(15,2),

    is_system BOOLEAN DEFAULT false,
    is_active BOOLEAN DEFAULT true,
    display_order INTEGER DEFAULT 0,

    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_categories_user ON transaction_categories(user_id);
CREATE INDEX idx_categories_type ON transaction_categories(user_id, transaction_type);
CREATE INDEX idx_categories_parent ON transaction_categories(parent_id);
CREATE INDEX idx_categories_system ON transaction_categories(is_system) WHERE is_system = true;

-- =============================================
-- Tabela: transactions
-- =============================================
CREATE TABLE transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    bank_account_id UUID NOT NULL REFERENCES bank_accounts(id) ON DELETE RESTRICT,
    category_id UUID REFERENCES transaction_categories(id) ON DELETE SET NULL,

    -- Valores
    amount DECIMAL(15,2) NOT NULL,
    transaction_type VARCHAR(10) NOT NULL,
    balance_before DECIMAL(15,2),
    balance_after DECIMAL(15,2),

    -- Datas
    transaction_date DATE NOT NULL,
    due_date DATE,
    paid_date DATE,
    competence_date DATE,

    -- Descrição
    description VARCHAR(300) NOT NULL,
    notes TEXT,

    -- Informações adicionais
    payment_method VARCHAR(30),
    document_number VARCHAR(100),
    receipt_url VARCHAR(500),

    -- Status
    status VARCHAR(20) DEFAULT 'pending',
    is_recurring BOOLEAN DEFAULT false,
    recurrence_id UUID,

    -- Conciliação
    is_reconciled BOOLEAN DEFAULT false,
    reconciled_at TIMESTAMPTZ,

    -- Tags
    tags TEXT[] DEFAULT '{}',

    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    deleted_at TIMESTAMPTZ
);

-- Índices transactions
CREATE INDEX idx_transactions_user_date ON transactions(user_id, transaction_date DESC);
CREATE INDEX idx_transactions_account ON transactions(bank_account_id, transaction_date DESC);
CREATE INDEX idx_transactions_category ON transactions(category_id);
CREATE INDEX idx_transactions_type ON transactions(user_id, transaction_type, transaction_date DESC);
CREATE INDEX idx_transactions_status ON transactions(user_id, status);
CREATE INDEX idx_transactions_due_date ON transactions(user_id, due_date) WHERE due_date IS NOT NULL AND status = 'pending';
CREATE INDEX idx_transactions_monthly ON transactions(user_id, transaction_type, DATE_TRUNC('month', transaction_date::timestamp));
CREATE INDEX idx_transactions_search ON transactions USING gin(to_tsvector('portuguese', description));
CREATE INDEX idx_transactions_tags ON transactions USING gin(tags);
CREATE INDEX idx_transactions_deleted ON transactions(deleted_at) WHERE deleted_at IS NOT NULL;

-- =============================================
-- Função: Atualizar updated_at automaticamente
-- =============================================
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Triggers para atualizar updated_at
CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON users FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_persons_updated_at BEFORE UPDATE ON persons FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_bank_accounts_updated_at BEFORE UPDATE ON bank_accounts FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_transaction_categories_updated_at BEFORE UPDATE ON transaction_categories FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_transactions_updated_at BEFORE UPDATE ON transactions FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
