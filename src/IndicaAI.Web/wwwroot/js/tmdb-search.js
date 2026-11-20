(function () {
    function searchEntry(query, year) {
        if (typeof query !== 'string' || typeof year !== 'string') return null;
        query = query.trim();
        if (!query || query.length > 160 || (year !== '' && !/^[0-9]{1,4}$/.test(year))) return null;
        if (year !== '' && Number(year) === 0) return null;
        return { query, year: year === '' ? '' : String(Number(year)) };
    }

    function createHistory(storage, scope = '') {
        const key = 'indicaai-tmdb-recent:v1:' + scope;
        let entries;

        function read() {
            if (entries) return entries;
            try {
                const saved = JSON.parse(storage?.getItem(key) ?? '[]');
                entries = Array.isArray(saved)
                    ? saved.map(item => searchEntry(item?.query, item?.year)).filter(Boolean).slice(0, 5)
                    : [];
            } catch {
                entries = [];
            }
            return entries;
        }

        function record(query, year) {
            const entry = searchEntry(query, year);
            const recent = read();
            if (!entry) return recent;
            const comparison = entry.query.normalize('NFC').toLowerCase();
            entries = [entry, ...recent.filter(item =>
                item.year !== entry.year || item.query.normalize('NFC').toLowerCase() !== comparison
            )].slice(0, 5);
            try {
                storage?.setItem(key, JSON.stringify(entries));
            } catch {
                // Recent entries still work on this page.
            }
            return entries;
        }

        return { read, record };
    }

    if (typeof module === 'object' && module.exports) {
        module.exports = { createHistory };
        return;
    }

    for (const poster of document.querySelectorAll('[data-tmdb-result-poster]')) {
        const fallback = poster.nextElementSibling;
        function showFallback() {
            poster.hidden = true;
            fallback.hidden = false;
        }
        poster.addEventListener('error', showFallback);
        if (poster.complete && poster.naturalWidth === 0) showFallback();
    }

    const form = document.querySelector('[data-tmdb-search]');
    const section = document.querySelector('[data-tmdb-recent]');
    if (!form || !section) return;

    let storage;
    try {
        storage = window.localStorage;
    } catch {
        // Some browser settings deny even access to the storage object.
    }
    const history = createHistory(storage, form.dataset.tmdbSearch);
    const currentUrl = new URL(window.location.href);
    if (currentUrl.searchParams.has('Remember')) {
        if (form.querySelector('[data-tmdb-search-completed]')) {
            history.record(form.elements.namedItem('Query').value, form.elements.namedItem('Year').value);
        }
        currentUrl.searchParams.delete('Remember');
        window.history.replaceState(window.history.state, '', currentUrl.href);
    }

    const list = section.querySelector('ul');
    function render(recent) {
        const links = recent.map(({ query, year }) => {
            const url = new URL(form.action);
            url.searchParams.set('Query', query);
            if (year) url.searchParams.set('Year', year);
            url.searchParams.set('Remember', 'true');
            const returnUrl = form.elements.namedItem('ReturnUrl')?.value;
            if (returnUrl) url.searchParams.set('ReturnUrl', returnUrl);
            const link = document.createElement('a');
            link.href = url.href;
            link.textContent = year ? query + ' (' + year + ')' : query;
            const item = document.createElement('li');
            item.append(link);
            return item;
        });
        list.replaceChildren(...links);
        section.hidden = links.length === 0;
    }

    render(history.read());
    window.addEventListener('pageshow', event => {
        if (event.persisted && storage) render(createHistory(storage, form.dataset.tmdbSearch).read());
    });
})();
