(() => {
 const form=document.querySelector('[data-batch-form]');if(!form)return;
 const mode=form.querySelector('[name=ActionKind]');const targets=[...form.querySelectorAll('[data-batch-target]')];
 const refresh=()=>{const states=mode.value==='states';form.classList.toggle('batch-for-lists',!states);let valid=targets.length>0;
 targets.forEach(select=>{const missing=!select.value;select.closest('[data-batch-group]').querySelector('[data-group-missing]').hidden=!states||!missing;select.setAttribute('aria-invalid',String(states&&missing));if(states&&missing)valid=false;});
 if(!states)valid=targets.length>0&&!!form.querySelector('[data-batch-list]').value;
 form.querySelector('[data-batch-confirm]').disabled=!valid;};form.addEventListener('change',refresh);refresh();
})();
