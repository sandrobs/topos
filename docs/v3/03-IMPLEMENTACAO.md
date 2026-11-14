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
