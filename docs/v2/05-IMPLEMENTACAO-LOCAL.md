# V2 — Implementação e validação local

## Separação do MVP

O repositório foi iniciado localmente em 25/09/2026. A tag
`mvp-visitantes-2026-09-25` registra a referência inicial `2c5dbbb`.
A V2 é desenvolvida em `feature/v2-membros`; `main` recebeu os ajustes
posteriores do convite de visitantes. Em 03/10/2026, o merge `8cbe535`
integrou esses ajustes aprovados na V2, sem publicar a V2 na VPS.

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

1. Texto e versão de adultos aprovados em 03/10/2026, conforme documento 06,
   e configurados no código e nos exemplos de ambiente. Quando explicitamente
   vazios, a consulta mostra indisponibilidade e o POST público responde 503.
   A configuração real de produção só será aplicada no deploy autorizado.
2. Definir e aprovar o procedimento para cadastro real de menores.
3. Revisar a interface, a impressão A4 e os fluxos com duas igrejas e os três
   perfis; conferir novamente os critérios de aceite da V2 e o MVP de visitantes.
4. Fazer backup verificável da base de produção antes da migração e manter um
   plano de restauração. Aplicar a migração somente após autorização expressa
   do responsável pelo produto.

## Validação inicial de 25/09/2026

- `dotnet test CrmIctm.slnx --no-restore`: 34 testes aprovados, incluindo
  isolamento do Pastor, negação à Equipe, privacidade desconfigurada,
  duplicidade, cadastro de menor e regressão dos testes do MVP.
- `npm run build`: TypeScript e Vite compilados sem erro.
- Migração EF aplicada em PostgreSQL 18 temporário com duas igrejas já
  existentes: tokens novos distintos, tokens de visitantes preservados.
- Fluxo fictício pelo navegador local: envio público pendente, visualização
  na fila e aprovação explícita; inspeção em celular, tablet e desktop.

Os testes locais não equivalem à aprovação final para publicação.

## Ajustes e validação de 03/10/2026

- Função obrigatória nas solicitações e nos membros, padrão Membro; cadastro,
  correção, aprovação e consulta preservam a função. Isso não cria usuário nem
  concede perfil de acesso.
- Migração incremental `20261003053222_FuncaoMembro`: somente acrescenta
  `Dados_Funcao` nas tabelas de membros e solicitações, com padrão MEMBRO para
  registros anteriores. Aplicada no PostgreSQL local da V2 depois de backup
  `pg_dump` em formato custom; o índice do arquivo foi conferido com `pg_restore`.
  Base, identificadores, contas e chaves existentes foram preservados.
- Formulário com a identidade de visitantes, opções marcáveis, máscaras de
  WhatsApp/CEP, preenchimento automático, alvos de toque e confirmação focada.
  Aviso de adultos aprovado, diálogo por teclado e autorização sem marcação prévia.
- Ação “Voltar à lista” nos detalhes, sem limpar pesquisa, filtros e paginação.
  Convite de membros volta para Membros na mesma igreja, inclusive em erro.
- PDFs de visitantes e membros gerados no servidor, A4 inteira ou quatro por
  A4. Os compactos possuem diagramação própria e guias de corte. Os tokens e
  destinos dos QR Codes não foram alterados.
- Backend: **55 testes aprovados**, incluindo regressão de visitantes, função,
  autorização, igreja inativa, acesso cruzado, privacidade vazia e PDFs.
  Build e publicação Release do ASP.NET Core 10 concluídos.
- Frontend: TypeScript/Vite compilados dentro de Docker/Linux; o compilador
  Windows bloqueado pelo antivírus não foi restaurado nem executado.
- Navegador Chromium: 1440, 834, 390 e 320 pixels; formulário, diálogo, convites,
  retorno correto, máscaras e ausência de rolagem horizontal ou erros JavaScript.
  Fluxo com dados fictícios: envio, revisão, aprovação, detalhe e retorno com
  pesquisa preservada. Os registros fictícios permaneceram somente na base local.
- Quatro PDFs renderizados a 150 dpi pelo Poppler e revisados visualmente.
  Todos com uma página A4; textos dentro da folha e **dez QR Codes decodificados**
  com os destinos corretos. Conferência automática em container sem rede e
  arquivos somente para leitura.

### Arquivos principais e reprodução

Backend: `Dominio/FuncoesMembro.cs`, `Dominio/DadosCadastraisMembro.cs`,
`Contratos/ContratosMembros.cs`, `Servicos/ValidacaoDadosMembro.cs`,
`Servicos/GeradorConvitesPdf.cs`, `Endpoints/MembrosEndpoints.cs`,
`Endpoints/IgrejasEndpoints.cs` e a migração/snapshot em `Infraestrutura/`.
Configuração de privacidade: `appsettings.json`, `.env.example` e
`deploy/.env.production.example` (não modifica ambientes reais).

Frontend: `CadastroMembro.tsx`, `DialogoAvisoPrivacidade.tsx`, `GestaoMembros.tsx`,
`QrCode.tsx`, `QrCodeMembros.tsx`, `Interno.tsx`, `contextoIgreja.ts`,
`App.tsx`, `api.ts` e `styles.css` em `src/CrmIctm.Web/src/`.

Testes: `CadastroMembrosTests.cs`, `ValidacaoDadosMembroTests.cs`,
`GeradorConvitesPdfTests.cs`, `tests/CrmIctm.Web.Tests/v2-ajustes.cjs` e
`tests/CrmIctm.Web.Tests/conferir-convites-pdf.py`.

- Backend: `dotnet test CrmIctm.slnx --configuration Release --no-restore`.
- Frontend: `docker build --target frontend-build -t crm-ictm:v2-frontend-20261003 .`.
- UI: fornecer `TESTE_BASE_URL` local, `TESTE_EMAIL`, `TESTE_SENHA` e Playwright
  instalado; executar `node tests/CrmIctm.Web.Tests/v2-ajustes.cjs`.
  Por padrão não cria registros; `TESTE_CRIAR_DADOS=1` habilita o fluxo fictício.
- PDF: renderizar os quatro PDFs salvos pela UI com `pdftoppm -png -r 150
  -singlefile`; executar `conferir-convites-pdf.py` com as dependências de teste
  indicadas no script. Não são dependências da aplicação.

### Ambiente e pendências

Aplicação local de avaliação: `http://localhost:8081/entrar`; mesma rede Wi-Fi:
`http://192.168.1.111:8081/entrar`. Banco e chaves separados da V1 e da produção.
Nenhum deploy ou push foi realizado por estes ajustes.

Ainda exigir avaliação do responsável pelo produto, impressão física e teste em
Safari/iPad/iPhone reais: a emulação Chromium não prova comportamento desses
dispositivos. Cores físicas dependem da impressora/papel; não prometemos CMYK.
O procedimento para dados reais de menores permanece pendente.
