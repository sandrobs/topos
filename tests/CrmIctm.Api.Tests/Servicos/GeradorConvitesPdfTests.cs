using CrmIctm.Api.Dominio;
using CrmIctm.Api.Servicos;
using PdfSharp.Pdf.IO;

namespace CrmIctm.Api.Tests.Servicos;

public sealed class GeradorConvitesPdfTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void TodosOsFormatosGeramUmaPaginaA4ComLinksCorretos(bool membros, bool quatro)
    {
        FontesConvitePdf.Configurar();
        var igreja = Igreja.Criar("Igreja Evangélica de Cristo - Três de Maio", "Rua São Roque",
            "88", "Por do Sol", null, "98910000", "Três de Maio", "RS", DateTimeOffset.UtcNow);
        var url = membros ? "https://exemplo.local/membros/cadastro/token" : "https://exemplo.local/visita/token";
        var bytes = new GeradorConvitesPdf().Gerar(igreja, url, membros, quatro);
        using var pdf = PdfReader.Open(new MemoryStream(bytes), PdfDocumentOpenMode.Modify);
        Assert.Single(pdf.Pages);
        Assert.Equal(210, pdf.Pages[0].Width.Millimeter, 2);
        Assert.Equal(297, pdf.Pages[0].Height.Millimeter, 2);
        Assert.Equal(quatro ? 4 : 1, pdf.Pages[0].Annotations.Count);
        for (var indice = 0; indice < pdf.Pages[0].Annotations.Count; indice++)
        {
            var acao = pdf.Pages[0].Annotations[indice].Elements.GetDictionary("/A");
            Assert.NotNull(acao);
            Assert.Contains(url, acao.Elements.GetString("/URI"));
        }
    }

    [Fact]
    public void ConviteCompactoAcomodaCamposNosLimitesSemDescartarEndereco()
    {
        FontesConvitePdf.Configurar();
        var igreja = Igreja.Criar(new string('M', 150), new string('R', 150), new string('8', 20),
            new string('B', 100), new string('C', 100), "98910000", new string('T', 100), "RS", DateTimeOffset.UtcNow,
            new string('M', 120), new string('M', 99));
        Assert.NotEmpty(new GeradorConvitesPdf().Gerar(igreja, "https://exemplo.local/visita/token", quatroPorFolha: true));
    }
}
