# Projeto-Integrador2


links uteis:

https://whiteboard.cloud.microsoft/me/whiteboards/p/c3BvOmh0dHBzOi8vc2VuYWNzYzc1NC1teS5zaGFyZXBvaW50LmNvbS9wZXJzb25hbC9tYXVyaWNpbzYxMTg0Njg2X2FsdW5vc19zY19zZW5hY19icg%3D%3D/b!COjkxJBZCkW-sK41v2rjLH2P3dB-AepJvkIe8dv8y2u42NcfHc1DRL1W0f5SeIKZ/012GH7RIN3AK4RIIEXNJBIXXMSM2VXNABF?lng=pt-br&ref=oib-09dc32fc-45b8-4913-a439-1a70f7a9ddfe

Link do kanban:

https://planner.cloud.microsoft/webui/v1/plan/D2rlXSdIO0GLktdGp6w4u2QAHi8w?tid=0917fe10-56db-44bf-b1c5-31061ab21cf9


Link das benchmarks:

https://www.gentrop.com/produtos/reservaai

https://agenda1.app/

# História do Usuário — Sistema de Reserva de Salas

## Quem está contando essa história

Meu nome é Renata, sou coordenadora administrativa de uma unidade do Senac Joinville. Entre outras coisas, eu cuido da organização das salas de aula do prédio: quem usa cada uma, em qual horário, e resolvo os problemas quando duas turmas acabam batendo na mesma sala.

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

No fim das contas, eu gasto um tempo enorme só tentando organizar manualmente uma coisa que deveria ser simples: garantir que a sala certa, no tamanho certo, com o que for preciso dentro dela, esteja disponível pra quem precisa, sem conflitos.

## Quem mais é afetado por esse problema

- **Os professores e colaboradores** que precisam de uma sala — hoje eles não têm nenhuma previsibilidade: mandam a mensagem e ficam esperando eu responder, às vezes demoro dias, às vezes a resposta se perde na conversa.
- **Os alunos** — chegam no prédio sem saber onde é a aula ou o que está rolando em cada sala/andar naquele turno.
- **Eu (coordenação)** — sou o gargalo de tudo: recebo os pedidos, decido, resolvo conflito, e ainda preciso lembrar de cadastrar/desativar pessoas manualmente.
- **Quem cuida da estrutura física das salas** — hoje ninguém tem uma lista organizada de quais salas existem, quantos alunos cabem em cada uma e quais equipamentos/recursos elas têm. Isso vive só na cabeça de quem já trabalha aqui há muito tempo.

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

## Um dia perfeito, se isso existisse

Um professor entra no sistema e pede uma sala para as terças e quintas, das 19h às 22h, até o fim do semestre, porque a disciplina dele é nesses dias. Ele recebe a confirmação de que o pedido foi enviado e fica esperando.

Eu entro no sistema, vejo que tem um pedido novo esperando aprovação, confiro se bate com a disponibilidade da sala (e o tamanho da turma cabe na sala escolhida), e aprovo tudo de uma vez — o sistema já cria as aulas de terça e quinta até o fim do semestre sozinho, sem eu ter que repetir a ação toda semana.

Se por acaso dois pedidos batessem na mesma sala e horário, eu quero saber disso antes de aprovar os dois, não descobrir depois.

Um aluno chega no prédio às 19h10, olha a TV do corredor, vê que no 2º andar, sala 204, está acontecendo a aula dele com o nome do professor, e vai direto pra lá sem precisar perguntar na recepção.

No fim do mês, se um colaborador sai da instituição, eu simplesmente desativo o acesso dele, sem apagar o que ele já tinha reservado.

## Algumas preocupações e limites que eu tenho

- Não quero que qualquer pessoa consiga aprovar reservas — isso tem que ficar restrito a quem realmente tem essa responsabilidade.
- Não quero perder o histórico de nada — se uma reserva for cancelada ou um usuário for desativado, prefiro guardar o registro a apagar de vez.
- Preciso que fique claro, a qualquer momento, quais reservas ainda estão pendentes de decisão minha, e quais já estão confirmadas.
- Quero poder confiar que a capacidade da sala e os recursos cadastrados estão corretos, porque é isso que vai evitar os erros de alocação que hoje acontecem por falta de informação.
- A tela que fica na TV para os alunos precisa funcionar sozinha, sem alguém precisar ficar de olho nela ou trocando de tela manualmente ao longo do dia.




Desenvolvimento:

- Qualquer professor ou colaborador consiga **pedir uma sala** informando o que precisa (data, horário, finalidade, quantas vezes se repete, se for o caso), sem precisar falar comigo diretamente antes.
- Quando o pedido é só de "vez em quando" (uma reunião pontual, por exemplo), tudo bem, mas quando é uma coisa que **se repete** (todo dia, toda semana, num intervalo específico, até uma certa data), eu não quero que a pessoa tenha que cadastrar uma por uma — quero que o sistema já organize essa série toda de uma vez.
- **Eu (ou alguém no meu lugar) preciso aprovar** cada pedido antes de valer — assim ninguém usa uma sala sem eu saber, e eu consigo enxergar tudo que está pendente de decisão num lugar só, sem depender de mensagens perdidas.


Um backlog de produto é uma lista viva e organizada por ordem de prioridade com tudo o que o produto precisa. Ele inclui novas funções, correções de erros e melhorias técnicas. Os itens mais importantes ficam sempre no topo para orientar o trabalho da equipe.
 
Meio da lista (Média Prioridade):Funcionalidade: Como comprador, quero filtrar os produtos por faixa de preço para achar itens que cabem no meu orçamento. (Estimativa: 8 pontos)
 
Topo da lista (Alta Prioridade / Pronto para a Sprint):História de Usuário: Como cliente, quero recuperar minha senha por e-mail para conseguir entrar na minha conta caso esqueça os dados. (Estimativa: 3 pontos)
 
Meio da lista (Média Prioridade):Funcionalidade: Como comprador, quero filtrar os produtos por faixa de preço para achar itens que cabem no meu orçamento. (Estimativa: 8 pontos)
 




💡 Ideias para o Backlog
Prioridade	Ideia / Funcionalidade	Descrição
🔴 Alta	🔐 Login de usuários	Professor, aluno e administrador entram com usuário e senha
🔴 Alta	👨‍🏫 Cadastro de professores	Cadastrar nome, e-mail, curso e outras informações
🔴 Alta	👨‍🎓 Cadastro de alunos	Registrar alunos e suas respectivas turmas
🔴 Alta	🏫 Cadastro de salas	Número/nome da sala, capacidade e localização
🔴 Alta	📅 Reserva de salas	Professor escolhe sala, data e horário
🔴 Alta	🚫 Evitar reservas duplicadas	Não permitir duas reservas da mesma sala no mesmo horário
🔴 Alta	✅ Salas disponíveis	Mostrar quais salas estão livres
🔴 Alta	❌ Salas indisponíveis	Mostrar salas já ocupadas
🟠 Média	🗓️ Calendário de reservas	Visualizar reservas por dia, semana e mês
🟠 Média	📚 Cadastro de cursos	Registrar os cursos da instituição
🟠 Média	👥 Cadastro de turmas	Associar alunos, professores e cursos às turmas
🟠 Média	⏰ Cadastro de horários	Registrar horários das aulas
🟠 Média	🔔 Notificações	Avisar quando ocorrer mudança ou cancelamento
🟠 Média	🔄 Alterar reserva	Professor poderá modificar uma reserva existente
🟠 Média	🗑️ Cancelar reserva	Permitir cancelamento de uma sala
🟠 Média	🔎 Pesquisa de salas	Pesquisar sala pelo número, capacidade ou tipo
🟡 Baixa	🖥️ Painel do professor	Mostrar as reservas feitas pelo professor
🟡 Baixa	🎓 Painel do aluno	Aluno consulta onde acontecerá sua aula
🟡 Baixa	👨‍💼 Painel administrativo	Administrador controla salas, professores e turmas
🟡 Baixa	📜 Histórico de reservas	Guardar reservas anteriores
🟡 Baixa	📊 Relatórios	Mostrar salas mais utilizadas, horários etc.
🟡 Baixa	🚩 Status da sala	Livre, ocupada, reservada ou em manutenção
🟡 Baixa	🔢 Capacidade da sala	Informar quantidade máxima de alunos
🟡 Baixa	💻 Tipo de sala	Laboratório, sala normal, auditório etc.
🟡 Baixa	📱 Sistema responsivo	Funcionar bem no computador e celular



⭐ Aprovação de reservas
O professor faz a solicitação:
Professor solicita → sistema verifica conflito → administrador aprova → sala é reservada → professor/alunos recebem notificação.
Isso deixaria o projeto mais completo.
E vocês podem transformar o backlog em User Stories, por exemplo:
👨‍🏫 Como professor, quero visualizar as salas disponíveis para poder reservar uma sala para minha aula.
👨‍🎓 Como aluno, quero consultar minha sala para saber onde acontecerá minha próxima aula.
👨‍💼 Como administrador, quero cadastrar e bloquear salas para manter o sistema atualizado.
🔔 Como usuário, quero receber uma notificação quando minha sala for alterada ou a aula for cancelada.
Para começar a desenvolver, eu priorizaria nesta ordem: Login → Cadastro de usuários → Cadastro de salas → Cadastro de turmas → Agenda → Disponibilidade → Reserva → Bloqueio de conflito → Cancelamento → Notificações → Histórico e relatórios. Isso já pode virar o Product Backlog oficial do trabalho.