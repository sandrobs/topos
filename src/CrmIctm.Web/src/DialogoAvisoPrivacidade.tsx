import { useEffect, useRef } from "react";

export function DialogoAvisoPrivacidade({ aberto, nomeIgreja, texto, aoFechar }: {
  aberto: boolean; nomeIgreja: string; texto: string; aoFechar: () => void;
}) {
  const dialogoRef = useRef<HTMLDialogElement>(null);
  useEffect(() => {
    const dialogo = dialogoRef.current;
    if (aberto && dialogo && !dialogo.open) dialogo.showModal();
    else if (!aberto && dialogo?.open) dialogo.close();
  }, [aberto]);

  return <dialog ref={dialogoRef} id="dialogo-aviso-privacidade"
    className="dialogo-privacidade" aria-labelledby="titulo-aviso-privacidade"
    aria-describedby="introducao-aviso-privacidade"
    onCancel={aoFechar} onClose={aoFechar}
    onClick={(evento) => { if (evento.target === evento.currentTarget) aoFechar(); }}>
    <div className="conteudo-dialogo-privacidade">
      <header><div><p className="rotulo">Seus dados, sua segurança</p>
        <h2 id="titulo-aviso-privacidade">Segurança e privacidade</h2></div>
        <button type="button" className="fechar-aviso-privacidade"
          aria-label="Fechar aviso de privacidade" onClick={aoFechar}>×</button>
      </header>
      <p id="introducao-aviso-privacidade" className="introducao-aviso-privacidade">
        Veja como a {nomeIgreja} utiliza e protege as informações deste formulário.
      </p>
      <div className="texto-aviso-privacidade">
        {texto.split(/\n\s*\n/).map((paragrafo) => <p key={paragrafo}>{paragrafo}</p>)}
      </div>
      <button type="button" className="entendi-aviso-privacidade" onClick={aoFechar}>
        Entendi e quero voltar ao formulário
      </button>
    </div>
  </dialog>;
}
