namespace CrmIctm.Api.Dominio;

// Os mesmos dados são preservados na solicitação e copiados para o membro na aprovação.
public sealed class DadosCadastraisMembro
{
    private DadosCadastraisMembro() { }

    public string Nome { get; private set; } = string.Empty;
    public string Sobrenome { get; private set; } = string.Empty;
    public string? Whatsapp { get; private set; }
    public string? Email { get; private set; }
    public DateOnly? DataNascimento { get; private set; }
    public string? Logradouro { get; private set; }
    public string? Numero { get; private set; }
    public string? Bairro { get; private set; }
    public string? Complemento { get; private set; }
    public string? Cep { get; private set; }
    public string? Cidade { get; private set; }
    public string? Estado { get; private set; }
    public string SituacaoBatismo { get; private set; } = "NAO_INFORMADO";
    public DateOnly? DataBatismo { get; private set; }
    public bool EhMenor { get; private set; }
    public string? NomeResponsavelLegal { get; private set; }
    public string? WhatsappResponsavelLegal { get; private set; }
    public string? VinculoResponsavelLegal { get; private set; }

    public static DadosCadastraisMembro Criar(
        string nome, string sobrenome, string? whatsapp, string? email,
        DateOnly? dataNascimento, string? logradouro, string? numero,
        string? bairro, string? complemento, string? cep, string? cidade,
        string? estado, string situacaoBatismo, DateOnly? dataBatismo,
        bool ehMenor = false, string? nomeResponsavelLegal = null,
        string? whatsappResponsavelLegal = null, string? vinculoResponsavelLegal = null)
    {
        return new DadosCadastraisMembro
        {
            Nome = nome.Trim(),
            Sobrenome = sobrenome.Trim(),
            Whatsapp = Limpar(whatsapp),
            Email = Limpar(email),
            DataNascimento = dataNascimento,
            Logradouro = Limpar(logradouro),
            Numero = Limpar(numero),
            Bairro = Limpar(bairro),
            Complemento = Limpar(complemento),
            Cep = Limpar(cep),
            Cidade = Limpar(cidade),
            Estado = Limpar(estado)?.ToUpperInvariant(),
            SituacaoBatismo = situacaoBatismo,
            DataBatismo = dataBatismo,
            EhMenor = ehMenor,
            NomeResponsavelLegal = Limpar(nomeResponsavelLegal),
            WhatsappResponsavelLegal = Limpar(whatsappResponsavelLegal),
            VinculoResponsavelLegal = Limpar(vinculoResponsavelLegal)
        };
    }

    public DadosCadastraisMembro Copiar() => Criar(
        Nome, Sobrenome, Whatsapp, Email, DataNascimento, Logradouro,
        Numero, Bairro, Complemento, Cep, Cidade, Estado, SituacaoBatismo,
        DataBatismo, EhMenor, NomeResponsavelLegal, WhatsappResponsavelLegal,
        VinculoResponsavelLegal);

    private static string? Limpar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
