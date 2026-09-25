import { useEffect, useMemo, useState, type FormEvent } from "react";
import {
  ErroDaApi,
  alterarSituacaoIgreja,
  alterarSituacaoUsuario,
  criarIgreja,
  criarUsuario,
  editarIgreja,
  editarUsuario,
  listarUsuariosParaGestao,
  obterSenhaSugerida,
  redefinirSenhaUsuario,
  type DadosIgreja,
  type IgrejaInterna,
  type NovoUsuario,
  type PerfilUsuario,
  type Sessao,
  type UsuarioGestao,
} from "./api";
import { formatarCep, formatarEndereco } from "./formatadores";

const unidadesFederativas = [
  ["AC", "Acre"], ["AL", "Alagoas"], ["AP", "Amapá"], ["AM", "Amazonas"],
  ["BA", "Bahia"], ["CE", "Ceará"], ["DF", "Distrito Federal"], ["ES", "Espírito Santo"],
  ["GO", "Goiás"], ["MA", "Maranhão"], ["MT", "Mato Grosso"], ["MS", "Mato Grosso do Sul"],
  ["MG", "Minas Gerais"], ["PA", "Pará"], ["PB", "Paraíba"], ["PR", "Paraná"],
  ["PE", "Pernambuco"], ["PI", "Piauí"], ["RJ", "Rio de Janeiro"], ["RN", "Rio Grande do Norte"],
  ["RS", "Rio Grande do Sul"], ["RO", "Rondônia"], ["RR", "Roraima"], ["SC", "Santa Catarina"],
  ["SP", "São Paulo"], ["SE", "Sergipe"], ["TO", "Tocantins"],
] as const;

const igrejaVazia: DadosIgreja = {
  nome: "",
  logradouro: "",
  numero: "",
  bairro: "",
  complemento: "",
  cep: "",
  cidade: "",
  estado: "",
};

export function GestaoIgrejas({
  igrejas,
  aoAtualizar,
}: {
  igrejas: IgrejaInterna[];
  aoAtualizar: () => Promise<void>;
}) {
  const [formulario, setFormulario] = useState<DadosIgreja>(igrejaVazia);
  const [igrejaEmEdicao, setIgrejaEmEdicao] = useState<IgrejaInterna | null>(null);
  const [formularioAberto, setFormularioAberto] = useState(false);
  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState("");
  const [mensagem, setMensagem] = useState("");

  function abrirNovo() {
    setIgrejaEmEdicao(null);
    setFormulario(igrejaVazia);
    setErro("");
    setMensagem("");
    setFormularioAberto(true);
  }

  function abrirEdicao(igreja: IgrejaInterna) {
    setIgrejaEmEdicao(igreja);
    setFormulario({
      nome: igreja.nome,
      logradouro: igreja.logradouro,
      numero: igreja.numero,
      bairro: igreja.bairro,
      complemento: igreja.complemento ?? "",
      cep: formatarCep(igreja.cep),
      cidade: igreja.cidade,
      estado: igreja.estado,
    });
    setErro("");
    setMensagem("");
    setFormularioAberto(true);
  }

  async function salvar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    setSalvando(true);
    setErro("");
    setMensagem("");
    try {
      if (igrejaEmEdicao) {
        await editarIgreja(igrejaEmEdicao.id, formulario);
        setMensagem("Dados da igreja atualizados.");
      } else {
        await criarIgreja(formulario);
        setMensagem("Igreja criada. O endereço público e o QR Code já estão disponíveis.");
      }
      await aoAtualizar();
      setFormularioAberto(false);
    } catch (falha) {
      setErro(obterMensagemErro(falha));
    } finally {
      setSalvando(false);
    }
  }

  async function alternarSituacao(igreja: IgrejaInterna) {
    const acao = igreja.ativa ? "desativar" : "ativar";
    if (!window.confirm(`Deseja ${acao} a igreja ${igreja.nome}?`)) return;

    setErro("");
    setMensagem("");
    try {
      await alterarSituacaoIgreja(igreja.id, !igreja.ativa);
      await aoAtualizar();
      setMensagem(`Igreja ${igreja.ativa ? "desativada" : "ativada"}.`);
    } catch (falha) {
      setErro(obterMensagemErro(falha));
    }
  }

  return (
    <section className="secao-administracao" aria-labelledby="titulo-igrejas">
      <header className="cabecalho-administracao">
        <div>
          <p className="rotulo-painel">Administração</p>
          <h1 id="titulo-igrejas">Igrejas</h1>
          <p>Gerencie os dados, o endereço público e a disponibilidade de cada igreja.</p>
        </div>
        <button type="button" onClick={abrirNovo}>+ Nova igreja</button>
      </header>

      {erro && <div className="alerta" role="alert">{erro}</div>}
      {mensagem && <div className="mensagem-sucesso-inline" role="status">{mensagem}</div>}

      <div className="grade-administrativa">
        {igrejas.map((igreja) => (
          <article className={`cartao-administrativo${igreja.ativa ? "" : " inativo"}`} key={igreja.id}>
            <header>
              <div><h2>{igreja.nome}</h2><span className={`selo-situacao ${igreja.ativa ? "ativo" : "inativo"}`}>{igreja.ativa ? "Ativa" : "Inativa"}</span></div>
              <small>{igreja.cidade} · {igreja.estado}</small>
            </header>
            <p className="endereco-administrativo">{formatarEndereco(igreja)}</p>
            <div className="acoes-cartao-administrativo">
              <button type="button" className="botao-secundario" onClick={() => abrirEdicao(igreja)}>Editar</button>
              <a className="botao-secundario" href={`/qrcode/${igreja.id}`} target="_blank" rel="noreferrer">Convite e QR</a>
              <button type="button" className={igreja.ativa ? "botao-perigo" : "botao-secundario"} onClick={() => void alternarSituacao(igreja)}>
                {igreja.ativa ? "Desativar" : "Ativar"}
              </button>
            </div>
          </article>
        ))}
      </div>

      {formularioAberto && (
        <div className="sobreposicao" role="presentation" onMouseDown={(evento) => evento.target === evento.currentTarget && setFormularioAberto(false)}>
          <form className="formulario-administrativo" onSubmit={salvar}>
            <header>
              <div><p>Igreja</p><h2>{igrejaEmEdicao ? "Editar cadastro" : "Nova igreja"}</h2></div>
              <button type="button" className="fechar" aria-label="Fechar" onClick={() => setFormularioAberto(false)}>×</button>
            </header>
            {erro && <div className="alerta" role="alert">{erro}</div>}
            <Campo label="Nome da igreja" valor={formulario.nome} aoAlterar={(nome) => setFormulario({ ...formulario, nome })} />
            <div className="linha-formulario linha-endereco">
              <Campo label="Rua/Avenida" valor={formulario.logradouro} aoAlterar={(logradouro) => setFormulario({ ...formulario, logradouro })} />
              <Campo label="Número" valor={formulario.numero} aoAlterar={(numero) => setFormulario({ ...formulario, numero })} />
            </div>
            <div className="linha-formulario">
              <Campo label="Bairro" valor={formulario.bairro} aoAlterar={(bairro) => setFormulario({ ...formulario, bairro })} />
              <Campo label="Complemento (opcional)" obrigatorio={false} valor={formulario.complemento ?? ""} aoAlterar={(complemento) => setFormulario({ ...formulario, complemento })} />
            </div>
            <div className="linha-formulario linha-localidade">
              <Campo label="CEP" valor={formulario.cep} placeholder="00000-000" aoAlterar={(cep) => setFormulario({ ...formulario, cep: formatarCep(cep) })} />
              <Campo label="Cidade" valor={formulario.cidade} aoAlterar={(cidade) => setFormulario({ ...formulario, cidade })} />
              <label className="campo-administrativo">UF
                <select required value={formulario.estado} onChange={(evento) => setFormulario({ ...formulario, estado: evento.target.value })}>
                  <option value="">Selecione</option>
                  {unidadesFederativas.map(([sigla, nome]) => <option key={sigla} value={sigla}>{sigla} — {nome}</option>)}
                </select>
              </label>
            </div>
            <footer>
              <button type="button" className="botao-secundario" onClick={() => setFormularioAberto(false)}>Cancelar</button>
              <button type="submit" disabled={salvando}>{salvando ? "Salvando…" : "Salvar igreja"}</button>
            </footer>
          </form>
        </div>
      )}
    </section>
  );
}

export function GestaoUsuarios({ sessao, igrejas }: { sessao: Sessao; igrejas: IgrejaInterna[] }) {
  const igrejaInicial = sessao.igrejaId ?? igrejas.find((igreja) => igreja.ativa)?.id ?? null;
  const [igrejaId, setIgrejaId] = useState<string | null>(igrejaInicial);
  const [usuarios, setUsuarios] = useState<UsuarioGestao[]>([]);
  const [usuarioEmEdicao, setUsuarioEmEdicao] = useState<UsuarioGestao | null>(null);
  const [formularioAberto, setFormularioAberto] = useState(false);
  const [nome, setNome] = useState("");
  const [email, setEmail] = useState("");
  const [perfil, setPerfil] = useState<PerfilUsuario>(igrejaInicial ? "EQUIPE" : "ADMINISTRADOR");
  const [senhaTemporaria, setSenhaTemporaria] = useState("");
  const [senhaEntregavel, setSenhaEntregavel] = useState("");
  const [usuarioRedefinindo, setUsuarioRedefinindo] = useState<UsuarioGestao | null>(null);
  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState("");
  const [mensagem, setMensagem] = useState("");

  const igrejaSelecionada = useMemo(
    () => igrejas.find((igreja) => igreja.id === igrejaId) ?? null,
    [igrejaId, igrejas],
  );

  async function carregarUsuarios() {
    setUsuarios(await listarUsuariosParaGestao(igrejaId));
  }

  useEffect(() => {
    void carregarUsuarios().catch((falha) => setErro(obterMensagemErro(falha)));
  }, [igrejaId]);

  async function sugerirSenha() {
    const sugestao = await obterSenhaSugerida();
    setSenhaTemporaria(sugestao.senha);
    return sugestao.senha;
  }

  async function abrirNovo() {
    setUsuarioEmEdicao(null);
    setNome("");
    setEmail("");
    setPerfil(igrejaId ? "EQUIPE" : "ADMINISTRADOR");
    setSenhaTemporaria("");
    setErro("");
    setMensagem("");
    setFormularioAberto(true);
    try {
      await sugerirSenha();
    } catch (falha) {
      setErro(obterMensagemErro(falha));
    }
  }

  function abrirEdicao(usuario: UsuarioGestao) {
    setUsuarioEmEdicao(usuario);
    setNome(usuario.nome);
    setEmail(usuario.email);
    setPerfil(usuario.perfil);
    setSenhaTemporaria("");
    setErro("");
    setMensagem("");
    setFormularioAberto(true);
  }

  async function salvar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    setSalvando(true);
    setErro("");
    setMensagem("");
    try {
      if (usuarioEmEdicao) {
        await editarUsuario(usuarioEmEdicao.id, { nome, email, perfil });
        setMensagem("Dados do usuário atualizados.");
      } else {
        const dados: NovoUsuario = { nome, email, perfil, igrejaId, senhaTemporaria };
        await criarUsuario(dados);
        setSenhaEntregavel(senhaTemporaria);
        setMensagem("Usuário criado. Copie a senha temporária e entregue-a por um canal seguro.");
      }
      await carregarUsuarios();
      setFormularioAberto(false);
    } catch (falha) {
      setErro(obterMensagemErro(falha));
    } finally {
      setSalvando(false);
    }
  }

  async function alternarSituacao(usuario: UsuarioGestao) {
    const acao = usuario.ativo ? "desativar" : "ativar";
    if (!window.confirm(`Deseja ${acao} o usuário ${usuario.nome}?`)) return;
    setErro("");
    setMensagem("");
    try {
      await alterarSituacaoUsuario(usuario.id, !usuario.ativo);
      await carregarUsuarios();
      setMensagem(`Usuário ${usuario.ativo ? "desativado" : "ativado"}.`);
    } catch (falha) {
      setErro(obterMensagemErro(falha));
    }
  }

  async function abrirRedefinicao(usuario: UsuarioGestao) {
    setUsuarioRedefinindo(usuario);
    setErro("");
    setMensagem("");
    try {
      await sugerirSenha();
    } catch (falha) {
      setErro(obterMensagemErro(falha));
    }
  }

  async function confirmarRedefinicao() {
    if (!usuarioRedefinindo || !senhaTemporaria) return;
    setSalvando(true);
    setErro("");
    try {
      await redefinirSenhaUsuario(usuarioRedefinindo.id, senhaTemporaria);
      setSenhaEntregavel(senhaTemporaria);
      setMensagem(`Nova senha temporária definida para ${usuarioRedefinindo.nome}.`);
      setUsuarioRedefinindo(null);
      await carregarUsuarios();
    } catch (falha) {
      setErro(obterMensagemErro(falha));
    } finally {
      setSalvando(false);
    }
  }

  async function copiarSenha() {
    try {
      await navigator.clipboard.writeText(senhaEntregavel || senhaTemporaria);
      setMensagem("Senha copiada.");
    } catch {
      setMensagem("Selecione a senha e copie manualmente.");
    }
  }

  return (
    <section className="secao-administracao" aria-labelledby="titulo-usuarios">
      <header className="cabecalho-administracao">
        <div>
          <p className="rotulo-painel">Administração</p>
          <h1 id="titulo-usuarios">Usuários</h1>
          <p>Gerencie acessos, perfis e senhas temporárias.</p>
        </div>
        <button type="button" onClick={() => void abrirNovo()}>+ Novo usuário</button>
      </header>

      {sessao.perfil === "ADMINISTRADOR" && (
        <label className="filtro-administrativo">Grupo de usuários
          <select value={igrejaId ?? ""} onChange={(evento) => {
            const novoId = evento.target.value || null;
            setIgrejaId(novoId);
            setPerfil(novoId ? "EQUIPE" : "ADMINISTRADOR");
            setSenhaEntregavel("");
          }}>
            <option value="">Administradores da plataforma</option>
            {igrejas.map((igreja) => <option key={igreja.id} value={igreja.id}>{igreja.nome}{igreja.ativa ? "" : " — inativa"}</option>)}
          </select>
        </label>
      )}

      {erro && <div className="alerta" role="alert">{erro}</div>}
      {mensagem && <div className="mensagem-sucesso-inline" role="status">{mensagem}</div>}
      {senhaEntregavel && (
        <div className="senha-entregavel">
          <div><strong>Senha temporária</strong><small>Ela não poderá ser consultada novamente depois que você sair desta tela.</small></div>
          <input readOnly value={senhaEntregavel} aria-label="Senha temporária gerada" />
          <button type="button" className="botao-secundario" onClick={() => void copiarSenha()}>Copiar senha</button>
          <button type="button" className="fechar" aria-label="Ocultar senha" onClick={() => setSenhaEntregavel("")}>×</button>
        </div>
      )}

      <div className="lista-usuarios">
        {usuarios.map((usuario) => (
          <article className={`cartao-usuario${usuario.ativo ? "" : " inativo"}`} key={usuario.id}>
            <div className="avatar-usuario" aria-hidden="true">{usuario.nome.slice(0, 1).toUpperCase()}</div>
            <div className="dados-usuario">
              <div><strong>{usuario.nome}</strong><span className={`selo-situacao ${usuario.ativo ? "ativo" : "inativo"}`}>{usuario.ativo ? "Ativo" : "Inativo"}</span></div>
              <span>{usuario.email}</span>
              <small>{nomePerfil(usuario.perfil)}{usuario.deveTrocarSenha ? " · troca de senha pendente" : ""}</small>
            </div>
            <div className="acoes-usuario">
              <button type="button" className="botao-secundario" onClick={() => abrirEdicao(usuario)}>Editar</button>
              <button type="button" className="botao-secundario" onClick={() => void abrirRedefinicao(usuario)}>Nova senha</button>
              <button type="button" className={usuario.ativo ? "botao-perigo" : "botao-secundario"} onClick={() => void alternarSituacao(usuario)}>{usuario.ativo ? "Desativar" : "Ativar"}</button>
            </div>
          </article>
        ))}
        {usuarios.length === 0 && <p className="lista-vazia-administrativa">Nenhum usuário cadastrado neste grupo.</p>}
      </div>

      {formularioAberto && (
        <div className="sobreposicao" role="presentation" onMouseDown={(evento) => evento.target === evento.currentTarget && setFormularioAberto(false)}>
          <form className="formulario-administrativo formulario-usuario" onSubmit={salvar}>
            <header>
              <div><p>Usuário</p><h2>{usuarioEmEdicao ? "Editar acesso" : "Novo acesso"}</h2></div>
              <button type="button" className="fechar" aria-label="Fechar" onClick={() => setFormularioAberto(false)}>×</button>
            </header>
            {erro && <div className="alerta" role="alert">{erro}</div>}
            <Campo label="Nome completo" valor={nome} aoAlterar={setNome} />
            <Campo label="E-mail de acesso" tipo="email" valor={email} aoAlterar={setEmail} />
            <label className="campo-administrativo">Perfil
              <select value={perfil} onChange={(evento) => setPerfil(evento.target.value as PerfilUsuario)}>
                {igrejaId ? (
                  <><option value="EQUIPE">Equipe</option><option value="PASTOR">Pastor</option></>
                ) : (
                  <option value="ADMINISTRADOR">Administrador da plataforma</option>
                )}
              </select>
            </label>
            {igrejaSelecionada && <p className="contexto-formulario">Igreja: <strong>{igrejaSelecionada.nome}</strong></p>}
            {!usuarioEmEdicao && (
              <label className="campo-administrativo">Senha temporária
                <div className="campo-com-acao">
                  <input required value={senhaTemporaria} onChange={(evento) => setSenhaTemporaria(evento.target.value)} />
                  <button type="button" className="botao-secundario" onClick={() => void sugerirSenha()}>Sugerir outra</button>
                </div>
                <small>O usuário deverá trocá-la no primeiro acesso.</small>
              </label>
            )}
            <footer>
              <button type="button" className="botao-secundario" onClick={() => setFormularioAberto(false)}>Cancelar</button>
              <button type="submit" disabled={salvando}>{salvando ? "Salvando…" : "Salvar usuário"}</button>
            </footer>
          </form>
        </div>
      )}

      {usuarioRedefinindo && (
        <div className="sobreposicao" role="presentation" onMouseDown={(evento) => evento.target === evento.currentTarget && setUsuarioRedefinindo(null)}>
          <section className="formulario-administrativo formulario-senha" role="dialog" aria-modal="true" aria-labelledby="titulo-redefinir-senha">
            <header>
              <div><p>Redefinir acesso</p><h2 id="titulo-redefinir-senha">Nova senha para {usuarioRedefinindo.nome}</h2></div>
              <button type="button" className="fechar" aria-label="Fechar" onClick={() => setUsuarioRedefinindo(null)}>×</button>
            </header>
            {erro && <div className="alerta" role="alert">{erro}</div>}
            <label className="campo-administrativo">Nova senha temporária
              <div className="campo-com-acao">
                <input value={senhaTemporaria} onChange={(evento) => setSenhaTemporaria(evento.target.value)} />
                <button type="button" className="botao-secundario" onClick={() => void sugerirSenha()}>Sugerir outra</button>
              </div>
            </label>
            <p>Após a confirmação, a senha anterior deixará de funcionar e o usuário deverá trocar esta senha no próximo acesso.</p>
            <footer>
              <button type="button" className="botao-secundario" onClick={() => setUsuarioRedefinindo(null)}>Cancelar</button>
              <button type="button" disabled={salvando || !senhaTemporaria} onClick={() => void confirmarRedefinicao()}>{salvando ? "Redefinindo…" : "Confirmar nova senha"}</button>
            </footer>
          </section>
        </div>
      )}
    </section>
  );
}

function Campo({
  label,
  valor,
  aoAlterar,
  obrigatorio = true,
  tipo = "text",
  placeholder,
}: {
  label: string;
  valor: string;
  aoAlterar: (valor: string) => void;
  obrigatorio?: boolean;
  tipo?: "text" | "email";
  placeholder?: string;
}) {
  return (
    <label className="campo-administrativo">{label}
      <input type={tipo} required={obrigatorio} value={valor} placeholder={placeholder} onChange={(evento) => aoAlterar(evento.target.value)} />
    </label>
  );
}

function obterMensagemErro(falha: unknown) {
  if (falha instanceof ErroDaApi) {
    const primeiraValidacao = Object.values(falha.erros).flat()[0];
    return primeiraValidacao ?? falha.message;
  }
  return falha instanceof Error ? falha.message : "Não foi possível concluir a operação.";
}

function nomePerfil(perfil: PerfilUsuario) {
  if (perfil === "ADMINISTRADOR") return "Administrador da plataforma";
  if (perfil === "PASTOR") return "Pastor";
  return "Equipe";
}
