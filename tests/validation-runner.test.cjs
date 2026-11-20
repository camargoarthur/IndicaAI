const {test}=require('node:test');
const assert=require('node:assert/strict');
const fs=require('node:fs'),path=require('node:path'),os=require('node:os');
const runnerPath=path.join(__dirname,'../scripts/validate.cjs');
test('executor precisa existir',()=>assert.ok(fs.existsSync(runnerPath),'Falta o executor com códigos de saída e resumo'));
if(fs.existsSync(runnerPath)){
 const {executeProcess,checkTestReport,checkNodeReport,windowsPowerShellEnvironment}=require(runnerPath);
 test('Node omitido com exit 0 não aprova a etapa',()=>{
  const dir=fs.mkdtempSync(path.join(os.tmpdir(),'IndicaAI-validator-'));
  const log=path.join(dir,'skip.log');
  const r=executeProcess(process.execPath,['--test-reporter=tap','-e',"require('node:test').skip('omitido',()=>{})"],{log});
  assert.equal(r.exitCode,0);assert.equal(typeof checkNodeReport,'function');
  assert.throws(()=>checkNodeReport(fs.readFileSync(log,'utf8')),/omitido|incompleto/);
 });
 test('Windows PowerShell carrega módulos nativos mesmo chamado de PowerShell 7',()=>{
  const dir=fs.mkdtempSync(path.join(os.tmpdir(),'IndicaAI-validator-'));
  assert.equal(typeof windowsPowerShellEnvironment,'function');
  const env=windowsPowerShellEnvironment({...process.env,PSModulePath:path.join(dir,'modulos-incompativeis')});
  const r=executeProcess(path.join(process.env.SystemRoot,'System32/WindowsPowerShell/v1.0/powershell.exe'),['-NoProfile','-Command',"$ErrorActionPreference='Stop'; Get-Command Get-FileHash,Get-CimInstance | Select-Object -ExpandProperty Name"],{log:path.join(dir,'ps51.log'),env});
  assert.equal(r.passed,true,fs.readFileSync(r.log,'utf8'));
 });
 test('falha real do processo é preservada com log',()=>{
  const dir=fs.mkdtempSync(path.join(os.tmpdir(),'IndicaAI-validator-'));
  const r=executeProcess(process.execPath,['-e','console.error("falha controlada");process.exit(7)'],{log:path.join(dir,'fail.log')});
  assert.equal(r.exitCode,7);assert.equal(r.passed,false);assert.match(fs.readFileSync(r.log,'utf8'),/falha controlada/);
 });
 test('timeout nunca passa como sucesso',()=>{
  const dir=fs.mkdtempSync(path.join(os.tmpdir(),'IndicaAI-validator-'));
  const r=executeProcess(process.execPath,['-e','setInterval(()=>{},1000)'],{log:path.join(dir,'timeout.log'),timeout:150});
  assert.equal(r.passed,false);assert.match(r.error,/ETIMEDOUT/);
 });
 test('processo ausente nunca passa como sucesso',()=>{
  const dir=fs.mkdtempSync(path.join(os.tmpdir(),'IndicaAI-validator-'));
  const r=executeProcess(path.join(dir,'missing.exe'),[],{log:path.join(dir,'missing.log')});
  assert.equal(r.passed,false);
 });
 test('zero testes, skips ou falhas não são uma validação aprovada',()=>{
  for(const summary of [{tests:0,passed:0,failed:0,skipped:0,pending:0,other:0},{tests:1,passed:0,failed:1,skipped:0,pending:0,other:0},{tests:1,passed:0,failed:0,skipped:1,pending:0,other:0}])assert.throws(()=>checkTestReport({results:{summary}}));
  assert.equal(checkTestReport({results:{summary:{tests:2,passed:2,failed:0,skipped:0,pending:0,other:0}}}).tests,2);
 });
}
