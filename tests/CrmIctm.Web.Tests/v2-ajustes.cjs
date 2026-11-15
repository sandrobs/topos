// Teste local: UI responsiva, consentimento, navegação e os quatro PDFs.
// Com TESTE_CRIAR_DADOS=1 cria UMA solicitação fictícia e aprova como membro.
// TESTE_VERIFICAR_PDF=0 permite repetir apenas a UI sem consumir o limite de PDFs.
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { chromium } = require(process.env.PLAYWRIGHT_MODULE_PATH || 'playwright');
const origem = new URL(process.env.TESTE_BASE_URL || 'http://localhost:8081');
assert.ok(['localhost', '127.0.0.1', '[::1]'].includes(origem.hostname), 'Somente ambiente local.');
assert.ok(process.env.TESTE_EMAIL && process.env.TESTE_SENHA, 'Informe conta local por variáveis.');
const saida = path.resolve(__dirname, '../../.tools/v2-ajustes-qa');
fs.mkdirSync(saida, { recursive: true });

async function login(contexto) {
  const resposta = await contexto.request.get(`${origem.origin}/api/seguranca/token-csrf`);
  const { token } = await resposta.json();
  const entrar = await contexto.request.post(`${origem.origin}/api/autenticacao/entrar`, {
    headers: { 'X-CSRF-TOKEN': token }, data: { email: process.env.TESTE_EMAIL, senha: process.env.TESTE_SENHA },
  });
  assert.equal(entrar.status(), 204);
}

async function conferirLayout(pagina) {
  const medidas = await pagina.evaluate(() => ({
    largura: window.innerWidth, larguraDocumento: document.documentElement.scrollWidth,
    fora: [...document.querySelectorAll('input:not(.controle-escondido),select,button,fieldset')]
      .filter(item => item.getClientRects().length && !item.closest('dialog:not([open])'))
      // A navegação existente usa rolagem interna horizontal no celular, não da página.
      .filter(item => { const nav = item.closest('.navegacao-interna'); return !nav || getComputedStyle(nav).overflowX !== 'auto'; })
      .filter(item => { const r = item.getBoundingClientRect(); return r.left < -1 || r.right > innerWidth + 1; })
      .map(item => ({ elemento: item.tagName, classe: item.className, texto: item.textContent.slice(0, 50) })),
  }));
  assert.ok(medidas.larguraDocumento <= medidas.largura + 1, JSON.stringify(medidas));
  assert.deepEqual(medidas.fora, []);
}

(async () => {
  const navegador = await chromium.launch({ headless: true, channel: process.env.PLAYWRIGHT_CHANNEL || 'chrome' });
  const telas = [
    { nome: 'desktop', viewport: { width: 1440, height: 1000 } },
    { nome: 'celular', viewport: { width: 390, height: 844 }, isMobile: true, hasTouch: true },
    { nome: 'celular-estreito', viewport: { width: 320, height: 740 }, isMobile: true, hasTouch: true },
    { nome: 'tablet', viewport: { width: 834, height: 1194 }, isMobile: true, hasTouch: true },
  ];
  let igreja;
  let sessaoAutenticada;
  const falhas = [];
  try {
    for (const tela of telas) {
      const contexto = await navegador.newContext({ ...tela, storageState: sessaoAutenticada });
      if (!sessaoAutenticada) {
        await login(contexto);
        sessaoAutenticada = await contexto.storageState();
      }
      igreja ||= (await (await contexto.request.get(`${origem.origin}/api/igrejas`)).json()).find(item => item.ativa);
      assert.ok(igreja);
      const pagina = await contexto.newPage();
      pagina.on('pageerror', erro => falhas.push(erro.message));
      pagina.on('response', resposta => { if (resposta.status() >= 400) falhas.push(`${resposta.status()} ${resposta.url()}`); });
      await pagina.goto(`${origem.origin}/membros/cadastro/${igreja.identificadorPublicoMembros}`);
      await pagina.getByRole('heading', { name: 'Uma família no amor de Deus' }).waitFor();
      assert.equal(await pagina.getByRole('radio', { name: 'Membro', exact: true }).isChecked(), true);
      assert.equal(await pagina.getByRole('checkbox').isChecked(), false);
      await pagina.getByRole('radio', { name: 'Diácono', exact: true }).locator('..').click();
      await pagina.getByRole('radio', { name: 'Sou batizado(a)', exact: true }).locator('..').click();
      await pagina.getByLabel('Data do batismo').fill('2000-05-20');
      await pagina.getByLabel('CEP', { exact: true }).fill('98910000');
      assert.equal(await pagina.getByLabel('CEP', { exact: true }).inputValue(), '98910-000');
      await pagina.getByRole('button', { name: /Segurança e privacidade/ }).click();
      await pagina.locator('dialog[open]').waitFor();
      await conferirLayout(pagina);
      assert.match(await pagina.locator('dialog').innerText(), /revogar a autorização/);
      await pagina.screenshot({ path: path.join(saida, `privacidade-${tela.nome}.png`) });
      await pagina.keyboard.press('Escape');
      await pagina.locator('dialog[open]').waitFor({ state: 'hidden' });
      await conferirLayout(pagina);
      await pagina.screenshot({ path: path.join(saida, `formulario-${tela.nome}.png`), fullPage: true });

      if (tela.nome === 'desktop' && process.env.TESTE_CRIAR_DADOS === '1') {
        const sobrenome = `Layout${Date.now()}`;
        await pagina.locator('#membro-nome').fill('Teste UI');
        await pagina.locator('#membro-sobrenome').fill(sobrenome);
        await pagina.getByLabel('WhatsApp com DDD').fill('5599000' + String(Date.now() % 10000).padStart(4, '0'));
        await pagina.getByRole('checkbox').locator('..').click();
        await pagina.getByRole('button', { name: 'Enviar cadastro para análise' }).click();
        await pagina.getByRole('heading', { name: 'Cadastro recebido' }).waitFor();
        await pagina.goto(`${origem.origin}/painel?igrejaId=${igreja.id}&secao=MEMBROS`);
        await pagina.getByRole('heading', { name: 'Gestão de membros' }).waitFor();
        await pagina.getByRole('tab', { name: /Solicitações/ }).click();
        await pagina.getByRole('button', { name: new RegExp(sobrenome) }).click();
        await pagina.getByRole('heading', { name: `Teste UI ${sobrenome}` }).waitFor();
        await pagina.getByRole('button', { name: '← Voltar à lista' }).click();
        await pagina.getByRole('button', { name: new RegExp(sobrenome) }).click();
        assert.match(await pagina.locator('.resumo-dados-membro').innerText(), /Diácono/);
        await pagina.getByRole('button', { name: 'Aprovar', exact: true }).click();
        await pagina.locator('.cartao-gestao-membro').getByText('Origem:', { exact: false }).waitFor();
        await pagina.getByRole('button', { name: '← Voltar à lista' }).click();
        await pagina.getByRole('searchbox', { name: 'Pesquisar por nome ou WhatsApp' }).fill(sobrenome);
        await pagina.getByRole('button', { name: 'Pesquisar', exact: true }).click();
        await pagina.getByRole('button', { name: new RegExp(sobrenome) }).click();
        await pagina.getByRole('heading', { name: `Teste UI ${sobrenome}` }).waitFor();
        assert.match(await pagina.locator('.resumo-dados-membro').innerText(), /Diácono/);
        await pagina.getByRole('button', { name: '← Voltar à lista' }).click();
        assert.equal(await pagina.getByRole('searchbox').inputValue(), sobrenome);
        console.log('Fluxo fictício de envio, revisão, aprovação, detalhe e retorno: aprovado.');
      }

      for (const membros of [false, true]) {
        await pagina.goto(`${origem.origin}/${membros ? 'qrcode-membros' : 'qrcode'}/${igreja.id}`);
        await pagina.locator('.cartaz-a4').waitFor();
        await pagina.waitForFunction(() => [...document.querySelectorAll('.cartaz-a4 img')].every(img => img.complete && img.naturalWidth));
        await conferirLayout(pagina);
        await pagina.screenshot({ path: path.join(saida, `convite-${membros ? 'membros' : 'visitantes'}-${tela.nome}.png`), fullPage: true });
        for (const quatro of process.env.TESTE_VERIFICAR_PDF === '0' ? [] : tela.nome === 'desktop' ? [false, true] : [false]) {
          const link = pagina.getByRole('link', { name: quatro ? '4 convites por A4' : 'Abrir PDF A4', exact: true });
          const url = await link.getAttribute('href');
          const resposta = await contexto.request.get(origem.origin + url);
          assert.equal(resposta.status(), 200);
          assert.match(resposta.headers()['content-type'], /application\/pdf/);
          if (tela.nome === 'desktop') fs.writeFileSync(path.join(saida, `${membros ? 'membros' : 'visitantes'}-${quatro ? '4-por-a4' : 'a4'}.pdf`), await resposta.body());
        }
        await pagina.getByRole('link', { name: '← Voltar ao painel' }).click();
        if (membros) await pagina.getByRole('heading', { name: 'Gestão de membros' }).waitFor();
        assert.equal(new URL(pagina.url()).searchParams.get('igrejaId'), igreja.id);
        if (membros) {
          assert.equal(new URL(pagina.url()).searchParams.get('secao'), 'MEMBROS');
          await conferirLayout(pagina);
          await pagina.screenshot({ path: path.join(saida, `gestao-${tela.nome}.png`), fullPage: true });
          const primeiroMembro = pagina.locator('.lista-membros button').first();
          if (await primeiroMembro.count()) {
            await primeiroMembro.click();
            await pagina.getByRole('button', { name: '← Voltar à lista' }).waitFor();
            assert.equal(await pagina.getByRole('button', { name: '← Voltar à lista' })
              .evaluate(botao => getComputedStyle(botao).color), 'rgb(16, 34, 88)');
            await conferirLayout(pagina);
            await pagina.screenshot({ path: path.join(saida, `detalhe-${tela.nome}.png`), fullPage: true });
            await pagina.getByRole('button', { name: '← Voltar à lista' }).click();
            await primeiroMembro.waitFor();
          }
        }
      }
      // Também não perdemos o contexto caso a igreja do convite esteja indisponível.
      await pagina.route('**/api/igrejas', rota => rota.fulfill({ json: [] }), { times: 1 });
      await pagina.goto(`${origem.origin}/qrcode-membros/${igreja.id}`);
      await pagina.getByRole('heading', { name: 'Cartaz indisponível' }).waitFor();
      assert.equal(await pagina.getByRole('link', { name: 'Voltar ao painel' }).getAttribute('href'),
        `/painel?igrejaId=${igreja.id}&secao=MEMBROS`);
      await contexto.close();
      console.log(`${tela.nome}: formulário, diálogo, convites e retorno aprovados.`);
    }
    assert.deepEqual(falhas, []);
    console.log('Sem erros JavaScript ou recursos HTTP com falha.');
    if (process.env.TESTE_VERIFICAR_PDF !== '0') console.log('PDFs salvos em .tools/v2-ajustes-qa.');
  } finally { await navegador.close(); }
})().catch(erro => { console.error(erro); process.exitCode = 1; });
