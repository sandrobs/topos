namespace CrmIctm.Api.Servicos;

public enum PeriodoAniversario
{
    Hoje,
    Semana,
    Mes
}

/// <summary>
/// A quem a saudação de aniversário se dirige.
/// </summary>
public enum DestinoAniversario
{
    /// <summary>Ninguém: não há WhatsApp no cadastro.</summary>
    Nenhum,
    Membro,
    Responsavel
}

/// <summary>
/// Regras de período e de destino da saudação de aniversário.
///
/// Está separado dos endpoints porque são as duas partes com armadilha: a
/// semana e o mês atravessam a virada de mês e de ano, e 29 de fevereiro não
/// existe em três de cada quatro anos.
/// </summary>
public static class Aniversariantes
{
    /// <summary>
    /// Brasília como deslocamento fixo, e não como fuso nomeado.
    ///
    /// O resto do sistema deriva a data de hoje do UTC, o que é inofensivo numa
    /// validação de "não pode ser futura" mas não aqui: das 21h à meia-noite o
    /// UTC já está no dia seguinte, e o pastor veria a lista de amanhã.
    ///
    /// Não usamos <c>TimeZoneInfo</c> com "America/Sao_Paulo" porque a imagem
    /// final é Alpine, que não traz o tzdata — a busca lançaria exceção em
    /// produção e funcionaria na máquina do desenvolvedor. O Brasil não adota
    /// horário de verão desde 2019, então -3 h é exato. Se voltar a adotar,
    /// este é o ponto único a mudar, e será preciso instalar o tzdata.
    /// </summary>
    private static readonly TimeSpan FusoBrasilia = TimeSpan.FromHours(-3);

    public static DateOnly Hoje(DateTimeOffset agora) =>
        DateOnly.FromDateTime(agora.ToOffset(FusoBrasilia).DateTime);

    public static bool TentarLerPeriodo(string? codigo, out PeriodoAniversario periodo)
    {
        switch (codigo?.Trim().ToUpperInvariant())
        {
            case null or "" or "HOJE":
                periodo = PeriodoAniversario.Hoje;
                return true;
            case "SEMANA":
                periodo = PeriodoAniversario.Semana;
                return true;
            case "MES":
                periodo = PeriodoAniversario.Mes;
                return true;
            default:
                periodo = PeriodoAniversario.Hoje;
                return false;
        }
    }

    /// <summary>
    /// O domingo da semana de <paramref name="hoje"/>. A igreja se reúne no
    /// domingo, então é ali que a semana começa.
    /// </summary>
    public static DateOnly InicioDaSemana(DateOnly hoje) =>
        hoje.AddDays(-(int)hoje.DayOfWeek);

    /// <summary>
    /// As datas do calendário que o período cobre, em ordem. Devolver datas
    /// reais, e não pares de mês e dia, é o que mantém a ordenação correta na
    /// semana que vira o ano — 29 de dezembro vem antes de 2 de janeiro.
    /// </summary>
    public static IReadOnlyList<DateOnly> DatasDoPeriodo(PeriodoAniversario periodo, DateOnly hoje)
    {
        switch (periodo)
        {
            case PeriodoAniversario.Hoje:
                return [hoje];

            case PeriodoAniversario.Semana:
            {
                var domingo = InicioDaSemana(hoje);
                return [.. Enumerable.Range(0, 7).Select(domingo.AddDays)];
            }

            case PeriodoAniversario.Mes:
            {
                var primeiro = new DateOnly(hoje.Year, hoje.Month, 1);
                var dias = DateTime.DaysInMonth(hoje.Year, hoje.Month);
                return [.. Enumerable.Range(0, dias).Select(primeiro.AddDays)];
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(periodo), periodo, null);
        }
    }

    /// <summary>
    /// Mês e dia reunidos num só inteiro (<c>1511</c> para 15 de novembro).
    /// A consulta compara esse número com o mês e o dia da data de nascimento,
    /// o que o banco resolve com um <c>IN</c>.
    /// </summary>
    public static int Chave(int mes, int dia) => (mes * 100) + dia;

    public static int Chave(DateOnly data) => Chave(data.Month, data.Day);

    /// <summary>
    /// De cada chave de nascimento para a data em que o aniversário cai dentro
    /// do período.
    ///
    /// Em ano comum, quem nasceu em 29 de fevereiro é celebrado no dia 28: a
    /// chave 229 aponta para a data de 28 de fevereiro. Sem isso, essas pessoas
    /// sairiam da lista em três de cada quatro anos, sem erro e sem aviso.
    /// </summary>
    public static IReadOnlyDictionary<int, DateOnly> DatasPorChave(IReadOnlyList<DateOnly> datas)
    {
        var mapa = new Dictionary<int, DateOnly>(datas.Count + 1);
        foreach (var data in datas)
        {
            mapa[Chave(data)] = data;
        }

        foreach (var data in datas)
        {
            if (data.Month == 2 && data.Day == 28 && !DateTime.IsLeapYear(data.Year))
            {
                mapa[Chave(2, 29)] = data;
            }
        }

        return mapa;
    }

    /// <summary>
    /// A idade que a pessoa completa na data em que o aniversário cai — não a
    /// idade de hoje. Nas abas de semana e de mês essas duas são diferentes.
    /// </summary>
    public static int IdadeQueCompleta(DateOnly nascimento, DateOnly ocorrencia) =>
        ocorrencia.Year - nascimento.Year;

    /// <summary>
    /// Para onde a conversa é aberta.
    ///
    /// Adulto: o próprio número, que o cadastro exige. Menor: o próprio número
    /// quando houver, e o do responsável legal quando não houver — para menor
    /// esse campo é obrigatório, então sempre há a quem falar.
    ///
    /// A regra vive aqui, e não na tela, para não se espalhar por cada lugar
    /// que mostre um aniversariante.
    /// </summary>
    public static (DestinoAniversario Destino, string? Whatsapp) Destino(
        bool ehMenor, string? whatsappMembro, string? whatsappResponsavel)
    {
        if (!string.IsNullOrWhiteSpace(whatsappMembro))
        {
            return (DestinoAniversario.Membro, whatsappMembro);
        }

        if (ehMenor && !string.IsNullOrWhiteSpace(whatsappResponsavel))
        {
            return (DestinoAniversario.Responsavel, whatsappResponsavel);
        }

        return (DestinoAniversario.Nenhum, null);
    }

    public static string ParaCodigo(this DestinoAniversario destino) => destino switch
    {
        DestinoAniversario.Nenhum => "NENHUM",
        DestinoAniversario.Membro => "MEMBRO",
        DestinoAniversario.Responsavel => "RESPONSAVEL",
        _ => throw new ArgumentOutOfRangeException(nameof(destino), destino, null)
    };
}
