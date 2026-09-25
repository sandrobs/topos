# Instruções para agentes desenvolvedores

## Versão vigente

Leia primeiro [docs/v2/README.md](docs/v2/README.md) e os documentos da V2 indicados ali. A próxima versão do produto é o **cadastro e a gestão de membros**. As instruções de implementação do MVP de visitantes não são o backlog ativo.

## Baseline congelada

Os cinco arquivos `docs/01-ESCOPO-MVP.md` a `docs/05-BACKLOG-E-PLANO.md` estão congelados como documentação histórica do MVP. Consulte [docs/BASELINE-MVP-2026-09-25.md](docs/BASELINE-MVP-2026-09-25.md). Não os reescreva para descrever a V2. Registre requisitos e mudanças da nova versão exclusivamente em `docs/v2/` ou em documentos de versões posteriores.

Preserve o formulário, o QR Code, o painel, as permissões e os dados já existentes de visitantes. Uma nova especificação de membros prevalece sobre o antigo item “fora do escopo do MVP” somente no assunto membros; ela não revoga as regras atuais do fluxo de visitantes.

## Antes de implementar a V2

As decisões marcadas como **pendentes** em `docs/v2/04-DECISOES-PENDENTES.md` não devem ser inventadas pelo implementador, sobretudo campos obrigatórios, forma de acesso ao formulário, menores de idade e permissões da Equipe. Apresente as opções ao responsável pelo produto e registre as decisões nos documentos da V2 antes de construir as partes afetadas. É possível trabalhar nas partes independentes enquanto isso.

Use migrações incrementais e preserve os registros de produção. Não trate um envio público como filiação confirmada: o cadastro entra em análise até aprovação por perfil autorizado.

