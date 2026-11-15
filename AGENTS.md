# Instruções para agentes desenvolvedores

## Versões vigentes

Três frentes correm em paralelo. Leia a que corresponde ao seu trabalho.

**V2 — cadastro e gestão de membros.** Leia [docs/v2/README.md](docs/v2/README.md) e os documentos indicados ali. É a frente de produto: núcleo publicado em 14/11/2026, com a expansão (conversão de visitante em membro) ainda por fazer. As instruções de implementação do MVP de visitantes não são o backlog ativo.

**V3 — revisão de interface, acessibilidade e desempenho visual.** Leia [docs/v3/README.md](docs/v3/README.md) e, em seguida, [docs/v3/01-ESCOPO-E-LOTES.md](docs/v3/01-ESCOPO-E-LOTES.md). A V3 não acrescenta funcionalidade: corrige a camada de apresentação do que já existe, em cinco lotes ordenados por impacto dividido por esforço. Cada achado está marcado como **confirmado** (reproduzido na aplicação em execução, com medição) ou **a validar**.

**V4 — aniversariantes.** Leia [docs/v4/README.md](docs/v4/README.md) e os
documentos indicados ali. Escopo, protótipo e implementação de 15/11/2026, na
branch `feature/v4-aniversariantes`; nada publicado em produção. É uma seção
nova, de leitura, montada sobre o cadastro de membros da V2: lista de quem faz aniversário hoje, na semana e no mês, com abertura da
conversa no WhatsApp. **A V4 não faz nenhuma migração no banco** — essa é uma
decisão registrada, não uma preferência, e é o que mantém o texto da mensagem
fixo no front. Se encontrar necessidade de coluna nova, pare e consulte o
responsável.

Se for mexer em `styles.css`, em qualquer componente de formulário, em sobreposição modal ou em estado de carregamento, leia a V3 antes: provavelmente há um achado registrado sobre aquele trecho, com a medição e a solução já definidas.

## Regra de precedência entre as versões

A V3 rege apenas apresentação. Ela não altera campos, validações de domínio, permissões, regras de negócio, contratos de API, conteúdo dos PDFs nem o destino dos QR Codes. Em qualquer conflito aparente, o comportamento funcional descrito em `docs/` e `docs/v2/` prevalece e o ajuste visual cede.

## Baseline congelada

Os cinco arquivos `docs/01-ESCOPO-MVP.md` a `docs/05-BACKLOG-E-PLANO.md` estão congelados como documentação histórica do MVP. Consulte [docs/BASELINE-MVP-2026-09-25.md](docs/BASELINE-MVP-2026-09-25.md). Não os reescreva para descrever a V2. Registre requisitos e mudanças da nova versão exclusivamente em `docs/v2/` ou em documentos de versões posteriores.

Preserve o formulário, o QR Code, o painel, as permissões e os dados já existentes de visitantes. Uma nova especificação de membros prevalece sobre o antigo item “fora do escopo do MVP” somente no assunto membros; ela não revoga as regras atuais do fluxo de visitantes.

## Antes de implementar a V2

As decisões marcadas como **pendentes** em `docs/v2/04-DECISOES-PENDENTES.md` não devem ser inventadas pelo implementador, sobretudo campos obrigatórios, forma de acesso ao formulário, menores de idade e permissões da Equipe. Apresente as opções ao responsável pelo produto e registre as decisões nos documentos da V2 antes de construir as partes afetadas. É possível trabalhar nas partes independentes enquanto isso.

Use migrações incrementais e preserve os registros de produção. Não trate um envio público como filiação confirmada: o cadastro entra em análise até aprovação por perfil autorizado.

## Antes de implementar a V3

Vale a mesma regra: as decisões marcadas como **pendentes** em `docs/v3/02-DECISOES-PENDENTES.md` não devem ser inventadas pelo implementador — hoje, a tipografia, o comportamento do kanban no celular e o significado dos contadores durante a busca. Apresente as opções ao responsável e registre a escolha antes de construir a parte afetada.

Preserve a identidade visual: azul institucional, dourado, cartões com sombra suave. A V3 corrige deriva e defeito, não redesenha. Não acrescente dependências sem necessidade; quando um padrão correto já existir no repositório, reuse-o em vez de criar outro.

Registre cada lote implementado em [docs/v3/03-IMPLEMENTACAO.md](docs/v3/03-IMPLEMENTACAO.md), com a medição que comprova o resultado.

## Antes de implementar a V4

As oito decisões da V4 já estão tomadas e registradas com data em
[docs/v4/02-DECISOES.md](docs/v4/02-DECISOES.md) — não as reabra por conta
própria. O que continua pendente está na última seção do mesmo documento, e
vale a regra das outras versões: não invente o que está marcado como pendente.

Não altere a estrutura do banco. Não altere campos, validações, permissões nem
o fluxo de aprovação da V2 para acomodar a V4: a seção nova é somente de
leitura e reusa `IgrejaAutorizadaAsync` como está. A apresentação segue os
padrões da V3, inclusive os limites de contraste e de alvo de toque — a tela
nova não cria padrão próprio.

