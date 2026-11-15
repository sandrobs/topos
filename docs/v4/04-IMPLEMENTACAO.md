# V4 — Implementação

Branch `feature/v4-aniversariantes`, cortada de `feature/ui-lote1` (V3), que por
sua vez está sobre `feature/v2-membros`. Nada publicado em produção.

**Nenhuma migração nova.** `dotnet ef migrations list` traz exatamente as mesmas
migrações de antes, como manda o critério 36.

## O que foi escrito

### API

- **`src/CrmIctm.Api/Servicos/Aniversariantes.cs`** (novo) — a lógica que tem
  armadilha, isolada dos endpoints e coberta por teste: leitura do período,
  início da semana, datas do período, a chave de mês e dia, a regra de 29 de
  fevereiro, a idade na data do aniversário e a escolha do destino da conversa.
- **`MembrosEndpoints.cs`** — dois `GET` novos, `/api/membros/aniversariantes` e
  `/api/membros/aniversariantes/contadores`, ambos atrás de
  `IgrejaAutorizadaAsync`, que não foi tocada.

### Front

- **`Aniversariantes.tsx`** (novo) — a tela.
- **`NavegacaoPainel.tsx`** — a seção entra como **segunda** linha da lista
  `secoes`, com ícone próprio e `perfis: ["ADMINISTRADOR", "PASTOR"]`; o
  componente passa a receber `aniversariosHoje` e exporta `ehSecaoDoPainel`.
- **`Interno.tsx`** — o tipo da seção deixa de ser uma união repetida e passa a
  vir de `NavegacaoPainel`; o endereço guarda qualquer seção, não só Membros;
  os contadores são buscados uma vez por igreja; `iniciais` saiu daqui para
  `formatadores.ts`, onde a tela nova também usa.
- **`api.ts`**, **`formatadores.ts`**, **`styles.css`** — tipos e chamadas,
  datas por extenso e intervalos, e o bloco de estilo da seção.

Sem dependência nova. A tela reusa `.apresentacao-painel`, `.resumo-painel`,
`.abas-membros`, `.lista-membros`, `.avatar-membro`, `.selo-membro`,
`.estado-lista` e `.vazio-membros`; só a linha é própria, porque
`.linha-membro` é um botão clicável e esta linha não é.

## O que a verificação encontrou

### O fuso horário — corrigido

O sistema deriva a data de hoje do UTC. Numa validação de "não pode ser futura"
isso é inofensivo; numa lista de aniversários do dia, não: das 21h à meia-noite
o UTC já está no dia seguinte. `Aniversariantes.Hoje` usa Brasília como
deslocamento fixo de -3 h. Não é `TimeZoneInfo` com nome de fuso porque a imagem
final é Alpine e não traz o tzdata — funcionaria aqui e lançaria exceção em
produção. Está em [01-ESCOPO.md](01-ESCOPO.md), seção 6.

### O cabeçalho entre 721 e 1024 px — corrigido

Defeito da V3, encontrado agora. Quando o cabeçalho foi refeito, a grade saiu de
`.topo-painel` para `.topo-painel > .faixa-painel`, mas a regra de 721–1024 px
continuou aplicando `grid-template-areas` ao pai, que deixou de ser grade. As
áreas nomeadas iam para os filhos sem nada que as definisse, e
`.identidade-painel` e `.acoes-topo` caíam na mesma célula, uma sobre a outra.

Ninguém via porque o seletor de igrejas era um bloco branco opaco que cobria a
identidade. Ao deixá-lo translúcido — item aprovado desta versão —, a logo e o
nome do sistema apareceram por baixo dele. Medido antes: identidade e seletor
ocupando o mesmo retângulo, de 543 a 761 px. Depois: identidade de 24 a 459,
conta de 475 a 761, seletor na linha de baixo, zero interseção.

### As abas a 375 px — corrigido

As três abas pediam 370 px nos 327 que sobram numa tela de 375. "Esta semana"
virou "Semana" e o respiro lateral das abas caiu de 17 px para 9 px no celular,
o que devolveu 54 px. Agora somam 316 px e a última termina em 338. Ficou também
uma rede de segurança: se um dia não couberem, rolam dentro da própria faixa em
vez de empurrar a página.

### A mensagem para o responsável — corrigida

O texto cumprimentava o aniversariante pelo nome, mas, no caso de menor, a
conversa abre no número do responsável legal. "Paz do Senhor, Davi!" chegando no
WhatsApp da mãe é mensagem entregue à pessoa errada. Quando o destino é o
responsável, a mensagem fala com ele e menciona o aniversariante.

### A data repetida — corrigida

Na aba Hoje a data aparecia duas vezes, no recorte e no cabeçalho do dia.

## Medições

| | |
|---|---|
| Testes | **97 passando**, 38 novos (eram 59) |
| Rolagem horizontal a 375 px | **0** |
| Alvos da barra inferior | 75 × 56 px |
| Alvos das abas | 49 px de altura |
| Botão de parabenizar no celular | 317 × 48 px |
| Ponto laranja `#d9480f` sobre o branco | **4,30:1** (mínimo 3:1) |
| Borda do seletor sobre a faixa azul | **5,50 a 5,94:1** (mínimo 3:1) |
| Texto do seletor sobre o próprio fundo | **11,29:1** |

## Verificado na aplicação em execução

Instância em `http://192.168.101.13:8082`, imagem `crm-ictm:v4-20261115`,
apontando para o mesmo banco PostgreSQL de teste da V3.

Isto importa porque os testes de integração usam o provedor em memória do EF,
que resolve `Month` e `Day` em LINQ-to-objects. **Só a instância real prova que a
consulta traduz para SQL no Npgsql** — e traduz.

Conferido: o ponto no menu com o nome acessível "Aniversariantes, 3 hoje"; a
lista de hoje com adulto, menor e adulto sem WhatsApp; o endereço gerado
(`wa.me/5541998124455?text=…`, sem o sinal de mais, com acentos preservados);
a aba do mês com os dias passados recolhidos atrás de "1 a 3 de novembro · 1
aniversário já passou"; o aviso de quem está sem data; a seção sobrevivendo a um
recarregamento pelo endereço; e o título da aba.

### O risco previsto, confirmado com dados reais

No banco de teste, **3 de 11 membros ativos têm data de nascimento**. Os outros 8
nunca apareceriam na tela, e é exatamente o que o aviso do pé da lista informa.
A medição equivalente em produção continua pendente, e está registrada em
[02-DECISOES.md](02-DECISOES.md).

### Dados fictícios inseridos no banco de teste

Para exercitar o caminho completo foram inseridos sete membros fictícios, todos
com sobrenome **"Teste V4"**. Para removê-los:

```sql
DELETE FROM crm.membros WHERE "Dados_Sobrenome" = 'Teste V4';
```

## O que continua sem verificação

- **Aparelho real.** Safari no iPhone e iPad: a barra inferior com o ponto, o
  toque nos alvos e a abertura do WhatsApp pelo aplicativo. Tudo acima foi
  medido em emulação de 375 px, que não substitui o aparelho.
- **Abrir o WhatsApp de verdade.** O endereço foi conferido como texto; nenhuma
  conversa foi aberta e nenhuma mensagem foi enviada.
- **Virada de dia às 21h.** A regra do fuso tem teste automatizado, mas não foi
  observada na aplicação em execução no horário.
- **Desempenho com muitos membros.** A igreja de teste tem 20 registros. O
  escopo diz para medir antes de concluir que há problema; não houve o que
  medir.

## Preparação para publicação — 15/11/2026

O responsável autorizou o deploy após o merge na `main`. Foi corrigida a
retenção da lista anterior ao trocar o período: a consulta agora limpa a lista
antes de carregar, inclusive quando a nova resposta falha. O teste de navegador
`tests/CrmIctm.Web.Tests/aniversariantes.cjs` cobre a falha de consulta e confere
a API e o layout em 375, 834 e 1440 px.

Imagem `crm-ictm:v4-producao-20261115`, compilada com sucesso. Os 97 testes da
API passaram. A regressão de navegador de formulários, diálogo, membros,
convites e PDFs passou em desktop, celular, celular estreito e tablet.
Nenhuma migração foi acrescentada. As pendências de procedimento interno para
contato com menores e de conferência em aparelho real continuam registradas
em `02-DECISOES.md` e neste documento.
