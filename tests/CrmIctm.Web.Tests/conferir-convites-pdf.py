"""QA local dos PDFs salvos por v2-ajustes.cjs e renderizados pelo Poppler.

Dependências de teste: pdfplumber, pypdf, Pillow e zxing-cpp.
Não envia arquivos para serviços externos e não acessa banco de dados.
"""
from pathlib import Path
import pdfplumber
from pypdf import PdfReader
from PIL import Image
import zxingcpp

saida = Path(__file__).resolve().parents[2] / ".tools" / "v2-ajustes-qa"
for membros in (False, True):
    for quatro in (False, True):
        nome = f"{'membros' if membros else 'visitantes'}-{'4-por-a4' if quatro else 'a4'}"
        with pdfplumber.open(saida / f"{nome}.pdf") as documento:
            assert len(documento.pages) == 1, nome
            pagina = documento.pages[0]
            assert abs(pagina.width * 25.4 / 72 - 210) < 0.01, nome
            assert abs(pagina.height * 25.4 / 72 - 297) < 0.01, nome
            for caractere in pagina.chars:
                assert 0 <= caractere['x0'] < caractere['x1'] <= pagina.width + 0.01, (nome, caractere)
                assert 0 <= caractere['top'] < caractere['bottom'] <= pagina.height + 0.01, (nome, caractere)
            texto = pagina.extract_text()
            quantidade = 4 if quatro else 1
            assert texto.count('Ainda há lugar.') == quantidade, nome
            assert texto.count('LUCAS 14:22') == quantidade, nome
            assert texto.count('CEP 98910-000') == quantidade, nome
            if membros:
                assert texto.count('CADASTRO DE MEMBROS') >= quantidade, nome
                assert texto.count('Romanos 12:4-5') == quantidade, nome

        leitor = PdfReader(saida / f"{nome}.pdf")
        links = [item.get_object()['/A']['/URI'] for item in leitor.pages[0]['/Annots']]
        assert len(links) == quantidade and len(set(links)) == 1, nome
        assert ('/membros/cadastro/' if membros else '/visita/') in links[0], nome
        with Image.open(saida / f"{nome}.png") as imagem:
            codigos = zxingcpp.read_barcodes(imagem)
            assert len(codigos) == quantidade, (nome, len(codigos))
            assert all(codigo.text == links[0] for codigo in codigos), nome
        print(f"{nome}: uma página A4, textos dentro da folha e {quantidade} QR(s) decodificado(s) com destino correto.")
