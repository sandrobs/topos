# V2 — Cadastro e gestão de membros

**Estado:** núcleo em implementação e validação local na branch `feature/v2-membros` desde 25/09/2026. Nenhuma alteração da V2 foi publicada na VPS. O formulário público permanece fechado sem o aviso específico aprovado e configurado.

## Objetivo

Permitir que membros da igreja preencham um formulário próprio, vinculado à sua igreja, e que Pastor e Administrador mantenham um cadastro confiável dos membros. Também deve ser possível cadastrar membros atuais pela equipe interna e, numa entrega posterior desta versão, converter um visitante em membro sem perder seu atendimento.

## Ordem de leitura para desenvolvimento

1. [Escopo e prioridades](01-ESCOPO-E-PRIORIDADES.md)
2. [Requisitos e modelo de informação](02-REQUISITOS-E-DADOS.md)
3. [Critérios de aceite](03-CRITERIOS-DE-ACEITE.md)
4. [Decisões pendentes](04-DECISOES-PENDENTES.md)
5. [Baseline histórica do MVP](../BASELINE-MVP-2026-09-25.md)
6. [Implementação e validação local](05-IMPLEMENTACAO-LOCAL.md)

## Regra de precedência

Os documentos desta pasta regem somente a V2. Os cinco documentos originais do MVP continuam como referência do fluxo de visitantes. Se houver aparente conflito, mantenha o comportamento do visitante e aplique a V2 ao domínio de membros. Nenhuma decisão pendente deve ser presumida como aprovada.

## Entrega em camadas

- **V2 núcleo:** formulário de membros com link e QR Code próprios, entrada em análise, aprovação pastoral, inclusão manual por Pastor ou Administrador e consulta/edição do cadastro.
- **V2 expansão:** conversão de visitante em membro com vínculo ao histórico.
- **Depois:** módulos pastorais adicionais, relatórios complexos, ministérios, famílias, contribuições e automações.
