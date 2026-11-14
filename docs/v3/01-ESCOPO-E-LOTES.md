# V3 — Escopo e lotes

Agrupamento por impacto sobre o uso dividido pelo esforço. Esforço: **P** até 1 h,
**M** 2 a 6 h, **G** 1 a 3 dias.

Leia antes a [regra de precedência](README.md#regra-de-precedência): nada aqui
altera campos, validações de domínio, permissões, regras de negócio, contratos
de API, conteúdo dos PDFs ou destino dos QR Codes.

---

## Diagnóstico em uma página

O produto tem identidade visual coerente e o formulário público de visitantes —
o artefato mais exposto, aberto pelo QR Code — está bem construído. Os problemas
são três dívidas estruturais, não falta de capricho:

1. **Não existe um sistema de design, existe um arquivo CSS.** 3.031 linhas,
   553 blocos de regra, 165 cores em hexadecimal fixo contra ~20 tokens em
   `:root`, 8 breakpoints sem escala (400, 420, 700, 720, 721–1024, 850, 900,
   1024) e 92 blocos que redeclaram um seletor já declarado.
2. **O mesmo componente foi reimplementado várias vezes.** Campo de formulário
   em quatro versões, sobreposição modal em quatro, confirmação destrutiva em
   três — com acessibilidade desigual entre elas. O padrão correto existe no
   repositório e não foi reaproveitado.
3. **O custo de entrega estava concentrado em imagens.** Resolvido no Lote 1.

---

## Lote 1 — concluído em 14/11/2026

Detalhes e medições em [03-IMPLEMENTACAO.md](03-IMPLEMENTACAO.md). Resumo:
logo de 1.385 kB para 41,5 kB com o original preservado para os PDFs; imagem de
1,8 MB sem referência removida; token `--foco` opaco aplicado às sete regras de
foco; contraste do rodapé público corrigido; marcador de múltipla escolha
deixou de ser circular; consentimento alinhado ao topo; datas ISO e enums
formatados na gestão de membros.

---

## Lote 2 — retorno ao usuário e peso do JavaScript

Prioridade mais alta do que o planejado originalmente: a validação de 14/11
mostrou que o foco de teclado escapa para trás das sobreposições, o que é falha
de uso real, não detalhe de conformidade.

### 2.1 · Sobreposições modais sem Escape, sem armadilha de foco · **M** · confirmado

Quatro sobreposições construídas à mão com `<div className="sobreposicao">` em
[Interno.tsx](../../src/CrmIctm.Web/src/Interno.tsx) e
[Administracao.tsx](../../src/CrmIctm.Web/src/Administracao.tsx).

Medido no modal de detalhe do atendimento:

| verificação | resultado |
|---|---|
| `role` / `aria-modal` / `aria-labelledby` | presentes ✓ |
| Escape fecha | **não** |
| foco entra no modal ao abrir | **não** — fica no `<body>` |
| foco fica preso dentro | **não** — 3 Tabs chegam ao botão "Atendimentos", atrás da sobreposição |
| focáveis alcançáveis atrás | **11** |
| fundo marcado `inert` ou `aria-hidden` | **não** |
| rolagem do fundo travada | **não** (`overflow: visible`) |

O formulário administrativo (Nova/Editar igreja, Novo/Editar usuário) é pior: um
`<form>` com `role` nulo, `aria-modal` nulo e nenhum rótulo acessível, 12
focáveis alcançáveis atrás. Para um leitor de tela, nada indica que um diálogo
abriu.

**Solução:** extrair um `<Dialogo>` a partir de
[DialogoAvisoPrivacidade.tsx](../../src/CrmIctm.Web/src/DialogoAvisoPrivacidade.tsx),
que já usa `<dialog>` nativo com `showModal()`. O elemento nativo entrega
Escape, armadilha de foco, devolução de foco, `inert` do fundo e backdrop sem
código. Migrar as quatro sobreposições. **Resultado esperado:** menos código do
que hoje e quatro modais acessíveis.

### 2.2 · Estado vazio falso durante o carregamento · **M** · confirmado

`selecionarIgreja` faz `setAtendimentos([])` e `setCarregando(true)`, mas
`carregando` só é consultado em `if (carregando && !sessao)`. Medido ao trocar
para uma igreja que **tem** 1 atendimento, com atraso artificial na API:

| momento | colunas com "Nenhum atendimento" | Total | indicador |
|---|---|---|---|
| 62 ms | 5 de 5 | 0 | nenhum |
| 906 ms | 5 de 5 | 0 | nenhum |
| 1513 ms | 5 de 5 | 0 | nenhum |
| 2800 ms | 4 de 5 | 1 | — |

Durante todo o carregamento o painel afirma que a igreja não tem ninguém.

Dois casos irmãos, com a mesma origem:

- **Busca sem retorno.** `pesquisarAtendimentos` nunca toca `carregando`. Medido:
  2,6 s após clicar em "Pesquisar", nada mudou — botão ativo, resultados antigos
  na tela, nenhum sinal.
- **Clique no cartão sem retorno.** `abrirDetalhe`
  ([Interno.tsx:312](../../src/CrmIctm.Web/src/Interno.tsx)) só faz `await` e
  `setDetalhe`. O `disabled` do cartão vem do arrasto, não da abertura: durante
  toda a espera o cartão segue com `cursor: pointer` e clicável.

**Solução:** esqueleto de cartões por coluna enquanto carrega, com `aria-busy`;
botão de busca desabilitado com texto de progresso; estado pendente no cartão
clicado. `GestaoMembros` já faz certo (`{carregando && <p>Carregando cadastros…</p>}`)
— alinhar os dois.

### 2.3 · Nenhuma região `aria-live` na aplicação · **P** · confirmado

Medido: **zero** elementos com `aria-live` em toda a aplicação. Nenhuma mensagem
de status é anunciada a leitor de tela (WCAG 2.1 SC 4.1.3). Esta é a raiz comum
de todos os "sem retorno" acima.

**Solução:** uma região de status polida por tela, alimentada pelos mesmos
estados que já governam os textos visíveis.

### 2.4 · Seção ativa indicada só por classe CSS · **P** · confirmado

A navegação principal (`.navegacao-interna`: Atendimentos, Membros, Igrejas,
Usuários) marca a seção aberta apenas com a classe `ativo`. Sem `aria-current`,
um leitor de tela lê quatro botões iguais e não informa onde o usuário está.

Relacionado: as opções do seletor de igreja não têm nome acessível na árvore de
acessibilidade e não expõem `aria-selected`; a seleção é indicada só pelo ✓.

**Solução:** `aria-current="page"` no botão da seção ativa. Mais simples do que
um `tablist`, que exigiria navegação por setas.

> **Correção a um levantamento anterior.** Chegou-se a registrar que havia um
> `role="tablist"` sem nenhum `role="tab"`. Estava errado: as abas
> Membros/Solicitações de `GestaoMembros` **têm** `role="tab"` e
> `aria-selected` corretos. A medição original misturou esse `tablist` com os
> botões da navegação principal, que são outro componente.

### 2.5 · "Total" não é o total quando há busca · **P** · confirmado

Os três indicadores derivam de `atendimentos.length`, e `atendimentos` é a lista
já filtrada. Medido numa igreja com 1 atendimento: ao pesquisar um termo sem
correspondência, **Total passou a 0**.

**Solução:** rotular como "Resultados" enquanto houver termo ativo, ou buscar os
contadores num endpoint próprio. `GestaoMembros` já usa `obterResumoMembros()`
separado da lista — é o padrão certo.

### 2.6 · O visitante baixa o painel administrativo inteiro · **M** · confirmado

Bundle único de 399 kB, sem code splitting.
[App.tsx](../../src/CrmIctm.Web/src/App.tsx) importa `Interno` estaticamente no
topo, e `Interno` importa `@dnd-kit/react`. Quem escaneia o QR Code na porta da
igreja baixa kanban, arrastar-e-soltar, gestão de igrejas e gestão de usuários
para preencher dois campos.

**Solução:** `React.lazy()` nas rotas internas (`Interno`, `Administracao`,
`GestaoMembros`, `QrCode`) com `<Suspense>`. O roteamento é por `pathname` em
`App.tsx`, então o corte é natural.

### 2.7 · A fonte Inter é declarada mas nunca entregue · **M** · resolvido

Decisão D1 tomada em 14/11/2026: a Inter passou a ser hospedada no projeto.
Resultado medido em [03-IMPLEMENTACAO.md](03-IMPLEMENTACAO.md) — os doze pesos
usados no CSS agora renderizam com larguras distintas. O diagnóstico original
segue abaixo.

`document.fonts` está vazio: não há `@font-face`, nem link para Google Fonts, nem
pacote npm. A família resolve só se o usuário tiver Inter instalada. Largura de
texto medida por peso:

```
Inter local:     400→533px  500→533px  700→557px  800→557px  850→557px  900→557px
Fallback Segoe:  400→570px  500→581px  700→606px  800→630px  850→630px  900→630px
```

O CSS usa 11 pesos distintos (450, 700, 720, 750, 760, 780, 800, 820, 850, 900) e
onde a Inter resolve **todos colapsam em duas larguras renderizadas**: a
hierarquia tipográfica planejada não aparece. E cada usuário vê uma tipografia
diferente conforme tenha ou não Inter instalada.

---

## Lote 3 — fundação do sistema de design

### 3.1 · Tokens e normalização de breakpoints · **G** · confirmado

Mesmo controle, duas especificações, medidas nos dois formulários públicos:

| | `.campo` (visitante) | `.campo-membro` (membro) |
|---|---|---|
| label `font-size` | 14,4 px | 13,76 px |
| label `font-weight` | 750 | 700 |
| input `padding` | 12px 14px | 10px 12px |
| input `border-radius` | 11px | 10px |
| input `border-color` | `#cbd2df` | `#cdd5e3` |

Não são escolhas, é deriva de copiar-e-colar. Some 165 hexadecimais fixos contra
~20 tokens e 8 breakpoints sem escala.

**Solução:** estender `:root` com escala de espaçamento (`--esp-1..6`), raio
(`--raio-sm/md/lg`), tipografia (`--txt-xs..xl`) e três breakpoints
(480 / 768 / 1200); substituir por token num passe.

### 3.2 · Quatro implementações de campo de formulário · **G** · confirmado

| implementação | arquivo | erro de campo | `aria-describedby` |
|---|---|---|---|
| `CampoTexto` | `App.tsx` | `<p class="erro-campo">` | ✓ |
| `campo()` (closure) | `CadastroMembro.tsx` | `<small class="erro-campo">` | ✗ |
| `Campo` | `Administracao.tsx` | nenhum | ✗ |
| `.campo-detalhe` em linha | `Interno.tsx` | nenhum | ✗ |
| `<label>`+`<input>` cru | `Interno.tsx` (login, troca de senha) | — | — |

Medido: **0 de 11** inputs do formulário público de membros têm
`aria-describedby`, então a mensagem de erro existe visualmente mas não é
anunciada.

Correção a um levantamento anterior: os rótulos da administração **estão**
corretos — 10 de 10 inputs têm rótulo por aninhamento implícito
(`<label>Nome da igreja<input></label>`). O que falta ali é erro por campo.

**Solução:** um `<CampoFormulario>` único (id, label, erro, `aria-invalid`,
`aria-describedby`, `autoComplete`, `maxLength`) e migrar as cinco.

### 3.3 · Três padrões de validação diferentes · **M** · confirmado

- `App.tsx`: erro por campo, com `role="alert"` e `aria-describedby`
- `CadastroMembro.tsx`: erro por campo, sem ligação ARIA
- `Administracao.tsx`: **validação nativa do navegador** (`noValidate: false`,
  7 campos `required`) — bolhas do navegador, no idioma e no estilo dele

**Solução:** decidido junto com 3.2; a administração passa a usar o mesmo
componente e `noValidate` com validação própria.

### 3.4 · Confirmação destrutiva em três padrões · **P** · confirmado

`window.confirm()` nativo em `Administracao.tsx` (linhas 110 e 291), bloco em
linha em `GestaoMembros.tsx` e modal customizado no kanban. Texto nativo
capturado: `"Deseja desativar a igreja Igreja de Teste V2?"`.

**Solução:** o `<Dialogo>` do item 2.1, em variante de confirmação.

### 3.5 · Limpeza estrutural do CSS · **M** · confirmado

- **92 dos 553 blocos redeclaram um seletor já declarado.** Exemplo sem media
  query no meio: `.grupo-campos-membro` na linha 40 (`padding: 19px`,
  `border-radius: 14px`) e de novo perto da 2997 (`padding: 20px`,
  `border-radius: 16px`); a primeira é código morto.
- **Três blocos `@media print` separados** (linhas 144, 2818, 2985).
- Os estilos da V2 foram **prependidos**: começam na linha 24, antes do reset
  global `* { box-sizing: border-box }` da linha 152 — e há um segundo bloco V2
  no fim, sob `/* Ajustes V2: ... */`, que existe só para sobrescrever o primeiro.

**Solução:** dividir em `tokens.css`, `base.css`, `componentes.css`,
`publico.css`, `painel.css`, `impressao.css` (o Vite resolve `@import` no build),
eliminando os pares conflitantes.

---

## Lote 4 — responsividade do painel

### 4.1 · Kanban exige rolagem horizontal em todo tablet · **M** · confirmado

```css
.kanban { grid-template-columns: repeat(5, minmax(255px, 1fr)); overflow-x: auto; }
```

5 × 255 px + 4 × 14 px de gap = 1.331 px mínimos. Medido: conteúdo de **1333 px**.

| janela | área visível | resultado |
|---|---|---|
| 1024 px | 969 px | rola; "Concluído" fora da tela |
| 768 px | 728 px | rola; 3 de 5 colunas fora |
| 375 px | — | empilha na vertical |

No celular, com 1 atendimento são 1,1 tela. Mas o cartão mede 140 px: com 20
atendimentos distribuídos o kanban daria ~3.720 px, **4,6 telas** de rolagem até
"Concluído". O problema escala com o uso real.

**Solução:** `repeat(auto-fit, minmax(240px, 1fr))` na faixa intermediária e, no
celular, um seletor de etapa (chips) que mostra uma coluna por vez.

**A validar:** o arrastar-e-soltar do `@dnd-kit` com alça dentro de contêiner
rolável pode conflitar com a rolagem por toque. Precisa de teste em aparelho
real — a emulação não prova.

### 4.2 · Largura do conteúdo muda ao trocar de aba · **P** · confirmado

`.pagina-painel` não tem `max-width`; `.gestao-membros` tem
`max-width: 1280px; margin: 0 auto`. Medido a 1600 px de janela: o conteúdo salta
de **1600 px para 1280 px** ao ir de Igrejas para Membros, enquanto a barra de
abas continua em largura total.

**Solução:** um contêiner de largura máxima único para o painel.

### 4.3 · `document.title` nunca muda · **P** · confirmado

Todas as rotas — login, painel, formulário público, cartaz — compartilham
"Igreja Evangélica de Cristo". Atrapalha abas múltiplas e histórico, que o painel
usa (`/qrcode/{id}` abre em nova aba).

**Solução:** título por rota, mantendo o nome da igreja como sufixo.

---

## Lote 5 — quando a expansão da V2 entrar

### 5.1 · `GestaoMembros` concentra cinco telas · **G** · confirmado

390 linhas, ~25 `useState`, e uma cadeia ternária
`novo || editando ? … : membro ? … : solicitacao ? … : lista` que decide entre
lista, detalhe de membro, detalhe de solicitação e formulário. O arquivo também
destoa do estilo do resto do projeto (vários comandos por linha, JSX comprimido).

**Solução:** extrair `ListaMembros`, `DetalheMembro`, `DetalheSolicitacao` e
`FormularioMembro`; estado de navegação num `useReducer`. Fazer junto com a
conversão de visitante em membro, prevista na expansão da V2, para não mexer
duas vezes no mesmo arquivo.

---

## Fora do escopo da V3

- Trocar biblioteca de componentes, adotar framework de CSS ou roteador.
- Redesenhar telas: a identidade visual é preservada.
- Qualquer alteração de domínio, permissão ou contrato de API.
- Mudanças puramente estéticas sem efeito descrito sobre uso, acessibilidade ou
  peso.

## Verificação física ainda pendente

Impressão dos PDFs em papel e teste em Safari/iPad/iPhone reais. A emulação
Chromium não prova o comportamento desses dispositivos, e cores físicas dependem
de impressora e papel.
