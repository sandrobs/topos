# V2 — Implementação e validação local

## Separação do MVP

O repositório foi iniciado localmente em 25/09/2026. `main` e a tag
`mvp-visitantes-2026-09-25` apontam para o commit `2c5dbbb`, referência
estável do MVP. A V2 é desenvolvida em `feature/v2-membros`, para que um
ajuste urgente no convite de visitantes possa partir da referência estável.
Ainda não existe remoto Git configurado; esse histórico está somente no
computador de desenvolvimento.

## Núcleo implementado localmente

- Token, rota e QR Code de membros próprios por igreja, sem alterar o QR de visitantes.
- Formulário público para adultos; o envio cria solicitação pendente e não
  cria membro ativo automaticamente.
- Fila de análise e correção interna, aprovação, recusa e vínculo a membro
  existente quando houver possível duplicidade por WhatsApp.
- Inclusão manual, edição e inativação de membros por Pastor ou Administrador.
- Cadastro interno de menores com responsável legal e WhatsApp dele;
  WhatsApp do menor é opcional. Não usar dados reais de menores antes da
  aprovação do procedimento e texto específicos.
- Pesquisa e filtro no servidor, paginação de 20 registros, contadores e
  interface responsiva.
- Migração EF `CadastroMembros`: preserva identificadores dos visitantes e
  atribui token aleatório de membros a cada igreja já cadastrada.

## Portões de publicação

1. Aprovar versão e texto de `AVISO_PRIVACIDADE_MEMBROS_*`. Vazios por padrão:
   a consulta mostra a indisponibilidade e o POST público responde 503.
2. Definir e aprovar o procedimento para cadastro real de menores.
3. Revisar a interface, a impressão A4 e os fluxos com duas igrejas e os três
   perfis; conferir novamente os critérios de aceite da V2 e o MVP de visitantes.
4. Fazer backup verificável da base de produção antes da migração e manter um
   plano de restauração. Aplicar a migração somente após autorização expressa
   do responsável pelo produto.

## Testes locais já executados

- `dotnet test CrmIctm.slnx --no-restore`: 34 testes aprovados, incluindo
  isolamento do Pastor, negação à Equipe, privacidade desconfigurada,
  duplicidade, cadastro de menor e regressão dos testes do MVP.
- `npm run build`: TypeScript e Vite compilados sem erro.
- Migração EF aplicada em PostgreSQL 18 temporário com duas igrejas já
  existentes: tokens novos distintos, tokens de visitantes preservados.
- Fluxo fictício pelo navegador local: envio público pendente, visualização
  na fila e aprovação explícita; inspeção em celular, tablet e desktop.

Os testes locais não equivalem à aprovação final para publicação.
