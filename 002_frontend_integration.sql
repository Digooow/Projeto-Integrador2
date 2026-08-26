-- Adds what the frontend (frontend/reserva-salas.html) needs that the original
-- schema didn't have yet: per-coordinator floor scoping and a free-text
-- "responsável" field on each reservation, plus a demo user roster so the
-- login screen has someone to pick from out of the box.

alter table users
    add column if not exists floors text[] not null default '{}';

alter table reservations
    add column if not exists responsavel text not null default '';

insert into users (id, name, email, role, active, floors) values
    ('u_renata',   'Renata Alves',      'renata.alves@senac.br',   'Administrator', true, '{}'),
    ('u_carlos',   'Carlos Menezes',    'carlos.menezes@senac.br', 'Coordinator',   true, '{"2º andar","3º andar"}'),
    ('u_fernanda', 'Fernanda Lima',     'fernanda.lima@senac.br',  'Teacher',       true, '{}'),
    ('u_joao',     'João Pedro Rocha',  'joao.rocha@senac.br',     'Teacher',       true, '{}')
on conflict (id) do nothing;
