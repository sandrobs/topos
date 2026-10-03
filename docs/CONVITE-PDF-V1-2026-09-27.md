# Convite A4 gerado no servidor — correção V1

## Decisão do responsável pelo produto

Em 27/09/2026 foi solicitada a substituição da impressão do HTML por um
PDF montado por um motor independente do navegador. A mesma igreja deve
receber uma página A4 com o mesmo layout em computador, celular e tablet.
O endereço completo passa para uma linha própria no rodapé; o CEP não deve
ficar dividido. Esta entrega é exclusivamente local, aguardando nova
aprovação para produção. A publicação anterior permanece sem alterações.

## Implementação

- PDFsharp Core 6.2.4, biblioteca .NET com [licença MIT](https://docs.pdfsharp.net/General/License/License.html),
  sem serviço externo, navegador headless ou alteração na infraestrutura.
- Fonte Liberation Sans e Liberation Serif incluídas como recursos, com
  licença SIL OFL 1.1 da [distribuição oficial 2.1.5](https://github.com/liberationfonts/liberation-fonts/releases/tag/2.1.5).
  Fontes e imagens são incorporadas ao artefato da API; não dependem das fontes
  instaladas no Windows ou no Ubuntu.
- `GET /api/igrejas/{id}/convite.pdf` autenticado, com o mesmo isolamento por
  igreja do QR existente, igreja ativa e limite de dez pedidos/minuto por IP.
  A resposta é `application/pdf`, sem cache, com nome seguro e abertura inline.
- O link **Abrir PDF A4** substitui `window.print()`. O usuário imprime ou salva
  o arquivo no leitor de PDF. A página HTML continua sendo uma prévia responsiva,
  não a fonte da paginação final; os ajustes específicos de escala do iPad foram
  removidos do fluxo.
- A4 retrato, uma página, QR vetorial e clicável, mesma URL do QR SVG atual.
  Mantidos os blocos azuis, foto sem sombreado, altar/cruz, chamada e Lucas 14:22.
- Endereço em bloco próprio, ocupando a largura útil do rodapé. Endereços
  excepcionalmente longos podem ocupar mais linhas dentro desse bloco; não
  ficam recortados nem dividem o CEP. O rodapé do PDF cresce para acomodá-los.
- Nenhuma migração, alteração de cadastro, regra de visitante ou permissão.

## Validação local concluída

- Build TypeScript/Vite e publish .NET em Release sem erros.
- 32 testes .NET aprovados, incluindo acesso anônimo, isolamento de Pastor/Equipe,
  igreja inativa/inexistente, destino permanente e campos no limite máximo.
- Cinco cenários Playwright: duas igrejas no computador, Três de Maio em celular,
  tablet e iPad emulado. Abertura real do link em outra aba, resposta PDF autenticada,
  prévia sem sobreposição/rolagem horizontal e retorno à igreja selecionada.
  Sem erros JavaScript ou recursos com falha.
- Seis PDFs renderizados com Poppler, incluindo uma prova sintética com campos
  nos limites máximos, sem alterar cadastros do banco. Uma página de 210 × 297 mm,
  fontes incorporadas, textos dentro da folha e QR lido e comparado com o link do PDF.
- Para a mesma igreja, imagens renderizadas idênticas nos quatro cenários de
  dispositivo, além das mesmas coordenadas dos textos. Revisão visual seguindo
  a skill de PDF; endereço em bloco próprio e CEP inteiro.
- Aplicação local em Docker/Linux, com o banco existente e suas configurações
  preservados. Nenhuma migração ou inicialização de dados foi executada.

O teste de navegador está em `tests/CrmIctm.Web.Tests/convite-a4.cjs` e aceita
`TESTE_EMAIL`, `TESTE_SENHA`, `PLAYWRIGHT_MODULE_PATH` e `PLAYWRIGHT_CHANNEL`.
Ele recusa endereços de produção e não cria registros.

## Publicação futura e validação física

Esta correção exige publicar **backend e frontend juntos**: a imagem anterior
não possui o endpoint PDF. Não basta substituir somente os arquivos do React.
O Dockerfile já inclui os recursos necessários ao gerador. Publicação na VPS
depende de aprovação após os testes locais; não foi realizada nesta entrega.

O PDF garante a geometria do documento, não a fidelidade de cor de cada
impressora. A configuração do leitor/impressora (A4, tamanho real ou ajuste
à área imprimível), o papel e a prova física continuam relevantes. O acesso
e a impressão no Safari/iOS real precisam da confirmação do usuário.
