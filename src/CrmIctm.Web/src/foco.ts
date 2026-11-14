/**
 * Leva o usuário até o primeiro erro de validação.
 *
 * Sem isto, num celular, quem rola até o fim de um formulário longo e toca em
 * enviar não vê o que impediu o envio: medimos os erros de nome e WhatsApp a
 * 231 px e 112 px acima da área visível, com o foco parado no <body>.
 *
 * Prefere focar o primeiro campo marcado como inválido, porque isso move o
 * foco do teclado e o cursor do leitor de tela de uma vez. Quando o erro não
 * pertence a um campo focável — um grupo de opções ou o aviso de privacidade —,
 * apenas rola até a mensagem.
 */
/**
 * Última forma de interação do usuário.
 *
 * Serve para distinguir "clicou/tocou" de "acionou pelo teclado". A primeira
 * tentativa usava `evento.detail`, que é 0 no teclado e maior que 0 no mouse
 * — mas no Safari do iPhone o clique sintetizado a partir do toque também
 * chega com `detail: 0`, então a verificação nunca disparava justamente no
 * aparelho que ela deveria atender.
 *
 * Eventos de ponteiro não existem na navegação por teclado, então observar
 * qual veio por último é confiável em qualquer navegador.
 */
let modalidade: "ponteiro" | "teclado" = "teclado";

if (typeof document !== "undefined") {
  document.addEventListener("pointerdown", () => { modalidade = "ponteiro"; }, true);
  document.addEventListener("touchstart", () => { modalidade = "ponteiro"; }, { capture: true, passive: true });
  document.addEventListener("keydown", () => { modalidade = "teclado"; }, true);
}

/**
 * Solta o foco de um botão acionado por toque ou clique.
 *
 * No Safari do iPhone o botão tocado continua sendo o `activeElement` e o
 * navegador desenha o anel de foco dele em volta. Numa barra de abas isso se
 * lê como "continua selecionado" mesmo depois de abrir outra seção.
 *
 * Pelo teclado o foco permanece, porque é exatamente aí que o anel precisa
 * aparecer.
 */
export function soltarFocoDePonteiro(evento: { currentTarget: HTMLElement }) {
  if (modalidade === "ponteiro") evento.currentTarget.blur();
}

export function focarPrimeiroErro(formulario: HTMLFormElement | null) {
  if (!formulario) return;

  requestAnimationFrame(() => {
    const campo = formulario.querySelector<HTMLElement>('[aria-invalid="true"]');
    if (campo) {
      campo.focus({ preventScroll: true });
      campo.scrollIntoView({ block: "center", behavior: "smooth" });
      return;
    }

    formulario
      .querySelector<HTMLElement>(".erro-campo")
      ?.scrollIntoView({ block: "center", behavior: "smooth" });
  });
}
