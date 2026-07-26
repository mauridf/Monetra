-- =============================================
-- Monetra - Cartões de Crédito e Faturas
-- =============================================

-- =============================================
-- Tabela: credit_cards
-- =============================================
CREATE TABLE credit_cards (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,

    name VARCHAR(100) NOT NULL,
    brand VARCHAR(30) NOT NULL,
    last_digits VARCHAR(4),

    -- Limite
    credit_limit DECIMAL(15,2) NOT NULL,
    available_limit DECIMAL(15,2),

    -- Fatura
    closing_day INTEGER NOT NULL,
    due_day INTEGER NOT NULL,

    -- Aparência
    color VARCHAR(7) DEFAULT '#EF4444',

    -- Status
    is_active BOOLEAN DEFAULT true,
    is_archived BOOLEAN DEFAULT false,
    display_order INTEGER DEFAULT 0,

    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_credit_cards_user ON credit_cards(user_id);

CREATE TRIGGER update_credit_cards_updated_at BEFORE UPDATE ON credit_cards FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- =============================================
-- Tabela: invoices
-- =============================================
CREATE TABLE invoices (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    credit_card_id UUID NOT NULL REFERENCES credit_cards(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,

    -- Período
    reference_month INTEGER NOT NULL,
    reference_year INTEGER NOT NULL,

    -- Datas
    closing_date DATE NOT NULL,
    due_date DATE NOT NULL,
    payment_date DATE,

    -- Valores
    total_amount DECIMAL(15,2) DEFAULT 0,
    minimum_payment DECIMAL(15,2),
    paid_amount DECIMAL(15,2),

    -- Status
    status VARCHAR(20) DEFAULT 'open',
    payment_transaction_id UUID REFERENCES transactions(id) ON DELETE SET NULL,

    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),

    UNIQUE(credit_card_id, reference_month, reference_year)
);

CREATE INDEX idx_invoices_card ON invoices(credit_card_id, reference_year DESC, reference_month DESC);
CREATE INDEX idx_invoices_due_date ON invoices(status, due_date) WHERE status IN ('open', 'closed');

CREATE TRIGGER update_invoices_updated_at BEFORE UPDATE ON invoices FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- =============================================
-- Tabela: invoice_transactions
-- =============================================
CREATE TABLE invoice_transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    invoice_id UUID NOT NULL REFERENCES invoices(id) ON DELETE CASCADE,
    category_id UUID REFERENCES transaction_categories(id) ON DELETE SET NULL,

    description VARCHAR(300) NOT NULL,
    amount DECIMAL(15,2) NOT NULL,
    purchase_date DATE NOT NULL,
    installments INTEGER DEFAULT 1,
    installment_number INTEGER DEFAULT 1,
    installment_total DECIMAL(15,2),

    merchant_name VARCHAR(200),

    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_invoice_tx_invoice ON invoice_transactions(invoice_id);
CREATE INDEX idx_invoice_tx_category ON invoice_transactions(category_id);
