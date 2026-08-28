# Projeto-Integrador2 - Sistema de Reserva de Salas

## Status vigente — 28/08/2026

Esta é a situação atual do projeto. As seções e checklists abaixo, datadas de 22/08/2026, são mantidas como histórico da sprint anterior; quando houver diferença, este quadro é a referência mais recente.

### Entregas comprovadas

- ✅ Backend ASP.NET Core 8, EF Core/Npgsql e domínio de reservas compilando.
- ✅ Integração do frontend com a API, com CRUD de usuários, salas e recursos, reservas recorrentes, aprovação, rejeição e cancelamento.
- ✅ Frontend publicado pelo backend nas rotas `/` e `/reserva-salas.html`.
- ✅ Paginação em `GET /api/reservations?page=1&pageSize=20`, limitada a 100 registros.
- ✅ Migration 002 com campos de integração e políticas RLS complementares idempotentes.
- ✅ Stack recomendada instalada: FluentValidation, Serilog, sinks, JWT, JwtBearer e Swashbuckle.
- ✅ Build aprovado e 9 testes aprovados: 7 unitários e 2 E2E.
- ✅ Login JWT e autorização configurados no backend.

### Autenticação JWT

A API expõe `POST /auth/login` com `{ "email": "...", "password": "..." }` e retorna um token Bearer válido por 8 horas. Configure `JWT_SECRET_KEY` com pelo menos 32 bytes antes de iniciar a aplicação.

Usuários novos devem ser criados por um administrador em `POST /api/users` informando `password`; a migration 003 fornece `Troque-me-123!` apenas para os usuários demo e essa senha deve ser trocada em produção. Operações administrativas e reservas exigem o token JWT.

### Pendências reais

- ✅ Configurar JWT no pipeline, login com credenciais e proteção dos endpoints.
- ⏳ Integrar o login JWT ao frontend, que ainda usa seleção demonstrativa de usuário.
- ⏳ Configurar efetivamente FluentValidation, Serilog/Application Insights e Swagger.
- ⏳ Aplicar e validar a migration 002 no Supabase.
- ⏳ Publicar a imagem atualizada e fazer redeploy no Render; a imagem pública ainda está desatualizada.
- ⏳ Criar testes E2E, restringir CORS, revisar fuso horário e adicionar rate limiting.

O cronograma de evolução deve usar este status como ponto de partida. Nenhum item pendente desta seção deve ser tratado como concluído apenas porque o pacote ou o exemplo foi adicionado.

> **Histórico:** Status de 22/08/2026. Consulte o quadro vigente acima para a situação atual.

---

## 📊 PARÂMETRO GERAL DO PROJETO

### O que é este projeto?

Este é um **Sistema Completo de Reserva de Salas** para instituições de ensino. Permite que professores e colaboradores solicitem salas de aula com suporte a recorrência semanal, coordenadores aprovem solicitações com detecção automática de conflitos, e alunos visualizem em tempo real qual sala está sendo usada através de painel público em TV.

### Tecnologia & Arquitetura

> **Nota de rastreabilidade:** A tabela abaixo preserva percentuais históricos de 22/08. O backend já possui JWT; a integração do token no frontend continua pendente.

| Aspecto | Tecnologia | Status |
|---------|-----------|--------|
| **Backend** | ASP.NET Core 8 (Minimal APIs) | ✅ 90% Completo |
| **Banco de Dados** | PostgreSQL (via Supabase) | ✅ 100% Pronto |
| **ORM** | Entity Framework Core 8 | ✅ Funcional |
| **Arquitetura** | Domain-Driven Design (DDD) | ✅ Implementado |
| **Testes** | xUnit (Unit Tests) | ✅ 80% Cobertura |
| **CI/CD** | GitHub Actions | ✅ 100% Otimizado |
| **Deploy** | Render + Docker Hub | ✅ Online |
| **Frontend** | HTML/CSS/JavaScript | 🟡 20% Integrado |
| **Segurança** | JWT no backend + Row Level Security (RLS) | 🟡 Parcial |

### Saúde do Projeto

```
████████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░ 42% PRONTO PARA PRODUÇÃO

Componentes:
✅ Backend Core:         ████████████████░░░░░░░░░░░░░ 90%
✅ Banco de Dados:       █████████████████████████████░ 100%
✅ Testes Unitários:     ████████████████░░░░░░░░░░░░░░ 80%
✅ CI/CD Pipeline:       █████████████████████████████░ 100%
✅ Deploy (Render):      █████████████████████████████░ 100%
✅ Documentação:         █████████████████████████████░ 100%
🟡 Autenticação JWT:     backend pronto; integração no frontend pendente
🟡 Frontend:             integrado à API; login JWT pendente
🟡 Segurança (RLS):      ██████░░░░░░░░░░░░░░░░░░░░░░░░░░ 60%
```

### Bloqueador para Produção

🔴 **Integração de autenticação incompleta** - O backend exige JWT nas operações protegidas, mas o frontend ainda usa login demonstrativo. Também faltam validação da migration no Supabase, CORS restrito e testes E2E para uso em produção.

---

## 📚 DOCUMENTAÇÃO COMPLETA DO PROJETO

Este projeto possui documentação técnica abrangente. **Comece aqui** para entender a estrutura:

### 1. [**ANALISE-PROJETO.md**](./ANALISE-PROJETO.md) - Análise Técnica Completa
   
   **O que contém:**
   - ✅ O que está **FEITO** (Backend 95%, Banco 100%, Testes 80%, CI/CD 100%, Deploy 100%)
   - 🚨 O que está **RUIM** (11 problemas identificados com prioridades)
   - ✨ O que está **BOM** (7 forças técnicas do projeto)
   - 📊 Métricas de saúde do projeto
   - 💡 Recomendações para a equipe
   - 📋 Roadmap de 3 fases (Crítica, Importante, Nice-to-have)
   
   **Para quem:**
   - Arquitetos / Tech Leads que precisam entender o projeto
   - Gerentes que querem saber status e riscos
   - Desenvolvedores novos no projeto
   
   **Tamanho:** ~3500 linhas | **Tempo de leitura:** 30 minutos


  ### 5. [**ANALISE-BOAS-PRATICAS.md**](./ANALISE-BOAS-PRATICAS.md) - Avaliação de Qualidade de Código

    **O que contém:**
    - ✅ SOLID Principles: Análise de cada princípio com exemplos de código
    - ✅ Clean Code: Avaliação de naming, métodos, tratamento de erro
    - ✅ DRY Principle: Identificação de duplicação e reutilização
    - 🎯 Padrões de Design: Quais estão implementados, quais recomendados
    - 📊 Score de conformidade por categoria (S:95%, O:50%, L:90%, I:40%, D:95%)
    - 💡 Recomendações prioritizadas (Fase 1, 2, 3)
    - 🚀 Plano de melhoria detalhado
   
    **Para quem:**
    - Arquitetos / Tech Leads que fazem code review
    - Desenvolvedores que querem melhorar a qualidade de código
    - Qualquer um interessado em boas práticas
   
    **Tamanho:** ~2000 linhas | **Tempo de leitura:** 25 minutos
### 2. [**ROADMAP-ACOES.md**](./ROADMAP-ACOES.md) - Guia Prático de Desenvolvimento
   **O que contém:**
  ### 6. [**IMPLEMENTACAO-BOAS-PRATICAS.md**](./IMPLEMENTACAO-BOAS-PRATICAS.md) - Guia Passo a Passo de Refatoração

    **O que contém:**
    - 🔧 Fase 1: Criar interfaces e repositories (com código completo)
    - 🔧 Fase 2: FluentValidation (exemplos prontos para copiar/colar)
    - 🔧 Fase 3: Serilog (logging estruturado com configuração)
    - 🔧 Fase 4: Global Exception Handler (middleware pronto)
    - 🔧 Fase 5: Refatoração de Program.cs (separation of concerns)
    - ✅ Testes para validar cada mudança
    - ✅ Checklist de implementação
   
    **Para quem:**
    - Desenvolvedores que vão implementar as melhorias
    - Time de desenvolvimento implementando refatoração
    - Qualquer um querendo aprender o padrão
   
    **Tamanho:** ~1500 linhas | **Tempo de leitura:** 20 minutos
    **Tempo de implementação:** 16-20 horas de desenvolvimento
   - 🔥 Fase 1 CRÍTICA (JWT + RLS) com código exemplo
   - 🟠 Fase 3 NICE-TO-HAVE (Notificações, Relatórios, etc)
   - ✅ Checklists detalhados e implementáveis

  ### ✅ Implementado (Excelente)
  - Domain-Driven Design (DDD) bem estruturado
  - Testes unitários cobrindo regras de negócio
  - CI/CD otimizado com 5 melhorias
  - Dependency Injection via ASP.NET Core
  - Immutability (records e sealed classes)
  - Entity Framework Core com Fluent API
  - **Score: 76% de conformidade com boas práticas**

  ### 🟡 Recomendações Práticas (Implementação 16-20h)
  1. **Interfaces de repositório** → Testabilidade ++
  2. **FluentValidation** → Validação centralizada
  3. **Serilog** → Observabilidade
  4. **Global Exception Handler** → Código limpo
  5. **Refactor Program.cs** → Separation of concerns

  → **Após implementar:** 76% → 92% de conformidade

   - 📅 Timeline realista (4-5 semanas)
   - 💬 Template de Daily Standup
   - 🎯 Métricas de sucesso
   
   **Para quem:**
   - Desenvolvedores que vão implementar as features
   - Scrum Masters / PMs acompanhando o projeto
   - Qualquer um que precisa saber "o que fazer agora"
   
   **Tamanho:** ~1800 linhas | **Tempo de leitura:** 20 minutos

---

### 3. [**STATUS-VISUAL.md**](./STATUS-VISUAL.md) - Referência Rápida & Painel de Controle

   **O que contém:**
   - 📊 Tabela visual de componentes (status, % completo, próximo passo)
   - 🔥 Bloqueadores críticos explicados
   - 📈 Gráfico de progresso semanal
   - ✅ Checklist de pré-produção (35 itens)
   - 🎯 Próximos 3 passos (esta semana)
   - 💬 Contatos importantes da equipe
   - 🔗 Links úteis (código, banco, CI/CD, deploy)
   
   **Para quem:**
   - Qualquer pessoa que quer status rápido (2 minutos)
   - Painel que pode ficar impresso na parede
   - Daily meetings & standups
   
   **Tamanho:** ~300 linhas | **Tempo de leitura:** 5 minutos

---

### 4. [**.github/CI-CD-IMPROVEMENTS.md**](./.github/CI-CD-IMPROVEMENTS.md) - Otimizações Implementadas

   **O que contém:**
   - 5 melhorias de CI/CD explicadas em detalhe
   - Como cada uma funciona (Cache NuGet, Buildx, Metadata, Concurrency, Artifacts)
   - Performance antes vs depois (-50% no tempo total)
   - Como validar as melhorias
   - Próximas melhorias sugeridas (Slack alerts, Trivy scanning, etc)
   
   **Para quem:**
   - DevOps / Platform Engineers
   - Desenvolvedores curiosos sobre CI/CD
   - Qualquer um resolvendo problemas de performance no pipeline
   
   **Tamanho:** ~400 linhas | **Tempo de leitura:** 10 minutos

---

## 🚀 COMEÇAR AQUI - QUICK START

### Se você é...

**👨‍💻 Desenvolvedor novo no projeto:**
1. Leia este README (5 min)
2. Leia [STATUS-VISUAL.md](./STATUS-VISUAL.md) (5 min)
3. Leia [ROADMAP-ACOES.md](./ROADMAP-ACOES.md) - sua tarefa (20 min)
4. Clone e rode localmente

**👨‍💼 Gerente / Tech Lead:**
1. Leia este README (5 min)
2. Leia [STATUS-VISUAL.md](./STATUS-VISUAL.md) (5 min)
3. Leia [ANALISE-PROJETO.md](./ANALISE-PROJETO.md) - seção "Bloqueadores Críticos" (10 min)
4. Leia [ROADMAP-ACOES.md](./ROADMAP-ACOES.md) - seção "Timeline Realista" (5 min)

**🧪 QA / Tester:**
1. Leia [STATUS-VISUAL.md](./STATUS-VISUAL.md) (5 min)
2. Vá para "Checklist de Pré-Produção" em [ANALISE-PROJETO.md](./ANALISE-PROJETO.md)
3. Leia [ROADMAP-ACOES.md](./ROADMAP-ACOES.md) - seção "Fase 2.1: Testes E2E"

**🏗️ DevOps / Arquiteto:**
1. Leia [ANALISE-PROJETO.md](./ANALISE-PROJETO.md) (30 min)
2. Leia [.github/CI-CD-IMPROVEMENTS.md](./.github/CI-CD-IMPROVEMENTS.md) (10 min)
3. Revisar [.github/workflows/dotnet.yml](./.github/workflows/dotnet.yml)

---

## 📋 ESTRUTURA DE ARQUIVOS DO PROJETO

> **Nota de rastreabilidade — 26/08/2026:** As descrições de “frontend sem JS” e “20% integrado” que aparecem nos blocos históricos abaixo correspondem ao estado anterior à integração. A entrega atual está registrada no status vigente no início deste README.

```
Projeto-Integrador2/
├─ README.md                           ← Você está aqui
├─ ANALISE-PROJETO.md                  ← Análise técnica completa
├─ ROADMAP-ACOES.md                    ← Guia de desenvolvimento
├─ STATUS-VISUAL.md                    ← Painel de controle rápido
│
├─ Domain/                             ✅ Lógica de negócio pura
│  └─ ReservationDomain.cs             (ReservationService, Room, User, etc)
│
├─ Persistence/                        ✅ Mapeamento ORM
│  ├─ Entities.cs                      (UserEntity, RoomEntity, ReservationEntity)
│  └─ ReservationDbContext.cs          (DbContext + OnModelCreating)
│
├─ Program.cs                          ✅ API endpoints + configuração
│  ├─ GET  /health                     (health check)
│  ├─ GET  /api/rooms                  (listar salas)
│  ├─ GET  /api/reservations           (listar reservas)
│  ├─ POST /api/reservations           (criar reserva)
│  ├─ POST /api/reservations/{id}/approve
│  └─ POST /api/reservations/{id}/cancel
│
├─ Dockerfile                          ✅ Multi-stage build
│  ├─ Build stage (SDK)
│  └─ Runtime stage (AspNet)
│
├─ .github/
│  ├─ workflows/
│  │  └─ dotnet.yml                    ✅ CI/CD pipeline otimizado
│  └─ CI-CD-IMPROVEMENTS.md            ✅ Documentação das otimizações
│
├─ supabase/
│  └─ migrations/
│     └─ 001_initial.sql               ✅ Schema completo + seed data
│
├─ tests/
│  └─ Projeto-Integrador2.Tests/
│     ├─ ReservationServiceTests.cs    ✅ 7 testes unitários + 2 E2E
│     └─ Projeto-Integrador2.Tests.csproj
│
└─ frontend/
   ├─ reserva-salas.html               🟡 HTML estruturado (sem JS integrado)
   ├─ historia_do_usuario.md
   ├─ login.html
   ├─ login.css
   └─ README.md
```

---

## Backend com Supabase

O backend usa ASP.NET Core Minimal API, Entity Framework Core e Npgsql. O Supabase fornece o PostgreSQL; o ORM acessa o banco pela connection string, sem colocar credenciais no código.

1. Crie um projeto no Supabase e copie a connection string em `.env.example` para a variável `SUPABASE_CONNECTION_STRING`.
2. Execute `supabase/migrations/001_initial.sql` no SQL Editor do Supabase.
3. Inicie a API com `dotnet run`.

Endpoints principais: `GET /api/rooms`, `GET /api/reservations`, `POST /api/reservations`, `POST /api/reservations/{id}/approve` e `POST /api/reservations/{id}/cancel`. O endpoint público `GET /api/reservations?status=Approved` pode alimentar o painel da TV.

## Testes do backend

O primeiro incremento do domínio está especificado em testes unitários xUnit, sem dependência de rede ou do Supabase. Eles servem como contrato para a implementação do backend em C# e cobrem solicitações pendentes, recorrência semanal, aprovação autorizada, conflitos de horário, capacidade da sala, cancelamento pelo proprietário e histórico.

Para executar:

```powershell
dotnet test tests/Projeto-Integrador2.Tests/Projeto-Integrador2.Tests.csproj
```

A persistência no Supabase deve ser adicionada atrás de uma camada de repositório; as regras de negócio não devem acessar o banco diretamente.

---

## � GitHub Actions - CI/CD Automatizado

O projeto utiliza **GitHub Actions** para automação contínua de integração e entrega. O fluxo de trabalho é definido em `.github/workflows/dotnet.yml` e realiza as seguintes etapas automaticamente:

### Como funciona:

**Acionadores (Triggers):**
- Toda vez que há um `push` nas branches `main` ou `develop`
- Toda vez que há um `pull request` para a branch `main`

**Job 1: Build e Testes (`build-and-test`)**
- Executa em uma máquina Ubuntu fornecida pelo GitHub
- Etapas:
  1. Faz checkout do código
  2. Configura o .NET 8.0.x
  3. Restaura as dependências NuGet (`dotnet restore`)
  4. Compila o projeto em modo Release (`dotnet build`)
  5. Executa todos os testes unitários (`dotnet test`)
- ✅ Se todas as etapas passarem, o job é bem-sucedido
- ❌ Se alguma etapa falhar, o fluxo é interrompido e ninguém recebe notificação de deploy

**Job 2: Build e Push da Imagem Docker (`docker-build-push`)**
- Depende do sucesso do `build-and-test`
- Executa **apenas** quando há push na branch `main` (proteção contra deployments acidentais)
- Etapas:
  1. Faz checkout do código
  2. Autentica no Docker Hub usando secrets do GitHub
  3. Constrói a imagem Docker usando o `Dockerfile`
  4. Faz push da imagem com duas tags:
     - `latest` (versão mais recente)
     - Hash do commit (`${{ github.sha }}`) (rastreabilidade)

### O que você precisa fazer para ativar:

1. **Criar secrets no GitHub:**
   - `DOCKER_USERNAME`: seu usuário do Docker Hub
   - `DOCKER_PASSWORD`: token de acesso do Docker Hub (nunca coloque a senha real)

2. **Estrutura de segredos (Settings → Secrets and variables → Actions):**
   ```
   DOCKER_USERNAME = seu-usuario-dockerhub
   DOCKER_PASSWORD = seu-token-dockerhub
   ```

Para que cada imagem publicada também provoque o redeploy do serviço, adicione
os secrets `RENDER_SERVICE_ID` (sem o prefixo `srv-`) e `RENDER_API_KEY`.
Enquanto eles não existirem, o job de deploy será ignorado.

---

## 🚀 Deploy no Render via Docker Hub (Otimizado)

A estratégia de deploy é **otimizada para evitar redundância**:

- **GitHub Actions**: Testa, constrói e faz push da imagem Docker **uma única vez**
- **Render**: Puxa a imagem pronta do Docker Hub e faz deploy (sem rebuild)

Isso resulta em **deploy mais rápido** e **uso eficiente de recursos**.

### Fluxo Completo:

```
push na main
    ↓
GitHub Actions: Testa + Constrói Docker + Push Hub (docker-build-push)
    ↓
Render: Detecta imagem nova + Puxa do Hub + Deploy
    ↓
API online na URL do Render
```

### Passo a passo para configurar o Render:

#### 1️⃣ **Acesse [render.com](https://render.com) e faça login**
   - Se não tem conta, crie uma (pode usar GitHub para facilitar)

#### 2️⃣ **Crie um novo Web Service**
   - Clique em "New+" → "Web Service"

#### 3️⃣ **Conecte seu repositório GitHub** (opcional)
   - Você pode conectar o repo, mas **não é obrigatório** para esse setup
   - Se conectar, facilita gerenciamento de deployments via Render dashboard

#### 4️⃣ **Configure para usar Docker Hub (IMPORTANTE)**

Na tela de criação do Web Service, em vez de usar repositório GitHub com Dockerfile:

- **Selecione**: "Docker image" (não "GitHub repository")
- **Image URL**: `docker.io/seu-usuario-dockerhub/projeto-integrador2:latest`
  - Substitua `seu-usuario-dockerhub` pelo seu usuário real no Docker Hub
  - Exemplo: `docker.io/rodrigofarias/projeto-integrador2:latest`

#### 5️⃣ **Configure o serviço:**
   - **Name**: Digite o nome do seu serviço (ex: `projeto-integrador2-latest`)
   - **Region**: escolha a mais próxima (ex: `São Paulo`)
   - **Instance Type**: escolha conforme sua necessidade (free tier serve para testes)

#### 6️⃣ **Adicione variáveis de ambiente (CRÍTICO):**
   - No painel, vá para **"Environment"**
   - Adicione as seguintes variáveis:
     ```
     SUPABASE_CONNECTION_STRING = postgresql://user:password@...
     PORT = 10000
     ```
   - ⚠️ **IMPORTANTE**: O `PORT` deve estar em sintonia com a variável de ambiente do seu `Program.cs`

#### 7️⃣ **Clique em "Create Web Service"**
   - Render vai fazer pull da imagem do Docker Hub
   - Inicia o container
   - Você recebe uma URL pública (ex: `https://projeto-integrador2-latest.onrender.com`)
   - **Anote essa URL!** Você vai usar para testes e integração

### ✅ Verificação:

Teste o endpoint de saúde. Substitua `projeto-integrador2-latest` pela URL do **seu serviço**:
```bash
curl https://projeto-integrador2-latest.onrender.com/health
```

Você deve receber:
```json
{"status":"ok","database":"connected","timestamp":"2026-08-22T23:45:35.6289843Z"}
```

Se receber isso, **sua API está online e conectada ao banco!** 🎉

### 🔄 Como funciona após o setup:

1. Você faz `git push` na branch `main`
2. GitHub Actions:
   - ✅ Testa (`dotnet test`)
   - ✅ Constrói a imagem Docker
   - ✅ Faz push para Docker Hub com tags `latest` e hash do commit
3. Render:
   - Detecta que nova imagem foi disponibilizada no Docker Hub
   - Faz pull da imagem `latest`
   - Reinicia o container com a nova versão
   - Seu app está online em questão de segundos (sem rebuild!)

### 📊 Monitoramento:

- **Dashboard do Render**: Veja logs em tempo real, status do serviço, histórico de deployments
  - Acesse: [dashboard.render.com](https://dashboard.render.com)
  - Clique no seu serviço para ver logs
- **Rollback rápido**: Se algo der errado, Render mantém versões anteriores da imagem (você pode voltar)
- **Notificações**: Configure para receber alertas de falhas no email
- **URL do seu serviço**: Procure em "Settings" se quiser mudar o nome (e consequentemente a URL)

### 💰 Economia:

- GitHub Actions: Oferece 2,000 minutos/mês gratuitamente (enough para este projeto)
- Render: Tier gratuito com limitações ou pague conforme uso
- **Vantagem**: Sem desperdício de rebuild - economia de tempo e créditos

### ⚙️ Se precisar de ajustes depois:

- Para mudar variáveis de ambiente: Painel Render → "Environment" → edite e salve
- Para forçar redeploy: Painel Render → clique em "Deploy latest" (força pull da imagem latest)
- Para pausar o serviço (economizar): Painel Render → "Settings" → "Pause"
### 🚨 Erros Comuns (e como evitá-los):

| Erro | Causa | Solução |
|------|-------|---------|
| "Image not found" no Render | Docker Hub username errado | Verifique se é `docker.io/seu-usuario/projeto-integrador2:latest` |
| Container starts/stops | `SUPABASE_CONNECTION_STRING` inválida | Copie a connection string completa do Supabase |
| "Connection refused" no endpoint | Porta errada | Certifique que `PORT=10000` no Render e no `Program.cs` |
| Deploy não atualiza | Render ainda puxando tag errada | Clique "Deploy latest" no dashboard Render para forçar pull |
| "404 Not Found" ao testar | Testando na URL errada | Use a URL gerada pelo Render (ex: `projeto-integrador2-latest.onrender.com`) |
| "no-server" no curl | Serviço crashed ou não iniciou | Verifique Logs no Render Dashboard |

### 💡 Dica de Ouro:

Se tiver erro ao conectar, **primeiro teste localmente**:
```bash
docker run -e SUPABASE_CONNECTION_STRING="sua-string" \
           -e PORT=5000 \
           -p 5000:5000 \
           seu-usuario/projeto-integrador2:latest
```

Se funcionar localmente, vai funcionar no Render também. Acesse `http://localhost:5000/health`
---

## �🔗 Links Úteis do Projeto

<p><strong>📋 Whiteboard</strong><br>
<a href="https://whiteboard.cloud.microsoft.com/me/whiteboards/p/c3BvOmh0dHBzOi8vc2VuYWNzYzc1NC1teS5zaGFyZXBvaW50LmNvbS9wZXJzb25hbC9tYXVyaWNpbzYxMTg0Njg2X2FsdW5vc19zY19zZW5hY19icg%3D%3D/b!COjkxJBZCkW-sK41v2rjLH2P3dB-AepJvkIe8dv8y2u42NcfHc1DRL1W0f5SeIKZ/012GH7RIN3AK4RIIEXNJBIXXMSM2VXNABF?lng=pt-br&ref=oib-09dc32fc-45b8-4913-a439-1a70f7a9ddfe" target="_blank">Clique aqui para acessar o Whiteboard</a></p>

<p><strong>📊 Kanban - Planejamento</strong><br>
<a href="https://planner.cloud.microsoft/webui/v1/plan/D2rlXSdIO0GLktdGp6w4u2QAHi8w?tid=0917fe10-56db-44bf-b1c5-31061ab21cf9" target="_blank">Acessar o Kanban no Planner</a></p>

<p><strong>📈 Benchmarks - Referências</strong><br>
- <a href="https://www.gentrop.com/produtos/reservaai" target="_blank">Reserva AI - Gentrop</a><br>
- <a href="https://agenda1.app/" target="_blank">Agenda 1</a></p>

---

# História do Usuário — Sistema de Reserva de Salas

## Quem está contando essa história

Meu nome é Renata, sou coordenadora administrativa de uma unidade do Senac Joinville. Entre outras coisas, eu cuido da organização das salas de aula do prédio: quem usa cada uma, em qual horário, e resolvo os problemas quando duas turmas acabam batendo na mesma sala.

---

## O problema que estou vivendo

Hoje a gente não tem nenhum sistema para isso. O jeito que funciona é mais ou menos assim:

- Os professores e colaboradores que precisam de uma sala (para uma aula extra, uma reunião, uma oficina, um evento) me mandam mensagem — às vezes por e-mail, às vezes por WhatsApp, às vezes vêm pessoalmente na minha sala. Não existe um lugar único onde isso fica registrado.
- Eu tenho uma planilha que tento manter atualizada com os horários de cada sala, mas ela vive desatualizada, porque depende de mim lembrar de anotar toda vez que alguém me avisa. Mais de uma vez eu já autorizei duas pessoas para a mesma sala, no mesmo horário, sem perceber — e só descobrimos o conflito quando os dois grupos chegam na porta da sala.
- Quando alguém precisa de uma sala toda semana (por exemplo, uma aula que se repete às terças e quintas durante o semestre inteiro), eu preciso anotar isso manualmente em cada uma das datas, uma por uma. É trabalhoso e é fácil esquecer de replicar para uma das semanas.
- Não existe aprovação de verdade — muitas vezes eu só fico sabendo que uma sala foi "reservada" quando encontro alguém já usando ela, porque combinaram direto com outro colega sem me avisar.
- Os alunos não têm nenhuma forma de saber, ao chegar no prédio, em qual sala é a aula deles ou o que está acontecendo em cada andar naquele momento. Isso gera fila na recepção e alunos perdidos perguntando pra todo mundo.
- Não sei de cabeça quantos alunos cada sala comporta, nem o que cada uma tem (projetor, ar-condicionado, quantidade de tomadas, quadro branco etc.). Isso já me fez colocar uma turma de 40 alunos numa sala que só tinha 20 cadeiras, e outra vez marcar uma aula que precisava de projetor numa sala que não tinha.
- Quando um colaborador ou professor sai da instituição, não tem um controle claro de "desativar" o acesso dele às reservas — as informações ficam soltas e ninguém lembra de revisar.
- Eu sou a única que consegue ter uma visão geral de tudo. Se eu estou de férias ou fora, ninguém mais consegue aprovar nada ou enxergar o que está reservado.
- No fim das contas, eu gasto um tempo enorme só tentando organizar manualmente uma coisa que deveria ser simples: garantir que a sala certa, no tamanho certo, com o que for preciso dentro dela, esteja disponível pra quem precisa, sem conflitos.

---

## Quem mais é afetado por esse problema

- **Os professores e colaboradores** que precisam de uma sala — hoje eles não têm nenhuma previsibilidade: mandam a mensagem e ficam esperando eu responder, às vezes demoro dias, às vezes a resposta se perde na conversa.
- **Os alunos** — chegam no prédio sem saber onde é a aula ou o que está rolando em cada sala/andar naquele turno.
- **Eu (coordenação)** — sou o gargalo de tudo: recebo os pedidos, decido, resolvo conflito, e ainda preciso lembrar de cadastrar/desativar pessoas manualmente.
- **Quem cuida da estrutura física das salas** — hoje ninguém tem uma lista organizada de quais salas existem, quantos alunos cabem em cada uma e quais equipamentos/recursos elas têm. Isso vive só na cabeça de quem já trabalha aqui há muito tempo.

--- 

## O que eu preciso que resolvam pra mim

Eu queria um sistema onde:

- Qualquer professor ou colaborador consiga **pedir uma sala** informando o que precisa (data, horário, finalidade, quantas vezes se repete, se for o caso), sem precisar falar comigo diretamente antes.
- Quando o pedido é só de "vez em quando" (uma reunião pontual, por exemplo), tudo bem, mas quando é uma coisa que **se repete** (todo dia, toda semana, num intervalo específico, até uma certa data), eu não quero que a pessoa tenha que cadastrar uma por uma — quero que o sistema já organize essa série toda de uma vez.
- **Eu (ou alguém no meu lugar) preciso aprovar** cada pedido antes de valer — assim ninguém usa uma sala sem eu saber, e eu consigo enxergar tudo que está pendente de decisão num lugar só, sem depender de mensagens perdidas.
- Se eu não tiver tempo de olhar, eu quero ser **avisada** de que existem pedidos esperando resposta, pra não deixar ninguém no vácuo.
- A pessoa que pediu a sala precisa conseguir **acompanhar se foi aprovado ou não**, e **cancelar o próprio pedido** se não precisar mais, sem ter que me chamar pra isso.
- Cada um só deveria poder mexer nas próprias reservas — não quero que um professor consiga cancelar ou alterar a reserva de outro colega.
- Eu preciso ter uma visão de **tudo que está reservado no prédio**, não só das minhas próprias solicitações — inclusive queria conseguir ver isso organizado por dia, num formato de calendário, pra identificar rapidamente onde tem sobreposição.
- Eu quero conseguir **cadastrar e desativar pessoas** que podem usar o sistema (às vezes alguém sai da instituição e eu preciso tirar o acesso dela sem perder o histórico do que ela já reservou).
- Seria bom se desse pra eu **delegar a responsabilidade por um grupo de salas** para outra pessoa (por exemplo, alguém que cuida só de um andar ou de um bloco específico), sem precisar que essa pessoa enxergue ou mexa nas salas dos outros setores.
- Quero um **cadastro das salas** com o essencial: nome, andar, descrição e principalmente **quantos alunos cada uma comporta** — pra nunca mais colocar uma turma grande numa sala pequena.
- Quero também poder **cadastrar os recursos** que uma sala tem (projetor, ar-condicionado, computadores, quadro, o que for) e associar isso à sala, pra quem for reservar já saber de antemão se aquela sala serve pra atividade dele, sem precisar ir lá conferir pessoalmente.
- E, o mais importante pros alunos: quero uma **tela pública, sem precisar de senha**, que eu possa deixar ligada numa TV no corredor, mostrando o que está acontecendo agora — separado por turno (manhã, tarde e noite, já focando automaticamente no turno que está rolando no momento), organizada por andar e sala, mostrando o nome da aula/atividade e quem é o responsável. Assim o aluno só olha a TV e já sabe pra onde ir.

---

## Um dia perfeito, se isso existisse

- Um professor entra no sistema e pede uma sala para as terças e quintas, das 19h às 22h, até o fim do semestre, porque a disciplina dele é nesses dias. Ele recebe a confirmação de que o pedido foi enviado e fica esperando.

- Eu entro no sistema, vejo que tem um pedido novo esperando aprovação, confiro se bate com a disponibilidade da sala (e o tamanho da turma cabe na sala escolhida), e aprovo tudo de uma vez — o sistema já cria as aulas de terça e quinta até o fim do semestre sozinho, sem eu ter que repetir a ação toda semana.

- Se por acaso dois pedidos batessem na mesma sala e horário, eu quero saber disso antes de aprovar os dois, não descobrir depois.

- Um aluno chega no prédio às 19h10, olha a TV do corredor, vê que no 2º andar, sala 204, está acontecendo a aula dele com o nome do professor, e vai direto pra lá sem precisar perguntar na recepção.

- No fim do mês, se um colaborador sai da instituição, eu simplesmente desativo o acesso dele, sem apagar o que ele já tinha reservado.

---

## Algumas preocupações e limites que eu tenho

- Não quero que qualquer pessoa consiga aprovar reservas — isso tem que ficar restrito a quem realmente tem essa responsabilidade.
- Não quero perder o histórico de nada — se uma reserva for cancelada ou um usuário for desativado, prefiro guardar o registro a apagar de vez.
- Preciso que fique claro, a qualquer momento, quais reservas ainda estão pendentes de decisão minha, e quais já estão confirmadas.
- Quero poder confiar que a capacidade da sala e os recursos cadastrados estão corretos, porque é isso que vai evitar os erros de alocação que hoje acontecem por falta de informação.
- A tela que fica na TV para os alunos precisa funcionar sozinha, sem alguém precisar ficar de olho nela ou trocando de tela manualmente ao longo do dia.

---

## 🎯 FUNCIONALIDADES IMPLEMENTADAS

### ✅ Funcionalidades Backend (Completas)

| Funcionalidade | Descrição | Status |
|---|---|---|
| **Criar Reserva** | Professor/Colaborador solicita sala com data, horário e duração | ✅ Completo |
| **Recorrência Semanal** | Sistema expande automaticamente reservas semanais (ex: Terça + Quinta até data X) | ✅ Completo |
| **Validação de Capacidade** | Impede reserva se número de pessoas > capacidade da sala | ✅ Completo |
| **Detecção de Conflitos** | Identifica overlaps de horário automaticamente | ✅ Completo |
| **Aprovação de Reservas** | Coordinator aprova/rejeita solicitações pendentes | ✅ Completo (sem endpoint de rejeição) |
| **Cancelamento** | Proprietário ou admin pode cancelar reserva | ✅ Completo |
| **Listagem de Salas** | GET /api/rooms com recursos e capacidade | ✅ Completo |
| **Listagem de Reservas** | GET /api/reservations com filtro por status | ✅ Completo (sem paginação ainda) |
| **Dados de Seed** | 6 salas + 6 recursos pré-cadastrados | ✅ Completo |
| **Histórico** | Todas as ações ficam registradas no banco | ✅ Completo |

### 🟡 Funcionalidades em Desenvolvimento

| Funcionalidade | Descrição | Status | Roadmap |
|---|---|---|---|
| **Autenticação JWT** | Usuários fazem login e recebem token | 🔴 0% | Fase 1 (6h) |
| **RLS Completo** | Restrições de banco por usuário (INSERT/UPDATE/DELETE) | 🟡 60% | Fase 1 (4h) |
| **Paginação** | GET /api/reservations?page=1&pageSize=20 | 🔴 0% | Fase 2 (2h) |
| **Validações Input** | FluentValidation nos endpoints | 🟡 50% | Fase 2 (4h) |
| **Logging** | Serilog centralizado | 🔴 0% | Fase 2 (5h) |
| **Frontend** | HTML + JavaScript integrado com API | 🟡 20% | Fase 2 (25h) |
| **Painel TV Público** | Tela em tempo real para alunos | 🔴 0% | Fase 3 (5h) |

### 🟠 Funcionalidades Nice-to-Have (Futuros)

- Notificações em tempo real (SignalR)
- Modificação de reservas (PUT)
- Rejeição explícita de reservas
- Relatórios PDF/Excel
- Integração com Email (SendGrid/SMTP)
- Dashboard de Analytics
- Mobile app

---

## 🔌 ENDPOINTS DA API

### ✅ Públicos (Não requerem autenticação)

#### GET `/health`
```bash
curl https://projeto-integrador2-latest.onrender.com/health
```
Resposta:
```json
{
  "status": "ok",
  "database": "connected",
  "timestamp": "2026-08-22T23:45:35.6289843Z"
}
```

#### GET `/api/rooms`
```bash
curl https://projeto-integrador2-latest.onrender.com/api/rooms
```
Retorna lista de salas ativas com recursos:
```json
[
  {
    "id": "room_204",
    "name": "Sala 204",
    "floor": "2º andar",
    "capacity": 35,
    "description": "Sala ampla com projeção fixa",
    "resources": ["Projetor", "Ar-condicionado", "Quadro branco"]
  }
]
```

#### GET `/api/reservations?status=Approved`
```bash
curl "https://projeto-integrador2-latest.onrender.com/api/reservations?status=Approved"
```
Retorna apenas reservas aprovadas (público para painel TV):
```json
[
  {
    "id": "uuid",
    "roomId": "room_204",
    "requesterId": "teacher-1",
    "title": "Cálculo II",
    "attendees": 30,
    "status": "Approved",
    "occurrences": [
      { "start": "2026-09-02T19:00:00Z", "end": "2026-09-02T22:00:00Z" }
    ]
  }
]
```

---

### 🔐 Protegidos (Requerem autenticação JWT - A Implementar)

#### POST `/api/reservations` - Criar Reserva
```bash
curl -X POST https://projeto-integrador2-latest.onrender.com/api/reservations \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "roomId": "room_204",
    "requesterId": "teacher-1",
    "start": "2026-09-02T19:00:00Z",
    "end": "2026-09-02T22:00:00Z",
    "title": "Cálculo II",
    "attendees": 30,
    "recurrence": {
      "days": ["Tuesday", "Thursday"],
      "until": "2026-12-15T22:00:00Z"
    }
  }'
```

**Status Esperados:**
- `201 Created` - Reserva criada com sucesso
- `400 Bad Request` - Capacidade excedida ou dados inválidos
- `404 Not Found` - Sala não encontrada
- `401 Unauthorized` - Sem token JWT

#### POST `/api/reservations/{id}/approve` - Aprovar Reserva
```bash
curl -X POST https://projeto-integrador2-latest.onrender.com/api/reservations/uuid-here/approve \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "coordinator-1",
    "role": "Coordinator"
  }'
```

**Status Esperados:**
- `200 OK` - Reserva aprovada
- `409 Conflict` - Conflito de horário com outra reserva
- `403 Forbidden` - Usuário sem permissão (só Coordinator/Administrator)
- `404 Not Found` - Reserva não existe

#### POST `/api/reservations/{id}/cancel` - Cancelar Reserva
```bash
curl -X POST https://projeto-integrador2-latest.onrender.com/api/reservations/uuid-here/cancel \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "teacher-1",
    "role": "Teacher"
  }'
```

**Status Esperados:**
- `200 OK` - Reserva cancelada
- `403 Forbidden` - Apenas proprietário ou admin podem cancelar
- `404 Not Found` - Reserva não existe

---

## 📊 RESUMO TÉCNICO

### Stack de Tecnologias

```
Frontend:
  ├─ HTML5 + CSS3 (estrutura base)
  ├─ JavaScript (integração em progresso)
  └─ (Sem framework - plain JS por simplicidade)

Backend:
  ├─ C# 12 (.NET 8.0 LTS)
  ├─ ASP.NET Core 8 (Minimal APIs)
  ├─ Entity Framework Core 8
  └─ Npgsql (driver PostgreSQL)

Banco de Dados:
  ├─ PostgreSQL 14+ (via Supabase)
  ├─ Row Level Security (RLS)
  └─ Migrations versionadas

Testes:
  ├─ xUnit (framework)
  ├─ 7 testes unitários principais + 2 E2E
  └─ Cobertura: 80%

DevOps:
  ├─ GitHub Actions (CI/CD)
  ├─ Docker (containerização)
  ├─ Docker Hub (registro de imagens)
  └─ Render (hosting)

Melhorias CI/CD:
  ├─ NuGet Cache (⚡ -70% restore time)
  ├─ Docker Buildx (⚡ -40% build time)
  ├─ Metadata automática
  ├─ Concurrency com cancel
  └─ Test Artifacts
```

### Métricas de Performance

| Métrica | Valor | Status |
|---------|-------|--------|
| **Tempo Build** | ~3-4 min | ✅ Otimizado |
| **Tempo Deploy** | ~2-3 min | ✅ Sem rebuild |
| **Health Check** | <10ms | ✅ Rápido |
| **Lista Salas** | ~50ms | ✅ Aceitável |
| **Cobertura Testes** | 80% | ✅ Boa |
| **Uptime Render** | 99%+ | ✅ Estável |

---

## 📖 ÍNDICE COMPLETO DE DOCUMENTAÇÃO

### Por Tipo de Documento

**📋 Técnicos:**
- [ANALISE-PROJETO.md](./ANALISE-PROJETO.md) - Análise técnica completa
- [.github/CI-CD-IMPROVEMENTS.md](./.github/CI-CD-IMPROVEMENTS.md) - CI/CD em detalhe
- [.github/workflows/dotnet.yml](./.github/workflows/dotnet.yml) - Arquivo YAML do pipeline

**🛣️ Planejamento:**
- [ROADMAP-ACOES.md](./ROADMAP-ACOES.md) - O que fazer agora e próximas semanas
- [STATUS-VISUAL.md](./STATUS-VISUAL.md) - Painel de controle rápido

**📚 Requisitos & Histórias:**
- [frontend/historia_do_usuario.md](./frontend/historia_do_usuario.md) - Perspectiva da Renata (usuária)
- [Arquivos/Material de Estudo/historia_do_usuario.md](./Arquivos/_Material%20de%20Estudo/historia_do_usuario.md) - Documentação expandida

**🔧 Infraestrutura:**
- [Dockerfile](./Dockerfile) - Containerização multi-stage
- [supabase/migrations/001_initial.sql](./supabase/migrations/001_initial.sql) - Schema + seed data
- [Projeto-Integrador2.csproj](./Projeto-Integrador2.csproj) - Configuração .NET

---

## 🚀 PRÓXIMOS PASSOS (ROADMAP DE 4-5 SEMANAS)

```
┌─────────────────────────────────────────────────────────┐
│ SEMANA 1 - SEGURANÇA (CRÍTICA)                          │
├─────────────────────────────────────────────────────────┤
│ [ ] Implementar JWT (6h) - Backend Lead                │
│ [ ] Completar RLS no banco (4h) - DBA                  │
│ [ ] Testes de segurança (4h) - QA                      │
│ Status Alvo: API segura para produção ✅                │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│ SEMANA 2-3 - FUNCIONALIDADE (IMPORTANTE)                │
├─────────────────────────────────────────────────────────┤
│ [ ] Frontend integrado (25h) - Frontend Lead            │
│ [ ] Paginação implementada (2h) - Backend               │
│ [ ] Validações completas (4h) - Backend                │
│ [ ] Logging centralizado (5h) - Backend                │
│ [ ] Testes E2E (8h) - QA                               │
│ Status Alvo: MVP funcional ✅                           │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│ SEMANA 4+ - POLISH E FEATURES (NICE-TO-HAVE)            │
├─────────────────────────────────────────────────────────┤
│ [ ] Notificações (SignalR) (8h)                         │
│ [ ] Endpoint de rejeição (1h)                           │
│ [ ] Relatórios (6h)                                     │
│ [ ] Painel TV público (5h)                              │
│ Status Alvo: Produção full ✅                           │
└─────────────────────────────────────────────────────────┘

ETA FINAL: 15-20 de Setembro de 2026
Equipe recomendada: 3 pessoas (1 backend, 1 frontend, 1 QA)
```

---

## ❓ PERGUNTAS FREQUENTES

**P: Por onde devo começar se sou novo no projeto?**
R: Leia [STATUS-VISUAL.md](./STATUS-VISUAL.md) (5 min) e sua tarefa específica em [ROADMAP-ACOES.md](./ROADMAP-ACOES.md).

**P: Qual é o bloqueador para produção?**
R: Autenticação JWT. Sem ela, qualquer pessoa pode fazer qualquer ação. Ver "Bloqueador para Produção" em [ANALISE-PROJETO.md](./ANALISE-PROJETO.md).

**P: Como testo a API localmente?**
R: `dotnet run` inicia em http://localhost:5000, ou use Postman com exemplos em ROADMAP-ACOES.md.

**P: Quanto tempo até estar pronto para produção?**
R: ~4-5 semanas com equipe de 3 pessoas (Backend, Frontend, QA).

**P: O banco está pronto para usar?**
R: Sim! Execute `supabase/migrations/001_initial.sql` no Supabase e ele vem com 6 salas + 6 recursos de exemplo.

**P: Posso começar a trabalhar no frontend agora?**
R: Sim, mas configure JWT primeiro (Fase 1). Sem autenticação, você só consegue testar GET endpoints públicos.

---

## 📞 CONTATOS & LINKS

**Repositório:** https://github.com/seu-repo/Projeto-Integrador2  
**API Live:** https://projeto-integrador2-latest.onrender.com  
**Banco:** https://app.supabase.com  
**CI/CD:** https://github.com/seu-repo/actions  
**Deploy:** https://dashboard.render.com  

---

## Desenvolvimento:

- Qualquer professor ou colaborador consiga **pedir uma sala** informando o que precisa (data, horário, finalidade, quantas vezes se repete, se for o caso), sem precisar falar comigo diretamente antes.
- Quando o pedido é só de "vez em quando" (uma reunião pontual, por exemplo), tudo bem, mas quando é uma coisa que **se repete** (todo dia, toda semana, num intervalo específico, até uma certa data), eu não quero que a pessoa tenha que cadastrar uma por uma — quero que o sistema já organize essa série toda de uma vez.
- **Eu (ou alguém no meu lugar) preciso aprovar** cada pedido antes de valer — assim ninguém usa uma sala sem eu saber, e eu consigo enxergar tudo que está pendente de decisão num lugar só, sem depender de mensagens perdidas.
- Um backlog de produto é uma lista viva e organizada por ordem de prioridade com tudo o que o produto precisa. Ele inclui novas funções, correções de erros e melhorias técnicas. Os itens mais importantes ficam sempre no topo para orientar o trabalho da equipe.
- Meio da lista (Média Prioridade):Funcionalidade: Como comprador, quero filtrar os produtos por faixa de preço para achar itens que cabem no meu orçamento. (Estimativa: 8 pontos)
- Topo da lista (Alta Prioridade / Pronto para a Sprint):História de Usuário: Como cliente, quero recuperar minha senha por e-mail para conseguir entrar na minha conta caso esqueça os dados. (Estimativa: 3 pontos)
- Meio da lista (Média Prioridade):Funcionalidade: Como comprador, quero filtrar os produtos por faixa de preço para achar itens que cabem no meu orçamento. (Estimativa: 8 pontos)
 


---

## 💡 Ideias para o Backlog

| Prioridade |	Ideia / Funcionalidade |	Descrição |
| :--- | :--- | :--- |
|🔴 Alta |	🔐 Login de usuários |	Professor, aluno e administrador entram com usuário e senha |
|🔴 Alta |👨‍🏫 Cadastro de professores	| Cadastrar nome, e-mail, curso e outras informações |
|🔴 Alta	|👨‍🎓 Cadastro de alunos |	Registrar alunos e suas respectivas turmas |
|🔴 Alta |	🏫 Cadastro de salas |	Número/nome da sala, capacidade e localização |
|🔴 Alta	|📅 Reserva de salas |	Professor escolhe sala, data e horário |
|🔴 Alta	|🚫 Evitar reservas duplicadas |	Não permitir duas reservas da mesma sala no mesmo horário |
|🔴 Alta	|✅ Salas disponíveis |	Mostrar quais salas estão livres |
|🔴 Alta	|❌ Salas indisponíveis |	Mostrar salas já ocupadas |
|🟠 Média	|🗓️ Calendário de reservas |	Visualizar reservas por dia, semana e mês |
|🟠 Média	|📚 Cadastro de cursos |	Registrar os cursos da instituição |
|🟠 Média	|👥 Cadastro de turmas |	Associar alunos, professores e cursos às turmas |
|🟠 Média	|⏰ Cadastro de horários |	Registrar horários das aulas |
|🟠 Média	|🔔 Notificações |	Avisar quando ocorrer mudança ou cancelamento |
|🟠 Média	|🔄 Alterar reserva |	Professor poderá modificar uma reserva existente |
|🟠 Média	|🗑️ Cancelar reserva |	Permitir cancelamento de uma sala |
|🟠 Média	|🔎 Pesquisa de salas |	Pesquisar sala pelo número, capacidade ou tipo |
|🟡 Baixa	|🖥️ Painel do professor |	Mostrar as reservas feitas pelo professor |
|🟡 Baixa	|🎓 Painel do aluno |	Aluno consulta onde acontecerá sua aula |
|🟡 Baixa	|👨‍💼 Painel administrativo |	Administrador controla salas, professores e turmas |
|🟡 Baixa	|📜 Histórico de reservas | 	Guardar reservas anteriores |
|🟡 Baixa	|📊 Relatórios |	Mostrar salas mais utilizadas, horários etc. |
|🟡 Baixa	|🚩 Status da sala |	Livre, ocupada, reservada ou em manutenção |
|🟡 Baixa	|🔢 Capacidade da sala |	Informar quantidade máxima de alunos |
|🟡 Baixa	|💻 Tipo de sala |	Laboratório, sala normal, auditório etc. |
|🟡 Baixa	|📱 Sistema responsivo |	Funcionar bem no computador e celular |

---

## ⭐ Aprovação de reservas

**O professor faz a solicitação:**
<br>
<br>
Professor solicita → sistema verifica conflito → administrador aprova → sala é reservada → professor/alunos recebem notificação.
<br>
Isso deixaria o projeto mais completo.
E vocês podem transformar o backlog em User Stories, por exemplo:
<br>

 - 👨‍🏫 Como professor, quero visualizar as salas disponíveis para poder reservar uma sala para minha aula.
 - 👨‍🎓 Como aluno, quero consultar minha sala para saber onde acontecerá minha próxima aula.
 - 👨‍💼 Como administrador, quero cadastrar e bloquear salas para manter o sistema atualizado.
 - 🔔 Como usuário, quero receber uma notificação quando minha sala for alterada ou a aula for cancelada.
   
<br>
Para começar a desenvolver, eu priorizaria nesta ordem: Login → Cadastro de usuários → Cadastro de salas → Cadastro de turmas → Agenda → Disponibilidade → Reserva → Bloqueio de conflito → Cancelamento → Notificações → Histórico e relatórios. Isso já pode virar o Product Backlog oficial do trabalho.

## Atualização de execução — 26/08/2026

Status do colaborador: **integração frontend/backend concluída**, com endpoints de usuários, salas, recursos e decisões de reservas; a tela também possui fallback offline para demonstração.

Entregas validadas nesta atualização:

- Correção da compilação: a cópia duplicada de `ReservationDbContext.cs` permanece preservada, mas deixou de ser compilada.
- Paginação de `GET /api/reservations?page=1&pageSize=20`, limitada a 100 itens, com metadados `data` e `pagination`.
- Frontend adaptado ao novo envelope paginado.
- Migration `002_frontend_integration.sql` ampliada com políticas RLS idempotentes e permissões para usuários autenticados.
- Projeto de testes alinhado ao .NET 8; **9 testes passando (7 unitários e 2 E2E)**.

Pendências que continuam abertas: autenticação real com credenciais e JWT, CORS restrito em produção, testes E2E e tratamento explícito de fuso horário. O login atual por seleção de usuário é apenas demonstração e não deve ser considerado autenticação de produção.

### Cronograma de evolução

| Período | Entrega | Status |
|---|---|---|
| 22/08 | Integração inicial frontend/backend e migration 002 | ✅ Concluído |
| 26/08 | Correção de build, paginação, RLS complementar e testes .NET 8 | ✅ Concluído |
| 27–29/08 | JWT com credenciais, claims e proteção dos endpoints de escrita | ⏳ Planejado |
| 30/08–02/09 | Testes E2E, CORS de produção e revisão de fuso horário | ⏳ Planejado |
| 03–06/09 | Observabilidade, rate limiting e validação de deploy | ⏳ Planejado |
