# V3 — Revisão de interface, acessibilidade e desempenho visual

**Estado:** em implementação local na branch `feature/ui-lote1`. **Lotes 1, 2 e
3 concluídos** e verificados em 14/11/2026. Nada publicado em produção.

## Próximos passos

Sobrou do Lote 3 um item que **não é cascata**, e por isso não foi feito junto:
os 123 hexadecimais fora do `:root` que não coincidem com nenhum token. Cada um
exige decidir se vira token novo ou se aproxima de um existente, o que muda cor
na tela. O caminho é aplicar as escalas `--esp-*`, `--raio-*` e `--txt-*`
componente a componente, com o responsável olhando, e não num passe automático.

Permanecem os **Lotes 4 e 5**, descritos em
[01-ESCOPO-E-LOTES.md](01-ESCOPO-E-LOTES.md): responsividade do kanban (ele
ainda exige 1.333 px e rola na horizontal em qualquer tablet), largura do
conteúdo que salta ao trocar de aba, `document.title` fixo em todas as rotas, e
a quebra de `GestaoMembros` em componentes, prevista para quando a expansão da
V2 entrar.

### Verificação física, nunca feita

Nada disto foi conferido em aparelho real, e a emulação não substitui:

- impressão dos PDFs em papel;
- Safari, iPad e iPhone reais — inclusive a renderização da Inter em tela
  Retina e o comportamento de foco do `<dialog>` nativo no iOS;
- arrastar-e-soltar por toque no kanban.

### Uma ferramenta que vale reusar

A limpeza da cascata foi verificada capturando os estilos computados de todos
os elementos (23 propriedades, 6 telas × 3 larguras, 1.614 elementos) antes e
depois, e comparando campo a campo. O procedimento está descrito em
[03-IMPLEMENTACAO.md](03-IMPLEMENTACAO.md) e serve para qualquer mudança de CSS
de risco — foi o que permitiu afirmar "zero diferenças" em vez de "parece
igual".

## Objetivo

A V3 não acrescenta funcionalidade. Ela corrige a camada de apresentação do que
já existe: hierarquia visual, consistência, responsividade, usabilidade,
acessibilidade, qualidade do front-end e peso dos recursos. O produto continua
sendo o acolhimento de visitantes (MVP) e o cadastro de membros (V2).

A motivação é concreta. A V2 foi escrita por cima do CSS do MVP em vez de reusar
seus tokens, e o resultado é mensurável: o mesmo campo de texto tem duas
especificações nos dois formulários públicos; há 165 cores em hexadecimal fixo
contra ~20 tokens; há 8 breakpoints sem escala; o mesmo componente de campo foi
reimplementado quatro vezes, com qualidade de acessibilidade desigual entre elas.

## Ordem de leitura para desenvolvimento

1. [Escopo e lotes](01-ESCOPO-E-LOTES.md) — os achados, a evidência de cada um e
   o que fazer, agrupados em cinco lotes por impacto e esforço
2. [Decisões pendentes](02-DECISOES-PENDENTES.md) — o que **não** deve ser
   decidido pelo implementador
3. [Implementação e validação](03-IMPLEMENTACAO.md) — registro do que já foi feito

## Regra de precedência

A V3 rege apenas apresentação. Ela **não** altera campos, validações de domínio,
permissões, regras de negócio, contratos de API, conteúdo dos PDFs nem o destino
dos QR Codes. Em qualquer conflito aparente, o comportamento funcional descrito
em `docs/` (MVP) e `docs/v2/` prevalece, e o ajuste visual cede.

Os cinco documentos originais do MVP e os documentos da V2 continuam congelados
como estão. Mudanças de apresentação são registradas aqui, em `docs/v3/`.

## Como cada achado está classificado

- **Confirmado** — reproduzido na aplicação em execução, com medição. O documento
  cita o número medido.
- **A validar** — derivado de leitura de código e ainda não reproduzido.

Nenhum achado deste documento é cosmético por si só: cada um tem um efeito
descrito sobre uso, acessibilidade ou peso. Achados puramente estéticos foram
deliberadamente deixados de fora.

## Ambiente de verificação

As medições foram feitas em `http://localhost:8082`, com a imagem
`crm-ictm:ui-lote1-20261114` apontando para o mesmo banco de testes da instância
`8081`, que permanece na versão anterior para comparação lado a lado.

Credenciais de teste existem apenas na instância local; não as registre neste
repositório.
