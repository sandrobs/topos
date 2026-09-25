# Escopo do MVP

## 1. Visão do produto

O produto é um CRM de acolhimento e acompanhamento de visitantes da Igreja Evangélica de Cristo. Seu objetivo é reduzir a perda de contatos, organizar o trabalho da equipe e garantir que pessoas interessadas em conhecer a igreja ou receber oração sejam atendidas.

O MVP não é um sistema completo de gestão de igrejas nem um cadastro de membros.

## 2. Meta de lançamento

- **Prazo:** produção até sexta-feira, 18/09/2026.
- **Estratégia:** entregar um fluxo vertical completo, do QR Code à conclusão do atendimento.
- **Critério de prioridade:** funcionalidades sem as quais o fluxo não opera com segurança são P0; todo o restante será adiado.

### Marco mínimo de sucesso

Com uma igreja e seus usuários previamente configurados, o fluxo abaixo deve funcionar de ponta a ponta:

`QR Code → formulário enviado → visitante no painel correto → contato por WhatsApp → observação e mudança de etapa`

Esse marco tem prioridade sobre telas administrativas, indicadores e conveniências operacionais. Autenticação, isolamento entre igrejas, validação e segurança mínima não podem ser retirados para acelerar a entrega.

## 3. Usuários

### Visitante

Acessa o formulário por QR Code, informa dados mínimos e solicita contato. Não cria conta e não acessa o painel.

### Equipe

Acompanha todos os visitantes de sua igreja, registra observações, atribui responsáveis e movimenta cartões.

### Pastor

Possui as capacidades da Equipe e também cadastra e administra usuários de sua igreja.

### Administrador da plataforma

Possui acesso a todas as igrejas, usuários e visitantes. Cadastra igrejas, presta suporte e pode realizar as mesmas ações do Pastor e da Equipe.

## 4. Jornada principal

1. O Administrador cadastra uma igreja.
2. O sistema disponibiliza um endereço público e um QR Code exclusivo e permanente para a igreja.
3. O visitante aponta o celular para o QR Code.
4. O formulário identifica automaticamente a igreja, sem solicitar cidade ou unidade.
5. O visitante informa nome, WhatsApp e pelo menos um interesse.
6. O visitante reconhece o aviso de privacidade e envia o formulário.
7. O sistema confirma o recebimento sem expor informações internas.
8. Um cartão aparece na coluna **Novo visitante** do painel da igreja correta.
9. Pastor, Equipe ou Administrador atribui o atendimento, inicia o contato por WhatsApp, registra observações e movimenta o cartão.
10. Ao finalizar, o usuário seleciona um resultado obrigatório.

## 5. Escopo P0 Core — entrega mínima protegida

### Captação e acompanhamento

- Uma igreja configurada com endereço público e QR Code funcional.
- Formulário curto e mobile-first, com os campos mínimos definidos.
- Criação do atendimento na igreja correta.
- Confirmação do envio.
- Autenticação dos usuários internos que participarão do lançamento.
- Isolamento dos dados por igreja.
- Painel responsivo com as etapas do atendimento.
- Visualização dos dados e interesses do visitante.
- Mudança de etapa sem depender de arrastar o cartão.
- Movimentação opcional dos cartões por arrastar e soltar, mantendo controles por toque/clique e teclado.
- Observações internas com autor e data.
- Botão para abrir uma conversa no WhatsApp.
- Conclusão com resultado obrigatório.

## 6. Escopo P0 Ampliado — implementar após o Core

### Captação

- Página pública vinculada à igreja pelo endereço do QR Code.
- Formulário curto, mobile-first e sem autenticação.
- Campos mínimos definidos no documento de requisitos.
- Confirmação de envio clara e acolhedora.
- Um QR Code por igreja.

### Administração

- Autenticação dos usuários internos.
- Cadastro, edição, ativação e desativação de igrejas pelo Administrador.
- Acesso ou download do QR Code da igreja.
- Cadastro, edição, ativação e desativação de usuários pelo Administrador.
- Gestão dos usuários da própria igreja pelo Pastor.
- Controle de acesso por perfil e igreja.

### Acompanhamento

- Kanban com as etapas definidas.
- Cartão com dados mínimos, interesses e responsável.
- Edição interna e opcional de dados complementares obtidos pela equipe durante o atendimento, sem ampliar o formulário público.
- Atribuição de responsável entre usuários ativos da mesma igreja.
- Mudança de etapa.
- Arrastar e soltar entre etapas adjacentes com alternativa acessível sem arraste.
- Observações internas com autor e data.
- Histórico de mudanças de etapa.
- Botão para abrir uma conversa no WhatsApp.
- Conclusão com resultado obrigatório.
- Pesquisa por nome ou WhatsApp.

### Qualidade mínima

- Uso em celular, tablet e computador.
- Isolamento de dados entre igrejas no servidor.
- Validação de entradas.
- Proteções básicas do formulário público contra abuso.
- Operação por HTTPS em produção.

Se o prazo estiver ameaçado, telas completas de administração podem ser simplificadas temporariamente, desde que exista um procedimento controlado e documentado para configurar a primeira igreja e seus usuários. Isso não autoriza remover autenticação ou isolamento de dados.

## 7. Escopo P1 — somente se todo o P0 estiver pronto

- Data opcional de próximo contato, exibida no cartão e na pesquisa.
- Filtros por responsável, interesse e resultado.
- Indicadores simples: novos, em acompanhamento e concluídos.

P1 não inclui notificações automáticas.

## 8. Fora do escopo do MVP

- Cadastro completo e gestão de membros.
- Conversão automática de visitante em membro.
- API oficial ou envio automático de WhatsApp.
- Chat dentro da plataforma.
- Texto livre de pedido de oração no formulário público.
- Múltiplos QR Codes por igreja.
- Eventos, campanhas ou identificação da origem do QR Code.
- Detecção, mesclagem ou resolução automática de duplicidades.
- E-mail, SMS ou outros canais de contato.
- Lembretes e notificações automáticas.
- Relatórios avançados e exportações.
- Aplicativo nativo.
- Gestão financeira, ministerial, de cultos ou de presença.
- Personalização visual por igreja.
- Exclusão automática por prazo de retenção.

## 9. Identidade visual inicial

A interface deve usar a identidade da logo fornecida pela igreja, com aparência acolhedora, simples e sóbria.

- Azul-marinho principal aproximado: `#080C5C`
- Azul secundário aproximado: `#243174`
- Branco: `#FFFFFF`
- Fundo neutro: `#F5F6FA`
- Texto principal: tom escuro de alto contraste

A logo recebida possui baixa resolução. Ela pode ser usada no MVP, mas deve ser substituída posteriormente por uma versão maior ou vetorial. A legibilidade e o contraste têm prioridade sobre o uso excessivo do azul.

## 10. Princípios de experiência

- O visitante deve compreender o formulário sem instruções externas.
- O formulário deve caber em uma única jornada curta, sem etapas desnecessárias.
- Não solicitar dados que a equipe possa obter durante o contato.
- Linguagem acolhedora, sem pressão para filiação.
- Em telas pequenas, o Kanban pode ser apresentado como lista agrupada ou filtrada por etapa; nenhuma funcionalidade essencial pode depender de arrastar com o mouse.
