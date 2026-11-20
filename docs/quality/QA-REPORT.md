# QA da entrega local 0.8 — 09/11/2026

**Roteiro para o usuário:** [TEST_CASES](../TEST_CASES.html) é o único documento de execução manual, organizado por percurso. Foi preparado para produto 0.9; sua criação não é evidência de execução dos casos pelo usuário. Este relatório preserva a evidência técnica já executada e seus limites.

**Adendo 0.9 — operação por janela:** [iniciador `.bat` e ensaios reais](LAUNCHER-0.9.md). Servidor encerrado antes da alteração; host Windows PowerShell 5.1 e pacote `.bat` testados com encerramento abrupto, servidor eliminado, HTTP indisponível e reabertura dos mesmos dados. Produto/recursos visuais não mudaram; a suíte 47/47 abaixo pertence ao fechamento 0.8. Não se alegou novo ensaio de navegador ou clique no X nesta alteração operacional.

Execução do plano aprovado (documento histórico removido na limpeza B14), autorizada em [G3-EXECUTION-008](../records/G3-EXECUTION-008.md). Nove etapas implementadas; candidato local G4 pronto para revisão do usuário. Não registrar aprovação G4 sem resposta real.

## Verificações executadas

| Verificação | Resultado real |
|---|---|
| xUnit standalone, SQLite real e HTTP TestServer | **47/47**, zero erros/falhas/pulos; [log](../evidence/10-delivery-suite.log) |
| Modelo JavaScript dos rascunhos | Passou: campos alterados, conflito de versão e exclusão de tokens; `node tests/draft-model.test.cjs` |
| Publish Windows x64 autocontido | Concluído; .NET 10.0.14, SDK isolado 10.0.403; `10-final-publish.log` |
| Recursos do pacote | 19 arquivos idênticos às fontes, idiomas presentes, nenhum banco; [inventário](../evidence/RELEASE-ASSETS.json) |
| CLI de backup → validate → restore em outra raiz | Todos concluídos; [evidência](../evidence/10-package-maintenance.json) |
| Kestrel do pacote sobre cópia restaurada | HTTP 200, coleção presente, bind somente 127.0.0.1, Host e Origin externos HTTP 403; [evidência](../evidence/10-kestrel-smoke.json) |
| Iniciador PowerShell `-NoBrowser` | Iniciou a cópia em 5274; encerramento por Ctrl+C observado; processo de smoke separado também encerrado |
| Revisão independente e reparos | Uma revisão; todos os Important corrigidos com RED→GREEN; [relatório](FINAL_REVIEW-0.8.md) |
| Navegador interno real | Galeria, acompanhamento, histórico, lote e CSV; quatro combinações tema/idioma; rascunho/conflito entre abas; sem erros de console nos estados consultados |
| Comparação visual | [design-qa](design-qa.md), `final result: passed`; desktop 1487×1058 e reflow 320/390 |

Contrastes dos tokens sRGB: texto/fundo **14,60:1 claro / 14,54:1 escuro**; texto auxiliar **6,23:1 / 8,81:1**; borda/superfície **3,51:1 / 4,05:1**; rótulo de botão **5,51:1 / 7,90:1**. [Cálculo](../evidence/CONTRAST.json). Isso verifica as combinações medidas, não constitui certificação global de acessibilidade.

Comando da suíte: `. ./scripts/Use-Dotnet.ps1` e `dotnet run --project tests/IndicaAI.Tests --no-restore --disable-build-servers`. O coordenador `dotnet test` foi bloqueado por named pipe no ambiente; não é resultado de teste do produto. Há avisos esperados do TestServer sobre limites HTTP e aviso de chave local sem criptografia; não afirmar ausência geral de avisos.

## Rastreabilidade de aceitação

| Cenários | Evidência efetiva |
|---|---|
| A01–A03, A17 | Bootstrap/BootstrapHttp/CatalogIntegrity, reabertura com identidade estável, anime inválido recusado, biblioteca/catálogo sem login; cinco obras/mídias observadas no navegador |
| A04–A07, A18–A20, A22 | Tracking/TrackingFailure/StateLabel/CustomLabel/TrackingHttp/FinalReview: obrigatoriedade/compatibilidade/versão, reenvio, ciclos, auditoria de datas e rollback estado-evento-recibo; inclusão com/sem nota e reconsumo reais no navegador |
| A08, A16 | OrganizationTests e JourneyTests: listas mistas independentes do estado, arquivo e restauração preservando dados |
| A09 | NameKey/Tag/CatalogIntegrity: normalização Unicode completa, unicidade e aplicabilidade; administração de grupo/tags real no QA |
| A10–A13, A23–A24 | Import/ImportFailure/ImportRecovery/CsvStructure: preview sem alterar taxonomia, MERGE preserva ausências, coleções em união, duplicatas/conflitos, preview obsoleto, rollback por registro, interrupção após dois commits, recuperação/retensão e limites |
| A14–A15, A21 | FilterBatch/RecoveryAndBatch/BatchReview/FinalReview: AND/OR/NOT e contexto, versões/compatibilidade e falha no último item desfazendo o lote inteiro; confirmação desabilitada para série ainda não revisada no navegador |
| A25 | Backup/BackupValidation/Migration/RecoveryAndBatch/RestoreDamagedActive: snapshot completo de tabelas/assets, hashes/FKs/schema, sete diários, falha de migração, troca interrompida, ativo corrompido/capa ausente; CLI real em cópia |
| A26 | Foco visível de 3 px, navegação real por teclado, nomes na árvore acessível e reflow 320/390; sem excesso horizontal nas telas estreitas consultadas após correção. Não foi ensaio integral com leitor de tela |
| A27–A31 | Preference/Journey/CustomLabel/FinalReview e Node: recursos/chaves/parâmetros, quatro combinações, persistência, conteúdo/IDs/semântica preservados e fixture adicional de idioma com fallback; navegador preservou seleção e rascunhos e exigiu revisão explícita após edição concorrente |

No CSV real, a primeira prévia válida teve uma inserção, uma atualização, uma sem alteração e uma rejeição. Confirmação mostrou **3 processadas, 1 rejeitada, 0 não processadas**, [captura](../evidence/ui/F-csv-result.jpg). Uma segunda prévia equivalente foi capturada após ajuste de espaçamento e cancelada. Fixtures intermediárias com IDs de automação inválidos foram rejeitadas e não aplicadas; não são defeitos do MERGE.

## Limites e operação

O catálogo é reduzido e demonstrativo, com cinco obras fictícias. Não há benchmark de grande escala, certificação WCAG, teste de vários navegadores/Windows limpo ou experiência multiusuário. Campos nativos de data seguem a apresentação do navegador/SO; datas de calendário e textos de leitura são localizados e não mudam de dia. O pacote usa recursos locais, sem CDN/integração externa em execução; não foi alterada a conectividade do sistema para simular ausência física de rede.

As raízes `.local/qa` e `.local/restore-smoke-20261109` são ensaios separados; `%LOCALAPPDATA%/IndicaAI` não foi usado nos testes. A aplicação local não fornece criptografia própria do banco/chaves. O limite de proteção e os procedimentos de recuperação estão em [OPERATIONS](../OPERATIONS.md). Entrega local não implica deploy, publicação, login, exportação pessoal ou funcionalidades comunitárias.

Para retomar sem reconstruir contexto: [CONTINUE_HERE](../CONTINUE_HERE.md). Para desvios e seus custos: [EXECUTION_LOG](EXECUTION_LOG.md).
