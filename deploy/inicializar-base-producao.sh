#!/usr/bin/env bash
set -euo pipefail

raiz_projeto="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
arquivo_ambiente="${1:-$raiz_projeto/deploy/.env.production}"

if [[ ! -f "$arquivo_ambiente" ]]; then
  echo "Arquivo de produção ausente: $arquivo_ambiente" >&2
  echo "Copie deploy/.env.production.example, preencha os dados e tente novamente." >&2
  exit 1
fi

if [[ "$(basename "$arquivo_ambiente")" == *.example ]]; then
  echo "Não inicialize a base usando o arquivo de exemplo." >&2
  exit 1
fi

ler_variavel() {
  local chave="$1"
  local linha
  linha="$(grep -E "^${chave}=" "$arquivo_ambiente" | tail -n 1 || true)"
  printf '%s' "${linha#*=}" | tr -d '\r'
}

exigir_variavel() {
  local chave="$1"
  if [[ -z "$(ler_variavel "$chave")" ]]; then
    echo "Preencha $chave em $arquivo_ambiente." >&2
    exit 1
  fi
}

for chave in \
  POSTGRES_DB POSTGRES_USER POSTGRES_PASSWORD IMAGEM_APLICACAO URL_PUBLICA \
  DOMINIO_GESTAO DOMINIO_VISITA \
  IGREJA_NOME IGREJA_LOGRADOURO IGREJA_NUMERO IGREJA_BAIRRO \
  IGREJA_CEP IGREJA_CIDADE IGREJA_ESTADO \
  ADMIN_NOME ADMIN_EMAIL ADMIN_SENHA \
  AVISO_PRIVACIDADE_VERSAO AVISO_PRIVACIDADE_TEXTO; do
  exigir_variavel "$chave"
done

if [[ "$(ler_variavel ASPNETCORE_ENVIRONMENT)" != "Production" ]]; then
  echo "ASPNETCORE_ENVIRONMENT deve ser Production." >&2
  exit 1
fi

if [[ "$(ler_variavel URL_PUBLICA)" != https://* ]]; then
  echo "URL_PUBLICA deve começar com https://." >&2
  exit 1
fi

for chave in DOMINIO_GESTAO DOMINIO_VISITA; do
  if [[ ! "$(ler_variavel "$chave")" =~ ^[A-Za-z0-9]([A-Za-z0-9.-]*[A-Za-z0-9])?$ ]] ||
     [[ "$(ler_variavel "$chave")" != *.* ]]; then
    echo "$chave deve conter somente o nome do subdomínio, sem protocolo ou caminho." >&2
    exit 1
  fi
done

if [[ "$(ler_variavel URL_PUBLICA)" != "https://$(ler_variavel DOMINIO_VISITA)" ]]; then
  echo "URL_PUBLICA deve ser https:// seguido exatamente por DOMINIO_VISITA." >&2
  exit 1
fi

if [[ "$(ler_variavel DOMINIO_GESTAO)" == "$(ler_variavel DOMINIO_VISITA)" ]]; then
  echo "Use subdomínios diferentes para gestão e formulário." >&2
  exit 1
fi

if [[ ! "$(ler_variavel IGREJA_CEP)" =~ ^[0-9]{8}$ ]]; then
  echo "IGREJA_CEP deve ter oito dígitos, sem hífen." >&2
  exit 1
fi

if [[ ! "$(ler_variavel ADMIN_EMAIL)" =~ ^[^[:space:]@]+@[^[:space:]@]+\.[^[:space:]@]+$ ]]; then
  echo "ADMIN_EMAIL deve conter um e-mail válido." >&2
  exit 1
fi

if [[ "$(ler_variavel POSTGRES_PASSWORD)" == "$(ler_variavel ADMIN_SENHA)" ]]; then
  echo "Use senhas diferentes para o banco e para o administrador." >&2
  exit 1
fi

imagem="$(ler_variavel IMAGEM_APLICACAO)"
if ! docker image inspect "$imagem" > /dev/null 2>&1; then
  echo "Imagem $imagem não encontrada. Carregue a imagem compilada antes de inicializar." >&2
  exit 1
fi

cd "$raiz_projeto"
docker compose --env-file "$arquivo_ambiente" -f compose.yaml -f deploy/compose.production.yaml config --quiet
docker compose --env-file "$arquivo_ambiente" -f compose.yaml -f deploy/compose.production.yaml up -d --no-build banco aplicacao proxy

echo "Contêineres iniciados. A API aplica as migrações e cria a igreja e o administrador se não existirem."
echo "Confira os logs sem expor o arquivo de ambiente:"
echo "docker compose --env-file '$arquivo_ambiente' -f compose.yaml -f deploy/compose.production.yaml logs --tail=80 aplicacao proxy"
