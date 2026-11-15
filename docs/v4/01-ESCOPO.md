# V4 — Escopo

## 1. O que já existe no cadastro

Levantado por leitura do código em 15/11/2026. Os fatos abaixo são o que tornam
esta versão pequena.

**Data de nascimento existe e é opcional.**
`DadosCadastraisMembro.DataNascimento` é `DateOnly?`
(`src/CrmIctm.Api/Dominio/DadosCadastraisMembro.cs`). `ValidacaoDadosMembro` só
a valida como "não pode ser futura" e, no formulário público, como "tem de ser
de adulto" — **não é campo obrigatório em nenhum caminho**. Essa é a maior
fragilidade da funcionalidade; ver a seção 8.

**O WhatsApp já está pronto para virar link.** `NormalizadorWhatsapp` grava
sempre no formato `+55DDNNNNNNNNN` (`varchar(14)`), rejeitando DDD inválido e
número que não tenha 10 ou 11 dígitos. O endereço do WhatsApp sai do banco sem
nenhum tratamento: basta remover o sinal de mais.

**Para adulto o WhatsApp é obrigatório.** `ValidacaoDadosMembro` só dispensa o
campo quando `EhMenor` é verdadeiro. Ou seja: todo membro adulto com data de
nascimento preenchida terá o botão funcionando.

**Para menor, o contato do responsável é obrigatório.** Quando `EhMenor`,
`NomeResponsavelLegal`, `VinculoResponsavelLegal` e `WhatsappResponsavelLegal`
são exigidos, e o WhatsApp do próprio menor fica opcional. O formulário público
recusa menores, então todo menor no cadastro entrou pela equipe da igreja.

**Permissão e igreja já estão resolvidas.** `IgrejaAutorizadaAsync`, em
`MembrosEndpoints.cs`, nega o perfil `Equipe` em todos os endpoints de membros,
exige `igrejaId` e impede que um Pastor leia igreja que não é a dele. A seção
nova reusa essa função sem alterá-la.

**Registrar saudação, no dia em que fizer falta, não pedirá tabela nova.**
`HistoricoMembro` é genérico: `Acao`, `Descricao`, `MembroId`, `AutorId`,
`CriadoEm`. Um código de ação novo bastaria. Está fora do escopo desta versão,
mas o caminho fica anotado.

**O aviso de privacidade já cobre o uso.** A versão `membros-2026-10-03` informa
ao membro que os dados, nascimento incluído, serão usados para "manter o cadastro
de membros e realizar contato e acompanhamento pastoral", e que o acesso é
restrito aos pastores autorizados da igreja e aos administradores da plataforma.
Parabenizar um aniversariante cabe nessa finalidade, e por isso a V4 **não**
exige nova versão do aviso — desde que o acesso continue restrito a esses dois
perfis, o que a decisão D1 garante.

## 2. A tela

Rota interna nova, seção `ANIVERSARIANTES`, seguindo o padrão de apresentação da
V3 sem inventar nada:

- `.apresentacao-painel` com rótulo, título e descrição, e `.lado-painel` à
  direita com `.resumo-painel` de três números: **hoje**, **semana** e **mês**.
- Seletor de período no padrão `.abas-membros`, com `role="tablist"`,
  `aria-selected` e a contagem em cada aba: **Hoje**, **Esta semana** e o mês
  corrente pelo nome. Aba inicial: Hoje.
- Lista agrupada por dia. Cada linha traz avatar de iniciais, nome completo, a
  idade que a pessoa completa naquela data, o telefone, a função na igreja e o
  botão de parabenizar.
- Quando a aba Hoje estiver vazia, o estado vazio não é um "nenhum resultado"
  seco: diz que ninguém faz aniversário hoje e aponta os próximos.
- Na aba do mês, os dias já passados ficam recolhidos atrás de uma linha que
  informa quantos foram e expande ao toque.
- No pé da lista, quando houver membro ativo sem data de nascimento, um aviso diz
  quantos são e leva ao cadastro de membros. Ver a seção 8.

A tela é só de leitura. Não edita cadastro.

## 3. O menu

**Aniversariantes entra em segundo lugar**, no computador e no celular:
Atendimentos, Aniversariantes, Membros, Igrejas, Usuários.

`NavegacaoPainel.tsx` já trata as seções como dados, então a mudança é
acrescentar uma linha na lista `secoes` na posição 2, com `rotulo`
"Aniversariantes", `curto` "Aniver.", ícone em SVG em linha e
`perfis: ["ADMINISTRADOR", "PASTOR"]`.

Com isso a barra chega a **cinco posições, o limite acordado**. A sexta seção
obriga o "Mais" descrito no próprio componente.

### O sinal de aniversário hoje

Quando houver aniversariante no dia, o item Aniversariantes do menu recebe um
**ponto laranja**, em qualquer tela em que a pessoa esteja — é o que chama o
pastor para a seção quando ele entra na plataforma.

Dois requisitos:

- **Cor não pode ser o único sinal.** A contagem entra no nome acessível do
  botão: `aria-label="Aniversariantes, 2 hoje"`. Quem usa leitor de tela recebe
  o mesmo recado.
- **O ponto precisa de 3:1 contra o fundo** (WCAG 1.4.11). O protótipo usa
  `#d9480f`, que mede 4,3:1 sobre o branco da faixa do menu. Na barra inferior
  do celular ele ganha um anel branco para se separar do ícone.

## 4. O endpoint

Dois endpoints novos em `MembrosEndpoints.cs`, ambos atrás de
`IgrejaAutorizadaAsync`, ambos somente leitura:

```
GET /api/membros/aniversariantes?igrejaId={guid}&periodo=HOJE|SEMANA|MES
GET /api/membros/aniversariantes/contadores?igrejaId={guid}
```

O primeiro devolve a lista do período pedido. O segundo devolve
`{ hoje, semana, mes, semDataNascimento }` e existe por um motivo específico: o
ponto do menu precisa da contagem de hoje em **todas** as telas, não só na de
aniversariantes. `Interno.tsx` o chama uma vez por igreja selecionada.

Cada item da lista carrega `id`, `nome`, `sobrenome`, `funcao`, `ehMenor`,
`dataNascimento`, `dataOcorrencia`, `idadeQueCompleta`, `destino`,
`whatsappDestino`, `nomeResponsavel` e `vinculoResponsavel`.

**O servidor é quem decide o destino do WhatsApp** — ver a decisão D4 —, para
que a regra de menores não fique espalhada pelo front. Ele devolve o número e um
código (`MEMBRO`, `RESPONSAVEL` ou `NENHUM`); o texto do botão é montado na
tela, porque é redação de interface e não regra de negócio.

`dataOcorrencia` é o dia em que o aniversário cai dentro do período, e não o dia
do nascimento. É por ela que a lista é agrupada e ordenada, e é o que faz a
semana que vira o ano sair na ordem certa.

## 5. A mensagem e o link

Texto fixo nesta versão, com três variáveis:

> Paz do Senhor, **{primeiro nome}**! Hoje a **{igreja}** celebra com você. Que
> Deus te dê saúde, paz e alegria neste novo ano de vida. Feliz aniversário! —
> **{nome de quem está usando o sistema}**

A assinatura é o nome do usuário da sessão, **sem título**: o sistema não sabe
tratar por "Pastor" quem está logado como Administrador.

O botão é um elemento de link de verdade, com `target="_blank"` e
`rel="noopener noreferrer"`, apontando para
`https://wa.me/55DDNNNNNNNNN?text=` seguido do texto passado por
`encodeURIComponent`.

No celular isso abre o aplicativo. No computador abre o WhatsApp Web ou o
aplicativo de desktop, que dependem de telefone pareado — é limitação do
WhatsApp, não do sistema, e a tela não tenta contornar.

**O sistema não envia nada.** Ele abre a conversa com o texto escrito; quem toca
em enviar é a pessoa.

Quando não houver destino — adulto sem WhatsApp, raro mas possível em cadastro
feito pela equipe —, a linha continua na lista com o botão desativado e o motivo
escrito. Não desaparece.

## 6. A consulta

Feita no banco, não em memória. Dois cuidados que não são óbvios:

**Semana e mês atravessam fronteiras.** A semana de 29 de novembro a 5 de
dezembro cruza o mês; a última semana de dezembro cruza o ano. Por isso o filtro
de semana **não** é um intervalo de datas: é o conjunto de pares (mês, dia) dos
sete dias, comparado com mês e dia da data de nascimento.

**29 de fevereiro.** Em ano comum, a consulta de 28 de fevereiro inclui também o
dia 29 (decisão D5).

**O fuso.** Achado durante a implementação, não estava neste escopo: o resto do
sistema deriva a data de hoje do UTC (`DateOnly.FromDateTime(relogio.Agora.UtcDateTime)`).
Isso é inofensivo numa validação de "não pode ser futura", mas não aqui — das
21h à meia-noite o UTC já está no dia seguinte, e o pastor veria a lista de
amanhã justamente no horário em que a igreja se reúne.

A data de hoje desta tela usa Brasília como deslocamento fixo de -3 h, e **não**
`TimeZoneInfo` com "America/Sao_Paulo": a imagem final é Alpine, que não traz o
tzdata, então a busca por nome funcionaria na máquina do desenvolvedor e
lançaria exceção em produção. O Brasil não adota horário de verão desde 2019,
então -3 h é exato. Se voltar a adotar, o ponto único a mudar é
`Aniversariantes.Hoje`, e será preciso instalar o tzdata na imagem.

**Índice: nenhum.** Um índice em `Dados_DataNascimento` não serve para consulta
por mês e dia, e um índice funcional seria migração, que esta versão não faz. A
coluna `IgrejaId` já é indexada e a varredura fica restrita aos membros daquela
igreja. Meça antes de concluir que há problema: na ordem de grandeza de alguns
milhares de membros por igreja, não há.

## 7. Ajustes visuais aprovados junto

Dois itens aprovados no protótipo que não são da funcionalidade.

**O seletor de igrejas.** Hoje `.gatilho-seletor-igreja` é um bloco branco opaco
com sombra de 18 px sobre a faixa azul — ele salta. Passa a ser translúcido, com
texto branco, borda clara fina e sem sombra, recuando para dentro da faixa.
**O componente não muda:** continua o popover próprio, com `aria-haspopup`,
fechamento por Escape e por clique fora. Muda só a aparência do gatilho.

Um cuidado: a borda precisa medir 3:1 contra a faixa. O protótipo usa branco a
30%, que provavelmente **não** atinge esse valor sobre o azul. Suba a opacidade
até medir, em vez de copiar o número do protótipo.

**A logo continua no cabeçalho.** `<LogoIgreja />` permanece onde está, com o
mesmo arquivo. Nenhuma versão desta série substitui a logo da igreja por sigla,
inicial ou marca genérica.

## 8. O risco que precisa de atenção

A data de nascimento é opcional no cadastro. Quem está registrado sem ela
**nunca** aparece nesta tela, e ninguém percebe: a lista simplesmente fica mais
curta, sem erro, sem aviso, sem rastro. É uma falha silenciosa, e é a razão de o
contador `semDataNascimento` fazer parte do endpoint e de o aviso aparecer no pé
da lista.

Antes de investir na tela, vale medir em produção quantos membros ativos estão
sem a data. Se forem muitos, a conversa deixa de ser sobre esta tela e passa a
ser sobre tornar o campo obrigatório — o que mexeria na V2 e, por isso, não é
decisão do implementador.

## 9. Fora de escopo

Registrar que já parabenizou; texto configurável por igreja; envio automático;
aniversário de casamento; aniversário de batismo ou de ingresso; lembrete por
e-mail ou notificação; exportação; aniversariantes de várias igrejas numa só
tela.
