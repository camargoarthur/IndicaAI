(() => {
 const form=document.querySelector('[data-selection-form]');if(!form)return;
 const items=[...form.querySelectorAll('[data-select-item]')];const toggle=form.querySelector('[data-selection-toggle]');let active=false;
 const update=()=>{const count=items.filter(x=>x.checked).length;if(count)active=true;form.classList.toggle('selection-active',active);toggle?.setAttribute('aria-pressed',String(active));const counter=form.querySelector('[data-selection-count]');if(counter)counter.textContent=count;const button=form.querySelector('[data-selection-submit]');if(button){button.disabled=!count;button.hidden=!active;}};
 items.forEach(x=>x.addEventListener('change',update));
 toggle?.addEventListener('click',()=>{active=!active;if(!active)items.forEach(x=>x.checked=false);update();});
 form.querySelector('[data-select-page]')?.addEventListener('change',e=>{items.forEach(x=>x.checked=e.target.checked);update();});
 const confirmChange=e=>{if(items.some(x=>x.checked)&&!confirm(document.documentElement.dataset.clearSelectionMessage))e.preventDefault();};
 document.querySelector('[data-selection-context]')?.addEventListener('submit',confirmChange);
 document.querySelectorAll('.masthead nav a,.pagination a,.browse-filter a').forEach(a=>a.addEventListener('click',confirmChange));update();
})();
