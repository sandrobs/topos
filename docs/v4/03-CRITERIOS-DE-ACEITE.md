# V4 — Critérios de aceite

Cada item é verificável. Onde o critério é de medição, o número tem de ser
medido, não estimado.

## Acesso e isolamento

1. Um usuário com perfil **Equipe** não vê a entrada Aniversariantes no menu, e
   uma chamada direta aos dois endpoints novos responde 403.
2. Um **Pastor** só obtém aniversariantes da sua igreja; pedir outra responde
   403.
3. Um **Administrador** obtém a igreja que estiver selecionada no seletor.
4. Chamada sem `igrejaId` responde 400, como os demais endpoints de membros.

## Períodos

5. A aba **Hoje** lista exatamente os membros ativos cujo dia e mês de
   nascimento são os de hoje.
6. A aba **Esta semana** lista de domingo a sábado da semana corrente. Numa
   semana que atravessa a virada de mês, os dois meses aparecem. Numa semana que
   atravessa a virada de ano, os dois anos aparecem.
7. A aba do **mês** lista o mês corrente inteiro, agrupado por dia, em ordem
   crescente de dia e, dentro do dia, por nome.
8. Em ano comum, um membro nascido em 29 de fevereiro aparece na lista de 28 de
   fevereiro. Em ano bissexto, aparece no dia 29.
9. Membro com `Situacao` igual a `INATIVO` não aparece em nenhuma aba e não entra
   em nenhum contador.
10. Membro sem data de nascimento não aparece em nenhuma aba, e está contado em
    `semDataNascimento`.
11. A idade mostrada é a que a pessoa completa na data do aniversário, não a
    idade atual.

## A mensagem e o link

12. O endereço gerado é `https://wa.me/55` seguido dos onze ou dez dígitos
    guardados, sem o sinal de mais, sem espaço e sem pontuação.
13. O texto chega ao WhatsApp com acentos corretos, inclusive o travessão da
    assinatura — ou seja, passou por `encodeURIComponent`.
14. As três variáveis são substituídas: primeiro nome do membro, nome da igreja
    selecionada e nome do usuário da sessão. A assinatura **não** traz título
    como "Pastor".
15. Nada é enviado pelo sistema. Abrir o link deixa a mensagem escrita e não
    enviada na conversa.
16. Para membro **menor com WhatsApp próprio**, o destino é o número do menor.
    Para menor **sem** WhatsApp próprio, o destino é o do responsável legal, e o
    botão diz com quem a conversa será aberta.
17. Para adulto **sem** WhatsApp, a linha aparece na lista com o botão desativado
    e o motivo escrito — não desaparece.

## O menu e o sinal

18. Aniversariantes é a **segunda** entrada do menu, no computador e na barra
    inferior do celular.
19. Havendo aniversariante hoje, o ponto laranja aparece no item do menu em
    **todas** as seções, não só na de aniversariantes.
20. Não havendo aniversariante hoje, o ponto não aparece.
21. O nome acessível do botão traz a contagem, no formato "Aniversariantes, N
    hoje". Com leitor de tela, a informação chega sem depender da cor.
22. O ponto mede **ao menos 3:1** contra o fundo onde está desenhado, medido nas
    duas situações: a faixa branca do menu no computador e a barra inferior do
    celular.
23. A contagem do menu é buscada uma vez por igreja selecionada, não a cada troca
    de seção.

## Apresentação, herdada da V3

24. O cabeçalho da seção usa `.apresentacao-painel`, com os três contadores em
    `.resumo-painel` — idêntico em estrutura ao de Visitantes, Membros, Igrejas e
    Usuários.
25. As abas de período usam o padrão `.abas-membros`, com `role="tablist"` e
    `aria-selected` correto.
26. Todo alvo de toque mede **ao menos 44 px** na menor dimensão, incluindo o
    botão de parabenizar no celular.
27. O título do documento vira "Aniversariantes · Tópos - Gestão de Igrejas",
    pela função `tituloInterno`.
28. A 375 px de largura não há rolagem horizontal, e a barra inferior não cobre o
    fim da lista.
29. O estado de carregamento é anunciado por `role="status"` e só aparece depois
    de 400 ms, pela regra `.aguardando-rota` já existente.
30. A seção é um componente importado por `Interno.tsx`, como `GestaoMembros` e
    `Administracao`. **Correção do escopo original:** este documento pedia
    `React.lazy` "como as demais seções internas", e isso estava errado — a
    divisão por `lazy` no projeto é por rota (`PainelAtendimentos`,
    `PaginaLogin`, `PaginaQrCode`), não por seção do painel. Acrescentar um
    limite de `lazy` aqui criaria um padrão que não existe em nenhuma outra
    seção.
31. Nenhuma dependência nova no `package.json`.
32. A data de "hoje" segue o fuso de Brasília, não o UTC. Às 22h de um dia, a
    lista é a desse dia, e não a do dia seguinte.

## O seletor de igrejas

32. O gatilho do seletor é translúcido sobre a faixa azul, com texto branco e sem
    sombra.
33. O componente continua sendo o popover próprio: `aria-haspopup`, fechamento
    por Escape e por clique fora continuam funcionando.
34. A borda do gatilho mede **ao menos 3:1** contra a faixa, medido — não
    copiado do protótipo.
35. A logo da igreja continua no cabeçalho, pelo componente `LogoIgreja`.

## Banco de dados

36. **Nenhuma migração nova** no diretório de migrações. `dotnet ef migrations
    list` traz exatamente as mesmas migrações de antes da V4.

## Regressão

37. Os 59 testes existentes continuam passando.
38. O fluxo de visitantes, o cadastro de membros, a aprovação pastoral, os PDFs
    de convite e os QR Codes continuam com o comportamento de antes.
