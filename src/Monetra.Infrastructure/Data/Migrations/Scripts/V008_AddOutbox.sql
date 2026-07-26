-- =============================================
-- Monetra - Outbox Pattern
-- =============================================

-- =============================================
-- Tabela: outbox_messages
-- =============================================
CREATE TABLE outbox_messages (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    type VARCHAR(500) NOT NULL,
    content JSONB NOT NULL,
    headers JSONB DEFAULT '{}',

    status VARCHAR(20) DEFAULT 'pending',
    retry_count INTEGER DEFAULT 0,
    max_retries INTEGER DEFAULT 5,
    last_error TEXT,
    error_stack_trace TEXT,

    created_at TIMESTAMPTZ DEFAULT NOW(),
    processed_at TIMESTAMPTZ,
    sent_at TIMESTAMPTZ
);

CREATE INDEX idx_outbox_pending ON outbox_messages(status, created_at) WHERE status = 'pending';
