#!/usr/bin/env bash
set -euo pipefail

arquivo_backup="${1:?Informe o caminho do backup .dump}"
if [[ ! -s "$arquivo_backup" ]]; then
  echo "Backup não encontrado ou vazio: $arquivo_backup" >&2
  exit 1
fi

nome_container="topos-restauracao-$$"
senha_temporaria="$(openssl rand -hex 24)"

limpar() {
  docker stop "$nome_container" >/dev/null 2>&1 || true
}
trap limpar EXIT

docker run --rm -d --name "$nome_container" --network none \
  -e POSTGRES_PASSWORD="$senha_temporaria" postgres:18-alpine >/dev/null

pronto=false
for _ in {1..60}; do
  if docker logs "$nome_container" 2>&1 | grep -q 'PostgreSQL init process complete; ready for start up.' &&
     docker exec "$nome_container" pg_isready -U postgres -d postgres >/dev/null 2>&1; then
    pronto=true
    break
  fi
  sleep 1
done

if [[ "$pronto" != true ]]; then
  echo "PostgreSQL temporário não ficou pronto para restauração." >&2
  exit 1
fi

docker exec -i "$nome_container" pg_restore -U postgres -d postgres \
  --exit-on-error --no-owner < "$arquivo_backup"

docker exec "$nome_container" psql -U postgres -d postgres -tAc \
  'SELECT (SELECT COUNT(*) FROM crm.igrejas), (SELECT COUNT(*) FROM crm.usuarios)'

echo "Restauração de prova concluída em contêiner temporário sem rede."
