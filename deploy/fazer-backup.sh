#!/usr/bin/env bash
set -euo pipefail

raiz_projeto="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
arquivo_ambiente="$raiz_projeto/deploy/.env.production"
diretorio_backup="${1:-/opt/topos/backups}"

if [[ ! -f "$arquivo_ambiente" ]]; then
  echo "Arquivo de produção ausente: $arquivo_ambiente" >&2
  exit 1
fi

umask 077
install -d -m 0700 "$diretorio_backup"
arquivo_backup="$diretorio_backup/crm-ictm-$(date -u +%Y%m%dT%H%M%SZ).dump"

if [[ -e "$arquivo_backup" ]]; then
  echo "Backup já existe: $arquivo_backup" >&2
  exit 1
fi

cd "$raiz_projeto"
docker compose --env-file "$arquivo_ambiente" \
  -f compose.yaml -f deploy/compose.production.yaml \
  exec -T banco sh -c 'pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" --format=custom' \
  > "$arquivo_backup"

if [[ ! -s "$arquivo_backup" ]]; then
  echo "Backup vazio; verifique o PostgreSQL." >&2
  exit 1
fi

chmod 600 "$arquivo_backup"
echo "Backup criado em $arquivo_backup"
