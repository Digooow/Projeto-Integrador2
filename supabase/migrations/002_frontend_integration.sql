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

-- Policies complementares para os fluxos autenticados do sistema.
-- Os blocos DO tornam a migration segura para reexecucao no SQL Editor.
do $$ begin
    create policy "users_can_create_own_reservations" on reservations
        for insert to authenticated with check (requester_id = auth.uid()::text);
exception when duplicate_object then null; end $$;

do $$ begin
    create policy "owners_and_managers_can_update_reservations" on reservations
        for update to authenticated
        using (
            requester_id = auth.uid()::text
            or exists (select 1 from users where id = auth.uid()::text and role in ('Coordinator', 'Administrator'))
        )
        with check (
            requester_id = auth.uid()::text
            or exists (select 1 from users where id = auth.uid()::text and role in ('Coordinator', 'Administrator'))
        );
exception when duplicate_object then null; end $$;

do $$ begin
    create policy "only_administrators_can_delete_users" on users
        for delete to authenticated
        using (exists (select 1 from users manager where manager.id = auth.uid()::text and manager.role = 'Administrator'));
exception when duplicate_object then null; end $$;

grant usage on schema public to authenticated;
grant select on all tables in schema public to authenticated;
grant insert, update on reservations to authenticated;
