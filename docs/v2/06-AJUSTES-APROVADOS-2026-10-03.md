# Ajustes aprovados em 03/10/2026

## Escopo e sequência

1. Integrar na branch V2 o convite de visitantes aprovado na V1, incluindo PDF
   no servidor, foto natural, altar/cruz e endereço em bloco próprio.
2. Disponibilizar PDF de página inteira e PDF com quatro convites por folha A4
   para visitantes e membros. O modelo compacto tem diagramação própria,
   margens e guias de corte; o QR continua apontando ao formulário permanente
   correspondente. Nenhum PDF depende da impressão do HTML pelo navegador.
3. Alinhar o formulário de membros à identidade e à responsividade do formulário
   de visitantes: campos com rótulos, erros próximos, máscaras, opções marcáveis,
   confirmação de envio e diálogo de privacidade acessível. Preservar os campos,
   o autoenvio exclusivo de adultos e a aprovação pastoral.
4. Destacar a ação de voltar à lista no detalhe e preservar filtro/paginação.
   O retorno do convite de membros abre Membros na mesma igreja, não Visitantes.
5. Recriar o convite de membros com identificação destacada CADASTRO DE MEMBROS,
   “Uma família no amor de Deus”, “Ainda há lugar”, Lucas 14:22 e referência
   discreta a Romanos 12:4-5. Sem copiar tradução bíblica não selecionada.

## Função na igreja

- Campo obrigatório, seleção única, nas solicitações e nos cadastros internos.
- Opções, nesta ordem: Membro, Diácono, Presbítero, Pastor.
- Padrão apresentado no formulário: Membro. Não oferecer “Não informado”.
- A função é confirmada/corrigida na revisão pastoral e não concede acesso ao
  painel nem altera o perfil da conta de usuário.
- Migração incremental acrescenta o campo nas duas tabelas sem apagar registros;
  registros anteriores recebem Membro como padrão técnico, editável internamente.

## Aviso de privacidade aprovado

Versão: `membros-2026-10-03`. A aprovação deste texto não autoriza deploy.

Resumo: “Seus dados serão usados para cadastro, contato e acompanhamento pela igreja.”

Confirmação obrigatória, sem marcação prévia:

> Sou maior de 18 anos e autorizo esta igreja a usar os dados informados, incluindo informações religiosas, para cadastro e acompanhamento de membros e contato pelo WhatsApp. Li o Aviso de Privacidade.

Conteúdo do diálogo:

> A igreja identificada neste formulário é responsável pelo uso dos dados informados. Nome, contato e, quando fornecidos, nascimento, endereço, batismo e função na igreja serão utilizados para analisar a solicitação, manter o cadastro de membros e realizar contato e acompanhamento pastoral.
>
> O acesso é restrito aos pastores autorizados da igreja e aos administradores da plataforma. O envio depende de análise pastoral e não confirma automaticamente o cadastro.
>
> Os dados serão mantidos enquanto necessários às finalidades informadas, observadas as obrigações legais aplicáveis. Para solicitar acesso, correção, exclusão ou revogar a autorização, procure o pastor ou a administração da igreja.
>
> Este formulário é destinado a adultos. Cadastros de menores devem ser tratados diretamente com a igreja e seu responsável legal.

Usar texto específico de membros e registrar a versão aceita no envio. O
procedimento e o texto para operar dados reais de menores permanecem pendentes;
esta aprovação não amplia o formulário público para menores nem substitui
validação jurídica. Não inventar contato institucional ou prazo de retenção.

## Validação e publicação

- Desenvolvimento, migração e dados fictícios somente no ambiente local.
- Build/testes de backend e frontend; regressão de visitantes e permissões.
- Verificar formulário e gestão em celular, tablet e computador, sem cortes,
  sobreposição ou rolagem horizontal; navegação por toque e teclado.
- Renderizar os quatro PDFs, confirmar página única A4 e decodificar todos os QRs.
- Preservar base, tokens e chaves existentes. Produção somente depois da aprovação
  expressa do responsável pelo produto, com backup e plano de recuperação.
- Não restaurar ou executar o compilador Windows bloqueado pelo antivírus;
  compilar o frontend dentro de Docker/Linux.
