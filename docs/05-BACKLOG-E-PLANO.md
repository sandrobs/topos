# Backlog priorizado e plano de entrega

## 1. Regra de execução

O agente implementador deve concluir e demonstrar o P0 Core de ponta a ponta antes de iniciar qualquer melhoria. A escolha de tecnologia deve favorecer entrega e operação seguras no prazo, sem adicionar serviços ou integrações que não sejam necessários.

## 2. Backlog P0 Core

Esta é a ordem de implementação que protege o valor principal:

1. igreja configurada, endereço público e QR Code;
2. formulário público com validação;
3. criação do visitante na igreja correta;
4. autenticação e isolamento dos dados;
5. painel inicial responsivo com etapas;
6. detalhe do visitante e interesses;
7. ação de WhatsApp;
8. observação interna;
9. mudança de etapa e conclusão;
10. teste de fumaça em produção.

O Core precisa funcionar para o Administrador e para ao menos um usuário de igreja. A infraestrutura deve respeitar o modelo multi-igreja, ainda que as telas completas de administração sejam entregues no bloco seguinte.

## 3. Backlog P0 Ampliado

### Bloco A — Fundação e isolamento

- estrutura multi-igreja;
- autenticação;
- perfis Administrador, Pastor e Equipe;
- autorização no servidor;
- cadastro e ativação de igrejas;
- cadastro e ativação de usuários;
- dados iniciais para a conta Administrador.

### Bloco B — Captação

- endereço público permanente por igreja;
- QR Code;
- formulário mobile-first;
- validação de nome, WhatsApp, interesse e reconhecimento do aviso;
- confirmação do envio;
- tratamento de igreja inativa ou endereço inválido;
- proteção básica contra abuso.

### Bloco C — Acompanhamento

- painel por igreja;
- etapas do Kanban;
- visualização e detalhe do cartão;
- mudança somente entre etapas adjacentes, por controles acessíveis e por arrastar e soltar com `@dnd-kit/react`;
- atribuição de responsável;
- observações internas;
- histórico mínimo;
- pesquisa por nome e WhatsApp;
- ação de abrir WhatsApp;
- conclusão com resultado e reabertura.

### Bloco D — Preparação para produção

- responsividade;
- testes de permissão e acesso cruzado;
- teste do QR Code;
- validação das jornadas de aceite;
- HTTPS e configuração segura;
- backup;
- aviso de privacidade aprovado;
- procedimento operacional de suporte.

## 4. Itens avaliados por valor versus escopo

| Item | Valor | Custo/risco | Decisão para sexta-feira |
|---|---|---|---|
| Responsável pelo atendimento | Alto | Baixo | Incluir no P0 |
| Pesquisa por nome/WhatsApp | Alto | Baixo | Incluir no P0 |
| Próxima data de contato | Médio/alto | Baixo | P1, somente se P0 pronto |
| Notificações de lembrete | Médio | Alto | Pós-MVP |
| Detecção e mesclagem de duplicados | Médio | Médio/alto | Pós-MVP |
| Retenção automática | Médio | Médio e depende de política | Pós-MVP |
| Filtros avançados | Médio | Médio | P1/pós-MVP |
| Indicadores | Médio | Baixo/médio | P1 |

## 5. Sequência sugerida até sexta-feira

### Etapa 1 — Base segura

Preparar a base mínima de autenticação, igreja e isolamento. Não aguardar telas administrativas completas para validar o fluxo público.

### Etapa 2 — Fluxo público completo

Cadastrar igreja, abrir endereço público, enviar o formulário e confirmar que o cartão aparece somente no painel correto.

### Etapa 3 — Operação do atendimento

Adicionar Kanban, responsável, observações, histórico, WhatsApp e conclusão.

### Etapa 4 — Estabilização

Corrigir responsividade, testar permissões, validar QR Code real, configurar produção, backup e aviso de privacidade.

Não reservar o último período para funcionalidades novas.

### Etapa 5 — Expansão controlada

Somente após o teste de fumaça do Core passar, completar telas administrativas, pesquisa, atribuição, auditoria e itens P1, nesta ordem de prioridade.

## 6. Ordem de corte se o prazo estiver em risco

Remover ou adiar nesta sequência:

1. indicadores;
2. filtros avançados;
3. próxima data de contato;
4. refinamentos visuais não essenciais;
5. pesquisa avançada, mantendo ao menos a listagem;
6. conveniências das telas administrativas, mantendo procedimento controlado de configuração.

Não cortar:

- formulário por QR Code;
- visualização do visitante no painel correto;
- autenticação;
- isolamento entre igrejas;
- WhatsApp;
- observação e mudança de etapa;
- responsividade do fluxo principal;
- validação e proteção mínima dos dados.

## 7. Pós-MVP imediato

Ordem recomendada após estabilização:

1. cadastro unificado de pessoa e tratamento de duplicidades;
2. próxima ação com lembretes;
3. filtros e indicadores operacionais;
4. política de retenção automatizada;
5. histórico mais completo de auditoria;
6. exportação controlada;
7. cadastro de membros;
8. conversão de visitante em membro preservando histórico;
9. múltiplas origens, eventos ou QR Codes, caso a operação demonstre necessidade;
10. integração oficial com WhatsApp somente após avaliação de custo, consentimento e operação.
11. proteção externa contra bots e indisponibilidade, avaliando Turnstile, CDN/WAF e o ambiente de deploy.

## 8. Definição de pronto

Um item só está pronto quando:

- funciona para o perfil autorizado;
- recusa o perfil ou igreja não autorizados;
- funciona nos tamanhos de tela previstos;
- apresenta erros compreensíveis;
- não expõe dados pessoais em mensagens ou logs;
- possui teste proporcional ao risco;
- está demonstrável no ambiente de produção ou homologação final.
