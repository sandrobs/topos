using CrmIctm.Api.Dominio;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using QRCoder;

namespace CrmIctm.Api.Servicos;

public sealed class GeradorConvitesPdf
{
    private static readonly XSolidBrush Azul = new(XColor.FromArgb(8, 12, 92));
    private static readonly XSolidBrush Cinza = new(XColor.FromArgb(88, 99, 126));
    private static readonly XSolidBrush Dourado = new(XColor.FromArgb(135, 98, 33));
    private static readonly XSolidBrush DouradoClaro = new(XColor.FromArgb(242, 207, 124));
    private static readonly Lazy<byte[]> Logo = new(() => FontesConvitePdf.LerRecurso("logo-igreja.jpg"));
    private static readonly Lazy<byte[]> Foto = new(() => FontesConvitePdf.LerRecurso("imagem-nave-igreja.png"));

    public byte[] Gerar(Igreja igreja, string enderecoFormulario, bool paraMembros = false, bool quatroPorFolha = false)
    {
        ArgumentNullException.ThrowIfNull(igreja);
        ArgumentException.ThrowIfNullOrWhiteSpace(enderecoFormulario);

        using var documento = new PdfDocument();
        documento.Info.Title = $"Convite de {(paraMembros ? "membros" : "visitantes")} - {igreja.Nome}";
        documento.Info.Author = "Topos";
        var pagina = documento.AddPage();
        pagina.Size = PageSize.A4;
        pagina.Width = XUnit.FromMillimeter(210);
        pagina.Height = XUnit.FromMillimeter(297);
        using (var desenho = XGraphics.FromPdfPage(pagina))
        {
            if (quatroPorFolha)
            {
                for (var indice = 0; indice < 4; indice++)
                {
                    var x = 6 + (indice % 2) * 101;
                    var y = 6 + (indice / 2) * 144.5;
                    var estado = desenho.Save();
                    desenho.TranslateTransform(Mm(x), Mm(y));
                    DesenharConviteCompacto(desenho, igreja, enderecoFormulario, paraMembros);
                    desenho.Restore(estado);
                    pagina.AddWebLink(new PdfRectangle(new XRect(Mm(x + 55), Mm(297 - y - 115), Mm(36), Mm(36))), enderecoFormulario);
                }
                var guia = new XPen(XColors.LightGray, 0.4) { DashStyle = XDashStyle.Dash };
                desenho.DrawLine(guia, Mm(105), Mm(4), Mm(105), Mm(293));
                desenho.DrawLine(guia, Mm(4), Mm(148.5), Mm(206), Mm(148.5));
            }
            else
            {
                DesenharCabecalho(desenho, igreja, paraMembros);
                DesenharFoto(desenho);
                DesenharChamada(desenho, paraMembros);
                DesenharConteudo(desenho, enderecoFormulario, paraMembros);
                DesenharRodape(desenho, igreja, paraMembros);
            }
        }

        // O QR também é clicável no arquivo, sem alterar seu destino permanente.
        if (!quatroPorFolha)
            pagina.AddWebLink(new PdfRectangle(new XRect(Mm(125), Mm(297 - 246), Mm(70), Mm(86))), enderecoFormulario);
        using var memoria = new MemoryStream();
        documento.Save(memoria, false);
        return memoria.ToArray();
    }

    private static void DesenharCabecalho(XGraphics desenho, Igreja igreja, bool paraMembros)
    {
        desenho.DrawRectangle(Azul, 0, 0, Mm(210), Mm(34));
        using var logo = XImage.FromStream(new MemoryStream(Logo.Value, writable: false));
        var estadoLogo = desenho.Save();
        var recorteLogo = new XGraphicsPath();
        recorteLogo.AddEllipse(Mm(15), Mm(7), Mm(20), Mm(20));
        desenho.IntersectClip(recorteLogo);
        desenho.DrawImage(logo, Mm(15), Mm(7), Mm(20), Mm(20));
        desenho.Restore(estadoLogo);
        Texto(desenho, paraMembros ? "CADASTRO DE MEMBROS" : "SEJA BEM-VINDO À", Sans(10, true), DouradoClaro, 40, 7, 155, 5);
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

    private static void DesenharChamada(XGraphics desenho, bool paraMembros)
    {
        Texto(desenho, paraMembros ? "NOSSA COMUNIDADE, NOSSOS LAÇOS" : "VOCÊ É NOSSO CONVIDADO", Sans(10, true), Cinza, 15, 96, 180, 5);
        Texto(desenho, "Ainda há lugar.", Serif(38), Azul, 15, 103, 180, 17);
        desenho.DrawRectangle(Dourado, Mm(15), Mm(122), Mm(12), Mm(0.6));
        Texto(desenho, "LUCAS 14:22", Sans(14, true), Dourado, 30, 118.5, 165, 7);
        desenho.DrawLine(new XPen(XColor.FromArgb(222, 226, 237), 0.5),
            0, Mm(129.4), Mm(210), Mm(129.4));
    }

    private static void DesenharConteudo(XGraphics desenho, string enderecoFormulario, bool paraMembros)
    {
        Texto(desenho, paraMembros ? "CADASTRO DE MEMBROS" : "VAMOS MANTER CONTATO?", Sans(9, true), Azul, 15, 159, 100, 5);
        Texto(desenho, paraMembros ? "Uma família" : "Queremos", Serif(28), Azul, 15, 165, 100, 12);
        Texto(desenho, paraMembros ? "no amor" : "continuar essa", Serif(28), Azul, 15, 175.5, 100, 12);
        Texto(desenho, paraMembros ? "de Deus." : "conversa.", Serif(28), Azul, 15, 186, 100, 12);
        var descricao = paraMembros ? "Escaneie o QR Code e informe seus dados para o cadastro de membros da igreja. Seu envio será analisado pela equipe pastoral." :
            "Escaneie o QR Code e deixe seu nome e WhatsApp. Nossa equipe entrará em contato "
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
        Texto(desenho, paraMembros ? "Exclusivo para membros" : "É rápido e seguro", Sans(8), XBrushes.LightGray,
            130, 237, 60, 4, XStringFormats.Center);
    }

    private static void DesenharRodape(XGraphics desenho, Igreja igreja, bool paraMembros)
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
        desenho.DrawString(paraMembros ? "Uma família no amor de Deus" : "Ficamos felizes com a sua visita.", Sans(10, true), XBrushes.White,
            new XRect(Mm(15), topo + Mm(4), Mm(90), Mm(5)), XStringFormats.TopLeft);
        desenho.DrawString(paraMembros ? "Romanos 12:4-5" : "Esperamos falar com você em breve.", Sans(8), XBrushes.LightGray,
            new XRect(Mm(105), topo + Mm(4.5), Mm(90), Mm(4)), XStringFormats.TopRight);
        for (var indice = 0; indice < linhas.Count; indice++)
        {
            desenho.DrawString(linhas[indice], fonte, DouradoClaro,
                new XRect(Mm(15), topo + Mm(11) + indice * alturaLinha, Mm(180), alturaLinha), XStringFormats.TopLeft);
        }
    }

    private static void DesenharConviteCompacto(XGraphics desenho, Igreja igreja, string url, bool paraMembros)
    {
        const double largura = 97;
        const double altura = 140.5;
        desenho.DrawRectangle(XBrushes.White, 0, 0, Mm(largura), Mm(altura));
        desenho.DrawRectangle(Azul, 0, 0, Mm(largura), Mm(18));
        using var logo = XImage.FromStream(new MemoryStream(Logo.Value, writable: false));
        desenho.DrawImage(logo, Mm(3), Mm(3), Mm(12), Mm(12));
        TextoAjustado(desenho, igreja.Nome, "Convite Sans", XBrushes.White,
            new XRect(Mm(18), Mm(3), Mm(75), Mm(10)), 10, 6);
        TextoAjustado(desenho, $"{igreja.Cidade}/{igreja.Estado}", "Convite Sans", DouradoClaro,
            new XRect(Mm(18), Mm(13), Mm(75), Mm(5)), 7, 5, XFontStyleEx.Regular);
        using var foto = XImage.FromStream(new MemoryStream(Foto.Value, writable: false));
        var estado = desenho.Save();
        desenho.IntersectClip(new XRect(0, Mm(18), Mm(largura), Mm(28)));
        var alturaFoto = largura * foto.PixelHeight / foto.PixelWidth;
        desenho.DrawImage(foto, 0, Mm(18 - (alturaFoto - 28) * 0.25), Mm(largura), Mm(alturaFoto));
        desenho.Restore(estado);
        Texto(desenho, paraMembros ? "CADASTRO DE MEMBROS" : "VOCÊ É NOSSO CONVIDADO", Sans(8, true), Cinza, 4, 49, 89, 4);
        Texto(desenho, "Ainda há lugar.", Serif(24), Azul, 4, 54, 89, 11);
        Texto(desenho, "LUCAS 14:22", Sans(8, true), Dourado, 4, 67, 89, 4);
        desenho.DrawLine(new XPen(XColors.LightGray, 0.4), Mm(4), Mm(74), Mm(93), Mm(74));
        TextoEmLinhas(desenho, paraMembros ? "Uma família no amor de Deus" : "Vamos manter contato?", Serif(15), Azul,
            new XRect(Mm(4), Mm(79), Mm(46), Mm(20)), 17);
        TextoEmLinhas(desenho, paraMembros ? "Informe seus dados. A equipe pastoral analisará seu cadastro." :
            "Deixe seu nome e WhatsApp. Queremos acolher você e continuar essa conversa.", Sans(8), Cinza,
            new XRect(Mm(4), Mm(101), Mm(46), Mm(17)), 10);
        desenho.DrawRoundedRectangle(Azul, new XRect(Mm(53), Mm(77), Mm(40), Mm(43)), new XSize(Mm(4), Mm(4)));
        desenho.DrawRectangle(XBrushes.White, Mm(55), Mm(79), Mm(36), Mm(36));
        DesenharQr(desenho, url, 55, 79, 36);
        Texto(desenho, "Escaneie aqui", Sans(8, true), XBrushes.White, 55, 115.5, 36, 4, XStringFormats.Center);

        var endereco = $"{igreja.Logradouro}, {igreja.Numero}";
        if (!string.IsNullOrWhiteSpace(igreja.Complemento)) endereco += $", {igreja.Complemento}";
        endereco += $" · {igreja.Bairro} · {igreja.Cidade}/{igreja.Estado}";
        if (igreja.Cep.Length == 8) endereco += $" · CEP\u00a0{igreja.Cep[..5]}-{igreja.Cep[5..]}";
        var fonte = Sans(7);
        var linhas = QuebrarLinhas(desenho, endereco, fonte, Mm(89));
        while (linhas.Count * fonte.Size * 1.1 > Mm(17.5) && fonte.Size > 5)
        {
            fonte = Sans(fonte.Size - 0.5);
            linhas = QuebrarLinhas(desenho, endereco, fonte, Mm(89));
        }
        // Rodapé separado do QR, inclusive com dados nos limites máximos.
        desenho.DrawRectangle(Azul, 0, Mm(120.5), Mm(largura), Mm(20));
        if (paraMembros) Texto(desenho, "Romanos 12:4-5", Sans(6.5), Dourado, 4, 118, 46, 2.5);
        TextoEmLinhas(desenho, endereco, fonte, DouradoClaro,
            new XRect(Mm(4), Mm(122), Mm(89), Mm(17.5)), fonte.Size * 1.1);
    }

    private static void DesenharQr(XGraphics desenho, string url, double x, double y, double tamanho)
    {
        using var qr = QRCodeGenerator.GenerateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        var modulo = Mm(tamanho) / qr.ModuleMatrix.Count;
        for (var linha = 0; linha < qr.ModuleMatrix.Count; linha++)
            for (var coluna = 0; coluna < qr.ModuleMatrix.Count; coluna++)
                if (qr.ModuleMatrix[linha][coluna])
                    desenho.DrawRectangle(XBrushes.Black, Mm(x) + coluna * modulo, Mm(y) + linha * modulo, modulo, modulo);
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
