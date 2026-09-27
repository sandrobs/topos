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
