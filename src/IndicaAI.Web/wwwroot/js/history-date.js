(() => {
    const form = document.querySelector('[data-history-date]');
    if (!form) return;

    const { CycleId: cycle, Field: field, Value: value, LoadedDateContext: context, ClearDate: clear } = form.elements;

    const update = loadStoredDate => {
        const storedDate = cycle.selectedOptions[0]?.dataset[field.value] || '';
        if (loadStoredDate) value.value = storedDate;
        context.value = cycle.value + '|' + field.value;
        clear.hidden = !storedDate;
    };

    // Restoring a draft sets the selectors and edited date together.
    if (context.value !== cycle.value + '|' + field.value || value.value !== value.defaultValue)
        form.closest('details').open = true;
    update(false);
    form.addEventListener('change', event => {
        if (event.target === cycle || event.target === field) update(true);
        else if (event.target === form) {
            update(false);
            form.closest('details').open = true;
        }
    });
})();
