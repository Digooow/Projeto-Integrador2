# Frontend — Ocupa

## Status vigente — 28/08/2026

O frontend principal está em `reserva-salas.html`, integrado à API ASP.NET
Core e servido pelas rotas `/` e `/reserva-salas.html` quando a API é publicada
no Render. Ele oferece solicitação e recorrência de reservas, aprovação,
rejeição, cancelamento, cadastros administrativos e painel público.

O login chama `/auth/login` com e-mail e senha, armazena o token JWT na sessão e
o envia nas chamadas protegidas. O cadastro público cria requisitantes e a
administração cria/edita usuários. O fallback local existe apenas para
demonstração quando a API está indisponível.

Consulte o [roadmap atual](../ROADMAP-ATUAL.md) para melhorias futuras.
