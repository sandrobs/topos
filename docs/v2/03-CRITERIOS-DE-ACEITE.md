# V2 — Critérios de aceite

Os cenários abaixo descrevem o comportamento esperado. Campos, permissões e fluxos marcados como pendentes em `04-DECISOES-PENDENTES.md` deverão ser ajustados após validação do responsável pelo produto.

## Núcleo

### CA-V2-01 — Separação dos formulários

O QR Code já usado por visitantes continua abrindo o formulário de visitantes. O endereço de cadastro de membros abre um formulário identificado com a mesma igreja, sem misturar os dois tipos de registro.

Cada igreja possui um link e QR Code próprios para membros, separados do link e QR Code de visitantes. O endereço abre sem login, e a submissão permanece pendente até aprovação.

### CA-V2-02 — Envio responsivo

Uma pessoa consegue abrir e enviar o formulário de membros em celular, tablet e computador. Campos inválidos recebem mensagens claras e o envio válido recebe confirmação. O público não descobre, pela resposta, se o WhatsApp já está cadastrado.

Nome, sobrenome, WhatsApp e reconhecimento do aviso são obrigatórios; os demais dados aprovados são opcionais. O autoenvio desta etapa é destinado a adultos.

### CA-V2-03 — Aprovação explícita

O envio cria solicitação pendente na igreja correta. Ela não aparece como membro ativo até Pastor ou Administrador aprovar. Toda aprovação registra autor e data.

### CA-V2-04 — Isolamento entre igrejas

Um Pastor da Igreja B não encontra, abre ou altera solicitação ou membro da Igreja A, inclusive por endereço direto. O Administrador global pode selecionar e operar ambas.

### CA-V2-05 — Lista e detalhe

Pastor e Administrador conseguem listar, buscar por nome ou WhatsApp, abrir e atualizar membros sob sua responsabilidade. Dados pendentes não são confundidos com membros aprovados.

A lista apresenta páginas de resultados, busca por nome ou WhatsApp, filtro por situação e contadores de pendentes, ativos e inativos utilizáveis em celular, tablet e computador.

### CA-V2-06 — Inclusão de membros atuais

Pastor ou Administrador consegue incluir diretamente um membro ativo pela área autenticada, com origem registrada e sem criar um atendimento fictício de visitante. A Equipe não consegue consultar ou alterar esse cadastro.

### CA-V2-07 — Possível duplicidade

Quando um WhatsApp já aparece em um membro da mesma igreja, o revisor recebe um alerta antes de criar outro. Nenhum registro é mesclado ou sobrescrito sem escolha explícita.

Pastor ou Administrador pode corrigir dados de uma solicitação internamente após contato, sem oferecer um link público de autoedição nesta etapa.

### CA-V2-08 — Privacidade e auditoria

O formulário apresenta aviso específico de membros e registra sua versão. Aprovação, edição e inativação registram autor e data. Dados pessoais não aparecem em logs técnicos.

### CA-V2-09 — Regressão do MVP

Após a entrega da V2, ainda é possível usar o QR Code de visitantes, enviar um cadastro de visitante, acompanhar no Kanban, entrar em contato por WhatsApp e concluir atendimento. As restrições de igreja e perfil continuam válidas.

## Expansão

### CA-V2-10 — Conversão de visitante

Um atendimento concluído como “Tornou-se membro” pode ser vinculado a um membro novo ou existente da mesma igreja após conferência dos dados. O atendimento e seu histórico permanecem acessíveis, sem duplicação silenciosa.

## Portão de publicação

Executar os cenários com pelo menos duas igrejas e contas de Administrador, Pastor e Equipe. Incluir teste de tentativa de acesso cruzado, duplicidade, formulário em celular e regressão do fluxo de visitantes. A publicação da parte pública exige texto de privacidade de membros aprovado e decisão registrada sobre o tratamento de menores.

Verificar também o QR Code de membros e a inclusão manual por Pastor e Administrador.
