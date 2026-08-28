# Sprint Review — Ocupa

**Produto:** Ocupa — Sistema de Reserva de Salas.

**Equipe:** Matheus, Maurício, Maycon, Raphaella, Rodrigo e Viviane.

**Professor:** Gabriel Caixeta Silva.

## Incremento demonstrado
Aplicação web com frontend HTML/CSS/JavaScript servido por backend ASP.NET Core 8, API HTTP e persistência PostgreSQL via Supabase.

Foram demonstrados: reservas pontuais e recorrentes, validação de capacidade, detecção de conflitos, aprovação, rejeição, cancelamento, cadastro de salas/recursos/usuários, calendário e painel público para TV.

O painel público não exige login. O backend possui login JWT e protege operações administrativas e de reserva; o login exibido no frontend ainda é demonstrativo e precisa ser integrado ao JWT.

## Evoluções pendentes
Migration 002 no Supabase, redeploy no Render, integração JWT no frontend, testes E2E, CORS restrito, fuso horário explícito, notificações, integrações externas e relatórios avançados.

## Retrospectiva
Preencher com fatos, dificuldades e ações realmente registrados pelo grupo. Não manter placeholders como “preencher com o grupo” na versão final.
