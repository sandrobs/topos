# CRM da Igreja Evangélica de Cristo

O MVP de acolhimento de visitantes está em produção, conforme informado pelo responsável pelo produto em 25/09/2026. O trabalho de produto vigente é a **V2: cadastro e gestão de membros**.

## Documentação vigente para o agente desenvolvedor

1. Leia [as instruções do repositório](AGENTS.md).
2. Siga [a especificação da V2](docs/v2/README.md) e suas decisões registradas.
3. Consulte [a baseline congelada do MVP](docs/BASELINE-MVP-2026-09-25.md) para preservar o fluxo de visitantes.

Os cinco arquivos originais em `docs/01-...` a `docs/05-...` são históricos e não constituem o backlog ativo da V2. Os demais detalhes deste README descrevem a implementação do MVP existente.

## Objetivo histórico do MVP

O MVP teve como meta de lançamento **sexta-feira, 18/09/2026**: um CRM responsivo para captação e acompanhamento de visitantes das igrejas.

O visitante acessa um formulário público por um QR Code exclusivo da igreja. A equipe recebe o cadastro em um painel no formato Kanban e acompanha a pessoa por WhatsApp até a conclusão do atendimento.

## Resultado mínimo que deve ser protegido

O lançamento não pode ser atrasado por funcionalidades complementares. O primeiro marco de sucesso é:

1. apontar o celular para o QR Code;
2. preencher e enviar o formulário público;
3. visualizar imediatamente o novo visitante no painel da igreja;
4. abrir seu WhatsApp, registrar uma observação e alterar a etapa do atendimento.

Depois que esse fluxo estiver estável e responsivo, o agente pode ampliar a entrega seguindo a ordem definida no backlog.

## Documentos históricos do MVP

1. [Escopo do MVP](docs/01-ESCOPO-MVP.md)
2. [Requisitos e regras de negócio](docs/02-REQUISITOS-E-REGRAS.md)
3. [Critérios de aceite](docs/03-CRITERIOS-DE-ACEITE.md)
4. [Privacidade, segurança e riscos](docs/04-PRIVACIDADE-SEGURANCA-RISCOS.md)
5. [Backlog e plano de entrega](docs/05-BACKLOG-E-PLANO.md)

## Restrições do MVP histórico

- Implementar primeiro o **P0 Core** e somente então avançar para o **P0 Ampliado** e o **P1**.
- Não criar, neste MVP, um cadastro completo de membros.
- Não integrar com a API do WhatsApp; apenas abrir uma conversa por link.
- Não coletar o texto ou o conteúdo do pedido de oração no formulário público.
- Não permitir que dados de uma igreja sejam acessados por usuários de outra igreja.
- Não iniciar itens pós-MVP antes de os critérios P0 estarem atendidos.

## Decisões já tomadas

- Plataforma multi-igreja.
- Um QR Code permanente por igreja, apresentado em um convite A4 pronto para impressão.
- Perfis: Administrador da plataforma, Pastor e Equipe.
- Administrador possui acesso global para suporte.
- Pastor e Equipe acessam somente sua igreja.
- Pastor e Administrador podem cadastrar usuários.
- Todo usuário da igreja pode acompanhar visitantes.
- Contato inicial exclusivamente por WhatsApp.
- Interface responsiva para celular, tablet e computador.
- Visitantes que se tornarem membros serão convertidos em uma fase futura, preservando os dados coletados.

## Estrutura da implementação

- `src/CrmIctm.Api`: monólito ASP.NET Core 10, APIs REST, Identity, EF Core e PostgreSQL.
- `src/CrmIctm.Web`: frontend React, TypeScript e Vite.
- `tests/CrmIctm.Api.Tests`: testes automatizados do backend.
- `Dockerfile` e `compose.yaml`: artefato único da aplicação e PostgreSQL para execução conteinerizada. O override `deploy/compose.production.yaml` adiciona Caddy para HTTPS e separação dos domínios de gestão e visitantes.

O frontend e a API são executados separadamente durante o desenvolvimento. Em produção, o ASP.NET Core serve os arquivos compilados do React, mantendo aplicação e API na mesma origem.

## Execução local com Docker

1. Copie `.env.example` para `.env` e substitua todas as senhas de exemplo.
2. Preencha os dados da igreja e do Administrador inicial.
   Para a igreja, informe também logradouro, número, bairro, complemento opcional e CEP.
3. Preencha o aviso de privacidade somente com o texto aprovado pelo responsável pelo produto.
4. Execute `docker compose up --build`.
5. Acesse `http://localhost:8080`.

As migrações são aplicadas automaticamente no modo Docker. O endereço público da igreja inicial é informado no log da aplicação na primeira criação.

O `.env.example` usa `ASPNETCORE_ENVIRONMENT=Development` para permitir a execução local por HTTP. No ambiente publicado, defina obrigatoriamente `ASPNETCORE_ENVIRONMENT=Production` e disponibilize a aplicação somente por HTTPS.

Para testar o QR Code em um celular conectado à mesma rede, defina `URL_PUBLICA` no `.env` com o IP da máquina, por exemplo `http://192.168.1.111:8080`, e reinicie a aplicação. `localhost` dentro do QR Code apontaria para o próprio celular.

Para produção, HTTPS deve ser encerrado pela plataforma de hospedagem ou por um proxy reverso. O arquivo `.env` contém segredos e não deve ser versionado.

## Preparação da VPS de produção

O modelo de variáveis, o script de bootstrap idempotente e os passos de
validação estão em [deploy/README.md](deploy/README.md). O exemplo de produção
configura `sandrobs@outlook.com` como Administrador e a igreja de Três de Maio,
mas exige o endereço real, senhas próprias, URL HTTPS e aviso de privacidade
aprovado antes de iniciar a base.
