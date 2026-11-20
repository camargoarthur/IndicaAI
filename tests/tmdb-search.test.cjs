const assert = require('node:assert/strict');
const test = require('node:test');
const { createHistory } = require('../src/IndicaAI.Web/wwwroot/js/tmdb-search.js');

function memoryStorage() {
    const values = new Map();
    return {
        getItem: key => values.get(key) ?? null,
        setItem: (key, value) => values.set(key, value)
    };
}

test('recent searches are newest first, limited to five and reopened from storage', () => {
    const storage = memoryStorage();
    const history = createHistory(storage);
    assert.deepEqual(history.read(), []);
    for (const query of ['One', 'Two', 'Three', 'Four', 'Five', 'Six']) {
        history.record(query, '');
    }
    assert.deepEqual(createHistory(storage).read().map(entry => entry.query), [
        'Six', 'Five', 'Four', 'Three', 'Two'
    ]);
});

test('repeated queries move to the top and different years stay separate', () => {
    const history = createHistory(memoryStorage());
    history.record('Nosferatu', '1922');
    history.record('Nosferatu', '2024');
    history.record(' NOSFERATU ', '1922');
    assert.deepEqual(history.read(), [
        { query: 'NOSFERATU', year: '1922' },
        { query: 'Nosferatu', year: '2024' }
    ]);
});

test('data roots sharing a browser origin keep separate search histories', () => {
    const storage = memoryStorage();
    createHistory(storage, 'personal').record('Nosferatu', '');
    createHistory(storage, 'isolated').record('Test title', '');
    assert.deepEqual(createHistory(storage, 'personal').read(), [{ query: 'Nosferatu', year: '' }]);
    assert.deepEqual(createHistory(storage, 'isolated').read(), [{ query: 'Test title', year: '' }]);
});

test('bad input and unavailable or corrupted storage leave search usable', () => {
    const storage = {
        getItem() { throw new Error('storage denied'); },
        setItem() { throw new Error('storage denied'); }
    };
    const history = createHistory(storage);
    assert.deepEqual(history.read(), []);
    assert.doesNotThrow(() => history.record('Nosferatu', '2024'));
    const corrupted = { ...memoryStorage(), getItem: () => '{unreadable' };
    assert.deepEqual(createHistory(corrupted).read(), []);
    assert.deepEqual(createHistory(memoryStorage()).record('   ', '2024'), []);
    assert.deepEqual(createHistory(memoryStorage()).record('Nosferatu', '-1'), []);
});
