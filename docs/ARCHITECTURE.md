# Arquitetura do IndicaAI

Arquitetura A1 escolhida e implementada: aplicação local em .NET 10, Razor Pages, EF Core e SQLite. O aplicativo atende somente loopback e usa identidade interna, sem login. Os cinco tipos de mídia compartilham catálogo e acompanhamento; Anime é atributo de filme/série.

## Organização do código

- src/IndicaAI.Web/Pages: telas e handlers HTTP.
- src/IndicaAI.Web/Features: regras e operações por funcionalidade.
- src/IndicaAI.Web/Domain e Persistence: entidades, restrições e migrações.
- src/IndicaAI.Web/Hosting e Operations: dados locais, ciclo de execução, backup e restauração.
- src/IndicaAI.Web/Localization, Text e wwwroot: localização e recursos visuais locais.
- tests: regras, SQLite, HTTP, JavaScript e ensaio do iniciador; critérios em [VALIDATION](VALIDATION.md).

Um acompanhamento por identidade/obra mantém estado atual, ciclos e histórico. Comandos validam versões e repetição de operação; lote pessoal é atômico. CSV administrativo faz preview e aplica registros válidos com falhas isoladas. Backup abrange banco e recursos e é validado antes da restauração.

## Referências técnicas

- [Modelo conceitual](architecture/DATA_MODEL.md) e [modelo físico](architecture/PHYSICAL_MODEL.md).
- [Contrato CSV](architecture/CSV_CONTRACT.md).
- [Preferências e idiomas](architecture/PREFERENCES_I18N.md).
- [Operação](OPERATIONS.md) e [licenças](THIRD_PARTY.md).
- [Decisões](DECISIONS.md) e registros em records/ mantêm a justificativa das escolhas aprovadas.

As referências técnicas incluem a especificação aprovada antes da implementação. Os testes e o código documentam o comportamento executável atual; divergências devem ser investigadas, sem reabrir escolhas já aprovadas.
