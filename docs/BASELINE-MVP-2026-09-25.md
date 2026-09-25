# Baseline congelada do MVP de visitantes

**Data do congelamento documental:** 25/09/2026. O responsável pelo produto informou que o MVP já está funcionando em produção. Este documento não é uma auditoria do ambiente publicado nem um snapshot do código ou do banco de dados.

Os arquivos abaixo permanecem no lugar para consulta histórica. Seus conteúdos não devem ser editados para acomodar a V2. Qualquer correção futura da documentação histórica deve ser explicitamente identificada como errata, sem mudar silenciosamente a baseline.

| Arquivo | SHA-256 no congelamento |
|---|---|
| `docs/01-ESCOPO-MVP.md` | `1FF91C395D9C473A9E1E4B5DC327173AA6B95FDBBFC8C246A8E779A93F8C8B5C` |
| `docs/02-REQUISITOS-E-REGRAS.md` | `B910C56AE0E906358D90FF94C14201FC00AA67D14C70E9D1216AB3EA993318D8` |
| `docs/03-CRITERIOS-DE-ACEITE.md` | `F0DFC5A866EDB92792DA84B62E5B59D3B15F5E99C9DCF9E23DC6D03D6EA6762E` |
| `docs/04-PRIVACIDADE-SEGURANCA-RISCOS.md` | `2066FEDA4A0FDA61C331E3881B43AF2CE7545912DAD2EB0DD65871FEE019C2F5` |
| `docs/05-BACKLOG-E-PLANO.md` | `B331EEC03CE40EC44DB3AFB87FEFAA89DA197677A266CE92CE9DE1FF6F65AE17` |

## O que a baseline contém

- captação de visitantes por QR Code exclusivo da igreja;
- formulário público com nome, WhatsApp e interesses;
- atendimento em Kanban com etapas adjacentes, responsável, observações e histórico;
- três perfis internos e isolamento entre igrejas;
- dados complementares opcionais coletados pela equipe após o contato;
- confirmação e versionamento do aviso de privacidade;
- resultado `TornouSeMembro`, que no MVP não cria um registro de membro.

## Uso daqui em diante

- Use estes arquivos para entender o comportamento que precisa continuar funcionando.
- Para novos requisitos de membros, siga [docs/v2/README.md](v2/README.md).
- O código e o banco em produção podem ter mudado após esta leitura; compare a implementação real antes de planejar migrações.

