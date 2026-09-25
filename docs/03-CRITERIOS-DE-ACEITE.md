# Critérios de aceite do MVP

O MVP está apto para produção quando o **P0 Core** e os requisitos de segurança indispensáveis forem demonstrados no ambiente de lançamento. Os demais cenários P0 formam o escopo ampliado e devem ser entregues na ordem indicada, sem colocar o fluxo principal em risco.

## Teste de fumaça do P0 Core

Este teste é o portão mínimo do lançamento:

1. abrir o formulário pelo QR Code em um celular;
2. cadastrar um visitante com interesse em conhecer a igreja e/ou receber oração;
3. confirmar que o cartão aparece somente no painel da igreja correta;
4. abrir o WhatsApp do visitante;
5. adicionar uma observação;
6. mudar a etapa;
7. concluir com um resultado;
8. repetir as ações essenciais em celular, tablet e computador.

Falha em qualquer passo bloqueia o lançamento. Recursos adicionais não compensam a falha desse fluxo.

## CA-001 — Cadastro de igreja e QR Code

**Dado** que o Administrador está autenticado  
**Quando** cadastra uma igreja ativa  
**Então** o sistema registra seu endereço, gera um endereço público permanente e disponibiliza seu QR Code.

## CA-002 — Captação pelo celular

**Dado** o QR Code de uma igreja ativa  
**Quando** um visitante o lê com um celular  
**Então** o formulário abre com a identidade da igreja, sem exigir autenticação ou escolha da unidade.

## CA-003 — Formulário mínimo

**Dado** o formulário público  
**Quando** o visitante informa nome, WhatsApp, seleciona ao menos um interesse, reconhece o aviso e envia  
**Então** o sistema confirma o recebimento e cria o atendimento em `NOVO`.

## CA-004 — Validação do formulário

O envio é recusado com mensagem compreensível quando:

- nome estiver vazio;
- WhatsApp estiver ausente ou inválido;
- nenhum interesse estiver selecionado;
- o aviso de privacidade não tiver sido reconhecido;
- a igreja estiver inativa ou o endereço for inválido.

O formulário não possui campo para descrição do pedido de oração.

## CA-005 — Isolamento entre igrejas

**Dado** um visitante cadastrado na Igreja A  
**Quando** um Pastor ou membro da Equipe da Igreja B consulta o painel, pesquisa ou tenta acessar diretamente o registro  
**Então** o visitante não é exibido e o acesso é recusado pelo servidor.

O mesmo teste deve ser executado para observações, histórico e alterações.

## CA-006 — Acesso global do Administrador

O Administrador consegue selecionar qualquer igreja autorizada, visualizar seus visitantes e prestar suporte sem alterar sua vinculação.

## CA-007 — Gestão de usuários

- Administrador consegue cadastrar Pastor ou Equipe para qualquer igreja.
- Pastor consegue cadastrar e desativar Pastor ou Equipe somente em sua igreja.
- Equipe não consegue cadastrar usuários.
- Pastor não consegue criar ou modificar Administrador.
- Usuário desativado não consegue entrar.
- A senha temporária é sugerida pelo sistema e deve ser trocada no primeiro acesso.
- Administrador e Pastor conseguem redefinir a senha dos usuários que podem administrar sem consultar a senha anterior.
- Pastor e Equipe de uma igreja inativa não conseguem entrar.

## CA-008 — Operação do Kanban

Pastor, Equipe e Administrador conseguem:

- visualizar os cartões autorizados;
- arrastar um cartão somente para a etapa imediatamente anterior ou posterior;
- receber uma recusa do servidor ao tentar pular uma ou mais etapas, tanto ao avançar quanto ao retroceder;
- mudar a etapa por toque/clique e teclado sem depender exclusivamente de arrastar;
- informar um resultado antes de concluir por arraste ou pelos controles do detalhe;
- atribuir o cartão a usuário ativo da mesma igreja;
- adicionar observação;
- consultar o histórico;
- pesquisar por nome ou WhatsApp.

## CA-009 — WhatsApp

**Quando** o usuário seleciona a ação de contato  
**Então** o sistema abre uma conversa no WhatsApp com o número normalizado, sem enviar mensagem automaticamente.

## CA-010 — Conclusão

- Não é possível colocar um atendimento em `CONCLUIDO` sem escolher um resultado.
- O resultado e a data ficam visíveis no cartão.
- Uma reabertura preserva no histórico quem reabriu, quando e qual era o resultado anterior.

## CA-011 — Responsividade

Todas as jornadas P0 devem ser executáveis:

- em um celular moderno em orientação vertical;
- em um tablet;
- em computador.

Não pode haver conteúdo essencial inacessível, sobreposição de campos, botões sem área adequada de toque ou ações disponíveis somente por mouse.

## CA-012 — Auditoria

Mudanças de etapa, responsável, conclusão e observações exibem ao menos autor, data e hora.

## CA-013 — Privacidade do envio

- A confirmação pública não informa se aquele WhatsApp já estava cadastrado.
- O aviso de privacidade fica acessível antes do envio.
- O formulário exibe apenas um resumo curto e abre o conteúdo completo em uma janela de diálogo acessível por toque, mouse e teclado.
- A confirmação não vem marcada previamente e informa que o titular é maior de 18 anos ou que o envio está sendo feito pelo responsável legal.
- O sistema registra a versão do aviso reconhecido.
- Logs técnicos não devem registrar o conteúdo de observações internas.

## CA-014 — Teste rápido antes da produção

Antes do lançamento, executar um teste com:

1. uma conta Administrador;
2. duas igrejas distintas;
3. um Pastor e um usuário de Equipe em cada igreja;
4. ao menos dois cadastros públicos por igreja;
5. tentativa explícita de acesso cruzado entre igrejas;
6. uma jornada de oração concluída;
7. uma jornada concluída como membro;
8. uma jornada encerrada sem retorno;
9. abertura do formulário pelo QR Code físico ou por outra tela;
10. restauração ou confirmação do backup do ambiente.

## Critério de corte

Se um item P1 ameaçar o prazo ou a estabilidade de qualquer cenário acima, o item P1 deve ser removido do lançamento.
