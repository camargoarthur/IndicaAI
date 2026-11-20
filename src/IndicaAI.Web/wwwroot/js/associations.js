(() => {
    const panel = document.querySelector('[data-association]');
    if (!panel) return;

    const work = panel.querySelector('#association-work');
    const tag = panel.querySelector('#association-tag');
    const placeholder = tag.options[0];
    const form = panel.querySelector('[data-association-form]');
    const status = panel.querySelector('[data-association-status]');
    const create = panel.querySelector('[data-association-hint] a');
    const save = panel.querySelector('[data-association-save]');
    const templates = new Map([...panel.querySelectorAll('[data-association-media]')]
        .map(template => [template.dataset.associationMedia, template]));
    const returnFields = document.querySelectorAll('[data-association-return-work]');

    const updateSave = () => {
        save.disabled = !work.value || !tag.value;
    };

    work.addEventListener('change', () => {
        const template = templates.get(work.selectedOptions[0]?.dataset.media);
        tag.replaceChildren(placeholder);
        if (template) tag.append(template.content.cloneNode(true));
        tag.value = '';
        tag.disabled = !template;
        form.elements.namedItem('workId').value = work.value;
        returnFields.forEach(field => { field.value = work.value; });
        updateSave();

        const noTags = Boolean(work.value) && tag.disabled;
        status.textContent = noTags ? status.dataset.noTags : '';
        create.hidden = !noTags;

        const url = new URL(window.location.href);
        if (work.value) url.searchParams.set('workId', work.value);
        else url.searchParams.delete('workId');
        history.replaceState(history.state, '', url);
    });

    form.addEventListener('change', updateSave);
    panel.querySelector('[data-association-fallback]').hidden = true;
    updateSave();
})();
