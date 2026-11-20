(() => {
    for (const form of document.querySelectorAll('form[data-tracking-dates]')) {
        const state = form.querySelector('[data-date-state]');
        const group = form.querySelector('[data-tracking-date-group]');
        const dateFields = Array.from(form.querySelectorAll('[data-date-field]'));
        const completionReference = form.querySelector('[data-completion-reference]');
        const warning = form.querySelector('[data-date-confirmation]');
        const confirmation = warning?.querySelector('[data-confirm-dates]');

        const updateFields = () => {
            if (!state) return;
            const selected = state.selectedOptions[0];
            group.hidden = selected?.dataset.start !== 'true';
            for (const field of dateFields) {
                const applicable = selected?.dataset[field.dataset.dateField] === 'true';
                field.hidden = !applicable;
                field.querySelector('input').disabled = !applicable;
            }
            completionReference.hidden = selected?.dataset.completion !== 'true';
        };

        const update = event => {
            if (event.target !== form && !event.target.matches('[data-date-edit]')) return;
            updateFields();
            if (confirmation) {
                confirmation.checked = false;
                confirmation.disabled = true;
                warning.hidden = true;
            }
        };

        updateFields();
        form.addEventListener('input', update);
        form.addEventListener('change', update);
    }
})();
