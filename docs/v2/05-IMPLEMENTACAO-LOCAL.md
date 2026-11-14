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

## Logo sem localidade — 14/11/2026

A pedido do responsável pelo produto, foi removida a inscrição “Três de Maio - RS”
de dentro da logo, a partir do arquivo fornecido. A versão editada com a ferramenta
de imagens integrada está em `src/CrmIctm.Web/public/logo-igreja.png` e substitui
o JPG no componente compartilhado, no recurso incorporado dos PDFs, no Dockerfile
e na rota de arquivos públicos do Caddy. Os dados de cidade/endereço da igreja
continuam sendo apresentados nos locais próprios dos formulários e convites.

A recompilação identificou bloqueio da auditoria NuGet na dependência transitiva
`System.Security.Cryptography.Pkcs` 8.0.1, trazida pelo PDFsharp. Foi adicionada
referência explícita à correção 8.0.2, mantendo a auditoria e os avisos como erros.
Referências: [aviso Microsoft GHSA-2fmv-rv2c-59x7](https://github.com/advisories/GHSA-2fmv-rv2c-59x7)
e [aviso Microsoft GHSA-846f-f764-76mx](https://github.com/advisories/GHSA-846f-f764-76mx).

Validação: build completo de frontend e backend em Docker/Linux e **55 testes
aprovados**. No ambiente de testes, foram conferidos saúde, login, consulta da
gestão de membros e igualdade SHA-256 entre a logo servida e o arquivo do projeto.
Os quatro PDFs foram renderizados a 150 dpi e revisados visualmente; todos mantêm
uma página A4, a nova logo incorporada com os mesmos pixels do PNG e os dez QR
Codes decodificados para os destinos esperados.

O contêiner local da V2 usa a imagem `crm-ictm:v2-logo-sem-localidade-20261114`,
com aplicação em `http://localhost:8081/entrar` e, na rede Wi-Fi atual,
`http://192.168.101.13:8081/entrar`. Banco, chaves e migrações foram preservados.
Esta atualização foi aplicada somente no ambiente local de testes.

## Lote 1 de UI/UX — 14/11/2026

Primeiro lote da revisão de layout e experiência, feito na branch
`feature/ui-lote1`. São correções de apresentação e acessibilidade: nenhum campo,
validação, permissão, regra de negócio ou conteúdo do QR Code e dos PDFs foi
alterado, e o fluxo de visitantes foi preservado.

### Peso dos recursos públicos

O original de 1254×1254 px da logo saiu de `src/CrmIctm.Web/public/` e passou a
viver em `src/CrmIctm.Api/Recursos/Convite/logo-igreja.png`, onde já ficam as
fontes dos convites; o glob `Recursos/Convite/*` do `.csproj` continua a
incorporá-lo com o mesmo nome lógico `Convite.logo-igreja.png`, então os PDFs
seguem usando a imagem em resolução plena. A `EmbeddedResource` explícita e a
cópia correspondente no `Dockerfile` foram removidas por redundância.

Em `public/` ficou uma versão de 192×192 px gerada do mesmo original, que serve
o maior render de tela (20 mm do cartaz) com folga de densidade: **41,5 kB no
lugar de 1.385 kB**. O caminho público continua sendo `/logo-igreja.png`, sem
`srcset` e sem rota nova, para não mexer na lista do Caddy. Em consequência, a
igualdade SHA-256 entre a logo servida e o arquivo incorporado, registrada na
seção anterior, deixa de valer por desenho: são agora o original e um derivado.

Também foi removida `public/imagem-louvor-convite.png` (1,8 MB), que não era
referenciada por nenhum código, CSS, template ou recurso incorporado.

Resultado medido no formulário público: a página passou de 1.826 kB para
**492 kB** em quatro recursos. O `<img>` da logo ganhou `width`/`height`
explícitos.

### Acessibilidade

O indicador de foco usava `rgba(46, 95, 202, 0.3)`, cerca de 1,5:1 sobre branco,
abaixo do mínimo de 3:1 do critério WCAG 2.2 1.4.11. Foi criado o token `--foco`
(opaco, `var(--azul-600)`) e aplicado às sete regras que tratavam de foco. Duas
delas (`.login input:focus`, `.barra-pesquisa input:focus`) e a do formulário de
membros anulavam o anel com `outline: 0`; a regra do membro ainda usava `:focus`
em vez de `:focus-visible`, disparando também no clique de mouse. Medições na
aplicação: **5,82:1** sobre branco, **5,38:1** sobre `--fundo` e **3,01:1** sobre
a faixa `--azul-950`.

O endereço no rodapé público usava `#8490a2` a 0,7 rem — 3,23:1, abaixo dos 4,5:1
do critério 1.4.3. Passou para `var(--texto-suave)` a 0,76 rem: **4,59:1**.

### Clareza do que é apresentado

O marcador de `.indicador-selecao` era circular numa pergunta de múltipla
escolha, enquanto o consentimento, logo abaixo, usava quadrado arredondado.
Os dois agora usam `border-radius: 7px`; os marcadores de escolha única do
formulário de membros seguem circulares.

A caixa de consentimento ficava centralizada na vertical ao lado de um parágrafo
de várias linhas. `align-items` passou de `center` para `start`, como já fazia
`.aviso-membro label`.

Na gestão de membros, datas ISO e enums apareciam crus: `2024-01-15`, `ATIVO`,
`APROVACAO SOLICITACAO`. Foram acrescentados a `formatadores.ts` as funções
`formatarDataIso`, `formatarDataHora`, `rotuloSituacaoMembro` e
`rotuloAcaoHistorico`. `formatarDataIso` não usa `new Date`: a string sem hora
seria lida como UTC e exibiria o dia anterior em fusos negativos. O
`formatarData` duplicado em `Interno.tsx` passou a reusar `formatarDataHora`.

### Validação

Build completo de frontend e backend em Docker/Linux e **59 testes aprovados**.
`GeradorConvitesPdfTests` exercita a leitura do recurso incorporado, então a
mudança de local da logo do PDF está coberta por teste.

Verificação na aplicação em execução, com a imagem `crm-ictm:ui-lote1-20261114`
subida em `http://localhost:8082` contra o mesmo banco de testes, sem tocar na
instância 8081: formulários públicos de visitantes e de membros a 375 px e
1024 px, sem rolagem horizontal; anel de foco conferido por Tab real no
formulário, no campo de pesquisa do painel e no cartão do kanban; datas e
histórico conferidos contra os valores brutos do banco, sem deslocamento de dia.

Pendente de verificação física: impressão dos PDFs e teste em Safari/iPad/iPhone
reais. A emulação Chromium não prova o comportamento desses dispositivos.
