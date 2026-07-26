-- =============================================
-- Monetra - Categorias Padrão do Sistema
-- =============================================

-- Categorias de Receita (Income)
INSERT INTO transaction_categories (id, user_id, name, description, icon, color, transaction_type, level, is_system, display_order, monthly_budget_limit) VALUES
(uuid_generate_v4(), NULL, 'Salário', 'Renda proveniente de salário', 'work', '#10B981', 'income', 0, true, 1, NULL),
(uuid_generate_v4(), NULL, 'Freela / Autônomo', 'Renda de trabalhos freelancer', 'code', '#3B82F6', 'income', 0, true, 2, NULL),
(uuid_generate_v4(), NULL, 'Investimentos', 'Rendimentos de investimentos', 'trending_up', '#8B5CF6', 'income', 0, true, 3, NULL),
(uuid_generate_v4(), NULL, 'Presente', 'Dinheiro recebido como presente', 'card_giftcard', '#EC4899', 'income', 0, true, 4, NULL),
(uuid_generate_v4(), NULL, 'Restituição', 'Restituição de impostos ou valores', 'reply', '#14B8A6', 'income', 0, true, 5, NULL),
(uuid_generate_v4(), NULL, 'Vendas', 'Renda de vendas', 'sell', '#F97316', 'income', 0, true, 6, NULL),
(uuid_generate_v4(), NULL, 'Outras Receitas', 'Outras fontes de renda', 'attach_money', '#6B7280', 'income', 0, true, 99, NULL);

-- Categorias de Despesa (Expense) - Categorias Pai
INSERT INTO transaction_categories (id, user_id, name, description, icon, color, transaction_type, level, is_system, display_order, monthly_budget_limit) VALUES
(uuid_generate_v4(), NULL, 'Moradia', 'Gastos com moradia', 'home', '#EF4444', 'expense', 0, true, 1, NULL),
(uuid_generate_v4(), NULL, 'Alimentação', 'Gastos com alimentação', 'restaurant', '#F59E0B', 'expense', 0, true, 2, NULL),
(uuid_generate_v4(), NULL, 'Transporte', 'Gastos com transporte', 'directions_car', '#3B82F6', 'expense', 0, true, 3, NULL),
(uuid_generate_v4(), NULL, 'Saúde', 'Gastos com saúde', 'local_hospital', '#EC4899', 'expense', 0, true, 4, NULL),
(uuid_generate_v4(), NULL, 'Educação', 'Gastos com educação', 'school', '#8B5CF6', 'expense', 0, true, 5, NULL),
(uuid_generate_v4(), NULL, 'Lazer', 'Gastos com lazer', 'sports_esports', '#F97316', 'expense', 0, true, 6, NULL),
(uuid_generate_v4(), NULL, 'Assinaturas', 'Assinaturas e serviços', 'subscriptions', '#6366F1', 'expense', 0, true, 7, NULL),
(uuid_generate_v4(), NULL, 'Vestuário', 'Gastos com roupas', 'checkroom', '#14B8A6', 'expense', 0, true, 8, NULL),
(uuid_generate_v4(), NULL, 'Utilidades', 'Contas e utilidades', 'build', '#6B7280', 'expense', 0, true, 9, NULL),
(uuid_generate_v4(), NULL, 'Pets', 'Gastos com animais', 'pets', '#A855F7', 'expense', 0, true, 10, NULL),
(uuid_generate_v4(), NULL, 'Crianças', 'Gastos com filhos', 'child_care', '#F43F5E', 'expense', 0, true, 11, NULL),
(uuid_generate_v4(), NULL, 'Outras Despesas', 'Outros gastos', 'more_horiz', '#9CA3AF', 'expense', 0, true, 99, NULL);

-- Subcategorias de Moradia
WITH parent AS (SELECT id FROM transaction_categories WHERE name = 'Moradia' AND is_system = true LIMIT 1)
INSERT INTO transaction_categories (id, user_id, name, description, icon, color, transaction_type, parent_id, level, is_system, display_order) VALUES
(uuid_generate_v4(), NULL, 'Aluguel', 'Pagamento de aluguel', 'apartment', '#EF4444', 'expense', (SELECT id FROM parent), 1, true, 1),
(uuid_generate_v4(), NULL, 'Condomínio', 'Taxa de condomínio', 'domain', '#EF4444', 'expense', (SELECT id FROM parent), 1, true, 2),
(uuid_generate_v4(), NULL, 'IPTU', 'Imposto predial', 'account_balance', '#EF4444', 'expense', (SELECT id FROM parent), 1, true, 3),
(uuid_generate_v4(), NULL, 'Manutenção', 'Reparos e manutenção', 'construction', '#EF4444', 'expense', (SELECT id FROM parent), 1, true, 4),
(uuid_generate_v4(), NULL, 'Água', 'Conta de água', 'water_drop', '#EF4444', 'expense', (SELECT id FROM parent), 1, true, 5),
(uuid_generate_v4(), NULL, 'Luz', 'Conta de energia', 'bolt', '#EF4444', 'expense', (SELECT id FROM parent), 1, true, 6),
(uuid_generate_v4(), NULL, 'Gás', 'Gás encanado ou botijão', 'local_fire_department', '#EF4444', 'expense', (SELECT id FROM parent), 1, true, 7);

-- Subcategorias de Alimentação
WITH parent AS (SELECT id FROM transaction_categories WHERE name = 'Alimentação' AND is_system = true LIMIT 1)
INSERT INTO transaction_categories (id, user_id, name, description, icon, color, transaction_type, parent_id, level, is_system, display_order) VALUES
(uuid_generate_v4(), NULL, 'Supermercado', 'Compras de supermercado', 'shopping_cart', '#F59E0B', 'expense', (SELECT id FROM parent), 1, true, 1),
(uuid_generate_v4(), NULL, 'Restaurante', 'Refeições em restaurantes', 'restaurant_menu', '#F59E0B', 'expense', (SELECT id FROM parent), 1, true, 2),
(uuid_generate_v4(), NULL, 'Delivery', 'Pedidos por aplicativo', 'delivery_dining', '#F59E0B', 'expense', (SELECT id FROM parent), 1, true, 3),
(uuid_generate_v4(), NULL, 'Feira', 'Compras na feira', 'storefront', '#F59E0B', 'expense', (SELECT id FROM parent), 1, true, 4);

-- Subcategorias de Transporte
WITH parent AS (SELECT id FROM transaction_categories WHERE name = 'Transporte' AND is_system = true LIMIT 1)
INSERT INTO transaction_categories (id, user_id, name, description, icon, color, transaction_type, parent_id, level, is_system, display_order) VALUES
(uuid_generate_v4(), NULL, 'Combustível', 'Gasolina, etanol, etc', 'local_gas_station', '#3B82F6', 'expense', (SELECT id FROM parent), 1, true, 1),
(uuid_generate_v4(), NULL, 'Estacionamento', 'Taxa de estacionamento', 'local_parking', '#3B82F6', 'expense', (SELECT id FROM parent), 1, true, 2),
(uuid_generate_v4(), NULL, 'Transporte Público', 'Ônibus, metrô, trem', 'directions_bus', '#3B82F6', 'expense', (SELECT id FROM parent), 1, true, 3),
(uuid_generate_v4(), NULL, 'Uber / Táxi', 'Corridas por aplicativo', 'local_taxi', '#3B82F6', 'expense', (SELECT id FROM parent), 1, true, 4);
