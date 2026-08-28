# Aula 02 — Briefing resolvido

**Nome:** Ocupa — Sistema de Reserva de Salas.

**Problema:** pedidos feitos por mensagens e planilhas manuais causam conflitos de horário, alocação inadequada por capacidade/recursos e falta de visibilidade para alunos.

**Público-alvo:** administradores e coordenação; professores e colaboradores; alunos e visitantes do painel público.

**Proposta de valor:** centralizar solicitação, aprovação e consulta da ocupação das salas, com recorrência, validação de capacidade, prevenção de conflitos e painel público.

**Escopo inicial:** reservas pontuais e recorrentes; aprovação, rejeição e cancelamento; cadastro de salas, recursos e usuários; calendário; histórico; painel de TV; API ASP.NET Core e PostgreSQL via Supabase.

**Fora do escopo atual:** notificações por WhatsApp/e-mail/push, integração acadêmica, QR Code, relatórios avançados e integrações de calendário.

**Restrições:** somente usuários autorizados aprovam; o solicitante pode cancelar a própria reserva; registros históricos não são apagados; a connection string permanece exclusivamente no Render.
