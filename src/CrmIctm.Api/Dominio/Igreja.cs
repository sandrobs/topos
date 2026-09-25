using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace CrmIctm.Api.Dominio;

public sealed class Igreja
{
    private Igreja()
    {
    }

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Logradouro { get; private set; } = string.Empty;
    public string Numero { get; private set; } = string.Empty;
    public string Bairro { get; private set; } = string.Empty;
    public string? Complemento { get; private set; }
    public string Cep { get; private set; } = string.Empty;
    public string Cidade { get; private set; } = string.Empty;
    public string Estado { get; private set; } = string.Empty;
    public bool Ativa { get; private set; }
    public string IdentificadorPublico { get; private set; } = string.Empty;
    public string IdentificadorPublicoMembros { get; private set; } = string.Empty;
    public DateTimeOffset CriadaEm { get; private set; }
    public DateTimeOffset AtualizadaEm { get; private set; }

    public static Igreja Criar(
        string nome,
        string logradouro,
        string numero,
        string bairro,
        string? complemento,
        string cep,
        string cidade,
        string estado,
        DateTimeOffset agora)
    {
        ValidarDados(nome, logradouro, numero, bairro, cep, cidade, estado);

        return new Igreja
        {
            Id = Guid.NewGuid(),
            Nome = nome.Trim(),
            Logradouro = logradouro.Trim(),
            Numero = numero.Trim(),
            Bairro = bairro.Trim(),
            Complemento = NormalizarComplemento(complemento),
            Cep = NormalizarCep(cep),
            Cidade = cidade.Trim(),
            Estado = estado.Trim().ToUpperInvariant(),
            Ativa = true,
            IdentificadorPublico = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(18)),
            IdentificadorPublicoMembros = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(18)),
            CriadaEm = agora,
            AtualizadaEm = agora
        };
    }

    public void AtualizarDados(
        string nome,
        string logradouro,
        string numero,
        string bairro,
        string? complemento,
        string cep,
        string cidade,
        string estado,
        DateTimeOffset agora)
    {
        ValidarDados(nome, logradouro, numero, bairro, cep, cidade, estado);

        Nome = nome.Trim();
        Logradouro = logradouro.Trim();
        Numero = numero.Trim();
        Bairro = bairro.Trim();
        Complemento = NormalizarComplemento(complemento);
        Cep = NormalizarCep(cep);
        Cidade = cidade.Trim();
        Estado = estado.Trim().ToUpperInvariant();
        AtualizadaEm = agora;
    }

    public void AlterarSituacao(bool ativa, DateTimeOffset agora)
    {
        Ativa = ativa;
        AtualizadaEm = agora;
    }

    private static void ValidarDados(
        string nome,
        string logradouro,
        string numero,
        string bairro,
        string cep,
        string cidade,
        string estado)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        ArgumentException.ThrowIfNullOrWhiteSpace(logradouro);
        ArgumentException.ThrowIfNullOrWhiteSpace(numero);
        ArgumentException.ThrowIfNullOrWhiteSpace(bairro);
        ArgumentException.ThrowIfNullOrWhiteSpace(cidade);

        if (NormalizarCep(cep).Length != 8)
        {
            throw new ArgumentException("O CEP deve conter oito dígitos.", nameof(cep));
        }

        if (!UnidadesFederativas.EhValida(estado))
        {
            throw new ArgumentException("Informe uma UF brasileira válida.", nameof(estado));
        }
    }

    private static string NormalizarCep(string cep) => new(cep.Where(char.IsDigit).ToArray());

    private static string? NormalizarComplemento(string? complemento) =>
        string.IsNullOrWhiteSpace(complemento) ? null : complemento.Trim();
}
