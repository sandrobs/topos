# V2 — Requisitos e proposta de dados

## 1. Conceitos

- **Solicitação de cadastro:** dados enviados pelo formulário de membros, aguardando análise.
- **Membro:** registro reconhecido pela igreja após aprovação ou inclusão interna autorizada.
- **Atendimento:** registro de visitante do MVP, que continua existindo separadamente.
- **Usuário interno:** conta Administrador, Pastor ou Equipe; não é criada automaticamente para um membro.

Cada solicitação e cada membro pertence a uma única igreja. O Administrador pode operar globalmente; o Pastor fica limitado à própria igreja. A Equipe não acessa a gestão de membros nesta etapa.

## 2. Formulário de membros

### Requisitos firmes

- Interface simples, responsiva e coerente com o formulário atual de visitantes.
- Igreja identificada pelo endereço; não pedir seleção de unidade ao membro.
- Endereço separado do formulário de visitantes.
- Link compartilhável e QR Code próprios por igreja, sem login. A igreja os distribui internamente, mas qualquer pessoa que receba o endereço pode abrir o formulário; todo envio depende de aprovação.
- Confirmação de envio sem informar publicamente se já existe cadastro.
- Nenhuma ativação automática do membro após o envio.
- Aviso de privacidade próprio para a finalidade de cadastro e gestão de membros, com versão registrada no envio.
- Formulário indisponível para igreja inativa.

### Campos aprovados para o autoenvio de adultos

| Campo | Regra | Motivo |
|---|---|---|
| Nome e sobrenome, em campos separados | Obrigatórios | Identificação e consulta |
| WhatsApp com DDD | Obrigatório | Contato e conferência |
| Reconhecimento do aviso de privacidade de membros | Obrigatório | Registrar ciência do uso dos dados |
| Data de nascimento | Opcional | Acompanhamento e identificação |
| E-mail | Opcional | Canal adicional, sem substituir WhatsApp |
| Logradouro, número, bairro, complemento, CEP, cidade e UF, em campos separados | Opcionais | Contato pastoral e futuro uso em mapas, sem geocodificação nesta etapa |
| Situação de batismo e data, quando conhecida | Opcionais | Informação eclesiástica a confirmar com a igreja |
| Data de ingresso na igreja | Interno, opcional | Deve ser confirmada pelo Pastor, não presumida pelo envio |
| Observação pastoral | Apenas área interna | Nunca solicitar texto livre no formulário público |

O batismo é esperado na prática da igreja, mas a ausência de batismo ou de sua data não bloqueia tecnicamente a análise ou a aprovação. O autoenvio desta etapa se destina a adultos. Cadastros de menores serão feitos internamente por Pastor ou Administrador e registrarão nome, WhatsApp e vínculo do responsável legal; o WhatsApp do menor é opcional. O procedimento e o texto específicos precisam de aprovação antes de operar cadastros reais de menores.

Não solicitar CPF, RG, estado civil, profissão, dados de saúde, foto ou conteúdo de oração sem uma necessidade concreta aprovada.

## 3. Ciclo da solicitação

Estados do núcleo: `PENDENTE`, `APROVADA` e `RECUSADA`. Uma solicitação que precisa de correção permanece pendente enquanto Pastor ou Administrador confere os dados com a pessoa e registra a alteração.

- O envio público cria `PENDENTE`.
- Pastor ou Administrador pode aprovar ou recusar; o motivo de recusa deve ficar no histórico interno, sem divulgação automática ao solicitante.
- Pastor ou Administrador pode entrar em contato e corrigir os dados da solicitação internamente antes de decidir; não haverá autoedição pelo membro nesta etapa.
- A aprovação deve verificar possíveis duplicidades na mesma igreja e decidir entre vincular a membro existente ou criar um novo registro.
- Toda decisão registra autor e data.
- O painel deve distinguir claramente solicitação pendente de membro aprovado.

## 4. Cadastro de membros

Situações administrativas: `ATIVO` e `INATIVO`, com motivo opcional de inativação. A situação de batismo é independente.

- Identificador estável, igreja, dados aprovados, origem do cadastro, data de criação e data da última alteração.
- Origem: formulário, inclusão interna ou conversão de visitante.
- Edição restrita a perfis autorizados, com autor e data.
- Desativar não apaga dados nem histórico.
- Pesquisa por nome e WhatsApp limitada à igreja selecionada.
- Na mesma igreja, possíveis duplicidades devem ser sinalizadas; o WhatsApp não deve ser tratado como prova absoluta de identidade, pois pode ser compartilhado ou mudar.
- O formulário interno de inclusão manual por Pastor ou Administrador integra o núcleo da V2 e cria diretamente um membro ativo.
- A lista deve usar paginação, busca por nome ou WhatsApp e filtro por situação no servidor para manter a consulta rápida com centenas de cadastros. O painel mostra contadores simples de pendentes, ativos e inativos; não exige gráficos nesta etapa.

## 5. Conversão de visitante — V2 expansão

- Oferecer a ação a partir de atendimento com resultado `TORNOU_SE_MEMBRO`.
- Exibir lado a lado os dados já coletados e os campos ainda necessários.
- Reaproveitar somente valores confirmados pela pessoa ou pela equipe; permitir correção antes de salvar.
- Verificar se já existe membro na mesma igreja antes de criar outro.
- Criar vínculo explícito entre membro e atendimento, preservando observações e histórico no atendimento original.
- Não alterar retroativamente o resultado, as datas ou o aviso de privacidade do visitante.
- Se a finalidade do tratamento mudar para gestão contínua de membros, apresentar e registrar o aviso específico de membros no fluxo apropriado.

## 6. Permissões propostas

| Ação | Administrador | Pastor | Equipe |
|---|---:|---:|---:|
| Ver membros e solicitações | Todas as igrejas | Sua igreja | Não |
| Aprovar/recusar solicitação | Todas as igrejas | Sua igreja | Não |
| Incluir membro manualmente | Todas as igrejas | Sua igreja | Não |
| Editar dados do membro | Todas as igrejas | Sua igreja | Não |
| Inativar/reativar | Todas as igrejas | Sua igreja | Não |
| Converter visitante em membro | Todas as igrejas | Sua igreja | Não |

O servidor deve aplicar o isolamento multi-igreja em listagens, buscas, detalhe e alterações. Nenhum endereço direto ou troca de igreja na interface pode contornar essa regra.

## 7. Segurança e experiência

- Reutilizar os padrões de autenticação e proteção de formulários públicos já adotados no MVP, ajustando limites à nova rota.
- Não expor dados de membros ou resultado de deduplicação ao público.
- Não registrar valores pessoais completos em logs técnicos.
- Garantir uso por toque e teclado no celular, tablet e computador.
- Registrar quem aprovou, incluiu, editou, inativou ou vinculou um membro e quando.
- Preparar backup e migração reversível ou plano de recuperação antes de alterar a base em produção.
