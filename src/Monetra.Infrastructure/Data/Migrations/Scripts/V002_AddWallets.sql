-- =============================================
-- Monetra - Carteiras e Metas
-- =============================================

-- =============================================
-- Tabela: wallets
-- =============================================
CREATE TABLE wallets (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,

    name VARCHAR(100) NOT NULL,
    description TEXT,
    wallet_type VARCHAR(30) NOT NULL,
    icon VARCHAR(50) DEFAULT 'savings',
    color VARCHAR(7) DEFAULT '#F59E0B',

    -- Meta
    target_amount DECIMAL(15,2) NOT NULL,
    current_amount DECIMAL(15,2) DEFAULT 0,
    target_date DATE,

    -- Status
    status VARCHAR(20) DEFAULT 'active',
    is_archived BOOLEAN DEFAULT false,
    completed_at TIMESTAMPTZ,

    -- Contribuição automática
    auto_contribute BOOLEAN DEFAULT false,
    auto_contribute_amount DECIMAL(15,2),
    auto_contribute_frequency VARCHAR(20),
    auto_contribute_day INTEGER,

    display_order INTEGER DEFAULT 0,

    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_wallets_user ON wallets(user_id);
CREATE INDEX idx_wallets_status ON wallets(user_id, status) WHERE status = 'active';

CREATE TRIGGER update_wallets_updated_at BEFORE UPDATE ON wallets FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- =============================================
-- Tabela: wallet_transactions
-- =============================================
CREATE TABLE wallet_transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    wallet_id UUID NOT NULL REFERENCES wallets(id) ON DELETE CASCADE,
    transaction_id UUID REFERENCES transactions(id) ON DELETE SET NULL,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,

    amount DECIMAL(15,2) NOT NULL,
    type VARCHAR(10) NOT NULL,
    description VARCHAR(300),

    balance_before DECIMAL(15,2),
    balance_after DECIMAL(15,2),

    date DATE NOT NULL,

    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_wallet_tx_wallet ON wallet_transactions(wallet_id, date DESC);
CREATE INDEX idx_wallet_tx_user ON wallet_transactions(user_id, date DESC);
