(() => {
 const form=document.querySelector('[data-quick-preferences]');if(!form)return;
 const initial=new FormData(form);const originalError=document.querySelector("[data-preference-error]")?.textContent;
 form.addEventListener('change',async()=>{const notice=document.querySelector('[data-preference-error]');try{if([...document.querySelectorAll("input[type=file]")].some(input=>input.files?.length))throw new Error("file-draft");window.indicaDrafts?.capture();const response=await fetch(form.action,{method:'POST',body:new FormData(form),credentials:'same-origin'});if(!response.ok)throw new Error();
 const selection=[...document.querySelectorAll('[data-select-item]:checked')].map(e=>e.value);sessionStorage.setItem('indicaai-selection',JSON.stringify({url:location.href,ids:selection}));location.reload();
 }catch(error){if(notice)notice.textContent=error.message==="file-draft"?document.documentElement.dataset.fileDraftMessage:originalError;for(const [name,value]of initial){const input=form.elements.namedItem(name);if(input)input.value=value;}if(notice)notice.hidden=false;}});
 try{const saved=JSON.parse(sessionStorage.getItem('indicaai-selection')||'null');if(saved?.url===location.href){document.querySelectorAll('[data-select-item]').forEach(e=>e.checked=saved.ids.includes(e.value));document.querySelector('[data-select-item]')?.dispatchEvent(new Event('change'));sessionStorage.removeItem('indicaai-selection');}}catch{}
})();

