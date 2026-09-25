# Requisitos funcionais e regras de negócio

## 1. Formulário público

### RF-001 — Identificação da igreja

O endereço público deve identificar a igreja automaticamente. O visitante não escolherá a igreja em uma lista.

### RF-002 — Campos do formulário

Campos obrigatórios:

- nome;
- WhatsApp;
- pelo menos um interesse;
- reconhecimento do aviso de privacidade.

O reconhecimento deve ser apresentado como uma confirmação explícita, não marcada previamente, de que a pessoa é maior de 18 anos ou responsável legal e autoriza o uso dos dados informados para contato por WhatsApp e acolhimento. Um resumo curto permanecerá visível no formulário e o aviso completo será aberto em uma janela de diálogo antes do envio.

Interesses disponíveis:

- **Quero conhecer melhor a igreja.**
- **Quero conversar e receber uma oração.**

Os dois interesses podem ser selecionados simultaneamente.

Não haverá campo para escrever o pedido de oração, idade, data de nascimento, endereço, e-mail ou documento pessoal.

### RF-003 — WhatsApp

- O número deve ser validado e armazenado em formato normalizado.
- O contato inicial será exclusivamente pelo WhatsApp.
- O sistema não enviará mensagens automaticamente.
- O painel deve oferecer um botão que abra a conversa com o número informado.

### RF-004 — Envio

Após um envio válido:

- criar um novo atendimento na igreja identificada pelo endereço;
- iniciar o atendimento em **Novo visitante**;
- registrar data e hora;
- registrar a versão do aviso de privacidade reconhecido;
- exibir confirmação de recebimento.

Para preservar privacidade, a resposta pública não deve revelar se o WhatsApp já existe no sistema.

### RF-005 — Igreja indisponível

Se a igreja estiver inativa ou o endereço for inválido, não aceitar o cadastro. Exibir uma mensagem neutra, sem detalhes técnicos.

## 2. Etapas do atendimento

Etapas do Kanban:

1. `NOVO` — cadastro recebido e ainda não tratado;
2. `AGUARDANDO_CONTATO` — atendimento assumido ou atribuído;
3. `EM_ATENDIMENTO` — contato inicial em andamento;
4. `EM_ACOMPANHAMENTO` — relacionamento requer novas conversas;
5. `CONCLUIDO` — atendimento encerrado com um resultado.

Regras:

- todo novo envio começa em `NOVO`;
- usuários autorizados podem mover cartões somente para a etapa imediatamente anterior ou posterior;
- não é permitido pular etapas, tanto ao avançar quanto ao retroceder;
- a restrição de etapas adjacentes deve ser validada no servidor e valer para qualquer forma de movimentação;
- o painel permite arrastar e soltar o cartão em uma coluna adjacente, sem substituir os controles acessíveis por toque/clique e teclado;
- soltar o cartão na própria etapa não gera atualização nem histórico;
- toda mudança registra etapa anterior, nova etapa, autor e data;
- entrar em `CONCLUIDO` exige um resultado;
- ao arrastar para `CONCLUIDO`, o sistema solicita o resultado antes de efetivar a mudança;
- reabrir um atendimento concluído somente é possível para `EM_ACOMPANHAMENTO`, remove o estado de conclusão e preserva o resultado anterior no histórico.

## 3. Resultados de conclusão

- `TORNOU_SE_MEMBRO`
- `ORACAO_ATENDIDA`
- `NAO_DESEJA_PROSSEGUIR`
- `SEM_RETORNO`

Regras complementares:

- `ORACAO_ATENDIDA` é apropriado para quem buscou somente conversa e oração.
- Se a pessoa selecionou os dois interesses e ainda está conhecendo a igreja, o cartão permanece em acompanhamento mesmo após a oração.
- `TORNOU_SE_MEMBRO` apenas registra o resultado no MVP; a criação do cadastro de membro pertence à segunda fase.

## 4. Cartão do visitante

Dados visíveis:

- nome;
- WhatsApp;
- igreja;
- interesses;
- etapa atual;
- responsável, se houver;
- data de entrada;
- última atividade;
- observações internas;
- histórico de etapas;
- resultado e data de conclusão, quando aplicável;
- próxima data de contato, somente se o item P1 for implementado.

O sistema não armazenará o conteúdo do pedido de oração no formulário público. As observações internas destinam-se ao acompanhamento e devem ser usadas com cautela, pois podem conter informações sensíveis fornecidas posteriormente.

### Dados complementares internos

Depois que o visitante estiver no funil, a equipe poderá registrar, de forma opcional e somente na área autenticada:

- logradouro, número da residência, bairro e complemento;
- data de nascimento completa;
- situação congregacional: não informada, não congrega ou congrega;
- nome da igreja onde congrega, quando informado.

Esses dados não serão solicitados no formulário público nem exibidos no Kanban. Eles serão visíveis apenas ao abrir o atendimento. Toda atualização deve registrar autor e data no histórico sem copiar os valores pessoais para logs técnicos.

## 5. Atribuição de responsável

- O responsável é opcional ao receber o cadastro e obrigatório apenas se a operação da igreja assim decidir.
- Pastor, Equipe e Administrador podem assumir o atendimento ou atribuí-lo a outro usuário ativo da mesma igreja.
- Um usuário de outra igreja nunca pode ser selecionado.
- A atribuição não restringe a visibilidade: toda a Equipe da igreja continua podendo acompanhar o cartão.

## 6. Observações

- Pastor, Equipe e Administrador podem adicionar observações.
- Cada observação registra autor, data e hora.
- No MVP, observações são registros internos anexados ao histórico; não há comunicação com o visitante.
- Recomenda-se que observações não sejam editadas silenciosamente. Se a implementação permitir edição ou remoção, deve manter auditoria da ação.

## 7. Duplicidades

Para proteger o prazo, não haverá detecção ou mesclagem automática no MVP.

- Reenvios são permitidos e podem criar outro atendimento.
- O número deve ser normalizado para facilitar pesquisa manual.
- A equipe pode localizar cadastros semelhantes pela pesquisa.
- O sistema não deve sobrescrever automaticamente um atendimento anterior.
- Identidade única de pessoa, mesclagem e histórico unificado ficam para a segunda fase.

## 8. Igrejas e QR Code

Dados mínimos de uma igreja:

- nome;
- logradouro, número e bairro;
- complemento opcional;
- CEP brasileiro;
- cidade;
- estado;
- situação ativa ou inativa;
- identificador público não previsível;
- endereço público associado;
- QR Code associado ao endereço.

Regras:

- cada igreja possui somente um QR Code;
- o QR Code deve continuar válido após alterações no nome ou nos dados internos;
- somente igreja ativa pode receber cadastros;
- desativar uma igreja não apaga seus dados;
- o Administrador pode visualizar ou baixar o QR Code.
- o endereço deve aparecer de forma discreta no rodapé do formulário público e do convite A4;
- o estado deve ser escolhido entre as UFs brasileiras na tela administrativa.

## 9. Usuários e permissões

| Capacidade | Administrador | Pastor | Equipe |
|---|---:|---:|---:|
| Visualizar todas as igrejas | Sim | Não | Não |
| Cadastrar e editar igrejas | Sim | Não | Não |
| Visualizar visitantes | Todas | Própria igreja | Própria igreja |
| Movimentar atendimentos | Todas | Própria igreja | Própria igreja |
| Adicionar observações | Todas | Própria igreja | Própria igreja |
| Atribuir responsável | Todas | Própria igreja | Própria igreja |
| Concluir e reabrir atendimento | Todas | Própria igreja | Própria igreja |
| Cadastrar usuários | Todas | Própria igreja | Não |
| Alterar perfil de usuário | Todas | Própria igreja, exceto Administrador | Não |
| Ativar ou desativar usuários | Todas | Própria igreja | Não |

Regras adicionais:

- Pastor e Equipe pertencem a exatamente uma igreja no MVP.
- Administrador é global e não precisa pertencer a uma igreja.
- Pastor não pode criar, promover, editar ou desativar um Administrador.
- Pastor não pode transferir usuários para outra igreja.
- Usuário inativo não pode autenticar nem receber atribuições.
- usuários criados pela tela de gestão recebem uma senha temporária armazenada somente como hash e devem trocá-la no primeiro acesso;
- a conta Administradora criada no bootstrap inicial é uma exceção aprovada: sua senha forte inicial não exige troca obrigatória no primeiro acesso;
- Administrador e Pastor podem gerar uma nova senha temporária para os usuários que podem administrar;
- nenhuma senha pode ser consultada posteriormente ou registrada em logs e auditoria;
- usuário não pode desativar a própria conta e o último Administrador ativo não pode ser desativado;
- Pastor e Equipe de uma igreja inativa não podem autenticar;
- usuários não são transferidos entre igrejas no MVP; a conta anterior deve ser desativada e outra criada;
- A autorização deve ser verificada no servidor, não apenas escondida na interface.

## 10. Responsividade e acessibilidade

- O formulário público é mobile-first.
- O painel deve funcionar em celular, tablet e computador.
- Em telas pequenas, o painel pode trocar colunas por lista com filtro de etapa.
- Todas as ações devem funcionar por toque e teclado; arrastar cartões não pode ser a única forma de mudar uma etapa.
- O recurso de arrastar e soltar usará `@dnd-kit/react`, com uma alça explícita no cartão para não conflitar com a abertura do detalhe.
- Campos devem ter rótulos visíveis, mensagens de erro claras e áreas de toque adequadas.
- Contraste e foco visual devem ser preservados.

## 11. Requisitos não funcionais

### RNF-001 — Multi-igreja

Toda consulta ou alteração de Pastor e Equipe deve ser limitada à igreja do usuário no servidor.

Para o Administrador, a igreja em uso deve permanecer claramente visível no painel. O sistema deve
lembrar a última igreja selecionada por usuário no mesmo navegador e restaurá-la em novos acessos,
desde que ela continue disponível. Um `igrejaId` válido informado explicitamente no endereço tem
precedência, e toda consulta ou alteração continua sendo validada no servidor para a igreja informada.

### RNF-002 — Segurança

- autenticação segura;
- senhas armazenadas por mecanismo de hash apropriado, se a solução utilizar senhas;
- sessões protegidas;
- HTTPS em produção;
- validação e higienização de entradas;
- limitação básica de frequência no formulário público;
- ausência de dados pessoais sensíveis em logs técnicos sempre que possível.

### RNF-003 — Desempenho percebido

O formulário e o painel devem apresentar resposta clara durante carregamentos e envios, evitando submissões repetidas por falta de feedback.

### RNF-004 — Auditoria mínima

Registrar autor e data de:

- mudança de etapa;
- atribuição de responsável;
- conclusão e reabertura;
- observações internas;
- criação, ativação e desativação de usuários e igrejas.

### RNF-005 — Recuperação

Deve existir uma rotina de backup compatível com o ambiente escolhido antes da entrada em produção.
