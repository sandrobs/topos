import type { HTMLInputTypeAttribute, ReactNode } from "react";

/**
 * Campo de formulário único do sistema.
 *
 * Substitui quatro implementações que haviam divergido: `CampoTexto` em
 * `App.tsx`, a closure `campo()` em `CadastroMembro.tsx`, `Campo` em
 * `Administracao.tsx` e os pares `<label>`+`<input>` soltos. Elas usavam
 * espaçamentos, raios e tamanhos de rótulo diferentes para o mesmo controle, e
 * só a primeira ligava a mensagem de erro ao campo por `aria-describedby`.
 *
 * A densidade menor do modal administrativo é a única variação que sobrou, e
 * agora é explícita (`denso`), não acidental.
 */

export type AtributosControle = {
  id: string;
  name: string;
  required: boolean;
  "aria-invalid": boolean;
  "aria-describedby": string | undefined;
};

type Props = {
  id: string;
  rotulo: string;
  erro?: string;
  ajuda?: string;
  obrigatorio?: boolean;
  denso?: boolean;
  className?: string;
  /** Controle próprio (select, textarea). Sem isto, renderiza um `<input>`. */
  children?: (atributos: AtributosControle) => ReactNode;
  // usados apenas quando `children` não é informado
  valor?: string;
  aoAlterar?: (valor: string) => void;
  tipo?: HTMLInputTypeAttribute;
  autoComplete?: string;
  inputMode?: "text" | "tel" | "email" | "numeric" | "decimal";
  maxLength?: number;
  placeholder?: string;
};

export function CampoFormulario({
  id, rotulo, erro, ajuda, obrigatorio = false, denso = false, className = "",
  children, valor = "", aoAlterar, tipo = "text", autoComplete, inputMode,
  maxLength, placeholder,
}: Props) {
  const erroId = `${id}-erro`;
  const ajudaId = `${id}-ajuda`;

  // A ordem importa: o leitor de tela anuncia a descrição antes do erro.
  const descricao = [ajuda ? ajudaId : null, erro ? erroId : null]
    .filter(Boolean).join(" ");

  const atributos: AtributosControle = {
    id,
    name: id,
    required: obrigatorio,
    "aria-invalid": Boolean(erro),
    "aria-describedby": descricao || undefined,
  };

  return (
    <div className={["campo", denso ? "denso" : "", className].filter(Boolean).join(" ")}>
      <label htmlFor={id}>
        {rotulo}
        {obrigatorio && <em aria-hidden="true"> *</em>}
      </label>

      {ajuda && <small id={ajudaId} className="ajuda-campo">{ajuda}</small>}

      {children ? children(atributos) : (
        <input
          {...atributos}
          type={tipo}
          value={valor}
          autoComplete={autoComplete}
          inputMode={inputMode}
          maxLength={maxLength}
          placeholder={placeholder}
          onChange={(evento) => aoAlterar?.(evento.target.value)}
        />
      )}

      {erro && <p id={erroId} className="erro-campo">{erro}</p>}
    </div>
  );
}
