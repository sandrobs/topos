# Bootstrap da base de produção

O PostgreSQL é criado pelo contêiner `banco`. Na primeira subida da API, o
Entity Framework Core aplica as migrações e o código `DadosIniciais` cria a
igreja e a conta administradora configuradas. Não execute SQL manual para
inserir a conta: a senha deve ser armazenada pelo ASP.NET Core Identity como
hash, nunca em texto puro no banco.

## Antes de executar

1. Na VPS Ubuntu, instale Docker Engine e o plugin Docker Compose pelo
   [repositório oficial do Docker](https://docs.docker.com/engine/install/ubuntu/).
   Confirme `docker compose version` e que as portas 80/443 não estão sendo
   usadas por outro serviço. O proxy Caddy roda no mesmo Compose, usa HTTPS
   automático e publica somente 80/443. A API fica em `127.0.0.1:8080` para
   diagnóstico local e o PostgreSQL não publica porta no host.
2. Confira a arquitetura da VPS com `uname -m`. Em uma VPS pequena, publique
   backend e frontend fora dela, envie apenas os resultados e monte uma imagem
   leve com `deploy/Dockerfile.runtime`. Essa imagem usa o runtime .NET, sem
   SDK nem Node na VPS. Use a tag indicada em `IMAGEM_APLICACAO`. O script de
   inicialização usa `--no-build`. Transfira também `compose.yaml` e a pasta
   `deploy`.
3. Confira na Cloudflare os dois registros `A` apontando ao IP público da VPS, um
   para a gestão e outro para o formulário. Para a primeira publicação, use
   **DNS only** (nuvem cinza): assim o Caddy obtém o certificado diretamente e
   o limitador de frequência da API recebe o IP do visitante. Caso a nuvem
   laranja seja ativada depois, revise o encaminhamento do IP real antes de
   manter o limitador por IP e configure o modo TLS **Full (strict)**.
4. Confira todos os campos de `deploy/.env.production.example` e execute
   `bash deploy/criar-ambiente-producao.sh` na VPS para gerar as duas senhas
   fortes e distintas em `deploy/.env.production`. O script não sobrescreve
   arquivo existente e aplica permissão `600`. Leia a senha inicial do
   administrador diretamente no terminal da VPS com
   `grep '^ADMIN_SENHA=' /opt/topos/deploy/.env.production` e guarde-a em um
   gerenciador de senhas; não a envie por chat nem a versione.
5. Verifique o nome oficial da igreja. O exemplo usa
   `Igreja Evangélica de Cristo - Três de Maio`. Rua, número, bairro e CEP não
   foram informados e não devem ser inventados.

Antes de iniciar, libere somente SSH, 80/tcp e 443/tcp no firewall da VPS e
confirme que o DNS dos dois subdomínios resolve para o IP público da máquina.
Não publique a porta 5432. Para inicializar a base, na raiz do projeto na VPS:

```bash
bash deploy/inicializar-base-producao.sh
```

O script valida os campos essenciais e a presença da imagem antes de iniciar
PostgreSQL, API e Caddy. O bootstrap é idempotente para a mesma configuração: procura a
igreja por nome, cidade e UF, e o administrador por e-mail. Reiniciar a API não
redefine a senha nem sobrescreve os dados existentes. Se o nome da igreja mudar
após a primeira subida, edite-a no painel; alterar `IGREJA_NOME` no arquivo
pode criar uma segunda igreja.

`sandrobs@outlook.com` será a conta **Administradora do aplicativo**, não um
usuário de conexão direta ao PostgreSQL. `POSTGRES_USER` é a conta técnica do
banco. A senha administradora é usada somente na criação inicial e fica
armazenada como hash. Por decisão do responsável pela arquitetura, essa conta
inicial não exige troca obrigatória no primeiro acesso. A regra de troca
continua válida para usuários criados e senhas redefinidas pela gestão.

Depois de confirmar o primeiro login, remova o valor de `ADMIN_SENHA` do
arquivo de ambiente e recrie somente o contêiner da aplicação. A conta já
criada continuará válida; isso evita manter a senha inicial nas variáveis do
contêiner. O script de bootstrap exige a senha apenas na primeira execução.

## Validação depois da subida

```bash
docker compose --env-file deploy/.env.production -f compose.yaml -f deploy/compose.production.yaml ps
docker compose --env-file deploy/.env.production -f compose.yaml -f deploy/compose.production.yaml logs --tail=80 aplicacao proxy
curl -fsS http://127.0.0.1:8080/api/saude
```

Confira `https://DOMINIO_GESTAO/api/saude` e que
`https://DOMINIO_VISITA/entrar` retorna 404. O domínio de visitantes permite
somente `/visita/*`, `/api/publico/igrejas/*` e os arquivos estáticos
necessários ao formulário. A área de gestão continua protegida pela própria
autenticação e autorização da API.

Depois de confirmar HTTPS e DNS, entre com `sandrobs@outlook.com`, verifique a
igreja de Três de Maio, gere o QR Code e faça o teste de fumaça descrito em
`docs/03-CRITERIOS-DE-ACEITE.md`. Antes de dados reais, faça backup e uma
restauração de prova, sem tocar no banco principal:

```bash
bash deploy/fazer-backup.sh
bash deploy/testar-restauracao.sh /opt/topos/backups/NOME-DO-ARQUIVO.dump
```

O backup local na própria VPS não protege contra perda do servidor: mantenha
também uma cópia externa protegida e defina frequência e responsável pela
execução. O teste de restauração usa um contêiner temporário sem rede.
