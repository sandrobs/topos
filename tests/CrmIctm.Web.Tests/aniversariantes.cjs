const assert = require('node:assert/strict');
const fs = require('node:fs');
const { chromium } = require(process.env.PLAYWRIGHT_MODULE_PATH || 'playwright');
const origem = process.env.TESTE_BASE_URL || 'http://localhost:8083';
assert.ok(['localhost', '127.0.0.1'].includes(new URL(origem).hostname));
const config = Object.fromEntries(fs.readFileSync('.env', 'utf8').split(/\r?\n/)
  .filter(line => /^[A-Z_]+=/.test(line)).map(line => {
    const i = line.indexOf('=');
    return [line.slice(0, i), line.slice(i + 1).replace(/^"|"$/g, '')];
  }));

(async () => {
  const browser = await chromium.launch({ headless: true, channel: 'chrome' });
  try {
    for (const width of [375, 834, 1440]) {
      const context = await browser.newContext({ viewport: { width, height: 950 } });
      const csrf = await (await context.request.get(origem + '/api/seguranca/token-csrf')).json();
      const login = await context.request.post(origem + '/api/autenticacao/entrar', {
        headers: { 'X-CSRF-TOKEN': csrf.token },
        data: { email: config.ADMIN_EMAIL, senha: config.ADMIN_SENHA },
      });
      assert.equal(login.status(), 204);
      const igrejas = await (await context.request.get(origem + '/api/igrejas')).json();
      const igreja = igrejas.find(item => item.ativa);
      const page = await context.newPage();
      const erros = [];
      page.on('pageerror', erro => erros.push(erro.message));
      await page.goto(`${origem}/painel?igrejaId=${igreja.id}&secao=ANIVERSARIANTES`);
      await page.getByRole('heading', { name: 'Aniversariantes', exact: true }).waitFor();
      await page.locator('.lista-membros[aria-busy="false"]').waitFor();
      assert.ok(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth + 1));
      for (const periodo of ['HOJE', 'SEMANA', 'MES']) {
        const response = await context.request.get(`${origem}/api/membros/aniversariantes?igrejaId=${igreja.id}&periodo=${periodo}`);
        assert.equal(response.status(), 200);
      }
      await page.route('**/api/membros/aniversariantes?**', route => route.fulfill({ json: {
        itens: [{ id: 'regressao', nome: 'Pessoa', sobrenome: 'Anterior', funcao: 'MEMBRO',
          ehMenor: false, idadeQueCompleta: 35, dataOcorrencia: '2026-11-15',
          destino: 'NENHUM', whatsappDestino: null }],
        hoje: '2026-11-15', inicio: '2026-11-15', fim: '2026-11-21',
      } }));
      await page.getByRole('tab', { name: /Semana/ }).click();
      await page.getByText('Pessoa Anterior', { exact: true }).waitFor();
      await page.unroute('**/api/membros/aniversariantes?**');
      await page.route('**/api/membros/aniversariantes?**', route => route.fulfill({ status: 500 }));
      await page.getByRole('tab', { name: /^Hoje/ }).click();
      await page.getByRole('alert').waitFor();
      assert.equal(await page.locator('.linha-aniversario').count(), 0);
      assert.equal(await page.getByText('Pessoa Anterior', { exact: true }).count(), 0);
      assert.deepEqual(erros, []);
      await context.close();
      console.log(`${width}px: API, layout e falha de consulta sem dados anteriores aprovados.`);
    }
  } finally { await browser.close(); }
})().catch(erro => { console.error(erro); process.exitCode = 1; });
