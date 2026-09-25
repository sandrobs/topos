# Privacidade, segurança e riscos

Este documento define requisitos de produto, não substitui avaliação jurídica.

## 1. Natureza dos dados

O sistema trata nome, WhatsApp e informações relacionadas ao interesse por uma organização religiosa. Esses dados exigem cuidado reforçado. Observações internas feitas após uma conversa também podem incluir informações sensíveis de saúde, família ou vida pessoal.

## 2. Minimização adotada no MVP

O formulário público não coletará:

- conteúdo do pedido de oração;
- idade ou data de nascimento;
- endereço;
- documento pessoal;
- e-mail;
- informações familiares;
- dados de saúde.

O visitante apenas sinalizará se deseja conhecer melhor a igreja e/ou receber contato para conversa e oração.

Após o contato, usuários autorizados poderão registrar dados complementares fornecidos voluntariamente pelo visitante, exclusivamente na área interna do atendimento. Endereço, data de nascimento e situação congregacional permanecem opcionais, não aparecem no Kanban e devem ser tratados segundo o mesmo isolamento por igreja. O histórico registra a ocorrência da atualização, seu autor e sua data, sem reproduzir os valores pessoais nos logs técnicos.

## 3. Menores de idade

Menores poderão chegar ao formulário, mas o MVP não perguntará idade para preservar a simplicidade e evitar a coleta de mais dados.

Premissas operacionais para o lançamento:

- usar texto de privacidade claro e compreensível;
- exigir a confirmação de que o envio é feito por uma pessoa maior de 18 anos ou pelo responsável legal do menor;
- não solicitar o conteúdo da oração no formulário;
- quando a equipe identificar um menor durante o contato, seguir o procedimento interno definido pela igreja para comunicação e envolvimento do responsável;
- não registrar detalhes desnecessários sobre o menor nas observações.

Essa solução reduz o escopo técnico, mas não resolve sozinha todas as obrigações legais. O responsável pelo produto deve validar, antes do uso real, o aviso de privacidade e o procedimento de contato com menores. O tratamento deve observar o melhor interesse da criança ou do adolescente.

## 4. Aviso de privacidade

Antes da produção, a igreja deve aprovar um texto que informe, em linguagem simples:

- quem é responsável pelos dados;
- quais dados são coletados;
- para que serão usados;
- que o contato ocorrerá por WhatsApp;
- quem poderá acessar os dados;
- por quanto tempo ou segundo qual critério serão mantidos;
- como solicitar correção ou exclusão;
- como entrar em contato com o responsável pela privacidade.

O formulário exibirá um resumo curto e um link **Segurança e privacidade**, que abrirá o conteúdo completo em uma janela de diálogo. A confirmação obrigatória, não marcada previamente, usará o texto aprovado pelo responsável pelo produto:

> Confirmo que sou maior de 18 anos ou responsável legal e autorizo esta igreja a usar os dados informados para entrar em contato comigo pelo WhatsApp e realizar meu acolhimento. Li o Aviso de Privacidade.

O sistema deve guardar a versão do aviso reconhecido e a data do envio. Mudanças materiais no texto exigem nova versão.

## 5. Retenção e exclusão

Para não ampliar o MVP:

- não haverá exclusão automática por tempo;
- registros poderão ser concluídos e permanecer no histórico;
- solicitações de correção ou exclusão serão tratadas pelo Administrador por procedimento manual;
- exclusão não deve apagar silenciosamente evidências necessárias de auditoria sem uma decisão administrativa;
- política de retenção automatizada será definida após o lançamento.

O responsável pelo produto deve registrar um canal para solicitações no aviso de privacidade.

## 6. Segurança mínima

- isolamento multi-igreja aplicado no servidor;
- princípio do menor privilégio;
- autenticação e sessão seguras;
- senhas armazenadas exclusivamente por hash do ASP.NET Core Identity;
- senhas temporárias de usuários criados ou redefinidos na gestão exibidas somente naquele momento e com troca obrigatória no primeiro acesso; a senha forte da conta Administradora inicial é exceção aprovada;
- HTTPS obrigatório;
- validação de todas as entradas;
- limitação de frequência e proteção básica contra spam no formulário;
- identificador público da igreja não sequencial ou facilmente enumerável;
- backups antes e depois do lançamento;
- segredos fora do código-fonte e dos logs;
- nenhum conteúdo de observação em logs técnicos;
- mensagens de erro públicas sem detalhes internos.

### Proteção adicional contra bots

A proteção externa com desafio anti-bot, CDN ou WAF não será adicionada nesta etapa. A limitação de frequência já existente permanece ativa. Após a estabilização do MVP, deverá ser reavaliada uma proteção em camadas com Cloudflare Turnstile, filtragem na borda e configuração segura do IP real do visitante atrás do proxy.

## 7. Matriz de riscos do lançamento

| Risco | Impacto | Tratamento no MVP |
|---|---|---|
| Usuário acessar outra igreja | Crítico | Isolamento no servidor e teste cruzado obrigatório |
| Exposição de dados sensíveis em logs | Alto | Não registrar observações e dados completos em logs |
| Contato com menor sem procedimento | Alto | Orientação no aviso e processo interno antes do lançamento |
| Spam no formulário público | Médio | Limitação de frequência existente; proteção externa será avaliada após o MVP |
| Cadastros duplicados | Médio | Pesquisa manual; automação fica para fase posterior |
| Número de WhatsApp incorreto | Médio | Validação e normalização; confirmação posterior pela equipe |
| Perda de dados | Alto | Backup e teste de recuperação conforme o ambiente |
| Escopo exceder o prazo | Alto | Congelamento de P0 e remoção imediata de P1 |
| Logo de baixa resolução | Baixo | Usar provisoriamente e solicitar versão vetorial depois |

## 8. Decisões que não bloqueiam o desenvolvimento

Podem ser finalizadas em paralelo, mas precisam estar resolvidas antes da operação real:

- nome e contato do responsável por solicitações dos titulares;
- procedimento interno para contato com menores;
- política formal de retenção;
- rotina e responsável operacional pelo backup.
