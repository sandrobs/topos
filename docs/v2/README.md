# V2 — Cadastro e gestão de membros

**Estado:** núcleo publicado em produção em 14/11/2026, após aprovação expressa do responsável. O aviso específico de adultos aprovado em 03/10/2026 está configurado. A validação pública pós-deploy foi concluída; a conferência autenticada com a senha atual do responsável está pendente. A operação com dados reais de menores continua pendente. Consulte o [registro de publicação](08-PUBLICACAO-2026-11-14.md).

## Objetivo

Permitir que membros da igreja preencham um formulário próprio, vinculado à sua igreja, e que Pastor e Administrador mantenham um cadastro confiável dos membros. Também deve ser possível cadastrar membros atuais pela equipe interna e, numa entrega posterior desta versão, converter um visitante em membro sem perder seu atendimento.

## Ordem de leitura para desenvolvimento

1. [Escopo e prioridades](01-ESCOPO-E-PRIORIDADES.md)
2. [Requisitos e modelo de informação](02-REQUISITOS-E-DADOS.md)
3. [Critérios de aceite](03-CRITERIOS-DE-ACEITE.md)
4. [Decisões pendentes](04-DECISOES-PENDENTES.md)
5. [Baseline histórica do MVP](../BASELINE-MVP-2026-09-25.md)
6. [Implementação e validação local](05-IMPLEMENTACAO-LOCAL.md)
7. [Ajustes e aviso aprovados em 03/10/2026](06-AJUSTES-APROVADOS-2026-10-03.md)
8. [Ajustes do convite compacto de visitantes em 14/11/2026](07-CONVITE-COMPACTO-2026-11-14.md)
9. [Publicação em produção em 14/11/2026](08-PUBLICACAO-2026-11-14.md)

## Regra de precedência

Os documentos desta pasta regem somente a V2. Os cinco documentos originais do MVP continuam como referência do fluxo de visitantes. Se houver aparente conflito, mantenha o comportamento do visitante e aplique a V2 ao domínio de membros. Nenhuma decisão pendente deve ser presumida como aprovada.

## Entrega em camadas

- **V2 núcleo:** formulário de membros com link e QR Code próprios, entrada em análise, aprovação pastoral, inclusão manual por Pastor ou Administrador e consulta/edição do cadastro.
- **V2 expansão:** conversão de visitante em membro com vínculo ao histórico.
- **Depois:** módulos pastorais adicionais, relatórios complexos, ministérios, famílias, contribuições e automações.
