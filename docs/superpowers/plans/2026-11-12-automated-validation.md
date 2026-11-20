# Testes e rotina de validação — plano de implementação

> **For agentic workers:** REQUIRED SUB-SKILL: superpowers:executing-plans. Execução inline autorizada no pedido atual; não pedir nova aprovação para implementar/verificar este escopo.

**Goal:** completar lacunas do roteiro manual e tornar a validação por impacto uma obrigação verificável de cada entrega.

**Architecture:** aproveitar xUnit/SQLite/TestServer e Node existentes, sem novas dependências. Um executor local seleciona perfis, grava logs completos e resumo JSON, falha com código não zero e não confunde testes HTTP com execução de JavaScript no navegador.

**Tech Stack:** .NET 10, xUnit v3, Node, PowerShell e iniciador Windows existente.

**Spec:** autorização direta de 12/11/2026 para criar/executar testes, aplicar a rotina nas alterações, manter testes atualizados e retirar B11 do backlog; roteiro [TEST_CASES.html](../../TEST_CASES.html) e regras [AGENTS](../../../AGENTS.md).

## Global Constraints

- Preservar dados pessoais; testes com raízes temporárias/isoladas, encerrar somente processos criados pelo ensaio.
- Não deixar servidor rodando; a autorização atual permite servidores temporários de teste necessários à verificação do iniciador.
- Manter TM01–TM39, sem marcar testes manuais aprovados automaticamente.
- Nova rotina executável recebe versão 0.10, sem alterar requisitos do produto ou lançar 1.0.
- Sem Git no projeto: registrar arquivos/evidências, sem inventar commits/worktrees.

## Review Focus

- Mudança desconhecida ou lista vazia não pode selecionar só documentação.
- Falha, timeout ou seleção de zero testes não pode produzir sucesso.
- Listas/arquivo/filtros e concorrência precisam verificar dados, não só HTTP 200.
- Cobertura deve apontar métodos reais e distinguir execução manual de integração.
- Iniciador de teste deve encerrar processos mesmo se houver falha, preservando outras instâncias.

### Task 1: Cobertura complementar do roteiro

**Files:** criar `tests/IndicaAI.Tests/Integration/ManualGuideTests.cs`, `tests/IndicaAI.Tests/Web/ManualFormTests.cs`; ampliar JS de rascunhos se houver lacuna.
**Interfaces:** WebFixture/TestDatabase existentes; testes independentes com GUID por raiz. Consumidos pela suíte completa e filtros por classe.
- [x] Medir baseline existente e identificar lacunas pelo roteiro.
- [x] Testar renomear lista/vínculos, remoção em lote, filtros por tag/mídia/anime/busca e isolamento do arquivo, cancelamento CSV, correção de ciclo antigo, estado ausente e conflito de formulários.
- [x] Executar novos testes e suíte, corrigir somente problemas comprovados. Testes retrospectivos podem passar de início; não fingir RED de comportamento já implementado.

### Task 2: Executor por impacto

**Files:** `scripts/validation-plan.cjs`, `scripts/validate.cjs`, `scripts/Validate.ps1`, `Validar-IndicaAI.bat`, `tests/validation-plan.test.cjs`, `tests/guide-contract.test.cjs`, `tests/manual-coverage.json`.
**Interfaces:** `selectPlan({profile,changedPaths,modules})` retorna perfil/módulos; perfis Auto, Docs, Visual, Functional, Module, Full. CLI gera pasta única `.tools/validation/<id>/summary.json`, logs e resultados xUnit; saída 0 somente se todas as etapas selecionadas passam.
- [x] Escrever/executar teste RED de seleção: documentação, CSS, funcional, infraestrutura/desconhecido, lista vazia, caminhos Windows e entradas inválidas.
- [x] Implementar seleção conservadora e execução; módulos incluem dependências compartilhadas e smoke básico. Suíte funcional inteira é o padrão quando rápida; Full acrescenta pacote/iniciador.
- [x] Validar contrato do guia: IDs, versão e chave local, referências de cobertura existentes, fontes/licenças; preservar uso offline.
- [x] Executar perfis necessários, saída de falha e resumo, sem imprimir logs completos no chat.

### Task 3: Manutenção e fechamento

**Files:** VERSION, AGENTS, README, BACKLOG, PATCH_NOTES, CONTINUE_HERE, DECISIONS, TEST_CASES.html, `docs/VALIDATION.md`, manifesto/evidências.
- [x] Documentar comandos e critérios; tornar atualização/testes por impacto obrigatórios antes da entrega.
- [x] Remover B11 dos dois backlogs após implementação, preservar registro nas patch notes; ligar TM aos testes e limites manuais.
- [x] Sincronizar versão 0.10 e marcações segregadas no HTML; atualizar manifesto.
- [x] Executar validação final, fazer uma revisão independente do escopo e corrigir achados importantes com regressão. Registrar resumo real, processos e próximos passos.

## Registro de execução

Pré-flight: Task 1 produz classes consumidas por Task 2; Task 2 consome cobertura/versionamento de Task 3. Definir nomes reais no mapa e gerar manifesto antes da validação final. Sem outros conflitos.

Task 1: concluída — baseline 47/47; suíte ampliada 61/61. Novos testes retrospectivos passaram sobre comportamento existente; sem inventar RED do produto.

Task 2: concluída — seletor/runner RED→GREEN; Full 8/8 e Module Preferences 30/30; contratos Node 23/23. Verificador do pacote corrigido após falha real em PS5.1; módulos nativos isolados após incompatibilidade reproduzida.

Task 3: concluída — B11 retirado dos dois backlogs, regras permanentes, versão 0.10/mapa TM/guia/notas/manifesto atualizados; evidências em docs/evidence/VALIDATION-0.10.md.

Final review: agente review_validation_010, contexto novo, somente leitura, uma revisão do escopo. Important: Node skip com exit 0 — reproduzido e corrigido com teste RED→GREEN e Full aprovado. Log fixo do iniciador reclassificado de Minor para Important pela perda de diagnóstico entre rodadas — logs exclusivos por execução confirmados no Full. Declined to judge: vazio; nenhum achado pendente.

Ruling: sem Git, helpers de commits/worktree da skill não se aplicam; plano, manifesto e evidências preservam o registro. Custo: não há histórico de commits.

Ruling: sem testes E2E de navegador nesta entrega; integração HTTP/SQLite e JS cobrem regras e modelo, com limites explícitos por TM. Custo: aparência/DOM/gestos continuam exigindo conferência manual.

Ruling: depois do Full aprovado, mudanças finais são documentais; rodar Docs sem repetir produto inalterado. Custo: nenhuma evidência nova de produto é alegada para esse fechamento documental.
