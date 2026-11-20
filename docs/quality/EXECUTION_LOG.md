# SDD ledger — plan: docs/IMPLEMENTATION_PLAN-0.7.md

**Alteração autorizada 0.9:** usuário pediu encerrar o processo e vincular serviço à janela de um `.bat`; servidor anterior encerrado primeiro. [LAUNCHER-0.9](LAUNCHER-0.9.md) registra plano curto, implementação e testes isolados. Não reabrir aprovação G3 nem iniciar preview automaticamente; entrega desta alteração com servidor desligado. O fechamento anterior 0.8 abaixo é histórico.

**Fechamento 09/11/2026 — candidato local G4 pronto:** nove etapas concluídas. Última suíte **47/47**, zero falhas/erros/pulos, em [evidence/10-delivery-suite.log](../evidence/10-delivery-suite.log); teste Node passou. Pacote sincronizado (19 recursos), CLI real de backup/validação/restauração em cópia e smoke Kestrel concluídos. QA visual [aprovado tecnicamente](design-qa.md), uma [revisão final independente](FINAL_REVIEW-0.8.md) concluída e reparos verificados. **Aprovação G4 pelo usuário ainda não registrada**; nenhuma publicação externa. Registros intermediários abaixo são históricos.

Autorização: docs/records/G3-EXECUTION-008.md. Execução inline pela skill executing-plans, com TDD e revisão final independente. O registro será preservado como evidência do projeto.

**Retomada 09/11/2026:** o pedido de pausa foi interrompido antes de salvar o checkpoint; o usuário disse “Continue” no dia seguinte. Estado detalhado em [CONTINUE_HERE](../CONTINUE_HERE.md). Etapas funcionais 1–8 implementadas; último resultado 41/41 em `09-release-suite.log`. A lista abaixo conserva registros intermediários; QA visual final, pacote atualizado e revisão independente ainda estão pendentes da etapa 9.

## Pre-flight

- 1→2→3→4: identidade, versões/recibos, estado/ciclo/evento e consultas compartilhados; APIs de DTO do plano serão centralizadas.
- 2→5→6→7: normalização deve existir na personalização antes de tags/importação; recursos base antecipados para não duplicar rótulos. Correção já prevista no plano aprovado.
- 1–7→8→9: testes usam raiz isolada; schema antigo recusado antes da proteção de migração/backup; QA final não usa dados pessoais.
- Ruling: trabalhar no diretório autorizado atual sem criar Git/worktree — pacote não contém .git e o plano prevê execução nele sem presumir inicialização de repositório; custo: não haverá commits por etapa, evidência fica neste ledger e logs.
- Ruling: manter ledger em docs em vez dos scripts de workspace baseados em Git — não existe histórico Git para recuperá-lo e as instruções exigem evidência preservada; custo: acompanhamento manual dos estados das tarefas.
- Baseline documental antes de alterações: 62 arquivos, 44 Markdown e 292 links locais conferidos. SDK 6.0.428 era o único instalado; SDK 10 local está sendo obtido com metadados oficiais/hash, sem alterar instalação global.

## Tarefas

- Task 1: Ruling: executar o assembly de teste por dotnet run — o coordenador dotnet test usa named pipe negado pelo sandbox; execução standalone do mesmo xUnit funcionou (2 testes), custo: comando de verificação difere do plano, suite e asserções idênticas.
- Task 1: SDK 10.0.403 baixado da URL indicada no metadado oficial, SHA-512 validado e extraído em .tools; restore NuGet e ferramenta EF 10.0.14 locais reais. Migração InitialLocalSchema gerada, bootstrap RED por APIs ausentes e depois GREEN 2/2 em SQLite; teste HTTP adicional RED por /Library e /Catalog inexistentes, implementação em andamento.

- Task 1: Ruling: usar Microsoft.Testing.Platform em vez de VSTest — xunit.v3 4.0.2 instalado traz MTP 2.5.1, cujo target rejeita VSTest no SDK 10 (log 01-red); custo: os comandos de filtro/runner do plano mudam, sem mudar testes ou produto. A falha de ferramenta não foi contada como RED do comportamento.

1. Complete — SDK/bootstrap, 3 testes reais, incluídos nas suítes posteriores.
2. Complete — acompanhamento/histórico/renomeação e HTTP com antiforgery; 15/15 no log 02-complete.
3. Complete — listas mistas e arquivo/restauração; suíte 17/17 no log 04-green após corrigir busca de chave composta de ListItem.
4. Implementado — filtros AND/OR/NOT/contexto e lote integral; suíte 19/19 em 05-layout, falta ensaio visual e teste adicional de falha no último item.
5. Implementado — preferências/recursos/temas/rascunhos; suíte 19/19 em 05-layout, QA de navegador pendente da etapa 9.
6. Complete — normalização, administração e associações; suíte 23/23 no log 07-api.
7. Implementado — CSV e recuperação; 24/24 em 07-fixed, complementos de retenção/interrupção real e QA pendentes.
8. Em execução — operação/recuperação. RED em 08-red por APIs ausentes.
9. Pendente — QA/pacote local/revisão final.

- Task 3: Ruling: consolidar criação/renomeação em SaveAsync e vínculo individual versionado, em vez das assinaturas ilustrativas do plano — a tela de acompanhamento salva vínculos em uma transação e o lote terá serviço próprio; custo: nomes de API diferem do mapa, invariantes mantidas.
- Task 4: Ruling: filtrar o catálogo local limitado em memória após recorte SQL de proprietário/contexto/mídia — garante comparação Unicode coerente sem depender de NOCASE ASCII; custo: consulta deve ser reavaliada se o catálogo crescer significativamente.
- Task 7: CsvHelper 34 tem API CsvReaderOptions/FromText e rastreamento LineNumber, confirmada no XML do pacote oficial instalado; implementação usa essa API, sem rebaixar versão. Teste de falha foi corrigido para atingir TagMedia (nome real da tabela), provando rollback da tag inserida antes do vínculo.

## Fechamento das tarefas 4–9

4. Complete — filtros/contexto/lote, incluindo falha no último item e retirada de grupo preservando outras escolhas; suíte final 47/47.
5. Complete — claro/escuro, PT-BR/EN, extensibilidade de recursos e rascunhos; suíte final, Node e navegador com duas abas.
7. Complete — MERGE, preview obsoleto, registros vazios, interrupção real após dois commits, recuperação e retenção; CSV confirmado no navegador com 3 processadas/1 rejeitada.
8. Complete — lock, validação, snapshots completos, sete diários, pré-migração, falha de migração, restauração de ativo danificado e recuperação de troca; CLI do executável também executada em cópia.
9. Complete — revisão independente única, QA visual, pacote Windows autocontido sincronizado, operação/licenças e evidências; candidato entregue para revisão G4. Sem Git existente, não houve commits, merge, PR ou push; workspace e logs preservados.

## Revisão final e decisões

- Final: fixed reenvio do formulário original — `ReplayingTheOriginalInclusionFormReturnsSuccessOnce` RED→GREEN, suíte 46/46 e final 47/47.
- Final: fixed rascunhos administrativos e recuperação após versão divergente — `EditableAdministrativeAndHistoryFormsDeclareDraftSupport`, teste Node e navegador com duas abas; regressão HTTP RED→GREEN, suíte 46/46 e final 47/47.
- Final: fixed mídia perdida em filtros — `FilterFormRetainsMediaAndOffersUnambiguousApplicableTags` RED→GREEN, suíte 46/46 e final 47/47.
- Final: fixed restauração sobre ativo danificado — `HealthyBundleRepairsDamagedActiveAndPreservesIt`, banco corrompido/capa ausente RED→GREEN, suíte 46/46 e final 47/47.
- Final: regraduado Minor de tags para Important: nomes homônimos sem grupo tornam a escolha funcional ambígua; não é só acabamento. Fixed aplicabilidade/grupo — mesmo teste de filtros RED→GREEN, suíte 46/46 e final 47/47. Nenhum Minor do revisor adiado.
- Final: fixed parâmetro literal no botão CSV descoberto em QA — `ImportActionFormatsItsRecordCount` RED→GREEN (47/47). Correções visuais posteriores de paleta/reflow/espaçamento recapturadas em `design-qa.md`; suíte final 47/47 permaneceu verde.
- Final: Ruling: fidelidade/contraste/acessibilidade deixados ao principal pelo revisor — comparação combinada, foco, contraste e reflow reais aceitos; leitor de tela não certificado — custo se errado: barreiras assistivas podem exigir revisão especializada adicional.
- Final: Ruling: sincronia do artefato deixada ao principal — publish atualizado e hash dos 19 recursos iguais, sem banco; aceito — custo se errado: binário/recurso divergente poderia apresentar comportamento antigo, exigindo novo pacote.
- Final: Ruling: desempenho em catálogo grande não medido pelo revisor — mantém recorte SQL e filtro Unicode em memória para catálogo reduzido aprovado — custo: crescimento pode exigir consulta/indexação/benchmark adicionais.
- Final: Ruling: proveniência de SDK/dependências deixada ao principal — metadado/hash oficial do SDK, locks e inventário/licenças reais aceitos — custo: futura atualização exige repetir verificação e revisar licenças/vulnerabilidades.
- Final: Ruling: dados pessoais excluídos da revisão — raízes de QA/cópia explícitas e pacote sem banco; coleção padrão não acessada — custo: não se comprovou migração de uma coleção pessoal arbitrária; usar backup/validação antes de atualizações reais.

Finishing-a-development-branch aplicada ao ambiente sem Git: menus de merge/PR e limpeza de worktree não se aplicam; entrega local e preservação dos arquivos seguem a autorização existente. Não há integração externa pendente dentro do escopo autorizado.
