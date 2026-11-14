# V3 — Revisão de interface, acessibilidade e desempenho visual

**Estado:** em implementação local na branch `feature/ui-lote1`. Lotes 1, 2 e a
maior parte do 3 concluídos e verificados em 14/11/2026. Nada publicado em
produção.

## Ponto de retomada — o que falta do Lote 3

Sobraram os dois itens que reordenam regras de CSS e, por isso, mudam a
cascata. O risco deles não é quebrar o build — ele continuaria passando — e sim
deslocar algo silenciosamente numa tela. Pedem uma passada tela a tela, em cada
largura, com a instância 8081 aberta ao lado para comparação.

1. **Normalização de breakpoints** (parte do 3.1). Hoje são sete pontos sem
   escala: 400, 420, 700, 720, 721–1024, 850 e 900. A proposta é 480 / 768 /
   1200. Mexer nisto é alterar onde cada layout muda de forma.
2. **3.5 — limpeza da cascata.** Remover os 92 blocos que redeclaram um seletor
   já declarado e juntar os três `@media print`. Exemplo do tipo de par a
   resolver: `.grupo-campos-membro` aparece duas vezes, com `padding` e
   `border-radius` diferentes, e só a segunda vale.

Também segue aberto, de 3.1: os 123 hexadecimais fora do `:root` que não
coincidem com nenhum token. Cada um exige decidir se vira token ou se aproxima
de um existente, o que muda cor na tela. O caminho é aplicar as escalas
(`--esp-*`, `--raio-*`, `--txt-*`) componente a componente, não num passe só.

O que já foi feito do Lote 3 está em
[03-IMPLEMENTACAO.md](03-IMPLEMENTACAO.md): alvo de 44 px, foco no primeiro
erro, validação própria nos três formulários, `CampoFormulario` único,
confirmação em `<dialog>` e as escalas de token no `:root`.

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
