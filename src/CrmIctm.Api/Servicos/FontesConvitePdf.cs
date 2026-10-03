using PdfSharp.Fonts;

namespace CrmIctm.Api.Servicos;

public sealed class FontesConvitePdf : IFontResolver
{
    private static readonly object TravaConfiguracao = new();

    public static void Configurar()
    {
        // PDFsharp mantém um resolvedor global. Vários hosts de teste usam o mesmo processo.
        lock (TravaConfiguracao)
        {
            GlobalFontSettings.FontResolver ??= new FontesConvitePdf();
        }
    }

    public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic) => familyName switch
    {
        "Convite Sans" => new FontResolverInfo(
            bold ? "LiberationSans-Bold.ttf" : "LiberationSans-Regular.ttf", false, italic),
        "Convite Serif" => new FontResolverInfo("LiberationSerif-Bold.ttf", false, italic),
        _ => null
    };

    public byte[] GetFont(string faceName) => LerRecurso(faceName);

    internal static byte[] LerRecurso(string nome)
    {
        using var recurso = typeof(FontesConvitePdf).Assembly.GetManifestResourceStream($"Convite.{nome}")
            ?? throw new InvalidOperationException($"Recurso do convite ausente: {nome}.");
        using var memoria = new MemoryStream();
        recurso.CopyTo(memoria);
        return memoria.ToArray();
    }
}
