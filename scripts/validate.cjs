const fs=require('node:fs'),path=require('node:path'),{spawnSync}=require('node:child_process');
const {selectPlan}=require('./validation-plan.cjs');
const root=path.resolve(__dirname,'..');

function executeProcess(command,args,{log,cwd=root,env=process.env,timeout=300000}={}){
 const started=Date.now(); const fd=fs.openSync(log,'w'); let r;
 try{r=spawnSync(command,args,{cwd,env,windowsHide:true,shell:false,timeout,stdio:['ignore',fd,fd]});}
 finally{fs.closeSync(fd);}
 return {passed:r.status===0&&!r.error,exitCode:r.status,durationMs:Date.now()-started,log,error:r.error?.message??null};
}
function checkTestReport(report){
 const s=report?.results?.summary;
 if(!s||!Number.isInteger(s.tests)||s.tests<1||s.passed!==s.tests||s.failed!==0||s.skipped!==0||s.pending!==0||s.other!==0)throw Error('Relatório de testes ausente/incompleto, sem testes ou com falhas/omissões.');
 return s;
}
function checkNodeReport(text){
 const summary={};
 for(const key of ['tests','pass','fail','cancelled','skipped','todo']){
  const match=text.match(new RegExp('^# '+key+' (\\d+)\\s*$','m'));
  if(!match)throw Error('Relatório Node incompleto.');
  summary[key]=Number(match[1]);
 }
 if(summary.tests<1||summary.pass!==summary.tests||summary.fail||summary.cancelled||summary.skipped||summary.todo)throw Error('Relatório Node incompleto ou com teste omitido/falha.');
 return summary;
}
function windowsPowerShellEnvironment(env){
 // PowerShell 7 -> Node -> Windows PowerShell não faz a limpeza que a chamada direta do pwsh faz.
 // Usamos apenas módulos nativos 5.1 necessários a estes scripts, sem carregar módulos Core incompatíveis.
 return {...env,PSModulePath:path.join(env.SystemRoot||'C:/Windows','System32/WindowsPowerShell/v1.0/Modules')};
}
function parse(args){
 const o={changedPaths:[],modules:[]};
 for(let i=0;i<args.length;i++){
  const key=args[i];
  if(key==='--plan'){o.planOnly=true;continue;}
  if(!['--profile','--changed','--module'].includes(key)||!args[i+1]||args[i+1].startsWith('--'))throw Error('Argumento inválido/incompleto: '+key);
  const value=args[++i];if(key==='--profile')o.profile=value;else o[key==='--changed'?'changedPaths':'modules'].push(value);
 }
 return o;
}
function main(args){
 const options=parse(args),plan=selectPlan(options);
 if(options.planOnly){console.log(JSON.stringify(plan,null,2));return 0;}
 const runDir=path.join(root,'.tools','validation',new Date().toISOString().replace(/[:.]/g,'-')+'-'+process.pid);
 fs.mkdirSync(runDir,{recursive:true});
 const summary={version:fs.readFileSync(path.join(root,'VERSION'),'utf8').trim(),startedAt:new Date().toISOString(),plan,changedPaths:options.changedPaths,steps:[],passed:false,
  limitations:['TestServer não executa JavaScript do navegador.','Aparência, foco, diálogos e clipboard continuam com conferência manual do roteiro.','Nenhum resultado marca TM automaticamente como aprovado.']};
 const env={...process.env,DOTNET_ROOT:path.join(root,'.tools/dotnet'),DOTNET_CLI_HOME:path.join(root,'.tools/cli-home'),NUGET_PACKAGES:path.join(root,'.tools/nuget'),DOTNET_CLI_TELEMETRY_OPTOUT:'1',DOTNET_NOLOGO:'1',DOTNET_GENERATE_ASPNET_CERTIFICATE:'false'};
 const dotnet=path.join(env.DOTNET_ROOT,'dotnet.exe');
 const ps=path.join(process.env.SystemRoot||'C:/Windows','System32/WindowsPowerShell/v1.0/powershell.exe');
 const step=(name,command,argv,extra={})=>{
  const result=executeProcess(command,argv,{log:path.join(runDir,name+'.log'),env,...extra});
  if(result.passed&&extra.verify)try{extra.verify(result);}catch(error){result.passed=false;result.error=error.message;}
  summary.steps.push({name,command,args:argv,...result});
  console.log(`${result.passed?'PASS':'FAIL'} ${name} (${(result.durationMs/1000).toFixed(1)}s)`);
  if(!result.passed)throw Error(name+' falhou; consulte '+result.log+(result.error?' — '+result.error:''));
 };
 const powershell=(name,file,argv=[],extra={})=>step(name,ps,['-NoLogo','-NoProfile','-ExecutionPolicy','RemoteSigned','-File',path.join(root,file),...argv],{env:windowsPowerShellEnvironment(env),...extra});
 console.log(`Validação ${summary.version}: ${plan.profile}. Logs: ${runDir}`);
 try{
  step('validation-contracts',process.execPath,['--test','--test-reporter=tap','tests/validation-plan.test.cjs','tests/validation-runner.test.cjs','tests/guide-contract.test.cjs'],{verify:r=>{summary.nodeContracts=checkNodeReport(fs.readFileSync(r.log,'utf8'));}});
  if(plan.profile!=='Docs'){
   step('draft-model',process.execPath,['tests/draft-model.test.cjs']);
   step('build',dotnet,['build','tests/IndicaAI.Tests','--no-restore','--disable-build-servers','-v','minimal']);
   const filters=plan.profile==='Visual'?['*BootstrapHttpTests','*JourneyTests','*CustomLabelTests','*FinalReviewTests']:plan.classes;
   const report=path.join(runDir,'product-tests.json');
   step('product-tests',dotnet,['tests/IndicaAI.Tests/bin/Debug/net10.0/IndicaAI.Tests.dll','-noLogo','-failSkips','-result-ctrf',report,...filters.flatMap(c=>['-class',c])],{verify:()=>{summary.productTests=checkTestReport(JSON.parse(fs.readFileSync(report,'utf8')));}});
   console.log(`Produto: ${summary.productTests.passed}/${summary.productTests.tests} aprovados.`);
  }
  if(plan.profile==='Full'){
   const packageRoot=path.join(runDir,'package');
   step('publish',dotnet,['publish','src/IndicaAI.Web','-c','Release','-r','win-x64','--self-contained','true','--no-restore','--disable-build-servers','-o',packageRoot,'-v','minimal']);
   for(const file of ['Start-IndicaAI.bat','Start-IndicaAI.ps1','LauncherProcess.cs'])fs.copyFileSync(path.join(root,'scripts',file),path.join(packageRoot,file));
   powershell('package','scripts/Verify-Release.ps1',['-PackageRoot',packageRoot,'-ReportPath',path.join(runDir,'release-assets.json')]);
   // O ensaio tem esperas limitadas e finally para seus processos. Não interromper o host com timeout externo.
   powershell('launcher','tests/launcher-lifetime.test.ps1',['-LauncherRoot',packageRoot,'-ThroughBat','-LogDirectory',path.join(runDir,'launcher-output')],{timeout:0});
  }
  powershell('documents','scripts/Verify-Documents.ps1',['-AllowExtra']);
  summary.passed=true;
 }catch(error){summary.error=error.message;console.error(error.message);}
 finally{
  summary.finishedAt=new Date().toISOString();
  fs.writeFileSync(path.join(runDir,'summary.json'),JSON.stringify(summary,null,2));
  console.log(`${summary.passed?'APROVADO':'REPROVADO'}: ${summary.steps.filter(s=>s.passed).length}/${summary.steps.length} etapas. Resumo: ${path.join(runDir,'summary.json')}`);
 }
 return summary.passed?0:1;
}
if(require.main===module){try{process.exitCode=main(process.argv.slice(2));}catch(e){console.error(e.message);process.exitCode=1;}}
module.exports={executeProcess,checkTestReport,checkNodeReport,windowsPowerShellEnvironment};
