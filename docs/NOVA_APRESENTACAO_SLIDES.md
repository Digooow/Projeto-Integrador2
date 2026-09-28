---
marp: true
theme: default
style: |
  section {
    background-color: #0b1121;
    color: #f1f5f9;
    font-family: 'Segoe UI', system-ui, sans-serif;
    padding: 70px 90px;
    font-size: 26px;
  }
  h1 {
    color: #ffffff;
    font-size: 2.2em;
    font-weight: 800;
    margin-bottom: 0.2em;
    letter-spacing: -0.02em;
  }
  h1::after {
    content: '';
    display: block;
    width: 150px;
    height: 4px;
    background: linear-gradient(90deg, #f97316, #fbbf24);
    margin-top: 15px;
    border-radius: 2px;
  }
  h3 {
    color: #94a3b8;
    font-weight: 400;
    font-size: 1.1em;
    margin-bottom: 30px;
  }
  p, li {
    font-size: 0.95em;
    line-height: 1.5;
    color: #cbd5e1;
  }
  ul {
    margin-top: 15px;
  }
  li {
    margin-bottom: 12px;
  }
  strong {
    color: #f97316;
    font-weight: 600;
  }
  
  /* Efeito de brilho de fundo */
  section::before {
    content: '';
    position: absolute;
    top: -200px;
    right: -200px;
    width: 600px;
    height: 600px;
    background: radial-gradient(circle, rgba(249,115,22,0.12) 0%, rgba(11,17,33,0) 70%);
    border-radius: 50%;
    z-index: -1;
  }
  
  /* Slide de Título Customizado */
  .title-slide {
    padding: 0;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: flex-start;
    padding-left: 90px;
    background: linear-gradient(115deg, #0b1121 55%, #1e293b 55.1%, #1e293b 56%, #f97316 56.1%, #ea580c 100%);
  }
  .title-slide h1 {
    font-size: 2.8em;
  }
  .title-slide h1::after {
    width: 200px;
    height: 6px;
  }
  .title-slide h3 {
    color: #e2e8f0;
    font-size: 1.2em;
    max-width: 650px;
    margin-top: 20px;
  }
  .team {
    margin-top: 40px;
    font-size: 0.8em;
    color: #cbd5e1;
  }
  
  /* Rodapé */
  .footer-text {
    position: absolute;
    bottom: 30px;
    left: 90px;
    font-size: 0.55em;
    color: #475569;
    letter-spacing: 0.05em;
    text-transform: uppercase;
  }
  
  /* Grid em colunas */
  .columns {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 30px;
    margin-top: 20px;
  }
  
  /* Cards elegantes */
  .box {
    background: rgba(30, 41, 59, 0.4);
    border: 1px solid rgba(255, 255, 255, 0.05);
    padding: 25px;
    border-radius: 12px;
    box-shadow: 0 10px 30px -10px rgba(0,0,0,0.5);
    position: relative;
    overflow: hidden;
  }
  .box::before {
    content: '';
    position: absolute;
    top: 0; left: 0; width: 4px; height: 100%;
    background: linear-gradient(180deg, #f97316, #fbbf24);
  }
  .box-green::before { 
    background: linear-gradient(180deg, #10b981, #34d399); 
  }
  .box-blue::before { 
    background: linear-gradient(180deg, #3b82f6, #60a5fa); 
  }
  
  .box h4 {
    margin: 0 0 10px 0;
    color: #f8fafc;
    font-size: 1.1em;
  }
  .box p {
    font-size: 0.85em;
    margin: 0 0 10px 0;
  }
  .box p:last-child {
    margin-bottom: 0;
  }
---
<!-- _class: title-slide -->

# Ocupa: Sistema de<br>Reserva de Salas

### **Reserva, aprovação e ocupação em tempo real.**
O incremento construído através do desenvolvimento ágil.

<div class="team">
<strong>Equipe:</strong> Matheus, Maurício, Maycon, Raphaella, Rodrigo e Viviane
</div>

---

# O Desafio
### Um dia normal na gestão manual

<div class="box">
<p><em>"Já autorizei duas pessoas para a mesma sala, no mesmo horário, sem perceber — e só descobrimos o conflito quando os dois grupos chegam na porta."</em></p>
</div>

<br>

**A dor real:**
- **Pedidos dispersos** através de WhatsApp, e-mail e falas de corredor.
- **Planilhas desatualizadas** geram conflitos de agenda irreparáveis.
- **Falta de visibilidade:** os alunos não sabem para onde ir ao chegar no prédio.

<div class="footer-text">Ocupa — Metodologias Ágeis em Projetos Web</div>

---

# Para quem estamos construindo?
### Entendendo as reais necessidades dos usuários

<div class="columns">
<div class="box">
<h4>Renata Alves (Coordenação)</h4>
<p><strong>Objetivo:</strong> Ter uma visão geral, sem depender de mensagens soltas na rotina.</p>
<p><strong>A Dor:</strong> "Preciso saber dos conflitos antes de aprovar, não depois."</p>
</div>

<div class="box box-green">
<h4>Fernanda Lima (Professora)</h4>
<p><strong>Objetivo:</strong> Solicitar salas de forma rápida, seja pontual ou recorrente no semestre.</p>
<p><strong>A Dor:</strong> "Eu só queria pedir uma vez e pronto até o fim do período."</p>
</div>
</div>

<div class="footer-text">Ocupa — Metodologias Ágeis em Projetos Web</div>

---

# A Solução: Ocupa
### O corredor nunca mais fica no escuro

Um **lugar único** para pedir, aprovar e visualizar a ocupação das salas.

<div class="columns">
<div class="box box-blue">
<h4>Pedidos Simplificados</h4>
<p>Fluxos claros para reservas pontuais e criação em lote para reservas recorrentes (semanais).</p>
</div>

<div class="box box-green">
<h4>Aprovações Inteligentes</h4>
<p>Detecção automática de sobreposição de horários. A plataforma avisa sobre conflitos.</p>
</div>
</div>

<br>

- **Painel de Ocupação:** Visão pública e dinâmica sobre o uso do prédio para alunos.

<div class="footer-text">Ocupa — Metodologias Ágeis em Projetos Web</div>

---

# Metodologia Ágil
### Como organizamos o trabalho (Scrum + Kanban)

<div class="box">
<p><strong>Sprints e Ciclos Curtos:</strong> Foco em entregar incrementos de valor a cada etapa do desenvolvimento (Sem grandes entregas apenas no final).</p>
<p><strong>Rotação de Papéis:</strong> Vivência prática e alternada de <em>Product Owner</em>, <em>Scrum Master</em> e <em>Time de Desenvolvimento</em> por toda a equipe.</p>
<p><strong>Ajuste de Rota e Corte de Escopo:</strong> A regra de ouro foi: comprometer-se apenas com o que pode ser entregue com qualidade, cortando excessos atempadamente.</p>
</div>

<div class="footer-text">Ocupa — Metodologias Ágeis em Projetos Web</div>

---

# Arquitetura e Tecnologias
### Divisão em camadas para escalabilidade e segurança

<div class="columns">
<div>
<ul>
<li><strong>Front-end Web:</strong> HTML, CSS e JavaScript puros <em>(foco total em leveza e sem dependências)</em>.</li>
<li><strong>Back-end (API):</strong> ASP.NET Core 8 utilizando Minimal APIs (C#) e Arquitetura DDD.</li>
</ul>
</div>
<div>
<ul>
<li><strong>Banco de Dados:</strong> PostgreSQL hospedado na nuvem do Supabase.</li>
<li><strong>Infraestrutura:</strong> Pipeline de CI/CD automatizado no GitHub Actions e containers Docker.</li>
</ul>
</div>
</div>

<div class="footer-text">Ocupa — Metodologias Ágeis em Projetos Web</div>

---

# Segurança e Dados
### Desenhado com segurança desde a base

- **Autenticação:** Login seguro e tokens estruturados (JWT) totalmente implementados.
- **Controle de Acesso:** Permissões estritas divididas por papéis *(Requisitantes, Coordenadores, Administradores)*.
- **Proteção de Dados:** Uso de mapeamento de entidades via Entity Framework Core e políticas de segurança rígidas contra injeções.

<div class="footer-text">Ocupa — Metodologias Ágeis em Projetos Web</div>

---

# Próximos Passos
### A evolução contínua do produto

<div class="columns">
<div class="box">
<h4>Monitoramento e Interface</h4>
<p>Ampliar a capacidade de rastrear a saúde da aplicação. Ajustes contínuos na experiência do usuário e na acessibilidade geral.</p>
</div>

<div class="box box-blue">
<h4>Funcionalidades e Resiliência</h4>
<p>Expandir integrações operacionais conforme a demanda de uso real. Refinamento das rotinas de backup e escalabilidade.</p>
</div>
</div>

<div class="footer-text">Ocupa — Metodologias Ágeis em Projetos Web</div>

---
<!-- _class: title-slide -->

# OBRIGADO!
### Perguntas, feedbacks e sugestões são bem-vindos.

<div class="footer-text" style="color: #cbd5e1; bottom: 50px;">Ocupa — Sistema de Reserva de Salas · Metodologias Ágeis em Projetos Web</div>
