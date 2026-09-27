# Histórico resumido

Este arquivo registra somente os principais marcos. Detalhes de sprints,
análises antigas e versões intermediárias foram removidos da documentação
operacional para evitar contradições com o estado atual.

Os PDFs em `Arquivos/` são materiais acadêmicos e registros de apresentação.
Eles preservam decisões e telas de fases anteriores, mas não são a fonte de
verdade sobre a implementação vigente; para isso, consulte o
[README](../README.md) e a [arquitetura atual](./ARQUITETURA.md).

## Evolução

- **Descoberta e concepção:** definição do problema de reserva de salas,
  usuários, recorrência, aprovação e consulta pública.
- **Domínio:** criação das regras de negócio para capacidade, conflitos,
  recorrência, aprovação e cancelamento.
- **Persistência:** adoção de PostgreSQL/Supabase, Entity Framework Core,
  migrations e políticas RLS.
- **Integração:** frontend incorporado à API ASP.NET Core, com cadastro,
  reservas, decisões administrativas e paginação.
- **Segurança:** login por e-mail e senha, hash de senha, tokens JWT e
  autorização por papel.
- **Entrega:** Docker, GitHub Actions, publicação no Docker Hub e deploy no
  Render.
- **Validação:** testes unitários do domínio e testes E2E da API.

## Decisões que permanecem válidas

- Segredos e connection strings são configurados por ambiente, nunca por
  código versionado.
- Reservas recorrentes são persistidas como ocorrências individuais ligadas por
  uma série.
- O README descreve o presente; o [ROADMAP.md](../ROADMAP.md) descreve apenas
  o futuro.

As pendências atuais e os critérios para encerrá-las estão no roadmap, não
neste histórico.
