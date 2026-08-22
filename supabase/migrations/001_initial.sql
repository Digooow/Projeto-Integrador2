create table if not exists users (
    id text primary key,
    name text not null,
    email text not null unique,
    role text not null check (role in ('Teacher', 'Collaborator', 'Coordinator', 'Administrator')),
    active boolean not null default true
);

create table if not exists rooms (
    id text primary key,
    name text not null,
    floor text not null,
    description text not null default '',
    capacity integer not null check (capacity > 0),
    active boolean not null default true
);

create table if not exists resources (
    id text primary key,
    name text not null unique
);

create table if not exists room_resources (
    room_id text not null references rooms(id),
    resource_id text not null references resources(id),
    primary key (room_id, resource_id)
);

create table if not exists reservations (
    id uuid primary key,
    requester_id text not null references users(id),
    room_id text not null references rooms(id),
    title text not null,
    attendees integer not null check (attendees > 0),
    status text not null default 'Pending' check (status in ('Pending', 'Approved', 'Rejected', 'Cancelled')),
    series_id uuid,
    created_at timestamptz not null default now(),
    decided_at timestamptz,
    decided_by text references users(id)
);

create table if not exists reservation_occurrences (
    id uuid primary key,
    reservation_id uuid not null references reservations(id) on delete cascade,
    starts_at timestamptz not null,
    ends_at timestamptz not null,
    check (ends_at > starts_at)
);

create index if not exists ix_reservations_room_status on reservations(room_id, status);
create index if not exists ix_occurrences_schedule on reservation_occurrences(starts_at, ends_at);

alter table users enable row level security;
alter table rooms enable row level security;
alter table resources enable row level security;
alter table room_resources enable row level security;
alter table reservations enable row level security;
alter table reservation_occurrences enable row level security;

create policy "public can view active rooms" on rooms for select using (active);
create policy "public can view resources" on resources for select using (true);
create policy "public can view room resources" on room_resources for select using (true);
create policy "public can view approved reservations" on reservations for select using (status = 'Approved');
create policy "public can view approved occurrences" on reservation_occurrences for select using (
    exists (select 1 from reservations where reservations.id = reservation_id and reservations.status = 'Approved')
);

insert into resources (id, name) values
    ('res_proj', 'Projetor'), ('res_ar', 'Ar-condicionado'), ('res_quadro', 'Quadro branco'),
    ('res_pc', 'Computadores'), ('res_som', 'Sistema de som'), ('res_tomadas', 'Tomadas extras')
on conflict (id) do nothing;

insert into rooms (id, name, floor, description, capacity) values
    ('room_101', 'Sala 101', 'Térreo', 'Sala padrão, carteiras individuais.', 20),
    ('room_102', 'Sala 102', 'Térreo', 'Carteiras em fileiras.', 25),
    ('room_aud', 'Auditório', 'Térreo', 'Palco, telão e som para eventos.', 80),
    ('room_204', 'Sala 204', '2º andar', 'Sala ampla com projeção fixa.', 35),
    ('room_205', 'Sala 205', '2º andar', 'Sala com estações de computador.', 30),
    ('room_301', 'Laboratório 301', '3º andar', 'Laboratório de informática.', 20)
on conflict (id) do nothing;

insert into room_resources (room_id, resource_id) values
    ('room_101', 'res_quadro'), ('room_102', 'res_quadro'), ('room_102', 'res_ar'),
    ('room_aud', 'res_proj'), ('room_aud', 'res_som'), ('room_aud', 'res_ar'),
    ('room_204', 'res_proj'), ('room_204', 'res_ar'), ('room_204', 'res_quadro'),
    ('room_205', 'res_proj'), ('room_205', 'res_pc'),
    ('room_301', 'res_pc'), ('room_301', 'res_ar'), ('room_301', 'res_proj')
on conflict do nothing;