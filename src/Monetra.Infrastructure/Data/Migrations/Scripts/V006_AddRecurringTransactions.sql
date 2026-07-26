-- =============================================
-- Monetra - Transações Recorrentes
-- =============================================

-- =============================================
-- Tabela: recurring_transactions
-- =============================================
CREATE TABLE recurring_transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    bank_account_id UUID NOT NULL REFERENCES bank_accounts(id) ON DELETE CASCADE,
    category_id UUID REFERENCES transaction_categories(id) ON DELETE SET NULL,

    description VARCHAR(300) NOT NULL,
    amount DECIMAL(15,2) NOT NULL,
    transaction_type VARCHAR(10) NOT NULL,

    -- Recorrência
    recurrence_type VARCHAR(20) NOT NULL,
    interval_value INTEGER DEFAULT 1,
    interval_unit VARCHAR(10),
    day_of_month INTEGER,
    day_of_week INTEGER,
    month_of_year INTEGER,

    -- Ciclo
    start_date DATE NOT NULL,
    end_date DATE,
    next_execution DATE NOT NULL,
    max_executions INTEGER,
    executions_count INTEGER DEFAULT 0,

    -- Status
    is_active BOOLEAN DEFAULT true,
    auto_create BOOLEAN DEFAULT true,
    notify_before_days INTEGER,

    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_recurring_user ON recurring_transactions(user_id);
CREATE INDEX idx_recurring_next ON recurring_transactions(next_execution, is_active) WHERE is_active = true;

CREATE TRIGGER update_recurring_updated_at BEFORE UPDATE ON recurring_transactions FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
