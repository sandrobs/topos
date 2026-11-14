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
