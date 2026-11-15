# V4 — Aniversariantes

**Estado:** escopo e protótipo aprovados pelo responsável pelo produto em
15/11/2026, e **implementados no mesmo dia** na branch
`feature/v4-aniversariantes`. 97 testes passando, nenhuma migração nova,
verificado na instância de teste. Nada publicado em produção, e nada conferido
em aparelho real. Ver [04-IMPLEMENTACAO.md](04-IMPLEMENTACAO.md).

## Objetivo

Dar ao pastor uma tela que responda a uma pergunta que hoje não tem resposta no
sistema: quem faz aniversário hoje, nesta semana e neste mês. E, a partir dela,
um caminho de um toque para abrir a conversa no WhatsApp com a mensagem de
parabéns já escrita.

A V4 se apoia inteiramente no cadastro de membros da V2. Não cria domínio novo,
não cria entidade nova e **não faz migração no banco**.

## Ordem de leitura para desenvolvimento

1. [Escopo](01-ESCOPO.md) — o que existe no cadastro, a tela, o menu, o endpoint
   e a consulta
2. [Decisões](02-DECISOES.md) — as oito decisões já tomadas, com data, e o que
   continua pendente
3. [Critérios de aceite](03-CRITERIOS-DE-ACEITE.md)
4. [Implementação e verificação](04-IMPLEMENTACAO.md) — o que foi escrito, o
   que a verificação encontrou e o que continua sem conferência

## Regra de precedência

A V4 acrescenta uma seção de **leitura** sobre o cadastro de membros. Ela não
altera campos, validações de domínio, permissões, o fluxo de aprovação pastoral
nem os contratos existentes da V2, e não toca em nada do fluxo de visitantes. Em
qualquer conflito aparente, o comportamento descrito em `docs/` (MVP) e
`docs/v2/` prevalece e a V4 cede.

A V4 também cede para a V3 em apresentação: os padrões visuais e de
acessibilidade descritos em `docs/v3/` valem para a tela nova, que não inventa
padrão próprio.

## A restrição que molda esta versão

**Nenhuma migração no banco.** A decisão está registrada como D3 e tem
consequência direta: o texto da mensagem de parabéns fica fixo no front, com
variáveis, em vez de configurável por igreja. Se durante a implementação
aparecer qualquer necessidade de coluna nova, a entrega para e volta ao
responsável — não se resolve inventando uma migração.

Isso é possível porque `DadosCadastraisMembro` já guarda data de nascimento e
WhatsApp normalizado. O levantamento está em [01-ESCOPO.md](01-ESCOPO.md).

## O que a V4 não faz

- **Não envia mensagem.** Abre o WhatsApp com o texto pronto; quem toca em
  enviar é a pessoa.
- **Não registra** que alguém já foi parabenizado.
- **Não configura** o texto por igreja.
- **Não notifica** por e-mail, push ou agendamento.
- **Não gera relatório** nem exportação.

Cada um desses itens é uma camada posterior, não um esquecimento.

## Pré-requisito que continua aberto

O procedimento interno de contato com menores de idade segue listado como
**pendente** em [docs/04-PRIVACIDADE-SEGURANCA-RISCOS.md](../04-PRIVACIDADE-SEGURANCA-RISCOS.md).
A decisão D4 se apoia nele. Ver [02-DECISOES.md](02-DECISOES.md).
