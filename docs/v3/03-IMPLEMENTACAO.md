# V3 — Implementação e validação

Registro do que já foi construído na V3. Escopo e justificativa de cada item em
[01-ESCOPO-E-LOTES.md](01-ESCOPO-E-LOTES.md).

Todo o trabalho está na branch `feature/ui-lote1`. Nada foi publicado em
produção e nenhuma alteração toca campos, validações de domínio, permissões,
regras de negócio, contratos de API, conteúdo dos PDFs ou destino dos QR Codes.

---

## Lote 1 de UI/UX — 14/11/2026

Primeiro lote da revisão de layout e experiência, feito na branch
`feature/ui-lote1`. São correções de apresentação e acessibilidade: nenhum campo,
validação, permissão, regra de negócio ou conteúdo do QR Code e dos PDFs foi
alterado, e o fluxo de visitantes foi preservado.

### Peso dos recursos públicos

O original de 1254×1254 px da logo saiu de `src/CrmIctm.Web/public/` e passou a
viver em `src/CrmIctm.Api/Recursos/Convite/logo-igreja.png`, onde já ficam as
fontes dos convites; o glob `Recursos/Convite/*` do `.csproj` continua a
incorporá-lo com o mesmo nome lógico `Convite.logo-igreja.png`, então os PDFs
seguem usando a imagem em resolução plena. A `EmbeddedResource` explícita e a
cópia correspondente no `Dockerfile` foram removidas por redundância.

Em `public/` ficou uma versão de 192×192 px gerada do mesmo original, que serve
o maior render de tela (20 mm do cartaz) com folga de densidade: **41,5 kB no
lugar de 1.385 kB**. O caminho público continua sendo `/logo-igreja.png`, sem
`srcset` e sem rota nova, para não mexer na lista do Caddy. Em consequência, a
igualdade SHA-256 entre a logo servida e o arquivo incorporado, registrada na
seção anterior, deixa de valer por desenho: são agora o original e um derivado.

Também foi removida `public/imagem-louvor-convite.png` (1,8 MB), que não era
referenciada por nenhum código, CSS, template ou recurso incorporado.

Resultado medido no formulário público: a página passou de 1.826 kB para
**492 kB** em quatro recursos. O `<img>` da logo ganhou `width`/`height`
explícitos.

### Acessibilidade

O indicador de foco usava `rgba(46, 95, 202, 0.3)`, cerca de 1,5:1 sobre branco,
abaixo do mínimo de 3:1 do critério WCAG 2.2 1.4.11. Foi criado o token `--foco`
(opaco, `var(--azul-600)`) e aplicado às sete regras que tratavam de foco. Duas
delas (`.login input:focus`, `.barra-pesquisa input:focus`) e a do formulário de
membros anulavam o anel com `outline: 0`; a regra do membro ainda usava `:focus`
em vez de `:focus-visible`, disparando também no clique de mouse. Medições na
aplicação: **5,82:1** sobre branco, **5,38:1** sobre `--fundo` e **3,01:1** sobre
a faixa `--azul-950`.

O endereço no rodapé público usava `#8490a2` a 0,7 rem — 3,23:1, abaixo dos 4,5:1
do critério 1.4.3. Passou para `var(--texto-suave)` a 0,76 rem: **4,59:1**.

### Clareza do que é apresentado

O marcador de `.indicador-selecao` era circular numa pergunta de múltipla
escolha, enquanto o consentimento, logo abaixo, usava quadrado arredondado.
Os dois agora usam `border-radius: 7px`; os marcadores de escolha única do
formulário de membros seguem circulares.

A caixa de consentimento ficava centralizada na vertical ao lado de um parágrafo
de várias linhas. `align-items` passou de `center` para `start`, como já fazia
`.aviso-membro label`.

Na gestão de membros, datas ISO e enums apareciam crus: `2024-01-15`, `ATIVO`,
`APROVACAO SOLICITACAO`. Foram acrescentados a `formatadores.ts` as funções
`formatarDataIso`, `formatarDataHora`, `rotuloSituacaoMembro` e
`rotuloAcaoHistorico`. `formatarDataIso` não usa `new Date`: a string sem hora
seria lida como UTC e exibiria o dia anterior em fusos negativos. O
`formatarData` duplicado em `Interno.tsx` passou a reusar `formatarDataHora`.

### Validação

Build completo de frontend e backend em Docker/Linux e **59 testes aprovados**.
`GeradorConvitesPdfTests` exercita a leitura do recurso incorporado, então a
mudança de local da logo do PDF está coberta por teste.

Verificação na aplicação em execução, com a imagem `crm-ictm:ui-lote1-20261114`
subida em `http://localhost:8082` contra o mesmo banco de testes, sem tocar na
instância 8081: formulários públicos de visitantes e de membros a 375 px e
1024 px, sem rolagem horizontal; anel de foco conferido por Tab real no
formulário, no campo de pesquisa do painel e no cartão do kanban; datas e
histórico conferidos contra os valores brutos do banco, sem deslocamento de dia.

Pendente de verificação física: impressão dos PDFs e teste em Safari/iPad/iPhone
reais. A emulação Chromium não prova o comportamento desses dispositivos.

---

## Lote 2 de UI/UX — 14/11/2026

Retorno ao usuário, acessibilidade dos modais e peso do JavaScript. Itens 2.1 a
2.6 de [01-ESCOPO-E-LOTES.md](01-ESCOPO-E-LOTES.md). O item 2.7 (tipografia)
não foi feito: depende da decisão D1.

### Sobreposições modais

Criado [`Dialogo.tsx`](../../src/CrmIctm.Web/src/Dialogo.tsx) sobre `<dialog>`
nativo, a partir do padrão que `DialogoAvisoPrivacidade` já usava. As cinco
sobreposições feitas à mão com `<div className="sobreposicao">` foram migradas:
detalhe do atendimento, confirmação de conclusão por arraste, nova/editar
igreja, novo/editar usuário e redefinir senha.

O CSS não mudou de aparência: o `<dialog>` recebe a mesma classe
`.sobreposicao` e um bloco de reset zera largura, altura, margem, borda e
preenchimento próprios do elemento. O escurecimento fica no próprio elemento,
não no `::backdrop`, para preservar o `backdrop-filter` existente.

Dois detalhes que só apareceram na verificação:

- **Foco preso em botão desabilitado.** O estado pendente do cartão começou
  usando `disabled`. Desabilitar o botão que tem o foco faz o navegador
  devolvê-lo ao `<body>`, e aí o `<dialog>` não tem para onde restituir o foco
  ao fechar. Passou a usar `aria-disabled` + `aria-busy`, com o clique repetido
  barrado em `abrirDetalhe`.
- **Restituição de foco.** Quem fecha o diálogo é o React: `aoFechar` muda o
  estado e o elemento é desmontado antes de o navegador restituir o foco. O
  `Dialogo` guarda o gatilho ao montar e o refoca na limpeza do efeito, se ele
  ainda estiver na página.

Medido no modal de detalhe, com teclado real:

| verificação | antes | depois |
|---|---|---|
| Escape fecha | não | **sim** |
| foco entra ao abrir | não (`<body>`) | **sim** |
| foco preso dentro | não (3 Tabs escapavam) | **sim** (6 Tabs) |
| foco devolvido ao fechar | não | **sim, ao cartão de origem** |
| focáveis alcançáveis atrás | 11 | **0** (inertes pelo navegador) |
| `role` / `aria-modal` no formulário administrativo | ausentes | **nativos do `<dialog>`** |

### Estados de carregamento e retorno ao usuário

`carregando` passou a alimentar o kanban. Durante a espera, cada coluna mostra
dois cartões-esqueleto no lugar de "Nenhum atendimento", a seção recebe
`aria-busy`, o botão de busca é desabilitado e vira "Pesquisando…", e os
contadores mostram "–". Acrescentada uma região `role="status"`
`aria-live="polite"` — a primeira da aplicação — que anuncia "Carregando
atendimentos…" e, ao fim, "N resultados para …".

Medido ao trocar para uma igreja que tem 1 atendimento, com atraso artificial:

| momento | colunas "Nenhum atendimento" | esqueletos | `aria-busy` | status |
|---|---|---|---|---|
| 89 ms | **0** (eram 5) | 10 | true | "Carregando atendimentos…" |
| 1405 ms | **0** | 10 | true | "Carregando atendimentos…" |
| 2812 ms | 5 (correto: igreja vazia) | 0 | false | — |

`abrirDetalhe` ganhou estado pendente: o cartão clicado fica `aria-busy` com
cursor de espera, e cliques repetidos são ignorados.

### Contadores durante a busca

Enquanto houver termo ativo, "Total" passa a "Resultados" (decisão D3, opção A,
reversível). Verificado: numa igreja com 1 atendimento, pesquisar um termo sem
correspondência agora exibe "0 Resultados" e anuncia "0 resultados para
“zzznaoexiste”." em vez de afirmar "Total 0".

### Seção ativa anunciada

Os quatro botões de `.navegacao-interna` receberam `aria-current="page"`.

### Carregamento sob demanda das rotas internas

`Interno`, `QrCode` e `QrCodeMembros` passaram a `React.lazy()` com `<Suspense>`
em `App.tsx`. `Administracao` e `GestaoMembros` acompanham, por serem
importadas por `Interno`.

| arquivo | antes | depois |
|---|---|---|
| bundle público | 399,0 kB (gzip 120,4) | **247,9 kB** (gzip 76,8) |
| chunk `Interno` | — | 149,4 kB, só nas rotas internas |
| chunk `QrCode` | — | 4,7 kB |
| **formulário público, total baixado** | 492 kB (após o Lote 1) | **341 kB** |

Partindo de 1.826 kB antes do Lote 1, o formulário que o visitante abre pelo QR
Code caiu **81%**.

**Efeito colateral corrigido na mesma sessão.** Como a navegação interna é feita
com recarga completa de página (`window.location.replace`, sem roteador), cada
troca de tela passou a piscar o fallback do `<Suspense>` por cerca de 16 ms —
o tempo entre o navegador interpretar o bundle principal e resolver o chunk, que
vem do cache (`Cache-Control: max-age=31536000, immutable`). Um layout inteiro
aparecendo e sumindo nesse intervalo é pior do que esperar. A mensagem de espera
ganhou a classe `aguardando-rota`, que a mantém `visibility: hidden` nos
primeiros 400 ms: no caso comum ela nunca é vista, e numa conexão ruim ela
aparece quando o usuário de fato precisa saber que algo está vindo.

### Validação

Build completo de frontend e backend em Docker/Linux e **59 testes aprovados**.

Verificação na aplicação em execução (`crm-ictm:ui-lote2-20261114` em
`http://localhost:8082`, mesmo banco de testes, instância 8081 intacta):
formulário público sem o chunk do painel; esqueleto, `aria-busy` e região de
status durante a troca de igreja e a busca; rótulo dos contadores; ciclo de
teclado completo nos modais de detalhe e de nova igreja, incluindo devolução do
foco ao elemento de origem. Nenhum registro foi criado ou desativado no banco.

Pendente de verificação física: impressão dos PDFs e teste em Safari/iPad/iPhone
reais — o `<dialog>` nativo e o `:has()` usados aqui têm suporte amplo, mas o
comportamento de foco no iOS merece conferência em aparelho real.

---

## Lote 2, item 2.7 — tipografia hospedada — 14/11/2026

Fecha o Lote 2. Decisão D1, opção A: a Inter passou a ser entregue pelo próprio
projeto, em vez de depender de o usuário tê-la instalada.

### O que foi feito

Dois subconjuntos da Inter variável (versão 20 do Google Fonts, SIL OFL 1.1)
ficam em `src/CrmIctm.Web/src/fontes/`, com a licença ao lado em
`LICENSE-Inter.txt`:

| arquivo | tamanho | quando é baixado |
|---|---|---|
| `inter-latin.woff2` | 48,3 kB | sempre |
| `inter-latin-ext.woff2` | 85,1 kB | só se a página trouxer um caractere da faixa |

Os arquivos ficam em `src/fontes/` e **não** em `public/`. O `deploy/Caddyfile`
libera, no domínio público dos formulários, apenas `/assets/*`,
`/logo-igreja.png` e as rotas da aplicação: servidos de `public/`, os arquivos
cairiam em `/inter-latin.woff2` e dariam 404 para o visitante. Importados pelo
CSS, o Vite os publica com hash em `/assets/`, que é permitido.

Um `<link rel="preload">` em `index.html` aponta para o subconjunto latino; o
Vite reescreve o caminho para o arquivo com hash no build. Sem ele a fonte só
seria descoberta ao interpretar o CSS, e o texto apareceria na fonte do sistema
antes de trocar — uma troca que desloca o layout, porque as métricas da Inter e
da Segoe UI são diferentes.

Declarados com `font-weight: 100 900` (eixo variável) e `font-display: swap`.

### Medições

Largura renderizada do mesmo texto, por peso declarado no CSS:

| peso | antes (Inter local, estática) | depois (Inter variável) |
|---|---|---|
| 400 | 533,20 | 319,08 |
| 450 | 533,20 | 320,56 |
| 500 | 533,20 | 322,10 |
| 700 | 556,99 | 328,01 |
| 720 | 556,99 | 328,74 |
| 750 | 556,99 | 329,90 |
| 800 | 556,99 | 331,68 |
| 850 | 556,99 | 333,67 |
| 900 | 556,99 | 335,66 |
| **valores distintos** | **2** | **12** |

(As duas colunas não são comparáveis em valor absoluto: a medição original usou
outro texto de amostra. O que importa é a contagem de valores distintos.)

Outras medições, no formulário público em execução:

- `document.fonts` traz a face variável com estado `loaded`; a face `latin-ext`
  fica `unloaded`, confirmando que o `unicode-range` evita o download.
- A requisição da fonte começa em 30 ms e termina em 46 ms — antes de o CSS
  terminar, em 47 ms. O preload funciona e não há troca de fonte visível.
- Peso total da página: **391 kB**, contra 341 kB antes da fonte. Partindo de
  1.826 kB antes do Lote 1, continua **79% abaixo**.

### O que deliberadamente não foi feito

Os onze valores de `font-weight` do CSS (450, 700, 720, 750, 760, 780, 800,
820, 850, 900) agora renderizam todos diferentes, mas diferenças de 20 unidades
são imperceptíveis e vieram de copiar-e-colar, não de projeto. Normalizá-los
para uma escala de três ou quatro pesos é outra mudança visível e pertence ao
Lote 3, junto com os demais tokens.

### Validação

Build completo em Docker/Linux e **59 testes aprovados**. Conferido na
aplicação em execução (`crm-ictm:ui-fonte-20261114` em `http://localhost:8082`).

Pendente de verificação física: renderização em Safari/iPad/iPhone reais. O
formato woff2 e as fontes variáveis têm suporte amplo, mas a aparência em tela
Retina merece conferência em aparelho real.

---

## Auditoria móvel dos formulários públicos — 14/11/2026

Verificação, sem alteração de código, do estado dos dois formulários públicos
em `http://localhost:8082` após os Lotes 1 e 2. Serve de linha de base: o que
está marcado como íntegro aqui não precisa ser reexaminado no Lote 3.

### Íntegro nas duas larguras (375 px e 320 px)

| | visitantes | membros |
|---|---|---|
| overflow horizontal | não | não |
| elementos vazando à direita | 0 | 0 |
| campos com rótulo associado | 5 de 5 | 20 de 20 |
| campos de texto abaixo de 16 px (zoom automático do Safari) | 0 | 0 |
| campos com `autocomplete` | 2 de 2 aplicáveis | 9 |
| grade de opções a 375 px | — | colapsa para 1 coluna |
| menor espaço entre alvos adjacentes | 12 px | — |
| botão de envio | 254×54 | largura total |
| altura total a 375 px / 320 px | 1,8 / 2,8 telas | 3,4 / 5,0 telas |

Os Lotes 1 e 2 melhoraram o uso em celular sem terem sido dirigidos a ele: a
página caiu de 1.826 kB para 391 kB, o anel de foco passou a ser visível para
teclado externo, o marcador de múltipla escolha deixou de parecer escolha
única e o consentimento foi alinhado ao topo — este último era pior justamente
no celular, onde o parágrafo ao lado fica mais alto.

### Problemas que ficaram para o Lote 3

Por decisão do responsável em 14/11/2026, os ajustes não foram feitos agora.
Estão descritos em [01-ESCOPO-E-LOTES.md](01-ESCOPO-E-LOTES.md), itens **3.3**,
**3.3b** e **3.3c**: validação divergente entre os dois formulários, erro que
sai da tela no celular sem mover o foco, e o alvo de 40 px do botão do aviso de
privacidade.

### Pendente de aparelho real

O formulário de membros depende das bolhas de validação nativas do navegador.
O comportamento do Safari no iPhone precisa ser conferido num aparelho antes de
se concluir o Lote 3 — se ele focar o campo sem exibir mensagem, hoje o
cadastro de membros está sem retorno de erro no iOS.

---

## Lote 3, primeira parte — retorno de erro e alvo de toque — 14/11/2026

Itens **3.3**, **3.3b** e **3.3c**, feitos primeiro por serem pequenos e
independentes do refactor de campos. O restante do Lote 3 segue pendente.

### O que mudou

- `.abrir-aviso-privacidade` passou de `min-height: 40px` para **44px**. Era o
  último alvo abaixo do mínimo nos dois formulários públicos.
- Novo [`foco.ts`](../../src/CrmIctm.Web/src/foco.ts) com `focarPrimeiroErro`:
  foca o primeiro campo `aria-invalid` e o rola para o centro; se o erro não
  pertencer a um campo focável, rola até a mensagem. `CadastroMembro` tinha uma
  versão própria disso e passou a usar a compartilhada; `App.tsx` ganhou o
  comportamento, que não tinha.
- O formulário público de membros recebeu `noValidate`. Os atributos `required`
  **continuam**, valendo como semântica para leitor de tela; sai apenas a bolha
  do navegador. A validação passa a ser a do servidor, que devolve erro por
  campo com as mesmas chaves do cliente (`ValidacaoDadosMembro`).
- Os campos de `CamposMembro` passaram a ligar `aria-describedby` à mensagem.

### Medições, no fluxo real a 375 px

Rolar até o fim do formulário e tocar em enviar:

| | antes | depois |
|---|---|---|
| **visitantes** — foco após o envio | `<body>` | **`#nome`**, visível na tela |
| rolagem | 625 → 799 (afastando) | **625 → 129** (aproximando) |
| erros de nome e WhatsApp | −231 px e −112 px, fora da tela | **439 px e 558 px, visíveis** |
| **membros** — foco após o envio | campo focado pelo navegador | **`#membro-nome`**, visível |
| rolagem | — | **2007 → 225** |
| mensagem de erro | bolha do navegador | **a da aplicação** |
| campos com `aria-describedby` | 0 de 20 | **3 de 3 com erro** |
| alvo do aviso de privacidade | 269×40 | **269×44** |

### Efeito colateral bem-vindo

O risco registrado no item 3.3 — o Safari do iPhone poder focar o campo sem
exibir a bolha nativa, deixando o cadastro de membros sem retorno de erro no
iOS — **deixa de existir**: o formulário não depende mais de validação nativa.
A mensagem é renderizada pela aplicação, igual em todos os navegadores.

### Validação

Build Docker completo e **59 testes aprovados**. Conferido na aplicação em
execução (`crm-ictm:ui-lote3a-20261114` em `http://localhost:8082`), nos dois
formulários, a 375 px. Nenhum cadastro foi criado: todos os envios falharam na
validação, que é o que se queria observar.

---

## Lote 3, segunda parte — componente único de campo, confirmação e tokens — 14/11/2026

Itens **3.2**, **3.3** (administração), **3.4** e a parte segura do **3.1**.
A normalização de breakpoints e o item **3.5** seguem pendentes; a razão está
no fim desta seção.

### 3.2 · Um componente de campo

Novo [`CampoFormulario.tsx`](../../src/CrmIctm.Web/src/CampoFormulario.tsx),
usado por `App.tsx`, `CadastroMembro.tsx`, `GestaoMembros.tsx` e
`Administracao.tsx`. Aceita um controle próprio por render prop, para `select`
e `textarea`. Ele cuida de id, rótulo com `for`, asterisco de obrigatório,
texto de ajuda, `aria-invalid`, `aria-describedby` e a mensagem de erro.

Foram removidas as quatro implementações anteriores: `CampoTexto`, a closure
`campo()`, o `Campo` da administração e os `<label>`+`<input>` soltos de
`GestaoMembros`.

`.campo-membro` foi **apagado** do CSS. A densidade menor do modal
administrativo sobreviveu como `.campo.denso`, uma variação declarada;
`.campo-administrativo` ficou como apelido da mesma régua, para os poucos
controles ainda escritos à mão.

Deriva eliminada, medida nos dois formulários públicos em execução:

| | antes | depois |
|---|---|---|
| rótulo | 14,4 px / 750 contra 13,76 px / 700 | **14,4 px / 750 nos dois** |
| `padding` do input | 12px 14px contra 10px 12px | **12px 14px nos dois** |
| `border-radius` | 11px contra 10px | **11px nos dois** |
| cor da borda | `#cbd2df` contra `#cdd5e3` | **a mesma nos dois** |
| sobras de `.campo-membro` | — | **0** |

### 3.3 · Validação da administração

Os dois formulários administrativos receberam `noValidate` e passaram a exibir
**erro por campo**, que nunca tiveram: antes, uma falha de validação virava uma
única `.alerta` no topo com a primeira mensagem. Medido ao enviar o formulário
de igreja vazio: **6 erros por campo**, 6 campos com `aria-invalid` e 6 com
`aria-describedby` válido, contra zero antes.

Um defeito apareceu na verificação: chamar `focarPrimeiroErro` dentro do
`catch` não funciona, porque o React ainda não confirmou os `aria-invalid` no
DOM. Os três formulários passaram a chamá-lo num `useEffect` dependente de
`erros`, que é o padrão que `CadastroMembro` já usava.

### 3.4 · Confirmação destrutiva

Nova `DialogoConfirmacao` em [`Dialogo.tsx`](../../src/CrmIctm.Web/src/Dialogo.tsx),
sobre o `<dialog>` nativo. Os dois `window.confirm` foram removidos — o diálogo
do navegador trava a aba, não é estilizável e aparece no idioma dele.

A confirmação agora explica a consequência em vez de só perguntar: "A igreja X
deixa de aparecer para novos visitantes e membros. Os dados já registrados são
preservados e ela pode ser reativada depois." O foco inicial cai em "Cancelar",
que é o padrão seguro para ação destrutiva. Verificado: Escape fecha, o foco
volta ao botão de origem e nada é desativado.

### 3.1 · Tokens (parte segura)

Acrescentadas ao `:root` as escalas de espaçamento (`--esp-1..6`), raio
(`--raio-sm/md/lg/xl`) e tipografia (`--txt-xs..xl`), mais três cores
recorrentes que não tinham nome: `--borda-campo`, `--borda-campo-hover` e
`--texto-rotulo`.

Substituídos **23 hexadecimais** que eram exatamente o valor de um token, de
162 para 142 no arquivo. Conferido na aplicação: os valores computados ficaram
idênticos — borda `rgb(203, 210, 223)`, rótulo `rgb(38, 49, 74)` —, ou seja,
nenhuma mudança visual.

Os 123 hexadecimais restantes fora do `:root` são, em boa parte, tons usados
uma única vez. Trocá-los exige decidir caso a caso se cada um vira token ou se
aproxima de um existente, o que muda cor na tela. Fica para quando as escalas
acima começarem a ser aplicadas nos componentes.

### O que não foi feito, e por quê

**Normalização de breakpoints** (parte do 3.1) e **limpeza da cascata**
(item 3.5, com os 92 blocos que redeclaram seletores e os três `@media print`).

Os dois reordenam ou removem regras e, por isso, mudam a cascata: o risco não é
quebrar o build, que continuaria passando, mas deslocar silenciosamente algo em
uma das telas. É o tipo de mudança que precisa de uma passada tela a tela, em
cada largura, e não de ser emendada no fim de uma sessão longa. Os sete pontos
de quebra atuais (400, 420, 700, 720, 721–1024, 850, 900) continuam
documentados no item 3.1.

### Validação

Build Docker completo e **59 testes aprovados**. Conferido na aplicação em
execução (`crm-ictm:ui-lote3-20261114` em `http://localhost:8082`): formulários
públicos de visitantes e de membros com especificação idêntica; modal
administrativo sem mudança visual (44px / 10px 12px / 9px preservados) e com
`label for` explícito, que antes era associação implícita; erro por campo na
administração; confirmação em diálogo nativo. Nenhum registro foi criado,
alterado ou desativado no banco de testes.

---

## Lote 3, parte final — cascata — 14/11/2026

Item **3.5** e a consolidação de breakpoints. Com isto o Lote 3 está fechado.

### Correção importante ao diagnóstico

O documento afirmava que **92 dos 553 blocos redeclaravam um seletor já
declarado**. O número estava errado, e de um jeito que inflava o problema:
ele contava seletores repetidos em **qualquer** lugar do arquivo, incluindo
dentro de `@media` — que é a forma normal e correta de escrever
responsividade, não uma duplicata.

Medido corretamente, considerando só o nível superior, fora de media queries:

| | |
|---|---|
| regras de nível superior | 427 |
| seletores declarados mais de uma vez | **4** |
| desses, com propriedade de fato sobrescrita (código morto) | **1** |

Os outros três eram declarações partidas em dois lugares do arquivo, sem
conflito entre si: feias de manter, mas sem efeito algum na tela.

### O que foi feito

**Quatro seletores fundidos.** `.grupo-campos-membro` perdia `padding` e
`border-radius` para uma segunda declaração 3.000 linhas abaixo; a primeira
agora carrega os valores vencedores e a segunda saiu.
`.acoes-gestao-membros .botao-secundario`, `.topo-cartao-membro` e
`.topo-cartao-membro h2` tiveram suas metades juntadas na origem. Seletores
duplicados no topo agora: **zero**.

**CSS morto de uma renomeação.** As classes `.cartaz-a4-membros` e
`.conteudo-cartaz-membros` não existem em componente nenhum: o cartaz de
membros é renderizado com `cartaz-a4 convite-membros` e `conteudo-cartaz`.
Eram 23 linhas de regras que nunca casavam com nada, mais duas regras de
especificidade maior (`.cartaz-a4.cartaz-a4-membros`) criadas só para vencer
as mortas. Tudo removido.

**Os três `@media print` viraram um.** Não por fusão: dois deles existiam
apenas para as classes mortas acima e saíram junto. O bloco de impressão que
importa ficou intacto, no mesmo lugar — nenhuma regra de impressão foi
movida, o que era o risco que mais preocupava, já que o cartaz A4 é um
entregável físico que não dá para conferir daqui.

**Breakpoints: de sete para quatro.** A V2 havia criado 400 e 700 onde o MVP
já usava 420 e 720 — a mesma raiz da deriva dos campos. Os dois pares foram
unificados nos valores do MVP.

| antes | depois |
|---|---|
| 400, 420, 700, 720, 721–1024, 850, 900 | **420, 720, 721–1024, 850 e 900 (cartaz)** |

Os 850 e 900 do cartaz **não** foram unificados de propósito: o bloco de 850 é
`screen and`, e o de 900 é um `@media` simples que, a 210 mm (≈794 px), também
vale na impressão. Juntá-los tiraria aquela regra do PDF impresso. Fica
documentado como a única divergência de breakpoint que permanece, e por quê.

### Verificação

Antes de mexer, foi capturada uma linha de base de estilos computados: 23
propriedades de **todos** os elementos, em 6 telas × 3 larguras — 1.614
elementos. Depois da mudança, a mesma captura foi comparada campo a campo.

**Zero diferenças.** Nenhum pixel mudou nas larguras 375, 768 e 1280.

Nas duas faixas que mudam por desenho (401–420 px e 701–720 px), conferido a
410 px e 710 px: as telas de membros passam ao layout compacto um pouco antes,
que é exatamente o que a unificação pretendia, sem overflow horizontal e sem
elemento vazando.

Build Docker completo e **59 testes aprovados**. O arquivo foi de 3.195 para
3.172 linhas.

### O que continua aberto

Os 123 hexadecimais fora do `:root` que não coincidem com token nenhum, e a
aplicação das escalas `--esp-*`, `--raio-*` e `--txt-*` nos componentes. Nada
disso é cascata: é substituição valor a valor, que muda cor e espaçamento na
tela e pede decisão de desenho, componente a componente.

---

## Lotes 4 e 5, padronização visual e ajustes do responsável — 14/11/2026

Rodada a partir da avaliação do responsável no aparelho real. Fecha os Lotes 4
e 5 e acrescenta itens que só apareceram no uso.

### Verificação física, finalmente feita

O responsável **imprimiu os PDFs em papel e aprovou**. Era a pendência mais
antiga da V3, aberta desde o Lote 1. Continua pendente apenas o teste em
Safari/iPad/iPhone além do que já foi observado aqui.

### Decisões fechadas

- **D2 — kanban no celular:** manter empilhado. A equipe usa o aparelho em
  modo retrato.
- **D3 — contadores na busca:** manter o rótulo "Resultados". O que o Lote 2
  fez como medida reversível vira definitivo; nenhum endpoint novo.

### O menu que ficava aceso no iPhone

Relatado com captura: estando em Atendimentos, o botão "Igrejas" continuava
destacado. Causa: **26 regras `:hover` e nenhuma protegida**. No Safari do
iPhone o `:hover` gruda no elemento tocado até o toque seguinte em outro
lugar — não era bug do menu, era de toda a aplicação.

As 17 regras de nível superior com `:hover` passaram a viver dentro de
`@media (hover: hover)`. Conferido depois: zero regras `:hover` desprotegidas,
inclusive dentro de media queries. Em aparelho com mouse, nada muda.

### Padronização das telas

As quatro seções do painel usavam três cabeçalhos diferentes:
`.apresentacao-painel` em Atendimentos, `.cabecalho-gestao-membros` em Membros
e `.cabecalho-administracao` em Igrejas e Usuários — cartão numa, texto solto
nas outras.

Agora todas usam `.apresentacao-painel`, no desenho de Atendimentos: rótulo,
título e descrição à esquerda; `.lado-painel` à direita com as ações em cima e
os indicadores embaixo. Igrejas e Usuários ganharam indicadores que não tinham
(total / ativas / inativas e total / ativos / inativos), calculados das listas
que já chegam — sem API nem banco.

Os cabeçalhos antigos foram removidos do CSS. Onde um seletor morto dividia a
regra com seletores vivos — os botões de rodapé dos modais —, só o morto saiu.

### Botão de ação primária

`+ Nova igreja`, `+ Novo usuário` e `Novo membro` tinham cada um o seu azul
chapado. Agora os três usam `.botao-principal`, com o mesmo degradê do botão
de envio: criar se parece com criar em qualquer tela.

### Marca e ícone

"Gestão da igreja" passou a **"Tópos - Gestão de Igrejas"** nas três
ocorrências da interface, e o autor nos metadados dos PDFs foi de "Topos" para
"Tópos", para a marca ficar escrita igual em todo lugar.

Favicon e ícone de tela inicial gerados da logo (32 px e 180 px). Ficam em
`src/icones/` e não em `public/`, pelo mesmo motivo das fontes: o Caddy só
libera `/assets/*` no domínio público.

### Lote 4

- **4.1 · Kanban.** `grid-template-columns: repeat(auto-fit, minmax(208px, 1fr))`
  no lugar das cinco colunas fixas de 255 px. Medido: 5 colunas a 1440 px,
  4 a 1024 px, 3 a 768 px, **sem rolagem horizontal em nenhuma largura** —
  antes eram 1.333 px fixos contra 728 px visíveis no tablet. No celular segue
  empilhado, conforme D2. O arraste é por código de etapa, não por posição,
  então a quebra de linha não muda quais movimentos são permitidos.
- **4.2 · Largura entre abas.** `.pagina-painel > *` recebeu
  `width: min(100%, 1280px)` e o teto saiu de `.gestao-membros`. O conteúdo
  parou de saltar de 1600 px para 1280 px ao trocar de aba.
- **4.3 · Título por rota.** Novo `titulo.ts`. As telas internas ficam
  "Seção · Tópos - Gestão de Igrejas"; as públicas, "Página · Nome da igreja",
  que é o que o visitante reconhece.

### Lote 5 · Gestão de membros em componentes

`GestaoMembros.tsx` tinha 401 linhas, 24 `useState` e cinco telas numa cadeia
ternária. Foi dividido:

| arquivo | linhas | papel |
|---|---|---|
| `GestaoMembros.tsx` | 301 | contêiner: dados, mutações, navegação |
| `membros/MembrosLista.tsx` | 118 | abas, filtros, lista e paginação |
| `membros/MembroPartes.tsx` | 105 | blocos compartilhados pelos dois detalhes |
| `membros/SolicitacaoDetalhe.tsx` | 73 | aprovar, recusar, duplicidade |
| `membros/MembroFormulario.tsx` | 72 | novo, edição e correção |
| `membros/MembroDetalhe.tsx` | 65 | detalhe e inativação |

O ganho principal não é o tamanho: os quatro sinalizadores de navegação
(`novo`, `editando`, `membro`, `solicitacao`) viraram **um** estado
discriminado. Antes nada impedia dois verdadeiros ao mesmo tempo, e cada
transição precisava lembrar de zerar os outros três. O contêiner foi de 24
para 17 `useState`, e os estados que pertenciam a uma tela só — motivo da
recusa, motivo da inativação, confirmação de inativação — desceram para os
componentes que os usam.

Os cabeçalhos dos cartões seguem o mesmo desenho das seções, como pedido.

### Validação

Build Docker completo e **59 testes aprovados**. Exercitado na aplicação em
execução, nas cinco telas: lista → detalhe → edição com campos preenchidos →
cancelar → lista; e aba de solicitações → detalhe → correção → cancelar. Os
campos pastorais aparecem no detalhe da solicitação e somem na correção, como
antes. Nenhum cadastro foi criado ou alterado.

### Correções da segunda avaliação — 14/11/2026

Três problemas relatados com captura, no navegador de computador.

**O painel desalinhou.** Causa minha, da rodada anterior: eu havia posto o teto
de largura em `.pagina-painel > *`. Vários filhos têm regra própria de
`margin` que zera o `margin-inline`, de mesma especificidade e declarada
depois — então a barra de abas ia para a borda da janela enquanto o cabeçalho
ficava centralizado. O teto passou para a própria `.pagina-painel`, que centra
o bloco inteiro de uma vez e não disputa com regra nenhuma.

Medido a 1900 px, nas quatro abas: topo, abas e cabeçalho todos em 303→1583.

**Sublinhado no "Ver QR de membros".** O `text-decoration: none` vivia em
`.acoes-gestao-membros .botao-secundario`, classe que deixou de existir na
padronização. Recriado em `.acoes-painel`, junto com os demais ajustes de
link-que-parece-botão. Dois seletores mortos de `.acoes-gestao-membros`
saíram do arquivo.

**Cartão externo em Igrejas e Usuários.** `.secao-administracao` tinha fundo
branco, borda e raio, embrulhando cabeçalho e lista num cartão maior — um
nível de moldura que o painel de atendimentos não tem. Agora é só espaçamento;
as peças ficam soltas sobre o fundo da página, como nas outras telas.

**Ainda:** `scrollbar-gutter: stable` no `html`. Sem isso o conteúdo
centralizado deslocava 7 px ao trocar para uma aba mais alta, quando a barra
de rolagem aparece. Agora as quatro abas renderizam na mesma caixa, medida.
