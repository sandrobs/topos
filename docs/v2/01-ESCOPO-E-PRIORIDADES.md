# V2 — Escopo e prioridades

## 1. Problema a resolver

O MVP conhece visitantes e registra o desfecho de um atendimento, mas não mantém um cadastro próprio de membros. Um visitante marcado como “Tornou-se membro” continua sendo apenas um atendimento. Os pastores precisam consultar e atualizar informações de membros atuais e novos sem usar o Kanban de acolhimento como cadastro permanente.

## 2. Jornada-alvo

1. A igreja imprime e compartilha internamente um endereço próprio de cadastro de membros, com QR Code próprio, diferente do endereço de visitantes. O link abre sem login para quem o receber.
2. A pessoa preenche um formulário responsivo com informações de contato e os dados aprovados para esta finalidade.
3. O envio cria uma **solicitação de cadastro pendente** vinculada à igreja, não um membro ativo automaticamente.
4. Pastor ou Administrador confere os dados, verifica possíveis registros existentes, corrige internamente após contato quando necessário e aprova ou recusa a solicitação.
5. Após aprovação, o registro aparece na lista de membros da igreja e pode ser atualizado por perfil autorizado.
6. Em etapa posterior, um visitante concluído como “Tornou-se membro” poderá ser vinculado a um cadastro de membro, preservando seu atendimento e histórico.

## 3. V2 núcleo — prioridade máxima

- Separar claramente as áreas de **Visitantes** e **Membros** no painel.
- Formulário de membros vinculado a uma igreja, acessível em celular e tablet.
- Link e QR Code específicos de membros para cada igreja, em rota separada da captação de visitantes.
- Validação dos campos aprovados, mensagem de confirmação e aviso de privacidade específico do cadastro de membros.
- Solicitações pendentes visíveis apenas na igreja correspondente e ao Administrador global.
- Revisão e aprovação por Pastor ou Administrador; nenhum envio público ativa uma filiação por si só.
- Inclusão manual dos membros atuais por Pastor ou Administrador em formulário da área autenticada, criando diretamente um membro ativo.
- Lista e detalhe de membros com busca por nome e WhatsApp.
- Painel de membros com contadores simples de solicitações pendentes, membros ativos e inativos, além de lista paginada, busca por nome ou WhatsApp e filtro por situação. A apresentação deve funcionar em celular, tablet e computador.
- Edição controlada, situação do membro e registro mínimo de autor/data das alterações.
- Proteção contra criação acidental de um segundo membro para o mesmo WhatsApp na mesma igreja: sinalizar possível duplicidade e exigir decisão explícita do revisor.
- Preservar integralmente a jornada de visitantes em produção.

## 4. V2 expansão — após estabilizar o núcleo

- Vinculação ou conversão de um visitante em membro, reaproveitando nome, WhatsApp e dados complementares existentes após conferência.
- Vínculo navegável entre o membro e o atendimento original; o histórico do atendimento não é copiado nem apagado.
- Tratamento de solicitações repetidas e correção de informações pelo próprio membro por mecanismo apropriado, a definir.
- Filtros adicionais, caso o uso real mostre necessidade.

Uma eventual importação de planilha será avaliada depois se houver uma fonte confiável. A inclusão manual não altera a regra de aprovação do formulário compartilhável.

## 5. Fora do escopo desta versão

- acesso do membro a um portal autenticado;
- carteirinha digital;
- cadastro de famílias ou dependentes;
- gestão de ministérios, escalas, frequência, discipulado ou pequenos grupos;
- contribuições financeiras;
- upload de documentos ou fotos;
- importação em massa;
- disparos automáticos de WhatsApp, e-mail ou SMS;
- transferência automática de membro entre igrejas;
- reconhecimento automático de filiação com base no número de WhatsApp.

## 6. Regras de evolução

- O QR Code atual de visitantes continua abrindo o formulário de visitantes.
- O cadastro de membros usa endereço e QR Code próprios; eles não substituem os de visitantes.
- O novo domínio deve permitir migração incremental do banco em produção, sem recriar a base e sem perder atendimentos existentes.
- Uma mesma pessoa pode ter histórico como visitante e cadastro de membro; o vínculo entre registros deve ser explícito e auditável.
- Um membro não é automaticamente um usuário do painel. O perfil de acesso interno continua sendo gerenciado separadamente.
