const modules={
 Tracking:['*TrackingTests','*TrackingFailureTests','*TrackingHttpTests','*OrganizationTests','*StateLabelTests'],
 Lists:['*OrganizationTests','*FilterBatchTests','*BatchReviewTests','*RecoveryAndBatchTests'],
 Tags:['*TagTests','*ImportTests','*ImportFailureTests','*ImportRecoveryTests','*CsvStructureTests'],
 Imports:['*TagTests','*ImportTests','*ImportFailureTests','*ImportRecoveryTests','*CsvStructureTests'],
 Preferences:['*PreferenceTests','*StateLabelTests','*CustomLabelTests'],
 Library:['*FilterBatchTests','*OrganizationTests','*BatchReviewTests','*CatalogIntegrityTests'],
 Recovery:['*BackupTests','*BackupValidationTests','*RestoreDamagedActiveTests','*RecoveryAndBatchTests','*MigrationTests','*BootstrapTests']
};
const ranks={Docs:0,Visual:1,Functional:2,Full:3};
function impact(paths){
 if(!paths.length)return 'Full';
 let rank=0;
 for(let p of paths){
  if(typeof p!=='string'||!p.trim())return 'Full';
  p=p.replaceAll('\\','/').replace(/^\.\//,'');
  let next=3;
  if(p.includes('..')||p.startsWith('/')||/^[A-Za-z]:/.test(p))return 'Full';
  if(/^docs\/.*\.(md|html|png|jpg|jpeg|json|csv)$/.test(p)||/^[^/]+\.md$/.test(p))next=0;
  else if(/^src\/IndicaAI\.Web\/wwwroot\/(css|covers|icons|fonts)\//.test(p))next=1;
  else if(/^src\/IndicaAI\.Web\/(Features|Pages|Domain|Localization|Text|wwwroot\/js)\//.test(p)||/^tests\/IndicaAI\.Tests\/.*\.cs$/.test(p)||p==='tests/draft-model.test.cjs')next=2;
  rank=Math.max(rank,next);
 }
 return Object.keys(ranks).find(k=>ranks[k]===rank);
}
function selectPlan({profile='Auto',changedPaths=[],modules:selected=[]}={}){
 if(!Array.isArray(changedPaths)||!Array.isArray(selected))throw Error('Caminhos e módulos devem ser listas.');
 if(!['Auto','Docs','Visual','Functional','Module','Full'].includes(profile))throw Error('Perfil inválido: '+profile);
 if(profile!=='Module'&&selected.length)throw Error('Módulos exigem perfil Module.');
 const automatic=impact(changedPaths);
 if(profile==='Module'){
  if(!selected.length||selected.some(m=>!Object.hasOwn(modules,m)))throw Error('Informe módulos válidos: '+Object.keys(modules).join(', '));
  if(changedPaths.length&&automatic==='Full')throw Error('Mudança compartilhada/desconhecida exige Full.');
  return {profile,modules:selected,classes:[...new Set(['*NameKeyTests','*BootstrapHttpTests','*JourneyTests','*FinalReviewTests','*ManualGuideTests','*ManualFormTests',...selected.flatMap(m=>modules[m])])],reason:'Seleção para desenvolvimento; entrega funcional exige Functional ou Full.'};
 }
 if(profile==='Auto')profile=automatic;
 else if(changedPaths.length&&ranks[profile]<ranks[automatic])throw Error('Perfil explícito insuficiente para os caminhos informados: use '+automatic);
 return {profile,modules:[],classes:[],reason:changedPaths.length?'Maior impacto dos caminhos informados.':'Perfil explícito ou fallback completo por ausência de caminhos.'};
}
module.exports={selectPlan};
