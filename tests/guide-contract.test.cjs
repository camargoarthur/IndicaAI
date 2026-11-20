const {test}=require('node:test'),assert=require('node:assert/strict');
const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm');
const root=path.resolve(__dirname,'..');
const html=new TextDecoder('utf-8',{fatal:true}).decode(fs.readFileSync(path.join(root,'docs/TEST_CASES.html')));
const version=fs.readFileSync(path.join(root,'VERSION'),'utf8').trim();
test('guia usa versão funcional e armazenamento separado por versão',()=>{
 assert.match(version,/^0\.\d+$/);
 assert.ok(html.includes('indicaai-manual-tests-'+version+'-v1'));
 assert.match(html,new RegExp('class="edition"[^>]*>[\\s\\S]*?'+version.replaceAll('.','\\.')));
});
test('43 IDs únicos, caminhos, execução e resultados continuam presentes',()=>{
 const articles=[...html.matchAll(/<article class="case" id="(TM\d+)"[\s\S]*?<\/article>/g)];
 assert.ok(articles.length>=43);assert.equal(new Set(articles.map(m=>m[1])).size,articles.length);
 for(let i=1;i<=43;i++)assert.ok(articles.some(m=>m[1]==='TM'+String(i).padStart(2,'0')));
 for(const [article]of articles)for(const text of ['Caminho da tela','Como executar','Resultado esperado','data-result=','data-field='])assert.ok(article.includes(text));
});
test('JavaScript válido, fontes/licenças incorporadas e links locais existentes',()=>{
 let scripts=0;
 for(const [,attrs,source]of html.matchAll(/<script([^>]*)>([\s\S]*?)<\/script>/g))if(!attrs.includes('text/plain')){new vm.Script(source);scripts++;}
 assert.equal(scripts,1);assert.ok(html.includes('data:font/ttf;base64,'));assert.ok(html.includes('id="font-licenses"'));
 for(const [,href]of html.matchAll(/href="([^"]+)"/g)){
  if(href.startsWith('#'))assert.ok(html.includes('id="'+href.slice(1)+'"'));
  else if(!/^https?:/.test(href))assert.ok(fs.existsSync(path.resolve(root,'docs',href)),href);
 }
});
test('todos os casos têm mapa honesto e referências de testes existentes',()=>{
 const map=JSON.parse(fs.readFileSync(path.join(root,'tests/manual-coverage.json'),'utf8'));
 const ids=[...html.matchAll(/data-case="(TM\d+)"/g)].map(m=>m[1]);
 assert.deepEqual(map.map(m=>m.id),ids);
 for(const entry of map){
  assert.ok(['partial','manual'].includes(entry.coverage));assert.ok(entry.remaining.length>10);
  for(const ref of entry.tests){
   const [file,method]=ref.split('#');assert.ok(file.startsWith('tests/')&&!file.includes('..'));
   const source=fs.readFileSync(path.join(root,file),'utf8');
   if(method)assert.match(source,new RegExp('\\b'+method+'\\s*\\('));
  }
 }
});
