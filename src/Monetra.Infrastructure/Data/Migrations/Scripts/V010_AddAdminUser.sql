-- =============================================
-- Monetra - Usuário Admin Padrão
-- =============================================

-- Senha: Admin@123 (hash BCrypt)
-- Este hash é apenas para desenvolvimento. Trocar em produção!
INSERT INTO users (id, name, email, password_hash, role, is_active, email_verified_at, is_premium) VALUES
(
    uuid_generate_v4(),
    'Administrador Monetra',
    'admin@monetra.com.br',
    '$2a$12$LJ3m4ys3Lk0TSwHCpNqrYeD3qRJGqZqOqOc9kYVPKH9PqFXuXqPCq', -- Admin@123
    'admin',
    true,
    NOW(),
    false
);
