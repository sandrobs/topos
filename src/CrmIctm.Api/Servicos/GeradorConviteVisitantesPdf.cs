using CrmIctm.Api.Dominio;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using QRCoder;

namespace CrmIctm.Api.Servicos;

public sealed class GeradorConviteVisitantesPdf
{
    private static readonly XSolidBrush Azul = new(XColor.FromArgb(8, 12, 92));
    private static readonly XSolidBrush Cinza = new(XColor.FromArgb(88, 99, 126));
    private static readonly XSolidBrush Dourado = new(XColor.FromArgb(135, 98, 33));
    private static readonly XSolidBrush DouradoClaro = new(XColor.FromArgb(242, 207, 124));
    private static readonly Lazy<byte[]> Logo = new(() => FontesConvitePdf.LerRecurso("logo-igreja.jpg"));
    private static readonly Lazy<byte[]> Foto = new(() => FontesConvitePdf.LerRecurso("imagem-nave-igreja.png"));

    public byte[] Gerar(Igreja igreja, string enderecoFormulario)
    {
        ArgumentNullException.ThrowIfNull(igreja);
        ArgumentException.ThrowIfNullOrWhiteSpace(enderecoFormulario);

        using var documento = new PdfDocument();
        documento.Info.Title = $"Convite de visitantes - {igreja.Nome}";
        documento.Info.Author = "Topos";
        var pagina = documento.AddPage();
        pagina.Size = PageSize.A4;
        pagina.Width = XUnit.FromMillimeter(210);
        pagina.Height = XUnit.FromMillimeter(297);
        using (var desenho = XGraphics.FromPdfPage(pagina))
        {
            DesenharCabecalho(desenho, igreja);
            DesenharFoto(desenho);
            DesenharChamada(desenho);
            DesenharConteudo(desenho, enderecoFormulario);
            DesenharRodape(desenho, igreja);
        }

        // O QR também é clicável no arquivo, sem alterar seu destino permanente.
        pagina.AddWebLink(new PdfRectangle(new XRect(Mm(125), Mm(297 - 246), Mm(70), Mm(86))),
            enderecoFormulario);
        using var memoria = new MemoryStream();
        documento.Save(memoria, false);
        return memoria.ToArray();
    }

    private static void DesenharCabecalho(XGraphics desenho, Igreja igreja)
    {
        desenho.DrawRectangle(Azul, 0, 0, Mm(210), Mm(34));
        using var logo = XImage.FromStream(new MemoryStream(Logo.Value, writable: false));
        var estadoLogo = desenho.Save();
        var recorteLogo = new XGraphicsPath();
        recorteLogo.AddEllipse(Mm(15), Mm(7), Mm(20), Mm(20));
        desenho.IntersectClip(recorteLogo);
        desenho.DrawImage(logo, Mm(15), Mm(7), Mm(20), Mm(20));
        desenho.Restore(estadoLogo);
        Texto(desenho, "SEJA BEM-VINDO À", Sans(10, true), DouradoClaro, 40, 7, 155, 5);
        TextoAjustado(desenho, igreja.Nome, "Convite Sans", XBrushes.White,
            new XRect(Mm(40), Mm(12), Mm(155), Mm(14)), 17, 10);
        TextoAjustado(desenho, $"{igreja.Cidade} · {igreja.Estado}", "Convite Sans", XBrushes.LightGray,
            new XRect(Mm(40), Mm(27), Mm(155), Mm(6)), 8.5, 6, XFontStyleEx.Regular);
    }

    private static void DesenharFoto(XGraphics desenho)
    {
        using var foto = XImage.FromStream(new MemoryStream(Foto.Value, writable: false));
        var area = new XRect(0, Mm(34), Mm(210), Mm(57));
        var altura = area.Width * foto.PixelHeight / foto.PixelWidth;
        var estado = desenho.Save();
        desenho.IntersectClip(area);
        // Mesmo enquadramento aprovado: object-position center 25%, sem sombreado.
        desenho.DrawImage(foto, area.X, area.Y - (altura - area.Height) * 0.25, area.Width, altura);
        desenho.Restore(estado);
    }

    private static void DesenharChamada(XGraphics desenho)
    {
        Texto(desenho, "VOCÊ É NOSSO CONVIDADO", Sans(10, true), Cinza, 15, 96, 180, 5);
        Texto(desenho, "Ainda há lugar.", Serif(38), Azul, 15, 103, 180, 17);
        desenho.DrawRectangle(Dourado, Mm(15), Mm(122), Mm(12), Mm(0.6));
        Texto(desenho, "LUCAS 14:22", Sans(14, true), Dourado, 30, 118.5, 165, 7);
        desenho.DrawLine(new XPen(XColor.FromArgb(222, 226, 237), 0.5),
            0, Mm(129.4), Mm(210), Mm(129.4));
    }

    private static void DesenharConteudo(XGraphics desenho, string enderecoFormulario)
    {
        Texto(desenho, "VAMOS MANTER CONTATO?", Sans(9, true), Azul, 15, 159, 100, 5);
        Texto(desenho, "Queremos", Serif(28), Azul, 15, 165, 100, 12);
        Texto(desenho, "continuar essa", Serif(28), Azul, 15, 175.5, 100, 12);
        Texto(desenho, "conversa.", Serif(28), Azul, 15, 186, 100, 12);
        var descricao = "Escaneie o QR Code e deixe seu nome e WhatsApp. Nossa equipe entrará em contato "
            + "para acolher você, apresentar melhor a igreja ou conversar e orar.";
        TextoEmLinhas(desenho, descricao, Sans(10), Cinza,
            new XRect(Mm(15), Mm(201), Mm(98), Mm(23)), 15);
        string[] passos = ["Abra a câmera do celular", "Aponte para o QR Code", "Toque no link e preencha"];
        for (var indice = 0; indice < passos.Length; indice++)
        {
            var topo = 227 + indice * 10;
            desenho.DrawEllipse(Azul, Mm(15), Mm(topo), Mm(7), Mm(7));
            Texto(desenho, (indice + 1).ToString(System.Globalization.CultureInfo.InvariantCulture),
                Sans(10, true), XBrushes.White, 15, topo, 7, 7, XStringFormats.Center);
            Texto(desenho, passos[indice], Sans(9, true), Cinza, 25, topo, 88, 7);
        }

        desenho.DrawRoundedRectangle(Azul, new XRect(Mm(125), Mm(160), Mm(70), Mm(86)),
            new XSize(Mm(12), Mm(12)));
        desenho.DrawRoundedRectangle(XBrushes.White, new XRect(Mm(130), Mm(165), Mm(60), Mm(60)),
            new XSize(Mm(8), Mm(8)));
        using var qr = QRCodeGenerator.GenerateQrCode(enderecoFormulario, QRCodeGenerator.ECCLevel.Q);
        var tamanhoModulo = Mm(52) / qr.ModuleMatrix.Count;
        for (var linha = 0; linha < qr.ModuleMatrix.Count; linha++)
        {
            for (var coluna = 0; coluna < qr.ModuleMatrix.Count; coluna++)
            {
                if (qr.ModuleMatrix[linha][coluna])
                {
                    desenho.DrawRectangle(XBrushes.Black, Mm(134) + coluna * tamanhoModulo,
                        Mm(169) + linha * tamanhoModulo, tamanhoModulo, tamanhoModulo);
                }
            }
        }
        Texto(desenho, "Escaneie aqui", Sans(14, true), XBrushes.White,
            130, 229, 60, 7, XStringFormats.Center);
        Texto(desenho, "É rápido e seguro", Sans(8), XBrushes.LightGray,
            130, 237, 60, 4, XStringFormats.Center);
    }

    private static void DesenharRodape(XGraphics desenho, Igreja igreja)
    {
        var complemento = string.IsNullOrWhiteSpace(igreja.Complemento) ? "" : $", {igreja.Complemento}";
        var localidade = $"{igreja.Cidade}/{igreja.Estado}";
        var endereco = localidade;
        if (igreja.Cep.Length == 8)
        {
            var cep = $"{igreja.Cep[..5]}-{igreja.Cep[5..]}";
            endereco = $"{igreja.Logradouro}, {igreja.Numero}{complemento} · {igreja.Bairro} · "
                + $"{localidade} · CEP\u00a0{cep}";
        }
        var fonte = Sans(8.5);
        var linhas = QuebrarLinhas(desenho, endereco, fonte, Mm(180));
        // Mesmo os limites máximos dos campos devem caber sem invadir os passos ou o QR.
        while (Mm(12) + linhas.Count * fonte.Size * 1.2 > Mm(37) && fonte.Size > 6.5)
        {
            fonte = Sans(fonte.Size - 0.5);
            linhas = QuebrarLinhas(desenho, endereco, fonte, Mm(180));
        }
        var alturaLinha = fonte.Size * 1.2;
        var alturaRodape = Math.Max(Mm(22), Mm(12) + linhas.Count * alturaLinha);
        var topo = Mm(297) - alturaRodape;
        desenho.DrawRectangle(Azul, 0, topo, Mm(210), alturaRodape);
        desenho.DrawString("Ficamos felizes com a sua visita.", Sans(10, true), XBrushes.White,
            new XRect(Mm(15), topo + Mm(4), Mm(90), Mm(5)), XStringFormats.TopLeft);
        desenho.DrawString("Esperamos falar com você em breve.", Sans(8), XBrushes.LightGray,
            new XRect(Mm(105), topo + Mm(4.5), Mm(90), Mm(4)), XStringFormats.TopRight);
        for (var indice = 0; indice < linhas.Count; indice++)
        {
            desenho.DrawString(linhas[indice], fonte, DouradoClaro,
                new XRect(Mm(15), topo + Mm(11) + indice * alturaLinha, Mm(180), alturaLinha), XStringFormats.TopLeft);
        }
    }

    private static void Texto(XGraphics desenho, string texto, XFont fonte, XBrush cor,
        double x, double y, double largura, double altura, XStringFormat? formato = null) =>
        desenho.DrawString(texto, fonte, cor, new XRect(Mm(x), Mm(y), Mm(largura), Mm(altura)),
            formato ?? XStringFormats.TopLeft);

    private static void TextoAjustado(XGraphics desenho, string texto, string familia,
        XBrush cor, XRect area, double tamanhoInicial, double tamanhoMinimo,
        XFontStyleEx estilo = XFontStyleEx.Bold)
    {
        for (var tamanho = tamanhoInicial; tamanho >= tamanhoMinimo; tamanho -= 0.5)
        {
            var fonte = new XFont(familia, tamanho, estilo);
            var linhas = QuebrarLinhas(desenho, texto, fonte, area.Width);
            if (linhas.Count * tamanho * 1.15 <= area.Height)
            {
                TextoEmLinhas(desenho, texto, fonte, cor, area, tamanho * 1.15);
                return;
            }
        }
        throw new InvalidOperationException("O nome da igreja não coube no cabeçalho do convite.");
    }

    private static void TextoEmLinhas(XGraphics desenho, string texto, XFont fonte,
        XBrush cor, XRect area, double alturaLinha)
    {
        var linhas = QuebrarLinhas(desenho, texto, fonte, area.Width);
        if (linhas.Count * alturaLinha > area.Height)
        {
            throw new InvalidOperationException("O texto não coube na região reservada do convite.");
        }
        for (var indice = 0; indice < linhas.Count; indice++)
        {
            desenho.DrawString(linhas[indice], fonte, cor,
                new XRect(area.X, area.Y + indice * alturaLinha, area.Width, alturaLinha), XStringFormats.TopLeft);
        }
    }

    private static List<string> QuebrarLinhas(XGraphics desenho, string texto, XFont fonte, double largura)
    {
        var linhas = new List<string>();
        var atual = "";
        foreach (var palavra in texto.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var candidata = atual.Length == 0 ? palavra : $"{atual} {palavra}";
            if (desenho.MeasureString(candidata, fonte).Width <= largura)
            {
                atual = candidata;
                continue;
            }
            if (atual.Length > 0) linhas.Add(atual);
            atual = "";
            foreach (var caractere in palavra)
            {
                if (desenho.MeasureString(atual + caractere, fonte).Width > largura)
                {
                    linhas.Add(atual);
                    atual = "";
                }
                atual += caractere;
            }
        }
        if (atual.Length > 0) linhas.Add(atual);
        return linhas;
    }

    private static XFont Sans(double tamanho, bool negrito = false) =>
        new("Convite Sans", tamanho, negrito ? XFontStyleEx.Bold : XFontStyleEx.Regular);
    private static XFont Serif(double tamanho) => new("Convite Serif", tamanho, XFontStyleEx.Bold);
    private static double Mm(double valor) => XUnit.FromMillimeter(valor).Point;
}
