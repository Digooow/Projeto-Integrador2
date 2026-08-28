# Frontend — Ocupa

## Status vigente — 28/08/2026

O frontend principal está em `reserva-salas.html`, integrado à API ASP.NET Core e servido também pelas rotas `/` e `/reserva-salas.html` quando a API é publicada. Ele oferece solicitação e recorrência de reservas, aprovação, rejeição, cancelamento, cadastros administrativos e painel público.

O frontend agora chama `/auth/login`, armazena o token Bearer na sessão e o envia nas chamadas protegidas. O fallback local continua disponível quando a API está indisponível. Pendências: testes automatizados do navegador, aplicação da migration 002 e atualização da imagem publicada no Render.