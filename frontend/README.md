# Frontend — Ocupa

## Status vigente — 28/08/2026

O frontend principal está em `reserva-salas.html`, integrado à API ASP.NET Core e servido também pelas rotas `/` e `/reserva-salas.html` quando a API é publicada. Ele oferece solicitação e recorrência de reservas, aprovação, rejeição, cancelamento, cadastros administrativos e painel público.

O backend já possui login JWT e protege as operações. Pendências do frontend: substituir a seleção demonstrativa de usuário pelo login em `/auth/login`, enviar o token Bearer nas chamadas protegidas, criar testes automatizados do navegador e atualizar a imagem publicada no Render.