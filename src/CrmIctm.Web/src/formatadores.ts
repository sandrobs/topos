type EnderecoIgreja = {
  logradouro: string;
  numero: string;
  bairro: string;
  complemento: string | null;
  cep: string;
  cidade: string;
  estado: string;
};

export function formatarCep(valor: string) {
  const digitos = valor.replace(/\D/g, "").slice(0, 8);
  return digitos.length > 5 ? `${digitos.slice(0, 5)}-${digitos.slice(5)}` : digitos;
}

export function formatarTelefoneBrasileiro(valor: string) {
  let digitos = valor.replace(/\D/g, "");
  if (digitos.startsWith("55") && digitos.length > 11) {
    digitos = digitos.slice(2);
  }

  digitos = digitos.slice(0, 11);
  if (!digitos) return "";
  if (digitos.length <= 2) return `(${digitos}`;

  const ddd = digitos.slice(0, 2);
  const numero = digitos.slice(2);
  const tamanhoPrefixo = numero.length > 8 ? 5 : 4;
  if (numero.length <= tamanhoPrefixo) return `(${ddd}) ${numero}`;

  return `(${ddd}) ${numero.slice(0, tamanhoPrefixo)}-${numero.slice(tamanhoPrefixo)}`;
}

export function formatarEndereco(igreja: EnderecoIgreja) {
  if (!igreja.logradouro || !igreja.numero || !igreja.bairro || !igreja.cep) {
    return `${igreja.cidade}/${igreja.estado}`;
  }

  const complemento = igreja.complemento ? `, ${igreja.complemento}` : "";
  return `${igreja.logradouro}, ${igreja.numero}${complemento} · ${igreja.bairro} · ${igreja.cidade}/${igreja.estado} · CEP ${formatarCep(igreja.cep)}`;
}

/**
 * Data ISO sem hora (AAAA-MM-DD). Não usa `new Date`: a string seria
 * interpretada como UTC e exibiria o dia anterior em fusos negativos.
 */
export function formatarDataIso(valor: string | null | undefined) {
  if (!valor) return "";
  const partes = /^(\d{4})-(\d{2})-(\d{2})/.exec(valor);
  if (!partes) return valor;
  return `${partes[3]}/${partes[2]}/${partes[1]}`;
}

/** Instante ISO completo, apresentado no fuso do navegador. */
export function formatarDataHora(valor: string) {
  return new Intl.DateTimeFormat("pt-BR", { dateStyle: "short", timeStyle: "short" })
    .format(new Date(valor));
}

function capitalizar(valor: string) {
  const texto = valor.toLocaleLowerCase("pt-BR");
  return texto.charAt(0).toLocaleUpperCase("pt-BR") + texto.slice(1);
}

const rotulosSituacaoMembro: Record<string, string> = {
  ATIVO: "Ativo",
  INATIVO: "Inativo",
};

export function rotuloSituacaoMembro(valor: string) {
  return rotulosSituacaoMembro[valor] ?? capitalizar(valor);
}

const rotulosAcaoHistorico: Record<string, string> = {
  CRIADO: "Cadastro criado",
  EDITADO: "Dados editados",
  INATIVADO: "Membro inativado",
  REATIVADO: "Membro reativado",
  SOLICITACAO_CORRIGIDA: "Solicitação corrigida",
  SOLICITACAO_APROVADA: "Solicitação aprovada",
  SOLICITACAO_VINCULADA: "Vinculada a membro existente",
  SOLICITACAO_RECUSADA: "Solicitação recusada",
};

export function rotuloAcaoHistorico(valor: string) {
  return rotulosAcaoHistorico[valor] ?? capitalizar(valor.replaceAll("_", " "));
}

/** "Sandro Bueno" vira "SB". Usado nos avatares do painel. */
export function iniciais(nome: string | undefined) {
  if (!nome) return "";
  const partes = nome.trim().split(/\s+/);
  const primeira = partes[0]?.[0] ?? "";
  const ultima = partes.length > 1 ? (partes[partes.length - 1]?.[0] ?? "") : "";
  return (primeira + ultima).toLocaleUpperCase("pt-BR");
}

const rotulosFuncaoMembro: Record<string, string> = {
  MEMBRO: "Membro",
  DIACONO: "Diácono",
  PRESBITERO: "Presbítero",
  PASTOR: "Pastor",
};

export function rotuloFuncaoMembro(valor: string) {
  return rotulosFuncaoMembro[valor] ?? capitalizar(valor);
}

/*
 * Datas sem hora, por extenso.
 *
 * Mesmo motivo de `formatarDataIso`: `new Date("2026-11-15")` é lido como UTC e
 * mostra o dia anterior em fuso negativo. Aqui a data é montada com
 * `Date.UTC` a partir dos números e lida de volta em UTC, então nenhum fuso
 * entra na conta.
 */
function dataUtc(valor: string) {
  const partes = /^(\d{4})-(\d{2})-(\d{2})/.exec(valor);
  if (!partes) return null;
  return new Date(Date.UTC(Number(partes[1]), Number(partes[2]) - 1, Number(partes[3])));
}

function formatar(data: Date, opcoes: Intl.DateTimeFormatOptions) {
  return new Intl.DateTimeFormat("pt-BR", { ...opcoes, timeZone: "UTC" }).format(data);
}

/** "Domingo, 15 de novembro" */
export function formatarDiaExtenso(valor: string) {
  const data = dataUtc(valor);
  if (!data) return valor;
  return capitalizar(formatar(data, { weekday: "long", day: "numeric", month: "long" }));
}

/** "Novembro de 2026" */
export function formatarMesExtenso(valor: string) {
  const data = dataUtc(valor);
  if (!data) return valor;
  return capitalizar(formatar(data, { month: "long", year: "numeric" }));
}

/** Só o nome do mês, para o rótulo da aba: "Novembro" */
export function nomeDoMes(valor: string) {
  const data = dataUtc(valor);
  if (!data) return valor;
  return capitalizar(formatar(data, { month: "long" }));
}

/**
 * "15 a 21 de novembro" quando o intervalo fica no mesmo mês, e
 * "29 de novembro a 5 de dezembro" quando atravessa — o que acontece em uma
 * semana de cada quatro.
 */
export function formatarIntervaloDeDias(inicio: string, fim: string) {
  const de = dataUtc(inicio);
  const ate = dataUtc(fim);
  if (!de || !ate) return `${formatarDataIso(inicio)} a ${formatarDataIso(fim)}`;

  const diaDe = formatar(de, { day: "numeric" });
  const diaAte = formatar(ate, { day: "numeric" });
  const mesDe = formatar(de, { month: "long" });
  const mesAte = formatar(ate, { month: "long" });

  return mesDe === mesAte
    ? `${diaDe} a ${diaAte} de ${mesAte}`
    : `${diaDe} de ${mesDe} a ${diaAte} de ${mesAte}`;
}
