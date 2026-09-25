using System.ComponentModel.DataAnnotations;
using CrmIctm.Api.Contratos;
using CrmIctm.Api.Dominio;

namespace CrmIctm.Api.Servicos;

public static class ValidacaoDadosMembro
{
    public static Dictionary<string, string[]> Validar(
        DadosMembroRequisicao? dados, bool formularioPublico, DateOnly hoje)
    {
        var erros = new Dictionary<string, string[]>();
        if (dados is null)
        {
            erros["dados"] = ["Informe os dados do membro."];
            return erros;
        }

        Texto(erros, "nome", dados.Nome, 100, true);
        Texto(erros, "sobrenome", dados.Sobrenome, 100, true);
        Texto(erros, "logradouro", dados.Logradouro, 150);
        Texto(erros, "numero", dados.Numero, 20);
        Texto(erros, "bairro", dados.Bairro, 100);
        Texto(erros, "complemento", dados.Complemento, 100);
        Texto(erros, "cidade", dados.Cidade, 100);

        if (formularioPublico && dados.EhMenor)
            erros["ehMenor"] = ["O cadastro de menores é realizado pela equipe da igreja."];

        if (!dados.EhMenor || !string.IsNullOrWhiteSpace(dados.Whatsapp))
        {
            if (!NormalizadorWhatsapp.TentarNormalizar(dados.Whatsapp, out _))
                erros["whatsapp"] = ["Informe um WhatsApp brasileiro válido, com DDD."];
        }

        if (dados.EhMenor)
        {
            Texto(erros, "nomeResponsavelLegal", dados.NomeResponsavelLegal, 150, true);
            Texto(erros, "vinculoResponsavelLegal", dados.VinculoResponsavelLegal, 60, true);
            if (!NormalizadorWhatsapp.TentarNormalizar(dados.WhatsappResponsavelLegal, out _))
                erros["whatsappResponsavelLegal"] = ["Informe o WhatsApp do responsável legal com DDD."];
        }

        if (!string.IsNullOrWhiteSpace(dados.Email) &&
            (dados.Email.Trim().Length > 254 || !new EmailAddressAttribute().IsValid(dados.Email.Trim())))
            erros["email"] = ["Informe um e-mail válido de até 254 caracteres."];

        if (!string.IsNullOrWhiteSpace(dados.Cep) &&
            new string(dados.Cep.Where(char.IsAsciiDigit).ToArray()).Length != 8)
            erros["cep"] = ["Informe um CEP válido com oito dígitos."];

        if (!string.IsNullOrWhiteSpace(dados.Estado) && !UnidadesFederativas.EhValida(dados.Estado))
            erros["estado"] = ["Selecione uma UF brasileira válida."];

        if (dados.DataNascimento > hoje)
            erros["dataNascimento"] = ["A data de nascimento não pode ser futura."];
        if (formularioPublico && dados.DataNascimento is { } nascimento &&
            nascimento > hoje.AddYears(-18))
            erros["dataNascimento"] = ["O formulário público é destinado a adultos."];
        if (dados.DataBatismo > hoje)
            erros["dataBatismo"] = ["A data de batismo não pode ser futura."];

        var situacao = dados.SituacaoBatismo ?? "NAO_INFORMADO";
        if (situacao is not ("NAO_INFORMADO" or "BATIZADO" or "NAO_BATIZADO"))
            erros["situacaoBatismo"] = ["Selecione uma situação de batismo válida."];
        if (dados.DataBatismo is not null && situacao != "BATIZADO")
            erros["dataBatismo"] = ["Selecione Batizado para informar a data."];

        return erros;
    }

    public static DadosCadastraisMembro Criar(DadosMembroRequisicao dados)
    {
        string? NormalizarTelefone(string? valor) =>
            NormalizadorWhatsapp.TentarNormalizar(valor, out var telefone) ? telefone : null;

        return DadosCadastraisMembro.Criar(
            dados.Nome!, dados.Sobrenome!, NormalizarTelefone(dados.Whatsapp),
            dados.Email?.Trim().ToLowerInvariant(), dados.DataNascimento,
            dados.Logradouro, dados.Numero, dados.Bairro, dados.Complemento,
            string.IsNullOrWhiteSpace(dados.Cep) ? null :
                new string(dados.Cep.Where(char.IsAsciiDigit).ToArray()),
            dados.Cidade, dados.Estado, dados.SituacaoBatismo ?? "NAO_INFORMADO",
            dados.DataBatismo, dados.EhMenor, dados.NomeResponsavelLegal,
            NormalizarTelefone(dados.WhatsappResponsavelLegal), dados.VinculoResponsavelLegal);
    }

    private static void Texto(Dictionary<string, string[]> erros, string campo,
        string? valor, int maximo, bool obrigatorio = false)
    {
        if (obrigatorio && string.IsNullOrWhiteSpace(valor))
            erros[campo] = ["Informe este campo."];
        else if (valor?.Trim().Length > maximo)
            erros[campo] = [$"Use no máximo {maximo} caracteres."];
    }
}
