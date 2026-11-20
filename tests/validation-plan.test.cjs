const {test}=require('node:test');
const assert=require('node:assert/strict');
const fs=require('node:fs');
const planPath=require('node:path').join(__dirname,'../scripts/validation-plan.cjs');
test('executor por impacto precisa existir',()=>assert.ok(fs.existsSync(planPath),'Falta o seletor de validação por impacto'));
if(fs.existsSync(planPath)){
 const {selectPlan}=require(planPath);
 test('documentação isolada não inicia produto',()=>assert.equal(selectPlan({changedPaths:['README.md','docs/TEST_CASES.html']}).profile,'Docs'));
 test('CSS e imagem pedem verificação visual e build',()=>assert.equal(selectPlan({changedPaths:['src/IndicaAI.Web/wwwroot/css/site.css']}).profile,'Visual'));
 test('formulário e serviço pedem toda suíte funcional',()=>{
  for(const path of ['src/IndicaAI.Web/Features/Tracking/TrackingService.cs','src/IndicaAI.Web/Pages/Works/Details.cshtml','tests/IndicaAI.Tests/Web/ManualFormTests.cs'])assert.equal(selectPlan({changedPaths:[path]}).profile,'Functional');
 });
 test('infraestrutura, dependência, versão, testes do executor e desconhecidos ampliam para Full',()=>{
  for(const path of ['src/IndicaAI.Web/Hosting/DataPaths.cs','src/IndicaAI.Web/Persistence/AppDbContext.cs','src/IndicaAI.Web/Operations/BackupService.cs','src/IndicaAI.Web/IndicaAI.Web.csproj','scripts/validate.cjs','tests/validation-plan.test.cjs','VERSION','novo-arquivo.xyz'])assert.equal(selectPlan({changedPaths:[path]}).profile,'Full',path);
 });
 test('sem histórico de alterações, validação é completa',()=>assert.equal(selectPlan({}).profile,'Full'));
 test('mistura de caminhos usa o nível de maior impacto',()=>assert.equal(selectPlan({changedPaths:['README.md','src/IndicaAI.Web/Pages/Index.cshtml']}).profile,'Functional'));
 test('normaliza separadores Windows e prefixo relativo',()=>assert.equal(selectPlan({changedPaths:['.\\src\\IndicaAI.Web\\Features\\Lists\\ListService.cs']}).profile,'Functional'));
 test('caminhos absolutos não são classificados como simples documentação',()=>assert.equal(selectPlan({changedPaths:['C:/fora/README.md']}).profile,'Full'));
 test('módulo inclui smoke e dependências compartilhadas',()=>{
  const p=selectPlan({profile:'Module',modules:['Tracking','Lists']});
  for(const c of ['*BootstrapHttpTests','*TrackingTests','*OrganizationTests','*FilterBatchTests','*ManualFormTests'])assert.ok(p.classes.includes(c),c);
  assert.equal(new Set(p.classes).size,p.classes.length);
 });
 test('perfil/módulo inválido nunca reduz silenciosamente cobertura',()=>{
  for(const p of [{profile:'Doc'},{profile:'Module'},{profile:'Module',modules:['Typo']},{profile:'Docs',modules:['Tracking']}])assert.throws(()=>selectPlan(p));
 });
 test('perfil explícito não pode baixar o impacto dos caminhos informados',()=>assert.throws(()=>selectPlan({profile:'Docs',changedPaths:['src/IndicaAI.Web/Program.cs']})));
}
