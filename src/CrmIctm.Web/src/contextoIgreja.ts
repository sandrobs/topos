const chaveIgrejaSelecionada = "crm-ictm:igreja-selecionada";
const prefixoUltimaIgrejaPorUsuario = "crm-ictm:ultima-igreja";

export function obterIgrejaSelecionadaDoEndereco() {
  return new URLSearchParams(window.location.search).get("igrejaId")?.trim() || null;
}

export function obterIgrejaSelecionadaDaSessao() {
  try {
    return window.sessionStorage.getItem(chaveIgrejaSelecionada);
  } catch {
    return null;
  }
}

export function obterUltimaIgrejaSelecionadaDoUsuario(usuarioId: string) {
  try {
    return window.localStorage.getItem(`${prefixoUltimaIgrejaPorUsuario}:${usuarioId}`);
  } catch {
    return null;
  }
}

export function salvarIgrejaSelecionada(
  igrejaId: string,
  atualizarEndereco = false,
  usuarioId?: string,
) {
  try {
    window.sessionStorage.setItem(chaveIgrejaSelecionada, igrejaId);
  } catch {
    // A URL ainda preserva o contexto quando o armazenamento do navegador está indisponível.
  }

  if (usuarioId) {
    try {
      window.localStorage.setItem(`${prefixoUltimaIgrejaPorUsuario}:${usuarioId}`, igrejaId);
    } catch {
      // A sessão e a URL continuam preservando o contexto neste navegador.
    }
  }

  if (atualizarEndereco && window.location.pathname === "/painel") {
    const endereco = new URL(window.location.href);
    endereco.searchParams.set("igrejaId", igrejaId);
    window.history.replaceState(window.history.state, "", endereco);
  }
}

export function enderecoPainelDaIgreja(igrejaId: string) {
  return `/painel?igrejaId=${encodeURIComponent(igrejaId)}`;
}
