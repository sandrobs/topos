# V3 — Revisão de interface, acessibilidade e desempenho visual

**Estado:** em implementação local na branch `feature/ui-lote1`. Lote 1 concluído
e verificado em 14/11/2026. Lote 2 em andamento. Nada publicado em produção.

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
