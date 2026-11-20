(() => {
 const model=window.IndicaDraftModel;
 const forms=[...document.querySelectorAll('form[data-draft]')];
 const eligible=e=>!e.hasAttribute('data-draft-ignore')&&model.eligible(e.name,e.type);
 const read=f=>[...f.elements].filter(eligible).map(e=>({name:e.name,type:e.type,value:e.value,checked:!!e.checked}));
 const baselines=new Map(forms.map(f=>[f,read(f)]));const pending=new Set();
 const apply=(form,fields)=>{[...form.elements].filter(eligible).forEach(e=>{const field=fields.find(v=>model.identity(v)===model.identity({name:e.name,type:e.type,value:e.value}));if(field){if(['checkbox','radio'].includes(e.type))e.checked=field.checked;else e.value=field.value;}});form.dispatchEvent(new Event('change',{bubbles:true}));};
 window.indicaDrafts={capture:()=>forms.forEach(f=>{if(pending.has(f))return;const fields=model.changes(baselines.get(f),read(f));if(fields.length)sessionStorage.setItem(model.key(f.dataset.draft),JSON.stringify({version:f.dataset.version||'0',fields}));else sessionStorage.removeItem(model.key(f.dataset.draft));})};
 forms.forEach(form=>{
  try{
   const key=model.key(form.dataset.draft);const saved=JSON.parse(sessionStorage.getItem(key)||'null');
   if(saved&&Array.isArray(saved.fields)){
    const plan=model.restorePlan(saved,form.dataset.version||'0');
    if(!plan.conflict){apply(form,plan.fields);sessionStorage.removeItem(key);}
    else{
     pending.add(form);const panel=document.createElement('section');panel.className='notice draft-conflict';panel.tabIndex=-1;panel.setAttribute('role','alert');
     const text=document.createElement('p');text.textContent=document.documentElement.dataset.draftConflict;panel.append(text);
     const preview=document.createElement('ul');plan.fields.forEach(field=>{const item=document.createElement('li');const control=[...form.elements].find(e=>e.name===field.name);const label=control?.labels?.[0]?.textContent.trim()||'';const option=control?.options?[...control.options].find(o=>o.value===field.value):null;item.textContent=label+': '+(option?.textContent??field.value);preview.append(item);});panel.append(preview);
     const resolve=useDraft=>{if(useDraft)apply(form,plan.fields);pending.delete(form);sessionStorage.removeItem(key);panel.remove();};
     for(const [label,useDraft]of [[document.documentElement.dataset.useDraft,true],[document.documentElement.dataset.keepCurrent,false]]){const button=document.createElement('button');button.type='button';button.className=useDraft?'':'secondary';button.textContent=label;button.addEventListener('click',()=>resolve(useDraft));panel.append(button);}
     form.before(panel);form.addEventListener('submit',event=>{if(pending.has(form)){event.preventDefault();panel.focus();}});
    }
   }
  }catch{/* A storage error never submits or replaces server data. */}
  form.addEventListener('submit',event=>{if(!event.defaultPrevented)sessionStorage.removeItem(model.key(form.dataset.draft));});
 });
 document.querySelectorAll('form[data-confirm]').forEach(f=>f.addEventListener('submit',e=>{if(!confirm(f.dataset.confirm))e.preventDefault();}));
 document.querySelector('[role="alert"]:not([hidden])')?.focus();
})();
