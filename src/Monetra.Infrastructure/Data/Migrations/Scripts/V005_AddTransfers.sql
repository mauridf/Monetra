-- =============================================
-- Monetra - Transferências
-- =============================================

-- =============================================
-- Tabela: transfers
-- =============================================
CREATE TABLE transfers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,

    from_account_id UUID REFERENCES bank_accounts(id) ON DELETE RESTRICT,
    to_account_id UUID REFERENCES bank_accounts(id) ON DELETE RESTRICT,
    from_transaction_id UUID REFERENCES transactions(id) ON DELETE SET NULL,
    to_transaction_id UUID REFERENCES transactions(id) ON DELETE SET NULL,
    to_wallet_id UUID REFERENCES wallets(id) ON DELETE SET NULL,

    amount DECIMAL(15,2) NOT NULL,
    transfer_date DATE NOT NULL,
    description VARCHAR(300),

    fee DECIMAL(15,2) DEFAULT 0,
    fee_account_id UUID REFERENCES bank_accounts(id) ON DELETE SET NULL,

    status VARCHAR(20) DEFAULT 'completed',

    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_transfers_user ON transfers(user_id, transfer_date DESC);
CREATE INDEX idx_transfers_from ON transfers(from_account_id);
CREATE INDEX idx_transfers_to ON transfers(to_account_id);

CREATE TRIGGER update_transfers_updated_at BEFORE UPDATE ON transfers FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
