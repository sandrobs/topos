#!/usr/bin/env python3
"""Confere o login inicial pela URL HTTPS sem imprimir a senha ou o cookie."""

import http.cookiejar
import json
import urllib.request
from pathlib import Path


def ler_configuracao(nome: str) -> str:
    arquivo = Path(__file__).with_name(".env.production")
    for linha in arquivo.read_text(encoding="utf-8").splitlines():
        if linha.startswith(f"{nome}="):
            return linha.split("=", 1)[1]
    raise RuntimeError(f"Configuração ausente: {nome}")


origem = f"https://{ler_configuracao('DOMINIO_GESTAO')}"
email = ler_configuracao("ADMIN_EMAIL")
senha = ler_configuracao("ADMIN_SENHA")
cliente = urllib.request.build_opener(
    urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar())
)

with cliente.open(f"{origem}/api/seguranca/token-csrf", timeout=15) as resposta:
    token = json.load(resposta)["token"]

requisicao = urllib.request.Request(
    f"{origem}/api/autenticacao/entrar",
    data=json.dumps({"email": email, "senha": senha}).encode("utf-8"),
    headers={"Content-Type": "application/json", "X-CSRF-TOKEN": token},
    method="POST",
)
with cliente.open(requisicao, timeout=15) as resposta:
    if resposta.status != 204:
        raise RuntimeError(f"Login inesperado: HTTP {resposta.status}")

with cliente.open(f"{origem}/api/autenticacao/sessao", timeout=15) as resposta:
    sessao = json.load(resposta)

if sessao["email"].lower() != email.lower() or sessao["perfil"] != "ADMINISTRADOR":
    raise RuntimeError("A sessão inicial não pertence ao administrador esperado.")

with cliente.open(f"{origem}/api/igrejas", timeout=15) as resposta:
    igrejas = json.load(resposta)

if len(igrejas) != 1 or not igrejas[0]["ativa"]:
    raise RuntimeError("A igreja inicial não foi encontrada ou não está ativa.")

identificador = igrejas[0]["identificadorPublico"]
dominio_visita = ler_configuracao("DOMINIO_VISITA")
with cliente.open(
    f"https://{dominio_visita}/api/publico/igrejas/{identificador}", timeout=15
) as resposta:
    igreja_publica = json.load(resposta)

if igreja_publica["nome"] != igrejas[0]["nome"]:
    raise RuntimeError("O formulário público não corresponde à igreja inicial.")

print("Login HTTPS, igreja inicial e API pública de visitantes validados.")
print(f"Formulário inicial: https://{dominio_visita}/visita/{identificador}")
