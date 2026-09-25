namespace CrmIctm.Api.Dominio;

public static class CodigosDominio
{
    public static string ParaCodigo(this EtapaAtendimento etapa) => etapa switch
    {
        EtapaAtendimento.Novo => "NOVO",
        EtapaAtendimento.AguardandoContato => "AGUARDANDO_CONTATO",
        EtapaAtendimento.EmAtendimento => "EM_ATENDIMENTO",
        EtapaAtendimento.EmAcompanhamento => "EM_ACOMPANHAMENTO",
        EtapaAtendimento.Concluido => "CONCLUIDO",
        _ => throw new ArgumentOutOfRangeException(nameof(etapa), etapa, null)
    };

    public static string ParaCodigo(this PerfilUsuario perfil) => perfil switch
    {
        PerfilUsuario.Administrador => "ADMINISTRADOR",
        PerfilUsuario.Pastor => "PASTOR",
        PerfilUsuario.Equipe => "EQUIPE",
        _ => throw new ArgumentOutOfRangeException(nameof(perfil), perfil, null)
    };

    public static string ParaCodigo(this ResultadoAtendimento resultado) => resultado switch
    {
        ResultadoAtendimento.TornouSeMembro => "TORNOU_SE_MEMBRO",
        ResultadoAtendimento.OracaoAtendida => "ORACAO_ATENDIDA",
        ResultadoAtendimento.NaoDesejaProsseguir => "NAO_DESEJA_PROSSEGUIR",
        ResultadoAtendimento.SemRetorno => "SEM_RETORNO",
        _ => throw new ArgumentOutOfRangeException(nameof(resultado), resultado, null)
    };

    public static string ParaCodigo(this SituacaoCongregacional situacao) => situacao switch
    {
        SituacaoCongregacional.NaoInformado => "NAO_INFORMADO",
        SituacaoCongregacional.NaoCongrega => "NAO_CONGREGA",
        SituacaoCongregacional.Congrega => "CONGREGA",
        _ => throw new ArgumentOutOfRangeException(nameof(situacao), situacao, null)
    };

    public static bool TentarLerEtapa(string? codigo, out EtapaAtendimento etapa)
    {
        etapa = codigo?.Trim().ToUpperInvariant() switch
        {
            "NOVO" => EtapaAtendimento.Novo,
            "AGUARDANDO_CONTATO" => EtapaAtendimento.AguardandoContato,
            "EM_ATENDIMENTO" => EtapaAtendimento.EmAtendimento,
            "EM_ACOMPANHAMENTO" => EtapaAtendimento.EmAcompanhamento,
            "CONCLUIDO" => EtapaAtendimento.Concluido,
            _ => default
        };

        return codigo?.Trim().ToUpperInvariant() is
            "NOVO" or
            "AGUARDANDO_CONTATO" or
            "EM_ATENDIMENTO" or
            "EM_ACOMPANHAMENTO" or
            "CONCLUIDO";
    }

    public static bool TentarLerPerfil(string? codigo, out PerfilUsuario perfil)
    {
        perfil = codigo?.Trim().ToUpperInvariant() switch
        {
            "ADMINISTRADOR" => PerfilUsuario.Administrador,
            "PASTOR" => PerfilUsuario.Pastor,
            "EQUIPE" => PerfilUsuario.Equipe,
            _ => default
        };

        return codigo?.Trim().ToUpperInvariant() is "ADMINISTRADOR" or "PASTOR" or "EQUIPE";
    }

    public static bool TentarLerResultado(string? codigo, out ResultadoAtendimento resultado)
    {
        resultado = codigo?.Trim().ToUpperInvariant() switch
        {
            "TORNOU_SE_MEMBRO" => ResultadoAtendimento.TornouSeMembro,
            "ORACAO_ATENDIDA" => ResultadoAtendimento.OracaoAtendida,
            "NAO_DESEJA_PROSSEGUIR" => ResultadoAtendimento.NaoDesejaProsseguir,
            "SEM_RETORNO" => ResultadoAtendimento.SemRetorno,
            _ => default
        };

        return codigo?.Trim().ToUpperInvariant() is
            "TORNOU_SE_MEMBRO" or
            "ORACAO_ATENDIDA" or
            "NAO_DESEJA_PROSSEGUIR" or
            "SEM_RETORNO";
    }

    public static bool TentarLerSituacaoCongregacional(
        string? codigo,
        out SituacaoCongregacional situacao)
    {
        situacao = codigo?.Trim().ToUpperInvariant() switch
        {
            "NAO_INFORMADO" => SituacaoCongregacional.NaoInformado,
            "NAO_CONGREGA" => SituacaoCongregacional.NaoCongrega,
            "CONGREGA" => SituacaoCongregacional.Congrega,
            _ => default
        };

        return codigo?.Trim().ToUpperInvariant() is
            "NAO_INFORMADO" or
            "NAO_CONGREGA" or
            "CONGREGA";
    }
}
