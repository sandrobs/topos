# Correção visual do convite de visitantes da V1

## Decisão aprovada em 27/09/2026

Esta nota complementa a baseline do MVP sem alterar seus cinco documentos
históricos. O responsável pelo produto aprovou a combinação das prévias:

- manter cabeçalho, rodapé e quadro do QR Code em azul-marinho;
- manter a foto real da igreja na mesma região da folha, sem sombreado;
- enquadrar a parte superior da foto para evidenciar o altar e a cruz,
  aceitando o corte das pessoas em primeiro plano;
- apresentar “Ainda há lugar.” e “Lucas 14:22” em faixa branca sob a foto,
  com texto azul e referência em dourado escuro;
- preservar o formato A4, o tamanho do QR Code, sua rota e seu destino;
- validar e disponibilizar somente no ambiente local antes de autorizar
  qualquer publicação em produção.

## Implementação

`QrCode.tsx` separa a foto da chamada bíblica. Os estilos do convite em
`styles.css` usam cinco regiões na folha: cabeçalho, foto, chamada, conteúdo
com QR Code e rodapé. A foto usa `object-position: center 25%`, sem modificar
o arquivo original. Os blocos azuis têm preenchimento sólido e não recebem
sombras impressas. O endereço permanece no rodapé, inclusive na tela móvel.

Não há migração de banco, mudança de permissões ou alteração no formulário
público. O tratamento de escala do iPad já existente foi preservado; uma
simulação em Chromium não substitui um teste no Safari/AirPrint real.

## Validação local

- `npm run build` no frontend.
- `dotnet test CrmIctm.slnx --no-restore` com o SDK .NET 10.
- `tests/CrmIctm.Web.Tests/convite-a4.cjs`: testa o convite real em computador,
  celular, tablet e o tratamento existente de iPad emulado; verifica imagens,
  enquadramento, faixa clara, cores, ausência de sobreposição e corte do nome,
  retorno ao contexto da igreja e disponibilidade do formulário de visitantes.
- O teste gera PDFs de prova em `.tools/v1-convite-qa`, que devem ter uma única
  página A4, e exige revisão visual das páginas renderizadas.

O teste de interface requer Playwright. Configure `TESTE_EMAIL` e
`TESTE_SENHA` com uma conta **local**, opcionalmente `TESTE_BASE_URL` (padrão
`http://localhost:8080`), e execute:

```text
node tests/CrmIctm.Web.Tests/convite-a4.cjs
```

Se Playwright estiver fora dos módulos do projeto, informe sua localização
em `PLAYWRIGHT_MODULE_PATH`. `PLAYWRIGHT_CHANNEL=chrome` permite usar o Chrome
instalado. O teste recusa origens que não sejam loopback e não cria dados.

### Resultado em 27/09/2026

- Build do frontend concluído e 27 testes .NET aprovados, sem falhas.
- Teste de interface aprovado nos quatro tamanhos/configurações, incluindo
  as duas igrejas locais no computador, sem erros JavaScript ou recursos HTTP
  com falha.
- Cinco PDFs conferidos com Poppler: uma página A4 em cada arquivo. As páginas
  renderizadas e as telas de celular/tablet foram revisadas visualmente, sem
  cortes ou sobreposição. A simulação de iPad conserva a escala reduzida já
  existente; não confirma o comportamento de Safari/AirPrint.
- Os SVGs dos dois QR Codes conservaram exatamente seus hashes SHA-256 e
  identificadores públicos após a atualização.
- Aplicação local disponível na porta 8080, com saúde HTTP 200. Foi atualizado
  somente o contêiner da aplicação, reutilizando o backend V1; o banco local
  e o volume de chaves foram preservados. Migrações e dados iniciais foram
  desabilitados nessa atualização exclusivamente visual.
- Nenhuma publicação na VPS, alteração de dados ou incorporação da V2.

## Aprovação antes de produção

A cor percebida ainda depende do papel, da impressora e da configuração de
impressão. Fazer uma prova física na impressora utilizada pela igreja,
confirmar a leitura do QR Code pelo celular e validar Safari/AirPrint no
iPad antes de afirmar compatibilidade com esse dispositivo. Não há promessa
de conversão para CMYK nem de correção automática de perfis de impressão.

## Publicação aprovada em produção

O responsável pelo produto confirmou o teste local e autorizou a publicação
em 27/09/2026. A release do código `a956e29` foi publicada na VPS Locaweb às
14:49 UTC dessa data (11:49 no horário de Brasília).

- Imagem ativa: `crm-ictm:v1-convite-a956e29`.
- Imagem anterior preservada: `crm-ictm:v1-antes-convite-a956e29`.
- Pasta da release: `/opt/topos/releases/v1-convite-a956e29`.
- Backup prévio: `/opt/topos/backups/crm-ictm-20260927T144852Z.dump`, com
  permissão `600` e conteúdo reconhecido por `pg_restore --list`. Isso não
  equivale a uma nova restauração completa de prova ou a uma cópia externa.
- Atualizados somente os arquivos estáticos, sobre a imagem do backend que
  já estava em execução. Os assets anteriores foram mantidos na imagem para
  não invalidar abas abertas. O hash da DLL da API permaneceu idêntico.
- Banco e proxy conservaram os mesmos contêineres; ambiente, portas e volumes
  da aplicação foram comparados antes/depois e permaneceram iguais.
- Nenhuma migração aplicada, conforme log da API; identificadores das três
  igrejas e contagens de registros preservados. Nenhum visitante de teste
  foi criado durante as verificações.
- Saúde HTTPS `200`; arquivos publicados conferidos por SHA-256 contra o
  build aprovado; conteúdo das três APIs públicas de igreja preservado.
- Telas de login e formulário público renderizadas e revisadas em computador,
  celular e tablet, sem erros JavaScript, recursos com falha ou rolagem
  horizontal. A gestão permanece bloqueada no domínio de visitantes e a
  API privada exige autenticação.

O teste de autenticação com a senha inicial ainda registrada no ambiente
retornou `401` **antes** da troca de imagem. Essa senha não foi reutilizada
em novas tentativas nem houve redefinição de conta. A autenticação com a
senha atual e a abertura autenticada do convite em produção precisam de
confirmação do responsável pelo produto; não foram declaradas validadas
automaticamente. O layout e os PDFs do convite foram validados localmente,
e os arquivos de produção são idênticos aos aprovados nesse teste.

Convite da igreja de Três de Maio em produção:
`https://toposgestao.cnic1932.com.br/qrcode/71d355c1-b1a3-4697-88e1-4ec78f079a0c`.

### Reversão da atualização visual

Somente se for necessária uma reversão aprovada, executar na VPS:

```bash
cd /opt/topos
cp -p releases/v1-convite-a956e29/.env.production.antes deploy/.env.production
docker compose -p topos --env-file deploy/.env.production \
  -f compose.yaml -f deploy/compose.production.yaml \
  up -d --no-build --no-deps aplicacao
curl -fsS https://toposgestao.cnic1932.com.br/api/saude
```

A reversão utiliza a configuração anterior e a imagem original ainda
preservada. Não restaura nem recria o banco, pois esta release não alterou
seu esquema ou dados. A V2 permanece separada e não foi publicada.
