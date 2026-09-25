#!/usr/bin/env bash
set -euo pipefail

raiz_projeto="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
modelo="$raiz_projeto/deploy/.env.production.example"
destino="$raiz_projeto/deploy/.env.production"

if [[ -e "$destino" ]]; then
  echo "O arquivo de produção já existe; não vou sobrescrevê-lo." >&2
  exit 1
fi

if ! command -v openssl >/dev/null 2>&1; then
  echo "Instale openssl antes de gerar as senhas." >&2
  exit 1
fi

senha_banco="$(openssl rand -hex 32)"
senha_admin="Ta!9$(openssl rand -hex 14)"

umask 077
cp "$modelo" "$destino"
sed -i "s/^POSTGRES_PASSWORD=$/POSTGRES_PASSWORD=$senha_banco/" "$destino"
sed -i "s/^ADMIN_SENHA=$/ADMIN_SENHA=$senha_admin/" "$destino"
chmod 600 "$destino"

echo "Ambiente de produção criado com senhas fortes e permissões restritas."
echo "Leia ADMIN_SENHA diretamente na VPS e guarde-a em um gerenciador de senhas."
