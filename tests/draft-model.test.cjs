const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const scope = {};
vm.runInNewContext(fs.readFileSync('src/IndicaAI.Web/wwwroot/js/draft-model.js','utf8'),scope);
const model=scope.IndicaDraftModel;
const baseline=[{name:'Note',type:'textarea',value:'original',checked:false},{name:'State',type:'select-one',value:'planned',checked:false}];
const edited=[{...baseline[0],value:'rascunho'},{...baseline[1]}];
const changed=model.changes(baseline,edited);
assert.equal(changed.length,1);assert.equal(changed[0].value,'rascunho');
assert.equal(model.restorePlan({version:'1',fields:changed},'2').conflict,true);
assert.equal(model.restorePlan({version:'1',fields:changed},'1').conflict,false);
assert.equal(model.key('work-42'),model.key('work-42'));
assert.equal(model.eligible('__RequestVerificationToken','hidden'),false);
assert.equal(model.eligible('Input.Version','hidden'),false);
assert.equal(model.eligible('Upload','file'),false);
assert.equal(model.eligible('Note','textarea'),true);
// TM04/TM17/TM30: campos repetidos, desmarcação e nota apagada não podem desaparecer do rascunho.
const checks=[{name:'ids',type:'checkbox',value:'book',checked:true},{name:'ids',type:'checkbox',value:'game',checked:false}];
const switched=[{...checks[0],checked:false},{...checks[1],checked:true}];
const selection=model.changes(checks,switched);
assert.equal(selection.length,2);assert.equal(selection[0].checked,false);assert.equal(selection[1].checked,true);
assert.notEqual(model.identity(checks[0]),model.identity(checks[1]));
assert.equal(model.changes(baseline,[{...baseline[0],value:''},baseline[1]])[0].value,'');
assert.equal(model.changes(baseline,baseline).length,0);
assert.notEqual(model.key('work-book'),model.key('work-game'));
assert.equal(model.restorePlan({version:1,fields:selection},'1').conflict,false);
assert.equal(model.eligible('Password','password'),false);assert.equal(model.eligible('','text'),false);
console.log('PASS: Draft model — texto apagado, checkboxes distintos/desmarcados, isolamento, conflito de versão e exclusão de tokens. Não executa DOM/navegador.');

function control() {
 const events = {};
 return {
  checked: false,
  addEventListener: (name, handler) => { events[name] = handler; },
  dispatch: name => events[name](),
  setAttribute(name, value) { this[name] = value; }
 };
}

require('node:test').test('seleção da página acompanha nenhum, parte ou todos os cartões', () => {
 const items = [control(), control()];
 const toggle = control();
 const page = control();
 const counter = {};
 const submit = {};
 const controls = { '[data-selection-toggle]': toggle, '[data-select-page]': page,
  '[data-selection-count]': counter, '[data-selection-submit]': submit };
 const form = {
  querySelectorAll: () => items,
  querySelector: selector => controls[selector],
  classList: { toggle() {} }
 };
 const document = { querySelector: selector => selector === '[data-selection-form]' ? form : null, querySelectorAll: () => [] };
 const window = control();
 vm.runInNewContext(fs.readFileSync('src/IndicaAI.Web/wwwroot/js/selection.js', 'utf8'), { document, window });
 toggle.dispatch('click');
 assert.equal(submit.disabled, true);
 page.checked = true;
 page.dispatch('change');
 assert.ok(items.every(item => item.checked));
 items[0].checked = false;
 items[0].dispatch('change');
 assert.equal(page.checked, false);
 assert.equal(page.indeterminate, true);
 assert.equal(counter.textContent, 1);
 items[1].checked = false;
 window.dispatch('pageshow');
 assert.equal(page.indeterminate, false);
 assert.equal(submit.disabled, true);
});
