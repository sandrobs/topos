# V4 — Decisões

As oito decisões abaixo foram tomadas pelo responsável pelo produto em
**15/11/2026**, antes de qualquer implementação. Estão registradas com a opção
escolhida e, onde houve, a recomendação técnica divergente — que fica preservada
como registro, não como pendência.

O que continua **pendente** está na última seção e **não deve ser inventado pelo
implementador**.

---

## D1 · Quem vê a tela — **decidido: Pastor e Administrador**

A mesma regra que o cadastro de membros já aplica. `IgrejaAutorizadaAsync` nega
o perfil `Equipe` em todos os endpoints de membros, e a seção nova herda isso
sem alteração. No menu, a entrada leva
`perfis: ["ADMINISTRADOR", "PASTOR"]`.

A alternativa considerada era incluir a Equipe, que é quem mais fala com as
pessoas. Foi recusada, e isso tem uma consequência boa: o aviso de privacidade
`membros-2026-10-03` promete ao membro que o acesso aos seus dados é restrito a
pastores autorizados e administradores. Mantendo esses dois perfis, **a V4 não
exige nova versão do aviso**. Incluir a Equipe exigiria.

## D2 · O recorte da semana — **decidido: domingo a sábado da semana corrente**

Não é uma janela móvel de sete dias. É a semana do calendário, começando no
domingo.

A razão é de uso: casa com o culto. No domingo o pastor abre a tela e vê os
aniversariantes da semana que começa, e esse recorte é o mesmo para todos os que
olharem. Uma janela móvel nunca fica vazia no fim da semana, mas muda de recorte
a cada dia, e duas pessoas olhando em dias diferentes veriam listas diferentes.

## D3 · O texto da mensagem — **decidido: fixo, sem migração**

O texto fica no front, com três variáveis: primeiro nome do membro, nome da
igreja e nome de quem está usando o sistema.

A opção de deixá-lo configurável por igreja foi considerada e **recusada nesta
versão** para que a V4 não faça nenhuma migração no banco. Seria uma coluna nova
em `Igreja`, no mesmo padrão de `CultoPrincipal` e `Instagram`.

Isso vale como regra, não só como escolha: **nenhuma migração nesta versão.** Se
a implementação encontrar necessidade de coluna nova, para o que estiver fazendo
e volta ao responsável.

## D4 · Membro menor de idade — **decidido: WhatsApp do menor quando houver, senão do responsável**

Abre a conversa com o número do próprio menor quando o cadastro tiver esse
campo; cai no WhatsApp do responsável legal quando não tiver. Para menor, o
contato do responsável é obrigatório, então sempre existe destino.

O botão diz a quem a conversa vai ser aberta, para que ninguém envie sem saber.
A escolha do destino é feita no servidor, não no front.

**Esta decisão se apoia num pré-requisito que não está pronto.** Ver a última
seção.

## D5 · Nascido em 29 de fevereiro — **decidido: cai em 28 de fevereiro**

Em ano comum, o aniversário aparece no dia 28. A alternativa era 1º de março;
28 foi escolhido por manter a pessoa dentro do mês de nascimento.

Sem essa regra, quem nasceu em 29 de fevereiro desapareceria da lista em três de
cada quatro anos — exatamente o tipo de falha que ninguém nota.

## D6 · Situação do membro — **decidido: somente ativos**

A tela mostra apenas membros com `Situacao` igual a `ATIVO`, como fazem os
contadores de `/membros/resumo`. Quem foi inativado não aparece e não entra nas
contagens.

## D7 · Posição no menu — **decidido: segundo lugar, no computador e no celular**

Atendimentos, Aniversariantes, Membros, Igrejas, Usuários.

Com cinco entradas a barra do celular chega ao limite acordado na V3. A próxima
seção obriga a implementar o "Mais" previsto em `NavegacaoPainel.tsx`.

## D8 · O sinal de aniversário hoje — **decidido: ponto laranja no menu**

Quando houver aniversariante no dia, o item do menu recebe um ponto laranja, em
qualquer tela em que a pessoa esteja.

Dois requisitos que acompanham a decisão, e não são negociáveis porque vêm da
V3: a contagem entra no nome acessível do botão, para que a cor não seja o único
sinal; e o ponto mede ao menos 3:1 contra o fundo.

---

## Pendente · Procedimento interno de contato com menores

`docs/04-PRIVACIDADE-SEGURANCA-RISCOS.md` lista como pendente, desde o MVP, o
procedimento interno da igreja para comunicação envolvendo menores, e o aviso de
privacidade de membros diz que o cadastro de menor deve ser tratado diretamente
com a igreja e o responsável legal.

A decisão D4 é compatível com isso, mas **se apoia nesse procedimento**. Antes de
a funcionalidade ir a produção, quem responde por privacidade no produto precisa
olhar a D4 e confirmar que o contato direto com menor cabe no procedimento
adotado. Não é decisão do implementador, e não é motivo para parar o
desenvolvimento das outras partes.

## Pendente · Cobertura da data de nascimento

Ninguém mediu quantos membros ativos estão sem data de nascimento. Como o campo
é opcional, essas pessoas nunca aparecem na tela.

A medição é simples e precisa ser feita em produção antes de a V4 ser
considerada útil. Se a cobertura for baixa, a decisão seguinte — tornar o campo
obrigatório — é mudança na V2 e cabe ao responsável, não ao implementador.

## Pendente · Registrar que já parabenizou

Fora do escopo desta versão por decisão de tamanho, não por impedimento técnico:
`HistoricoMembro` já aceitaria o registro com um código de ação novo, sem tabela
nem coluna.

Vale reabrir quando duas pessoas da mesma igreja começarem a usar a tela no mesmo
dia. Até lá, não há problema a resolver.
