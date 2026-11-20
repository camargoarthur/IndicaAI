# Estrutura e auditoria B14 — 15/11/2026

## Organização mantida

| Pasta | Responsabilidade e razão para manter |
|---|---|
| src/ | Produto, recursos visuais, migrações e licenças incorporadas. Organização por funcionalidade já adequada. |
| tests/ | Cobertura de domínio, integração, HTTP, JS e operação; mapa TM e contratos da rotina. |
| scripts/ | Iniciadores e ferramentas de verificação; verificador documental transferido para cá. |
| examples/ | Fixtures CSV usadas pelo verificador documental. |
| docs/ | Documentação atual, roteiro HTML e índice de leitura. |
| docs/architecture/ | Modelo conceitual/físico, CSV e preferências aprovados, úteis à manutenção. |
| docs/quality/ | Relatórios finais e registro de implementação que sustentam a evidência entregue. |
| docs/records/ | Aprovações e decisões; necessários para não perder autoridade e contexto. |
| docs/design/ | Direção visual aprovada e referências usadas na implementação. |
| docs/evidence/ | Evidências finais, inventários de terceiros e resultados. |
| docs/licenses/ | Obrigações e proveniência dos recursos incorporados. |
| .tools/ | SDK, pacotes, ferramentas e logs locais; fora do versionamento. |
| .local/ | Dados locais/ensaios existentes; protegidos contra limpeza documental. |
| artifacts/ | Pacote executável usado pelo BAT; fora do versionamento. |
| .agents/ | Configuração local reservada do ambiente; não contém skill de produto. |

A raiz mantém solução/configuração de build, README, AGENTS, VERSION, PATCH_NOTES, manifesto ativo e BATs de uso. Não foi criada infraestrutura de hospedagem nem publicado repositório.

## Exclusões sem backup

48 arquivos eliminados nesta conclusão, além dos dez auxiliares de geração do guia removidos anteriormente. Propostas e templates concluíram sua finalidade; decisões aprovadas foram preservadas. Manifestos antigos descreviam estados já substituídos; o manifesto ativo permanece. Capturas intermediárias tinham equivalentes finais preservados. O verificador Python duplicava a verificação PowerShell usada pela rotina atual.

- CODEX_START.md
- VERIFY_PACKAGE.py
- PACKAGE_VERIFICATION.md
- docs/G1_REVIEW-0.4.md
- docs/G1_REVIEW-0.5.md
- docs/G2_REVIEW-0.6.md
- docs/IMPLEMENTATION_PLAN-0.7.md
- docs/REVIEW_PACKAGE-0.8.md
- docs/REVIEW-0.2.md
- docs/UX_BRIEF.md
- docs/UX_PROPOSAL.md
- docs/design/estudo-1.png
- docs/design/estudo-3.png
- docs/design/PROMPT-THEME-LOCALE-0.5.md
- docs/design/PROMPTS-G1-0.4.md
- docs/design/PROMPTS.md
- docs/evidence/PACKAGE_MANIFEST-v0.1.json
- docs/evidence/PACKAGE_MANIFEST-v0.2.json
- docs/evidence/PACKAGE_MANIFEST-v0.3.json
- docs/evidence/PACKAGE_MANIFEST-v0.4.json
- docs/evidence/PACKAGE_MANIFEST-v0.5.json
- docs/evidence/PACKAGE_MANIFEST-v0.6.json
- docs/evidence/PACKAGE_MANIFEST-v0.7.json
- docs/evidence/PACKAGE_MANIFEST-v0.8.json
- docs/evidence/PACKAGE_MANIFEST-v0.9-before-automation.json
- docs/evidence/PACKAGE_MANIFEST-v0.9-before-bat-guide.json
- docs/evidence/PACKAGE_MANIFEST-v0.9-before-copy-controls.json
- docs/evidence/PACKAGE_MANIFEST-v0.9-before-html-guide.json
- docs/evidence/PACKAGE_MANIFEST-v0.9-before-manual-guide.json
- templates/APPROVAL_RECORD.md
- templates/DECISION_RECORD.md
- templates/IMPLEMENTATION_PLAN.md
- templates/README.md
- templates/REVIEW_CHECKLIST.md
- templates/UX_HANDOFF.md
- docs/evidence/ui/detail-light-en-draft.jpg
- docs/evidence/ui/detail-light-pt-draft.jpg
- docs/evidence/ui/detail-mobile.jpg
- docs/evidence/ui/history-light-pt.jpg
- docs/evidence/ui/library-before.jpg
- docs/evidence/ui/library-dark-en.jpg
- docs/evidence/ui/library-dark-pt-selection.jpg
- docs/evidence/ui/library-light-pt.jpg
- docs/evidence/ui/mobile-320-before.jpg
- docs/evidence/ui/compare-library-before.png
- docs/evidence/test-guide/desktop-first.jpg
- docs/evidence/test-guide/mobile-320.jpg
- docs/evidence/test-guide/copy-mobile.jpg

## Movimentações

- docs/ACCEPTANCE.md → docs/quality/ACCEPTANCE.md
- docs/QA-REPORT.md → docs/quality/QA-REPORT.md
- docs/FINAL_REVIEW-0.8.md → docs/quality/FINAL_REVIEW-0.8.md
- docs/LAUNCHER-0.9.md → docs/quality/LAUNCHER-0.9.md
- docs/EXECUTION_LOG.md → docs/quality/EXECUTION_LOG.md
- design-qa.md → docs/quality/design-qa.md
- docs/DATA_MODEL.md → docs/architecture/DATA_MODEL.md
- docs/PHYSICAL_MODEL-0.6.md → docs/architecture/PHYSICAL_MODEL.md
- docs/CSV_CONTRACT.md → docs/architecture/CSV_CONTRACT.md
- docs/PREFERENCES_I18N.md → docs/architecture/PREFERENCES_I18N.md
- docs/SOURCES.md → docs/records/SOURCES.md
- scripts/Verify-Documents.ps1 → scripts/Verify-Documents.ps1

Links locais foram recalculados pelos destinos reais. Referências históricas a documentos excluídos permanecem identificadas como material removido, sem apontar para arquivos inexistentes. Patch notes e registros de aprovação não foram apagados. O guia HTML conserva a abertura offline, fontes, IDs e marcações da 0.10.

## Validação

Full aprovado: **8/8 etapas**, com **61/61 testes de produto**, **23/23 contratos Node**, modelo JS, build, publicação isolada, 22 recursos do pacote, encerramento/reabertura do iniciador e integridade documental. [Resumo estruturado](evidence/B14-validation-full.json). Logs em `.tools/validation/2026-11-15T12-31-08-898Z-18184`. A revisão documental final usa Docs, pois os ajustes posteriores são somente de documentação.

Também removido o instalador `.tools/downloads/dotnet-sdk-10.0.403-win-x64.zip` (300.745.255 bytes), após confirmar a instalação do SDK utilizada com sucesso na validação. Total de arquivos antigos removidos: **59** (48 desta lista, dez auxiliares anteriores e o instalador). Diretórios vazios `templates`, `TestResults` e `.tools/downloads` foram eliminados. Scripts temporários desta própria limpeza também foram retirados ao finalizar. Nenhuma cópia de backup foi criada.
