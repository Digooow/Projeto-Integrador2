# Ocupa - Metodologia Ágil e Desenvolvimento

Este documento descreve o processo de desenvolvimento ágil adotado pela equipe na construção do **Ocupa** (anteriormente chamado "Smart Sala"), documentando os aprendizados e o método empregado que embasou o ciclo de vida do software. 

A documentação reflete o pilar mais valioso do projeto acadêmico original, que independe de futuras trocas de tecnologia ou reestruturações de produto.

## 1. O Problema e as Personas

Antes da escrita de código (Delivery), a equipe passou pela fase de **Discovery**, priorizando escutar o usuário real. 

Através da identificação de dores de coordenações reais, a equipe mapeou que **o controle via WhatsApp e planilhas gerava conflitos de alocação de salas e falta de visibilidade para alunos**. Desse estudo de campo emergiram duas personas vitais:

1.  **Renata (A Coordenadora Administrativa):** 
    *   *Dor principal:* Fazer aprovações "no escuro" e descobrir a duplicidade apenas quando duas turmas chegam na porta da sala ao mesmo tempo.
    *   *Necessidade:* Ser informada automaticamente de conflitos.
2.  **Fernanda (A Professora):** 
    *   *Dor principal:* O processo burocrático de pedir repetidas vezes a sala da sua disciplina fixa de toda semana.
    *   *Necessidade:* Criação de uma reserva com fluxo de recorrência transparente.

## 2. A Metodologia e Ferramentas de Gestão

Para guiar a construção do software e promover a previsibilidade, a equipe mesclou duas ferramentas clássicas das metodologias ágeis:

*   **Scrum (Ritmo):** Foram utilizados ciclos curtos e delimitados de desenvolvimento (Sprints). Cada Sprint passava pelos ritos de *Sprint Planning* (Planejamento), *Dailies* (Acompanhamento diário), *Review* (Validação do incremento) e *Retrospectiva* (Evolução do time).
*   **Kanban (Fluxo):** Com limite de WIP (*Work in Progress*), a equipe controlou visualmente as atividades no modelo clássico (Product Backlog -> Sprint Backlog -> Fazendo -> Revisão -> Feito), garantindo transparência e foco na conclusão da tarefa iniciada. O GitHub consolidou os artefatos.

## 3. Auto-organização e Rotação de Papéis

Com uma equipe formada por 6 integrantes (Matheus, Maurício, Maycon, Raphaella, Rodrigo e Viviane), instituiu-se uma **rotação de papéis a cada Sprint**. Isso garantiu empatia entre as frentes e disseminação do conhecimento do framework:

*   **Product Owner (PO):** Representando o negócio, priorizando o backlog com base na dor da persona Renata e decidindo se o incremento era aceitável.
*   **Scrum Master:** Removendo impedimentos, blindando o time de escopo não-oficial e conduzindo as cerimônias ágeis.
*   **Time de Desenvolvimento:** Responsáveis pela construção técnica, infraestrutura, modelagem do domínio e testes E2E.

## 4. O Mindset de Incremento e Corte de Escopo

O maior aprendizado documentado no processo foi a premissa fundamental: **cortar cedo protege a entrega final.** 

Caso uma tarefa durante a Sprint (como a implementação de notificações push/SMS ou complexidades de Frontend como React/Node.js) se demonstrasse mais arriscada que o esperado para o ciclo, o time e o PO alinhavam a retirada dessa tarefa do escopo, preferindo entregar uma funcionalidade base, como o Backend em ASP.NET Minimal API e Frontend simples em JS Vanilla, que atendesse ao problema das personas e pudesse ser validado, ao invés de buscar a perfeição e não entregar nada (Evitando falsos compromissos).

## 5. Legado e Evolução

A cultura metodológica impulsionada na fase de prototipação do Ocupa continua sendo o guia para seu futuro. Embora o sistema possa expandir a *stack* ou pivotar sua regra de negócio futuramente, o pilar ágil de **Discovery** centrado no usuário e validação através de Sprints sustentam as bases de qualidade do projeto.
