# V3 — Decisões pendentes

Estas decisões **não devem ser inventadas pelo implementador**. Apresente as
opções ao responsável pelo produto, registre a escolha aqui com a data e só
então construa a parte afetada. É possível trabalhar em todo o resto enquanto
isso.

---

## D1 · Tipografia — **decidido em 14/11/2026: opção A, entregar a Inter**

> O responsável pelo produto escolheu hospedar a Inter no próprio projeto,
> contra a recomendação técnica registrada abaixo. Implementado no mesmo dia;
> ver [03-IMPLEMENTACAO.md](03-IMPLEMENTACAO.md). O texto original da decisão
> segue preservado como registro.

**Situação medida.** `:root` declara
`font-family: Inter, ui-sans-serif, system-ui, …`, mas a Inter nunca é entregue:
não há `@font-face`, nem link para Google Fonts, nem pacote npm. `document.fonts`
está vazio. Hoje, quem tem Inter instalada vê Inter; quem não tem vê Segoe UI ou
a fonte do sistema. **Dois usuários veem tipografias diferentes.**

O CSS usa 11 pesos distintos (400, 450, 500, 700, 720, 750, 760, 780, 800, 820,
850, 900). Onde a Inter local resolve, todos colapsam em **duas** larguras
renderizadas — a hierarquia planejada não existe.

**Opção A — entregar a Inter.** Hospedar o arquivo variável no próprio projeto
(~100 kB woff2, `font-display: swap`, `preload`). Os pesos intermediários passam
a funcionar de verdade e todos os usuários veem a mesma tipografia.
*Custo:* ~100 kB por visitante na primeira visita; um binário a mais no
repositório. A Inter é licenciada em SIL OFL, então redistribuir é permitido.

**Opção B — assumir a fonte do sistema.** Remover a Inter da pilha e normalizar
os pesos para 400 / 600 / 700. Zero bytes extras e renderização previsível.
*Custo:* quem hoje tem Inter instalada passa a ver outra tipografia.

**Recomendação técnica:** B. Não acrescenta peso à página que o visitante abre
no estacionamento da igreja, e a identidade impressa não depende da Inter — os
PDFs já embutem Liberation Sans.

**Por que depende do responsável:** é a mudança mais visível possível na
aplicação inteira, e a instrução vigente é preservar a identidade visual. As
duas opções são defensáveis; a diferença é estética.

**Decisão:** **opção A**, em 14/11/2026.

---

## D2 · Kanban no celular: empilhar ou um estágio por vez

**Situação medida.** Abaixo de 720 px o kanban vira `display: block` e empilha as
cinco colunas. Com 1 atendimento são 1,1 tela de rolagem; com 20 atendimentos
distribuídos seriam ~3.720 px, **4,6 telas** até chegar em "Concluído".

**Opção A — manter empilhado.** Nada muda; a equipe rola. Simples e já funciona.

**Opção B — um estágio por vez.** Chips de filtro no topo mostram uma coluna de
cada vez. Encurta a rolagem, mas esconde a visão geral do funil, que é
justamente o que um kanban oferece.

**Por que depende do responsável:** envolve como a equipe pastoral realmente usa
o painel no celular — se consulta um estágio específico ou se acompanha o fluxo
inteiro. Isso não se deduz do código.

**Decisão:** _pendente_

---

## D3 · Contadores do painel durante a busca

**Situação medida.** "Total", "Novos" e "Em atendimento" derivam da lista já
filtrada. Numa igreja com 1 atendimento, pesquisar um termo sem correspondência
faz "Total" exibir 0.

**Opção A — rotular como resultado.** Enquanto houver termo ativo, os rótulos
viram "Resultados", "Novos nos resultados" etc. Sem mudança de API.

**Opção B — contadores sempre da igreja.** Buscar os totais num endpoint
próprio, como `GestaoMembros` já faz com `obterResumoMembros()`. Os números
param de reagir à busca. **Exige endpoint novo na API.**

**Recomendação técnica:** A para o Lote 2, por não tocar a API; B quando houver
oportunidade de mexer no backend.

**Decisão:** _pendente_ — o Lote 2 implementa A como medida imediata, de forma
reversível.

---

## Decisões já tomadas

| # | Decisão | Quando |
|---|---|---|
| — | Servir uma variante de 192 px da logo em `public/` e manter o original de 1254 px como recurso embutido dos PDFs | 14/11/2026 |
| — | Manter um único caminho público `/logo-igreja.png`, sem `srcset`, para não alterar a lista de rotas do Caddy | 14/11/2026 |
| — | Usar `<dialog>` nativo como padrão único de modal, a partir de `DialogoAvisoPrivacidade.tsx` | 14/11/2026 |
