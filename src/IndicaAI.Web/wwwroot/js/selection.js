(() => {
 const form = document.querySelector('[data-selection-form]');
 if (!form) return;

 const items = [...form.querySelectorAll('[data-select-item]')];
 const toggle = form.querySelector('[data-selection-toggle]');
 const selectPage = form.querySelector('[data-select-page]');
 const counter = form.querySelector('[data-selection-count]');
 const submit = form.querySelector('[data-selection-submit]');
 let active = false;

 function update() {
  const count = items.filter(item => item.checked).length;
  if (count > 0) active = true;
  form.classList.toggle('selection-active', active);
  toggle?.setAttribute('aria-pressed', String(active));
  if (counter) counter.textContent = count;
  if (submit) {
   submit.disabled = count === 0;
   submit.hidden = !active;
  }
  if (selectPage) {
   selectPage.checked = items.length > 0 && count === items.length;
   selectPage.indeterminate = count > 0 && count < items.length;
  }
 }

 items.forEach(item => item.addEventListener('change', update));
 toggle?.addEventListener('click', () => {
  active = !active;
  if (!active) items.forEach(item => { item.checked = false; });
  update();
 });
 selectPage?.addEventListener('change', () => {
  items.forEach(item => { item.checked = selectPage.checked; });
  update();
 });

 const confirmChange = event => {
  if (items.some(item => item.checked) && !confirm(document.documentElement.dataset.clearSelectionMessage)) event.preventDefault();
 };
 document.querySelector('[data-selection-context]')?.addEventListener('submit', confirmChange);
 document.querySelectorAll('.masthead nav a,.pagination a,.browse-filter a,.quick-states a,.work-card a')
  .forEach(link => link.addEventListener('click', confirmChange));
 window.addEventListener('pageshow', update);
 update();
})();
