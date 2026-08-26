alter table users
    add column if not exists password_hash text;

-- Development-only password for the demo users from migration 002:
-- Troque-me-123! Replace this hash before deploying to production.
update users
set password_hash = 'pbkdf2-sha256$100000$aV0bQ7n/y6G1kIlXD23lFw==$inQ5UYGYn3vJMHpe8tcHvg+EuP6Ddktztk2tRzmrtb4='
where password_hash is null;