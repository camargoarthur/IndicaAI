# Aprovação — G0 e G1, revisão 0.5

Status: APROVADO. Data: 08/11/2026. Registro baseado em APPROVAL_RECORD.

| Campo | Valor |
|---|---|
| Gates | G0 — base funcional; G1 — UX |
| Artefatos apresentados | PRD/decisões/pendências consolidados e G1_REVIEW-0.5.md; inclui referências aos mocks, UX_PROPOSAL e PREFERENCES_I18N |
| Responsável | Usuário desta conversa |
| Evidência imediata | Pergunta anterior: “Para avançar conforme os gates do projeto, você aprova a consolidação funcional e a UX 0.5 (G0/G1)?”; resposta atual: “Sim” |
| Resultado | G0/G1 aprovados na revisão apresentada |
| Ressalvas fornecidas | Nenhuma |
| Escopo liberado | Consolidar os impactos da UX e apresentar arquitetura/contratos G2 para escolha |
| Continua pendente | Stack/banco/contratos físicos/CSV/operação G2; plano G3 e autorização expressa de execução; entrega G4 |

## Alcance aprovado

Base R01–R21; direção editorial 2 e seus desdobramentos; navegação; cinco significados de estado por mídia; renomeação sem alterar significado; ciclos internos e datas com referência na inclusão original; correção com registro preservado; filtros com recorte externo a AND/OR e exclusões; lote pessoal integral revisado por mídia; arquivo/restauração; apresentação administrativa de tags e CSV.

Também inclui a interação de preferências da revisão 0.5: claro/escuro e PT-BR/EN independentes, defaults claro/PT-BR, escolha local persistida, rótulos padrão traduzidos e conteúdo/personalizações preservados. Mecanismo físico de persistência/localização/tema continua G2; mock não comprova contraste ou funcionamento. WCAG 2.2 AA é objetivo de UX para verificação, não conformidade já demonstrada.

G1 aprovou o fluxo CSV (preview, preservação, parcial e relatório), não suas chaves/colunas/normalização/limites/retenção. Não aprovou alternativas técnicas citadas incidentalmente nos documentos, SQL, ORM, distribuição, instalação, banco criado, scaffold ou deploy. Não perguntar novamente a aprovação de G0/G1 deste alcance; mudança material deve ser identificada e registrada.

Revisão apresentada: 0.5, sem commit informado. Manifesto anterior à atualização de registro preservado em PACKAGE_MANIFEST-v0.5 (documento histórico removido na limpeza B14).
