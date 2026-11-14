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
 * Solta o foco de um botão quando ele foi acionado por toque ou clique.
 *
 * No Safari do iPhone o botão tocado continua sendo o `activeElement`, e o
 * navegador desenha o próprio anel de foco em volta dele. Em abas de
 * navegação isso se lê como "continua selecionado", mesmo depois de abrir
 * outra seção — foi o que o responsável relatou.
 *
 * `detail` é 0 quando o clique veio do teclado (Enter ou Espaço) e maior que
 * 0 quando veio de ponteiro. Assim quem navega por teclado não perde o foco,
 * que é justamente o que o anel existe para mostrar.
 */
export function soltarFocoDePonteiro(evento: { detail: number; currentTarget: HTMLElement }) {
  if (evento.detail > 0) evento.currentTarget.blur();
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
