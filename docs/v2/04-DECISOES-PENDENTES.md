# V2 — Decisões do responsável pelo produto

As respostas de 25/09/2026 estão registradas abaixo. Pontos ainda marcados como **pendentes** não devem ser presumidos pelo implementador.

## D1 — Quem recebe o formulário?

**Aprovado:** endereço compartilhável específico por igreja, em rota distinta do formulário de visitantes, com QR Code próprio. O QR Code de visitantes permanece exclusivo dos visitantes. O responsável pelo produto prefere usar uma rota no ambiente público já existente, sem novo subdomínio nesta etapa.

**Aprovado:** o link e o QR Code serão distribuídos fisicamente dentro da igreja. Quem receber o endereço poderá abrir o formulário sem login; todo envio exige aprovação pastoral. A distribuição interna não torna o endereço tecnicamente privado, pois ele pode ser encaminhado.

## D2 — Quais campos são necessários?

**Aprovado:** nome e sobrenome em campos separados, WhatsApp com DDD e reconhecimento do aviso como obrigatórios no autoenvio de adultos. Data de nascimento, e-mail, endereço estruturado e informações de batismo são opcionais. O endereço terá logradouro, número, bairro, complemento, CEP, cidade e UF em campos separados para permitir uso futuro em mapas. A data de ingresso na igreja será preenchida somente na área interna, quando conhecida.

O batismo é esperado na prática da igreja, mas sua ausência ou falta de informação não impedirá tecnicamente o cadastro ou a aprovação. Não se deve presumir batismo a partir da aprovação.

## D3 — Como tratar membros menores de idade?

**Aprovado:** o autoenvio será destinado a adultos. O cadastro de menores ficará na área interna, sob responsabilidade de Pastor ou Administrador. Não haverá autoenvio de menor pelo formulário desta etapa.

**Aprovado:** no cadastro interno de menor, registrar nome, WhatsApp e vínculo do responsável legal. O WhatsApp do menor é opcional. O procedimento e o texto de privacidade específicos para menores devem ser aprovados antes de operar cadastros reais.

## D4 — Quais permissões terá a Equipe?

**Aprovado:** somente Pastor e Administrador acessam a gestão de membros, inclusive consulta, aprovação, edição e inativação. A Equipe continua no fluxo de visitantes. Uma eventual ampliação das permissões será avaliada depois.

## D5 — O que significa membro ativo ou inativo?

**Aprovado:** `ATIVO` e `INATIVO` bastam como estados administrativos na V2, com motivo opcional de inativação. A situação de batismo é independente da situação administrativa do membro.

## D6 — Como conferir identidade e resolver duplicidades?

**Aprovado:** revisão humana antes de aprovar; WhatsApp repetido na mesma igreja gera alerta, sem bloqueio automático. Pastor ou Administrador decide após conferir, pois familiares podem compartilhar número e o telefone pode mudar. Dados incorretos de uma solicitação serão corrigidos internamente por Pastor ou Administrador após contato com a pessoa; não haverá link de autoedição nesta etapa.

**Decisão de implementação decorrente do fluxo aprovado:** no núcleo, não haverá estado separado `CORRECAO_NECESSARIA`. A solicitação permanece `PENDENTE` até Pastor ou Administrador corrigir os dados internamente e aprovar ou recusar. A correção registra autor e data.

## D7 — Aviso de privacidade de membros

**Pendente para publicação do formulário:** aprovar texto e versão próprios para cadastro e gestão de membros, incluindo finalidade, perfis com acesso, retenção e canal de solicitação. O texto do visitante não deve ser reutilizado automaticamente, pois descreve acolhimento e contato inicial.

## D8 — Ordem operacional

**Aprovado:** antecipar para o núcleo da V2 um formulário interno de inclusão manual por Pastor ou Administrador, junto com o formulário compartilhável e a fila de aprovação. Os membros atuais são controlados manualmente hoje. Se aparecer uma planilha, a possibilidade de importação será avaliada mais adiante. A vinculação ou conversão de visitantes permanece para a etapa seguinte.

**Aprovado:** a inclusão manual por Pastor ou Administrador cria imediatamente um membro ativo. O fluxo pendente de aprovação aplica-se somente ao autoenvio pelo formulário compartilhável.
