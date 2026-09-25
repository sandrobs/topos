namespace CrmIctm.Api.Servicos;

public static class NormalizadorWhatsapp
{
    private const string CodigoBrasil = "55";

    public static bool TentarNormalizar(string? valor, out string whatsappNormalizado)
    {
        whatsappNormalizado = string.Empty;
        if (string.IsNullOrWhiteSpace(valor))
        {
            return false;
        }

        var digitos = new string(valor.Where(char.IsAsciiDigit).ToArray());
        if (digitos.StartsWith(CodigoBrasil, StringComparison.Ordinal) && digitos.Length is 12 or 13)
        {
            digitos = digitos[2..];
        }

        if (digitos.Length is not (10 or 11))
        {
            return false;
        }

        var ddd = int.Parse(digitos[..2], System.Globalization.CultureInfo.InvariantCulture);
        if (ddd is < 11 or > 99 || digitos[2] is '0' or '1')
        {
            return false;
        }

        whatsappNormalizado = $"+{CodigoBrasil}{digitos}";
        return true;
    }
}

