# IndicaAI

Aplicativo local para acompanhar jogos, livros, mangás, filmes e séries, com listas, estados, histórico, anotações privadas e filtros. A versão atual está em [VERSION](VERSION).

## Usar o aplicativo

No Windows, abra **Iniciar-IndicaAI.bat** com duplo clique. A biblioteca deve abrir no navegador. Mantenha a janela do iniciador aberta enquanto usa o aplicativo; ao fechá-la, o servidor é encerrado. Fechar somente o navegador não encerra o servidor.

Para testar com dados separados da coleção pessoal, abra **Iniciar-Testes-IndicaAI.bat** e siga o [roteiro de testes manuais](docs/TEST_CASES.html). Ao reabrir esse mesmo arquivo, a rodada anterior é preservada.

## Documentação

- [Índice da documentação](docs/README.md): organização dos documentos de uso, decisões e histórico.
- [Roteiro de testes manuais](docs/TEST_CASES.html): casos organizados por fluxo, para abrir no navegador.
- [Operação local](docs/OPERATIONS.md): dados, backup, recuperação e desenvolvimento.
- [Validação automatizada](docs/VALIDATION.md): perfis de teste, comandos e limites.
- [Backlog](docs/BACKLOG.md): lista única de trabalhos futuros e dependências; não implica autorização de execução.
- [Patch notes](PATCH_NOTES.md): histórico contínuo das entregas.
- [Requisitos](docs/PRD.md), [decisões](docs/DECISIONS.md) e [pendências](docs/OPEN_QUESTIONS.md): regras e histórico de aprovação.
- [Continuidade](docs/CONTINUE_HERE.md) e [AGENTS.md](AGENTS.md): estado para retomada e instruções de manutenção.

## Desenvolvimento e validação

O código está em `src/`, os testes em `tests/`, os scripts operacionais em `scripts/` e os exemplos de CSV em `examples/`. A solução usa .NET, Razor Pages e SQLite. O aplicativo publicado é autocontido para Windows x64; o desenvolvimento e a validação precisam das ferramentas descritas na documentação de operação.

Abra **Validar-IndicaAI.bat** para uma validação completa, ou consulte os perfis por impacto em [VALIDATION](docs/VALIDATION.md). A validação completa usa dados isolados e pode iniciar temporariamente um servidor de teste.

As pastas `.local/`, `.tools/` e `artifacts/` contêm, respectivamente, dados locais, ferramentas/logs e pacotes gerados. Elas não devem ser tratadas como código-fonte nem removidas sem verificar seu conteúdo e suas dependências. Documentos de levantamento, decisões e evidências anteriores estão em `docs/`.
