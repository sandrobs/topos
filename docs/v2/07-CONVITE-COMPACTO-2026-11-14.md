# Convite compacto de visitantes — ajustes de 14/11/2026

## Aprovação e escopo

O responsável pelo produto forneceu `Proposta_convite.pdf` e confirmou que os
ajustes são exclusivos do **PDF de visitantes com quatro convites por folha A4**.
A foto mantém posição, altura e enquadramento do modelo anterior. O PDF A4 de
página inteira, os convites de membros e os formulários mantêm seus modelos.

Alterações aprovadas:

- Aproximar o apoio de “Vamos manter contato?”, com texto mais escuro.
- Acrescentar “Dados usados apenas para contato da igreja.”
- Trocar a chamada do QR para “Aponte a câmera do celular”, em duas linhas.
- Reduzir e baixar o rodapé, ocupando-o com culto principal, endereço sem CEP
  e Instagram. O CEP permanece no cadastro da igreja e nos outros modelos.
- Manter geração no servidor, margens, guias de corte e destinos permanentes
  dos QR Codes.

## Configurações por igreja

O responsável confirmou que culto principal e Instagram devem ser armazenados
na configuração da igreja, em vez de fixados globalmente no modelo.

Dois campos opcionais, editados pelo Administrador na gestão de igrejas:

- **Culto principal:** texto de até 120 caracteres, incluindo nome, dia e horário.
  Exemplo aprovado: `CULTO DA FAMÍLIA · Domingo, 19h`.
- **Instagram:** nome de usuário, com ou sem `@`; salvo e impresso com `@`.
  Aceita letras, números, ponto e sublinhado, até 99 caracteres sem o prefixo.
  Exemplo aprovado: `@igrejadecristotm`.

Campos vazios não imprimem informações substitutas. Os dados de uma igreja não
são reutilizados no convite de outra. As permissões de gestão de igrejas e o
registro de auditoria existentes são preservados.

A migração incremental `ConfiguracaoConviteIgreja` apenas acrescenta duas
colunas anuláveis à tabela de igrejas, sem preencher valores, alterar tokens
ou recriar registros. A aprovação desta mudança não autoriza publicação na VPS.

## Implementação e validação local

- Migração `20261114131730_ConfiguracaoConviteIgreja` gerada por EF Core,
  revisada e aplicada somente no PostgreSQL isolado da V2 após backup custom
  e conferência do índice com `pg_restore`. Identificadores, tokens e contagens
  de igrejas, atendimentos, membros, solicitações e usuários foram comparados.
- Inclusão e edição pela API com os dois campos, listagem para o formulário
  administrativo e auditoria existente. Campos em branco são salvos como nulos.
- Os exemplos aprovados foram preenchidos somente em “Igreja de Teste V2”;
  as demais igrejas de teste permaneceram sem alterações.
- Build completo em Docker/Linux e **59 testes aprovados**, incluindo criação,
  edição e remoção das configurações, isolamento por igreja, rejeição de
  Instagram inválido e geração de PDF com campos nos limites máximos.
- Gestão de igrejas conferida em Chromium a 1440 e 390 pixels: os dois campos
  aparecem preenchidos, são opcionais e o formulário cabe na tela, sem erros JS.
- Quatro PDFs renderizados a 150 dpi: página única A4, textos dentro da folha
  e dez QR Codes decodificados para os respectivos destinos permanentes.
- Comparação de pixels confirmou as quatro fotos idênticas ao modelo anterior.
  Também confirmou renderização idêntica dos outros três PDFs: visitantes em
  página inteira e os dois formatos de membros.

Ambiente local atualizado com `crm-ictm:v2-convite-compacto-20261114`, em
`http://localhost:8081/entrar` ou `http://192.168.101.13:8081/entrar` na rede
Wi-Fi atual. O ambiente da VPS não foi alterado.

A conferência de PDFs existente aceita uma pasta alternativa como argumento:
`python tests/CrmIctm.Web.Tests/conferir-convites-pdf.py PASTA_DOS_PDFS_E_PNGS`.

## Ajuste posterior — recorte circular da logo

Em nova solicitação de 14/11/2026, o responsável pediu o recorte circular da
logo nos PDFs para eliminar os cantos quadrados visíveis. O modelo compacto
passou a usar o mesmo recorte vetorial circular já aplicado ao modelo de página
inteira. O método compartilhado restaura a região de desenho após a logo.
Aplica-se aos convites de visitantes e membros, preservando textos, foto, QR,
posição e tamanho da logo. O arquivo de imagem original não foi alterado.

Build em Docker/Linux e **59 testes aprovados**. Os quatro PDFs foram gerados
no ambiente local, renderizados a 150 dpi e conferidos: dez QR Codes válidos e
textos dentro da página A4. Amostragem de pixels confirmou que os cantos têm a
cor do cabeçalho; a comparação com a versão anterior limitou as diferenças às
áreas das logos nos compactos. Os PDFs de página inteira permaneceram idênticos
na renderização. Ambiente de testes atualizado para
`crm-ictm:v2-logo-redonda-20261114`, preservando o banco e as configurações.
