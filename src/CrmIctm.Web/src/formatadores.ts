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
