// Validação local do convite real. Requer Playwright e um navegador instalado.
// Não cria visitantes, igrejas ou usuários e recusa endereços de produção.
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { chromium } = require(process.env.PLAYWRIGHT_MODULE_PATH || 'playwright');

const endereco = new URL(process.env.TESTE_BASE_URL || 'http://localhost:8080');
const email = process.env.TESTE_EMAIL;
const senha = process.env.TESTE_SENHA;
const pastaSaida = path.resolve(__dirname, '../../.tools/v1-convite-qa');

assert.ok(['localhost', '127.0.0.1', '[::1]'].includes(endereco.hostname),
  'Execute este teste somente no ambiente local.');
assert.ok(email && senha, 'Configure TESTE_EMAIL e TESTE_SENHA da conta local.');

const telas = [
  { nome: 'computador', viewport: { width: 1440, height: 1200 } },
  { nome: 'celular', viewport: { width: 390, height: 844 }, isMobile: true, hasTouch: true },
  { nome: 'tablet', viewport: { width: 834, height: 1194 }, isMobile: true, hasTouch: true },
  // Exercita o tratamento já existente de iPad, mas não substitui Safari real.
  { nome: 'ipad-emulado', viewport: { width: 834, height: 1194 }, isMobile: true,
    hasTouch: true, userAgent: 'Mozilla/5.0 (iPad; CPU OS 17_0 like Mac OS X) AppleWebKit/605.1.15 Version/17.0 Mobile/15E148 Safari/604.1' },
];

async function autenticar(contexto) {
  const csrf = await contexto.request.get(`${endereco.origin}/api/seguranca/token-csrf`);
  assert.equal(csrf.status(), 200);
  const { token } = await csrf.json();
  const login = await contexto.request.post(`${endereco.origin}/api/autenticacao/entrar`, {
    headers: { 'X-CSRF-TOKEN': token }, data: { email, senha },
  });
  assert.equal(login.status(), 204, 'A conta de teste deve autenticar.');
}

async function verificarLayout(pagina, impressao) {
  const medidas = await pagina.evaluate(() => {
    const cartaz = document.querySelector('.cartaz-a4');
    const foto = document.querySelector('.imagem-cartaz');
    const chamada = document.querySelector('.chamada-cartaz');
    const titulo = chamada.querySelector('strong');
    const limites = cartaz.getBoundingClientRect();
    const regioes = [...cartaz.children].map(elemento => {
      const retangulo = elemento.getBoundingClientRect();
      return { classe: elemento.className, topo: retangulo.top, fim: retangulo.bottom,
        direita: retangulo.right, esquerda: retangulo.left };
    });
    return {
      semRolagemHorizontal: document.documentElement.scrollWidth <= window.innerWidth + 1,
      secoes: cartaz.children.length,
      fotoSeguidaDaChamada: foto.nextElementSibling === chamada,
      enquadramento: getComputedStyle(foto.querySelector('img')).objectPosition,
      camadaEscura: getComputedStyle(foto, '::after').content,
      sombraTexto: getComputedStyle(titulo).textShadow,
      corTitulo: getComputedStyle(titulo).color,
      fundos: ['.cabecalho-cartaz', '.area-qr-code', '.rodape-cartaz']
        .map(seletor => getComputedStyle(document.querySelector(seletor)).backgroundColor),
      imagensCompletas: [...cartaz.querySelectorAll('img')].every(img => img.complete && img.naturalWidth > 0),
      nomeCompleto: (() => { const nome = document.querySelector('.cabecalho-cartaz strong');
        return nome.scrollWidth <= nome.clientWidth + 1; })(),
      conteudoDentroDaFolha: regioes.every(regiao => regiao.topo >= limites.top - 1
        && regiao.fim <= limites.bottom + 1 && regiao.esquerda >= limites.left - 1
        && regiao.direita <= limites.right + 1),
      semSobreposicao: regioes.every((regiao, indice) => indice === 0 || regiao.topo >= regioes[indice - 1].fim - 1),
    };
  });
  assert.equal(medidas.secoes, 5);
  assert.ok(medidas.fotoSeguidaDaChamada);
  assert.equal(medidas.enquadramento, '50% 25%');
  assert.ok(['none', 'normal'].includes(medidas.camadaEscura));
  assert.equal(medidas.sombraTexto, 'none');
  assert.equal(medidas.corTitulo, 'rgb(8, 12, 92)');
  assert.ok(medidas.fundos.every(cor => cor === 'rgb(8, 12, 92)'));
  assert.ok(medidas.imagensCompletas);
  assert.ok(medidas.nomeCompleto, 'O nome da igreja não pode ser cortado.');
  assert.ok(medidas.conteudoDentroDaFolha, 'As seções devem caber na folha.');
  assert.ok(medidas.semSobreposicao, 'As seções não podem se sobrepor.');
  if (!impressao) assert.ok(medidas.semRolagemHorizontal);
}

async function executar() {
  fs.mkdirSync(pastaSaida, { recursive: true });
  const navegador = await chromium.launch({ headless: true,
    ...(process.env.PLAYWRIGHT_CHANNEL ? { channel: process.env.PLAYWRIGHT_CHANNEL } : {}) });
  const falhasJavaScript = [];
  const falhasRecursos = [];
  try {
    for (const tela of telas) {
      const { nome, ...opcoes } = tela;
      const contexto = await navegador.newContext({ baseURL: endereco.origin, ...opcoes });
      try {
        await autenticar(contexto);
        const respostaIgrejas = await contexto.request.get('/api/igrejas');
        assert.equal(respostaIgrejas.status(), 200);
        const igrejas = (await respostaIgrejas.json()).filter(igreja => igreja.ativa);
        assert.ok(igrejas.length > 0);
        const igrejaPrincipal = igrejas.find(igreja => igreja.cidade.includes('Maio')) || igrejas[0];
        const igrejasParaTestar = nome === 'computador' ? igrejas : [igrejaPrincipal];
        for (const igreja of igrejasParaTestar) {
          const pagina = await contexto.newPage();
          pagina.on('pageerror', erro => falhasJavaScript.push(erro.message));
          pagina.on('response', resposta => {
            if (resposta.status() >= 400) falhasRecursos.push(`${resposta.status()} ${resposta.url()}`);
          });
          await pagina.goto(`/qrcode/${igreja.id}`);
          await pagina.getByRole('button', { name: 'Imprimir em A4' }).waitFor();
          await pagina.waitForFunction(() => [...document.querySelectorAll('.cartaz-a4 img')]
            .every(img => img.complete && img.naturalWidth > 0));
          await verificarLayout(pagina, false);
          const prefixo = `${nome}-${igreja.id}`;
          await pagina.locator('.cartaz-a4').screenshot({ path: path.join(pastaSaida, `${prefixo}-tela.png`) });
          await pagina.emulateMedia({ media: 'print' });
          await verificarLayout(pagina, true);
          await pagina.pdf({ path: path.join(pastaSaida, `${prefixo}.pdf`),
            preferCSSPageSize: true, printBackground: true, displayHeaderFooter: false });
          await pagina.emulateMedia({ media: 'screen' });
          const voltar = pagina.getByRole('link', { name: 'Voltar ao painel', exact: false });
          assert.ok((await voltar.getAttribute('href')).includes(`igrejaId=${igreja.id}`));
          await voltar.click();
          await pagina.waitForURL(url => url.pathname === '/painel');
          assert.equal(new URL(pagina.url()).searchParams.get('igrejaId'), igreja.id);
          const formulario = await contexto.request.get(`/api/publico/igrejas/${igreja.identificadorPublico}`);
          assert.equal(formulario.status(), 200, 'O formulário de visitantes deve continuar acessível.');
          console.log(`${nome}: convite, impressão e retorno à igreja validados.`);
          await pagina.close();
        }
      } finally { await contexto.close(); }
    }
    assert.deepEqual(falhasJavaScript, []);
    assert.deepEqual(falhasRecursos, []);
    console.log('Sem erros JavaScript ou falhas de recursos. PDFs de prova em .tools/v1-convite-qa.');
  } finally { await navegador.close(); }
}

executar().catch(erro => { console.error(erro.message); process.exitCode = 1; });
