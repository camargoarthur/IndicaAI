# Continuidade — 20/11/2026

## Estado atual

### 0.13 instalada — filtro por estado e melhorias de usabilidade

O usuário autorizou aplicar todas as 11 correções da [auditoria de usabilidade](quality/AUDITORIA-USABILIDADE.md) e aprovou a prévia das notas oficiais. A decisão D39 registra o escopo. `VERSION`, `PATCH_NOTES.md` e o roteiro HTML estão na 0.13, com marcações separadas no navegador.

As mudanças abrangem a consulta da Biblioteca, a navegação entre o Catálogo, a obra e o lote, os formulários de Histórico e Preferências, a administração de tags e CSV, as telas de Arquivadas e Listas e o comando de retomada de capas. O [roteiro](TEST_CASES.html) acrescenta TM43 e atualiza os casos afetados. Testes novos ou ampliados em `BrowseOriginTests`, `AuditFormsTests`, `OrganizationAdminHttpTests`, `WatchListImportTests` e `draft-model.test.cjs` complementam `QuickStateTests` e TM42.

A primeira rodada do filtro passou em Functional 5/5, 83 testes e 23 contratos em `.tools/validation/2026-11-20T15-09-30-761Z-12892`. Os ensaios direcionados das correções passaram em Histórico/Preferências (17), Administração/Arquivadas/Listas (21) e retomada de capas e TMDB simulado (16). Logs em `.tools/logs/audit-data-forms-green3-tests.log`, `.tools/logs/audit-organization/green.log` e `.tools/logs/audit-poster-recovery/green-tests-eventlog-disabled.log`.

**Validação final.** Full 8/8, com 99/99 testes de produto, 23/23 contratos e 27 recursos do pacote conferidos. O iniciador passou no ciclo de abertura, encerramento e reabertura com dados preservados. Resumo em `.tools/validation/2026-11-20T20-01-31-488Z-25620/summary.json`. O executável aprovado também passou no Edge pelos fluxos de seleção, origem, datas, rótulos, mídias e filtros, tags, CSV, restauração e listas. Capturas e base isolada em `.tools/logs/audit-browser-la3pEA` (`data-root`). O ensaio do iniciador usou `.local/launcher test-0d76b4efdc23400c8576987196d37a21`. Processos temporários encerrados. O TMDB foi simulado nos testes de lote e retomada de capas; não houve nova carga na coleção pessoal.

**Instalação.** Pacote validado em `artifacts/IndicaAI-win-x64`. O anterior foi preservado em `artifacts/IndicaAI-win-x64.previous-20261120-cb4e2c28`. Para abrir a coleção pessoal, usar `Iniciar-IndicaAI.bat`. Para executar os 43 casos manuais, abrir `TEST_CASES.html` e usar `Iniciar-Testes-IndicaAI.bat`. A próxima evolução segue o escopo que o usuário autorizar.

**Entrega anterior 0.12.** O usuário autorizou aplicar o plano em três frentes e pediu a retomada. A entrega incluiu o lote de filmes e séries do TMDB, a busca principal local, estados nos cartões, cinco buscas recentes no TMDB, pôster e sinopse na busca externa, datas condicionais e retirada segura dos exemplos iniciais. O roteiro e o mapa de cobertura receberam TM41. A busca individual no TMDB está disponível para filmes.

**Carga pessoal concluída.** A base em `%LOCALAPPDATA%\IndicaAI` contém as 78 obras de `docs/Assistir.txt`: 70 filmes e 8 séries, incluindo dois animes episódicos. As 78 estão em **Quero assistir**. O lote vinculou 69 ao TMDB e guardou 9 localmente para revisão. Após duas tentativas de preenchimento, 21 obras têm pôster do TMDB em cache. A conferência somente leitura do SQLite confirmou a integridade, 78 IDs distintos com acompanhamento, a preservação dos dados pessoais anteriores e a retirada dos cinco exemplos de fábrica. Backup prévio validado: `b-m-20261120-141514-f458258b`. Logs de carga, conferência e a lista privada para revisão estão em `.tools/logs/assistir-012`.

**Verificação da 0.12.** Full 8/8, com 81 testes de produto aprovados. Logs em `.tools/validation/2026-11-20T14-18-41-102Z-15652`. O ensaio em `.local/qa-offline-012-ef8c470d08be44daaef56bd9ea6fb61f` importou um filme e uma série, repetiu o lote sem duplicar e abriu o Catálogo sem token do TMDB. A inspeção no navegador confirmou os cartões, os estados e uma capa servida pelo cache local. O pacote que precedeu a 0.12 foi preservado em `artifacts/IndicaAI-win-x64.previous-20261120-d6e4903c`.

**Próximo passo.** A revisão dos nove vínculos está em B06, com as linhas identificadas em `.tools/logs/assistir-012/revisao-local.md`. A busca individual de séries e a carga ampla do catálogo ainda estão no [backlog](BACKLOG.md). Processos de manutenção e ensaio encerrados. Para abrir a coleção pessoal, usar `Iniciar-IndicaAI.bat`.

MVP local implementado, com extensão B06 para buscar e confirmar um filme por vez no TMDB na 0.11. B11 entregou a rotina de validação por impacto na 0.10. Backlog canônico somente em [BACKLOG](BACKLOG.md); README apenas direciona. Servidor pessoal controlado pelos BATs.

## Planejamento anterior — base local e usabilidade

Em 20/11/2026, as três frentes avaliaram a carga da lista pessoal, as capas, as obras demonstrativas, a busca local e externa, os estados nos cartões e o formulário de datas. O plano definiu o ensaio isolado de um filme e uma série antes da carga pessoal e registrou a carga ampla como B15 em [BACKLOG](BACKLOG.md). Na avaliação inicial, o ensaio em `.local/catalog-ux-plan-1094eb457e29494c9442dd510c264ffb` recebeu respostas HTTP 200 no Catálogo e na página do TMDB sem autenticação. A execução autorizada e os resultados finais estão no estado atual acima.

## B06 — primeira integração de catálogo

Autorização em D37. Fluxo **Catálogo → Buscar no TMDB → prévia → Adicionar ao catálogo**, com deduplicação por ID externo e redirecionamento ao acompanhamento local. TM40 cobre o exemplo Nosferatu (2024). Alterações em cliente TMDB, páginas, recursos PT-BR/EN, migração, iniciador, testes, roteiro e manifesto. Os arquivos `.env` e `docs/Assistir.txt` são locais. O próximo trabalho de B06, se autorizado, amplia mídias ou permite revisar a lista em lote.

Validação de produto: **67/67** testes e **23/23** contratos. O Full `2026-11-20T12-05-37-905Z-24724` aprovou build, produto, publicação isolada, pacote e iniciador antes de identificar um hash documental divergente já presente no commit anterior, referente ao lock file da aplicação. O manifesto foi alinhado aos bytes do lock file, que permaneceu intacto. O perfil Docs verifica o fechamento documental.

Ensaio com o TMDB real: busca e prévia de Nosferatu (2024), duração de 132 minutos retornada pela API, confirmação, gravação do acompanhamento como **Quero assistir**, nota preservada e segunda busca apontando para a mesma obra. A raiz usada foi `.local/tmdb-smoke-3e6d1409953e4bd695b971b2b2b7ed14`; logs no diretório de mesmo nome em `.tools/logs`. O servidor de ensaio foi encerrado. O pacote verificado está em `artifacts/IndicaAI-win-x64`; o anterior foi preservado em `artifacts/IndicaAI-win-x64.previous-20261120-d02ab09d`.

## Preparação local para Git

Usuário autorizou preparar a pasta e reservou para si a conexão e a publicação. `.gitignore` protege `.env`, credenciais, dados locais e `docs/Assistir.txt`. `.gitattributes` preserva os bytes exigidos pelo manifesto.

## B14 — organização e limpeza

Usuário autorizou auditar toda a estrutura, excluir material desnecessário sem backup e reorganizar. Limpeza aplicada: propostas de levantamento superadas, templates, manifestos históricos, verificador Python redundante, prompt inicial e capturas intermediárias. Dez scripts temporários de geração do guia já haviam sido excluídos. Requisitos, decisões aprovadas, registros de autoridade, licenças, contratos técnicos e evidências finais foram preservados.

Referências técnicas estão em docs/architecture/; relatórios em docs/quality/; evidências em docs/evidence/; registros em docs/records/. Verificador documental transferido para scripts/Verify-Documents.ps1, com chamada e raiz corrigidas. README, índice, PRD, arquitetura, gates, pendências e regras foram atualizados. O conteúdo de acompanhamento manual mantém IDs e chave de armazenamento. Nenhuma mudança funcional; versão continua 0.10.

Auditoria detalhada em [STRUCTURE](STRUCTURE.md). B14 retirado das pendências por entrega local; não houve publicação no GitHub. Dados em .local/ e coleção pessoal preservados. Nenhum backup dos arquivos removidos foi criado.

Validação de fechamento: Full **8/8**, produto **61/61**, contratos **23/23**, pacote e iniciador aprovados. Resumo em `docs/evidence/B14-validation-full.json`; logs em `.tools/validation/2026-11-15T12-31-08-898Z-18184`. Ajustes de documentos posteriores são conferidos pelo perfil Docs. Removido também o ZIP do SDK já instalado: total de 59 arquivos antigos removidos, sem backup. Processos de ensaio encerrados; dados pessoais não usados. B14 concluído. Próximo passo: aguardar solicitação do usuário; não reiniciar a limpeza nem o servidor pessoal. Aceitação G4/publicação externa continuam sem confirmação.
