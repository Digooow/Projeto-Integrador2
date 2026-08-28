# Relatório de consistência dos materiais

## Escopo da análise

Comparação dos PDFs existentes em `Arquivos/` com o código, migrations, testes e documentação atuais do projeto, considerando o estado verificado em 28/08/2026. Os PDFs são binários; por isso, este arquivo reúne o texto corrigido pronto para transcrição nas atividades e apresentações.

## Correções transversais

### 1. Nome do produto

**Problema:** a atividade resolvida da Aula 2 chama o produto de `SmartSala`, enquanto os materiais das Sprints, o frontend e a documentação técnica usam `Ocupa`.

**Texto corrigido:**

> Nome do projeto: **Ocupa — Sistema de Reserva de Salas**.

### 2. Cronograma das aulas

**Problema:** o plano da Aula 00 prevê oito encontros e coloca Sprint Review/Retrospectiva na Aula 7, mas os materiais disponíveis estão organizados até a Aula 06 e contêm a Sprint Review nessa pasta. A Sprint Review também contém a frase “ao longo de todo o ciclo” e a tabela “Aula 3 a 7”, sem correspondência consistente com a pasta.

**Texto corrigido, sem inventar datas ausentes:**

> Os materiais entregues no repositório cobrem as Aulas 00 a 06: ideação, discovery, times/frameworks, três sprints e Sprint Review. A numeração da devolutiva final deve ser confirmada com o docente antes de ser mantida como Aula 7 ou Aula 8.

### 3. Status técnico

**Problema:** alguns materiais e blocos históricos dizem que não existe backend, banco, autenticação ou integração; isso não descreve mais o estado atual.

**Texto corrigido:**

> O projeto possui backend ASP.NET Core 8, persistência PostgreSQL via Supabase, integração do frontend com a API, CRUD de usuários/salas/recursos, reservas pontuais e recorrentes, aprovação, rejeição, cancelamento, controle de capacidade, detecção de conflitos, paginação e testes unitários.
>
> O backend possui login JWT e proteção dos endpoints. O frontend agora solicita a senha, chama `/auth/login`, guarda o token na sessão e envia Bearer nas chamadas protegidas. A migration 002 precisa ser aplicada no Supabase e o frontend publicado precisa ser atualizado no Render. CORS restrito, E2E, fuso horário explícito, notificações e integração com e-mail/WhatsApp continuam pendentes.

### 4. Funcionalidades prometidas, mas não comprovadas como entregues

Os seguintes itens aparecem nas atividades, mas não devem ser descritos como concluídos: notificações por WhatsApp/e-mail/push; matrícula de alunos e consulta por curso; troca de sala pelo professor; check-in por QR Code; relatórios avançados; integração com Google Agenda/Outlook; sugestão automática de sala; autenticação efetiva no frontend; validação E2E em Supabase/Render.

**Texto corrigido para esses itens:**

> Item fora do incremento validado ou pendente de integração. Foi mantido como requisito/evolução futura e não como funcionalidade comprovadamente entregue.

## Correções por material

### Aula 01 — Desafio resolvido

**Problemas encontrados:** o documento mistura requisito e implementação futura; afirma notificações por WhatsApp, CRUD de alunos, matrículas por curso, troca de salas e notificações administrativas, sem correspondência no incremento atual. Também há a expressão “professores devem saber ... quando a sala reservada está indisponível”, que não define se é consulta, aviso ou bloqueio.

**Substituir a decisão inicial por:**

> Decidimos criar uma aplicação web para centralizar solicitações e aprovações de salas, com persistência em banco de dados. O MVP contempla cadastro administrativo de salas, recursos e usuários; reservas pontuais e recorrentes; validação de capacidade; detecção de conflitos; aprovação, rejeição e cancelamento; calendário; histórico preservado; e painel público para TV.
>
> Notificações por WhatsApp/e-mail, matrícula de alunos, consulta por curso, troca de sala pelo professor e notificações automáticas ficam como evolução futura, pois não fazem parte do incremento validado.

### Aula 01 — Brainstorming resolvido

**Problema:** o PDF resolvido não apresenta conteúdo legível da atividade; não é possível comprovar ideias, critérios de priorização ou decisão do grupo.

**Texto mínimo corrigido para a entrega:**

> Ideias priorizadas: centralizar pedidos; permitir recorrência; exigir aprovação; detectar conflitos antes da aprovação; cadastrar capacidade e recursos; preservar histórico; e exibir a ocupação atual em painel público.

### Aula 01 — Benchmarking resolvido

**Problemas encontrados:** a análise é útil, mas a recomendação e a seção de backlog misturam referência externa com requisito já entregue. Os recursos de preço, roadmap e funcionalidades externas são informações datadas de 21/08/2026 e devem permanecer identificados como consulta pública, não como fato do projeto.

**Texto corrigido para a conclusão:**

> O benchmarking orientou o backlog, mas não comprova que as funcionalidades dos produtos analisados existam integralmente nem que tenham sido implementadas no Ocupa. No incremento atual foram validados os fluxos de reserva, recorrência, aprovação, conflito, cadastro, calendário e painel público. Notificações, integrações externas, QR Code, relatórios avançados e sugestão automática permanecem como evolução.

### Aula 02 — Briefing resolvido

**Problemas encontrados:** nome `SmartSala`; problema e proposta de valor estão incompletos; “remover aluguéis de salas” não corresponde ao domínio; e “alunos e professores não devem poder fazer alterações” conflita com o produto atual, no qual o solicitante pode criar e cancelar a própria reserva.

**Substituir o preenchimento por:**

> **Nome do projeto:** Ocupa — Sistema de Reserva de Salas.
>
> **Problema:** pedidos feitos por mensagens e planilhas manuais causam conflitos de horário, alocação inadequada por capacidade/recursos e falta de visibilidade para alunos.
>
> **Público-alvo:** coordenação e administradores, que cadastram e aprovam; professores e colaboradores, que solicitam; alunos e visitantes, que consultam o painel público.
>
> **Proposta de valor:** centralizar a solicitação, aprovação e consulta da ocupação das salas, com recorrência, validação de capacidade, prevenção de conflitos e painel público.
>
> **Escopo inicial:** reservas pontuais e recorrentes; aprovação/rejeição/cancelamento; cadastro de salas, recursos e usuários; calendário; histórico; painel público para TV; backend ASP.NET Core e persistência PostgreSQL/Supabase.
>
> **Fora do escopo atual:** notificações por WhatsApp/e-mail/push, integração acadêmica, QR Code, relatórios avançados, integrações de calendário e autenticação efetivamente integrada ao frontend.
>
> **Restrições:** somente usuários autorizados aprovam; cada solicitante pode cancelar suas próprias reservas; registros históricos não são apagados; o ambiente publicado depende da aplicação das migrations e do redeploy.

### Aula 02 — Personas resolvidas

**Problemas encontrados:** os campos e respostas aparecem desalinhados no PDF; a persona 1 tem dados de dor/citação deslocados; a persona 2 não está apresentada com a mesma estrutura e clareza. Isso dificulta identificar qual informação pertence a cada campo.

**Texto corrigido para as personas:**

> **Renata Alves — Coordenadora administrativa**  
> Papel: aprova solicitações e administra salas, recursos e usuários.  
> Objetivo: enxergar conflitos e a ocupação do prédio antes de decidir.  
> Dor: hoje depende de mensagens e planilhas e pode aprovar duas turmas no mesmo horário.  
> Citação: “Preciso saber dos conflitos antes de aprovar, não depois.”
>
> **Fernanda Lima — Professora**  
> Papel: solicita salas para aulas pontuais ou recorrentes.  
> Objetivo: fazer um único pedido para as aulas de terça e quinta e acompanhar a decisão.  
> Dor: não sabe se o pedido foi visto nem se a sala possui os recursos necessários.  
> Citação: “Eu só queria pedir uma vez e pronto até o fim do semestre.”
>
> **Lucas — Aluno**  
> Papel: consulta onde e quando as atividades estão acontecendo.  
> Objetivo: encontrar a sala correta sem depender da recepção.  
> Dor: pode deslocar-se ao prédio e encontrar informação desatualizada ou aula cancelada.  
> Citação: “Quero olhar o painel e saber direto para onde devo ir.”

### Aula 03 — Times e frameworks resolvido

**Problemas encontrados:** o quadro é uma fotografia de 21/08/2026, não o estado final; registra login com usuário e senha no backlog, mas o frontend atual ainda não usa login por senha; e os cartões apresentados não refletem todas as entregas atuais. Isso deve ser explicitamente tratado como histórico.

**Texto corrigido para o resumo:**

> Este documento registra o quadro Kanban observado em 21/08/2026. Ele não representa sozinho o backlog final nem o estado atual do produto. Na evolução posterior, o projeto recebeu integração frontend/backend, persistência, CRUD, recorrência, decisões, paginação e JWT no backend. O frontend ainda precisa integrar o login JWT.

### Aulas 04 e 05 — Sprints 1 e 2

**Correção de consistência:** manter as entregas de cada sprint apenas quando houver registro real no quadro ou no código. Não afirmar que uma cerimônia ocorreu, que um stakeholder validou ou que uma funcionalidade foi entregue quando o PDF contém apenas modelo, exemplo ou placeholder.

**Texto de validação recomendado:**

> Resultado técnico comprovado no repositório: regras de reserva e recorrência, aprovação autorizada, conflito de horário, capacidade da sala, cancelamento pelo proprietário, persistência via API e testes unitários. Registros de daily, review, retrospectiva, validação externa e responsáveis precisam ser preenchidos com evidências reais do grupo.

### Aula 06 — Documentação técnica

**Problema principal:** a documentação ainda descreve `window.storage`, ausência de backend HTTP, ausência de validação no servidor e login sem senha, embora esses trechos tenham sido superados pela implementação atual. Ela deve ser preservada como histórico apenas se estiver rotulada como “estado anterior à integração”.

**Texto corrigido para o status:**

> A documentação original descreve o protótipo local baseado em `window.storage`. Esse texto corresponde ao estado anterior à integração e não deve ser usado como descrição vigente. No estado atual, o frontend chama a API ASP.NET Core e usa fallback local apenas quando a API não responde. A API persiste no PostgreSQL/Supabase, valida capacidade e autorização, oferece JWT no backend e expõe paginação de reservas. O fluxo visual de login do frontend ainda é demonstrativo e precisa enviar o token JWT para que a autenticação seja efetiva de ponta a ponta.

### Aula 06 — Sprint Review

**Problemas encontrados:** permanecem placeholders: `[substituir pelos nomes reais do grupo]`, `[Inserir aqui o print do quadro real do Planner]`, `[ajustar os itens ...]`, `[preencher com o grupo]`; a apresentação diz “aplicação web única, sem servidor próprio”, embora exista backend ASP.NET Core; e a divisão “Aulas 1 e 2 / Aulas 3 a 7” não coincide com a organização dos materiais.

**Substituições obrigatórias:**

> Grupo: Matheus, Maurício, Maycon, Raphaella, Rodrigo e Viviane. Professor: Gabriel Caixeta Silva.
>
> O Ocupa é uma aplicação web com frontend HTML/CSS/JavaScript servido pelo backend ASP.NET Core 8, API HTTP e persistência PostgreSQL via Supabase. O painel público não exige login; as operações administrativas e de reserva exigem autorização no backend. O login exibido no frontend ainda é demonstrativo e será integrado ao JWT em uma etapa posterior.
>
> Discovery: materiais das Aulas 1 e 2. Delivery: materiais das Aulas 3 a 6 e Sprint Review, conforme a organização atual do repositório. Confirmar a numeração oficial da devolutiva com o docente.
>
> Retrospectiva: preencher somente com fatos, dificuldades e ações realmente registrados pelo grupo; não manter os textos `[preencher com o grupo]` na versão final.

## Prioridade de atualização

1. Remover todos os placeholders da Sprint Review e preencher nomes, quadro, validação e retrospectiva com evidências reais.
2. Corrigir `SmartSala` para `Ocupa` e alinhar o cronograma dos materiais.
3. Marcar a documentação antiga da Aula 06 como histórico e substituir o status técnico pelo texto vigente acima.
4. Reformatar as personas da Aula 02 para que cada resposta fique no campo correto.
5. Separar requisitos futuros de funcionalidades comprovadamente entregues.
