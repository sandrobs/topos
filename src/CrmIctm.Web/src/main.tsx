import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { App } from "./App";
import "./styles.css";

window.addEventListener("pageshow", (evento) => {
  if (evento.persisted) {
    window.location.reload();
  }
});

const enderecoAtual = new URL(window.location.href);
if (enderecoAtual.searchParams.has("atualizacao")) {
  enderecoAtual.searchParams.delete("atualizacao");
  window.history.replaceState(window.history.state, "", enderecoAtual);
}

const raiz = document.getElementById("root");

if (!raiz) {
  throw new Error("Elemento raiz da aplicação não encontrado.");
}

createRoot(raiz).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
