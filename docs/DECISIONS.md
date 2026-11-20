# Decisões e rastreabilidade

**D36 — APROVADO, 15/11/2026:** backlog único em BACKLOG, com referência no README. O usuário autorizou executar B14, reorganizar a estrutura e excluir arquivos considerados desnecessários sem backup. Entrega local descrita em [STRUCTURE](STRUCTURE.md); a autorização não inclui publicar no GitHub. Instruções antigas de duplicação de backlog e fases iniciais abaixo são históricas.

**D35 — APROVADO, 12/11/2026:** usuário autorizou expressamente criação/execução dos testes, aplicação da rotina de validação e atualização dos testes a cada mudança/nova adição. B11 implementado na 0.10 e retirado dos dois backlogs pendentes. [Rotina e comandos](VALIDATION.md), [plano](superpowers/plans/2026-11-12-automated-validation.md). Autorização inclui processos temporários isolados necessários aos ensaios, sem deixar servidor ativo, tocar coleção pessoal ou presumir aceitação G4/publicação.

**D34 — APROVADO, apresentação documental:** pedido direto de 09/11 para rebranding do roteiro usando UX/UI, em formato agradável para o navegador. [TEST_CASES.html](TEST_CASES.html) substitui o Markdown como fonte única dos casos; o `.md` é apontador. Identidade editorial existente reaproveitada, sem mudar o app ou sua versão 0.9. [QA visual do documento](evidence/test-guide/design-qa.md).

**D33 — APROVADO, manutenção:** instrução direta de 09/11 determina versão funcional `0.xx` até autorização explícita de 1.0, sem incremento para somente visual/documentação; roteiro manual único sempre reavaliado, patch notes contínuos, backlog no README e em BACKLOG sincronizados e checkpoint antes de esgotar tokens/contexto. Regras em [AGENTS](../AGENTS.md); roteiro em [TEST_CASES](TEST_CASES.html), versão [0.9](../VERSION), histórico em [PATCH_NOTES](../PATCH_NOTES.md). Inclusão de B11 para futura automação não remove testes existentes nem autoriza implementá-la agora. Esta entrega é documental, sem nova versão funcional.

**D32 — APROVADO, operação 0.9:** por pedido direto do usuário em 09/11, servidor anterior encerrado e iniciador `.bat` vinculado à janela implementado. Não há serviço permanente/inicialização automática. [Registro de escopo e testes](quality/LAUNCHER-0.9.md). Não altera requisitos do MVP nem presume aceitação G4.

**Estado atual 0.8, 09/11/2026:** D30 — APROVADO: execução integral do plano 0.7 por autorização expressa [G3-EXECUTION-008](records/G3-EXECUTION-008.md). Nove etapas concluídas, candidato local G4 pronto, [QA](quality/QA-REPORT.md) e [revisão final](quality/FINAL_REVIEW-0.8.md). D31 — PENDENTE: revisão/aceitação do entregável pelo usuário; não foi inferida aprovação G4 ou distribuição externa. Registros anteriores são históricos. Detalhes técnicos reversíveis e seus custos estão no [ledger](quality/EXECUTION_LOG.md), sem alteração dos requisitos R01–R21.

> **Atualização de execução 0.8:** o usuário aprovou o plano e autorizou expressamente a execução em [G3-EXECUTION-008](records/G3-EXECUTION-008.md). A implementação A1 está em verificação para entrega local G4; resultados reais em [EXECUTION_LOG](quality/EXECUTION_LOG.md). Os status anteriores abaixo ficam preservados como histórico, sem reabrir autorizações.


Status é da regra na fonte, não aprovação formal deste texto. [Fontes](records/SOURCES.md).

| ID | Status | Decisão | Fonte | Consequência |
|---|---|---|---|---|
| D01 | APROVADO | Todas as cinco mídias no MVP | S1 | R01; listas/filtros não podem presumir mídia única |
| D02 | APROVADO | Anime atributo de filme/série | E3, S1 | R02; uma obra, diferentes visualizações |
| D03 | APROVADO | Local sem login, identidade interna | S1 | R03; propriedade pessoal sem autenticação |
| D04 | APROVADO | Master no MVP, híbrido comunitário no multiusuário | E3, S1 | R04; sem votos agora |
| D05 | APROVADO | Tags por grupos/mídias aplicáveis | E3, S1 | R05/R13; validar associações |
| D06 | APROVADO | Listas livres mistas | S1 | R06; independentes do estado |
| D07 | APROVADO | Estado único obrigatório por usuário/obra e mídia | S1 | R07; impedir ausência/duplicata |
| D08 | APROVADO | Só renomear estados existentes | E2 | R08; IDs estáveis |
| D09 | APROVADO | Transições livres e repetição com histórico | E2 | R09; preservar eventos anteriores |
| D10 | APROVADO | Datas opcionais com fallback inclusão | E1, S1 | R10; informado distinto de inferido |
| D11 | APROVADO | Anotação privada, sem ratings | E2, S1 | R11; não publicar como metadado |
| D12 | APROVADO | Catálogo reduzido via banco e campos comuns/específicos | E2, S1 | R12; sem editor completo implícito |
| D13 | APROVADO | Obra principal única; versões futuramente vinculáveis | E2 | R12; sem modelo de edições no MVP |
| D14 | APROVADO | CSV global ; e MERGE, ausentes preservados | S1 | R14; chaves/colunas ainda pendentes |
| D15 | APROVADO | Preview, válidas importadas, rejeitadas reportadas | E2, S1 | R15; atomicidade por registro |
| D16 | APROVADO | AND/OR/exclusão e filtros contextuais | E2, S1 | R16; gramática final a apresentar |
| D17 | APROVADO | Estados/listas em lote | E2 | R17; estado deve ser compatível |
| D18 | APROVADO | Arquivo pessoal reversível | E2 | R18; restauração preserva vínculos |
| D19 | APROVADO | CSV pessoal e exportação no backlog | E2 | Sem menus/endpoints dessas funções no MVP |
| D20 | APROVADO | Documentação e ZIP, sem implementar | E4, S1 | Nenhum scaffold/stack escolhido |
| D21 | APROVADO | Perguntas relevantes agrupadas | E1–E3, S1 | Não repetir descoberta já respondida |
| P01 | PROPOSTO | Entidades conceituais separadas | Este pacote | DATA_MODEL não é esquema físico aprovado |
| P02 | PROPOSTO | CSV ilustrativo incremental por tipo | Este pacote | Aprovar contrato antes do importador |
| P03 | PROPOSTO | Comparação arquitetural A/B/C | Este pacote | Nenhuma selecionada |
| P04 | PROPOSTO | Modo AND ou OR de inclusão, depois exclusões NOT | Este pacote | UX/arquitetura definem gramática |
| P05 | PROPOSTO | Associações comunitárias pendentes visíveis fora de filtros oficiais | S3 | Só futuro; aprovação detalhada não recuperada |
| P06 | PROPOSTO | Organização dos gates/templates | S1 pede gates | Não são aprovações já obtidas |

Arquivos correspondentes: [modelo](architecture/DATA_MODEL.md), [CSV](architecture/CSV_CONTRACT.md), [arquitetura](ARCHITECTURE.md), [aceitação](quality/ACCEPTANCE.md), [gates](GATES.md).

## Elaborações da rodada 0.2 — 08/11/2026

As regras D01–D21 e propostas P01–P06 acima foram preservadas. A mensagem atual autoriza preparar as propostas ([AUTH-001](records/AUTH-001.md)), não aprova seus resultados.

| ID | Status | Proposta | Evidência e consequência |
|---|---|---|---|
| P07 | PROPOSTO | Biblioteca unificada com recortes e detalhe pessoal/global separado | UX_PROPOSAL; três apresentações estáticas, escolha visual pendente |
| P08 | PROPOSTO | Cinco significados de estado por mídia, rótulos apenas renomeáveis | UX_PROPOSAL; validar conjunto em G1 antes de seed |
| P09 | PROPOSTO | Ciclos no mesmo acompanhamento, fallback na inclusão original, eventos/correções preservados | UX_PROPOSAL + DATA_MODEL; G1/G2, sem novo estado de reconsumo |
| P10 | PROPOSTO | Filtro com modo AND/OR e exclusão de qualquer critério NOT; contexto e busca externos à combinação | Detalha P04, sem construtor booleano aninhado |
| P11 | PROPOSTO | Lote pessoal por grupos de mídia e commit integral da seleção revisada | UX_PROPOSAL; não confundir com sucesso parcial do CSV |
| P12 | PROPOSTO | Aplicação integrada e SQLite; A1 e A2 comparadas com B/C | ADR-001; recomendação do agente, nenhuma stack selecionada |
| P13 | PROPOSTO | Contrato CSV candidato 0.2, preservar vazio, união incremental, conflitos explícitos e revisão de preview | Detalha P02; CSV_CONTRACT, aguarda G2 |

Nenhuma dessas propostas substitui uma decisão aprovada sem confirmação real. Nenhuma pergunta histórica respondida foi reaberta.

Mudanças futuras usam DECISION_RECORD (documento histórico removido na limpeza B14). Preserve o registro anterior como substituído e a evidência da confirmação; não simule consentimento retroativo.

## Atualização 0.3 — seleção visual

| ID | Status | Decisão | Fonte | Consequência |
|---|---|---|---|---|
| D22 | APROVADO, alcance limitado | Direção visual 2 selecionada para refinamento | Resposta atual “2” à pergunta de escolha visual; G1-VISUAL-002 | Seguir galeria editorial; não reabrir seleção; G1 completo/G2/G3 pendentes |
| P14 | PROPOSTO | Aplicação da direção escolhida à obra, filtros/lote e administração | design/DIRECTION-2.md e novo mock | Revisar desdobramento; sem aprovação funcional implícita |

O estado “escolha visual pendente” de P07 na tabela histórica 0.2 é atualizado por D22 apenas quanto à direção. A navegação e os demais comportamentos de P07 continuam em revisão. P08–P13 não receberam aprovação pela escolha numérica.

## Continuidade 0.4

Usuário respondeu “Nesse momento nada” à revisão da tela de acompanhamento e depois “Então seguimos”. [G1-REVIEW-004](records/G1-REVIEW-004.md) registra revisão sem ajustes e autorização para continuar a documentação; não aprova retroativamente todos os comportamentos de P08–P14.

P15 — PROPOSTO: consolidação G1 0.4 (documento histórico removido na limpeza B14), com três mocks complementares e resumo dos estados/ciclos/filtros/lote para aprovação conjunta. O recorte de mídia faz parte do contexto externo a AND/OR; precisão documentada, ainda proposta. Arquitetura/CSV técnico/execução continuam nos gates posteriores.

## Escopo 0.5 — temas e idiomas

| ID | Status | Decisão | Evidência/consequência |
|---|---|---|---|
| D23 | APROVADO | Modo claro e escuro no MVP | Resposta atual “Sim” à inclusão explícita no MVP; R19 |
| D24 | APROVADO | Interface multilíngue PT-BR e EN no MVP | Mensagem atual em SCOPE-005; R20 |
| D25 | APROVADO | Possibilidade e maneira facilitada de adicionar idiomas | Mesma mensagem; R21, sem exigir terceira tradução nesta entrega |
| P16 | PROPOSTO | Preferências locais independentes, defaults claro/PT-BR, interface traduzida e conteúdo preservado | PREFERENCES_I18N; inclui política para rótulos personalizados e fallback |
| P17 | PROPOSTO | Recursos por idioma, chaves estáveis e estilos semânticos para temas | PREFERENCES_I18N e adendo ADR-001; mecanismos finais em G2 |

[SCOPE-005](records/SCOPE-005.md) guarda a mensagem exata e os limites. P15/UX 0.4 é sucedido para revisão conjunta pela UX 0.5 (documento histórico removido na limpeza B14), preservando o histórico. D23–D25 não significam aprovação global G0/G1 nem escolha de stack ou código.

## Aprovação 0.6 — G0/G1 e proposta G2

| ID | Status atual | Decisão/alcance | Evidência |
|---|---|---|---|
| D26 | APROVADO | Consolidação funcional G0 e UX 0.5 G1, sem ressalvas fornecidas | Pergunta explicitou G0/G1 e resposta atual “Sim”; G0-G1-APPROVED-006 |
| D27 | APROVADO no alcance UX | P07–P11/P14/P16 e comportamentos da consolidação: navegação, estados, ciclos/datas, filtros/lote e preferências | G1_REVIEW-0.5 e registro de alcance; não inclui esquema/stack/CSV técnico |
| P18 | PROPOSTO G2 | A1 C#/Razor Pages + EF Core/SQLite, operação Windows/loopback e recuperação | G2_REVIEW-0.6; alternativas A2/B/C preservadas, sem seleção pelo usuário |
| P19 | PROPOSTO G2 | Modelo físico e contratos de CSV/normalização/retenção/concorrência | PHYSICAL_MODEL-0.6 e G2_REVIEW-0.6; nenhuma migração/banco criado |

As tabelas anteriores descrevem o status no momento em que foram escritas; D26/D27 atualizam o alcance de UX sem apagar histórico. P12/P13 e a parte técnica de P17 continuam pendentes G2; P15 histórico é sucedido pela revisão aprovada 0.5. Não solicitar novamente aprovação G0/G1 nem presumir G2/G3 pela confirmação atual.

## Escolha 0.7 — A1 e plano proposto

| ID | Status atual | Decisão/alcance | Evidência |
|---|---|---|---|
| D28 | APROVADO | Seguir A1: C#/.NET, Razor Pages, EF Core e SQLite local | “Siga pela A1”, em resposta ao pedido de A1/contratos G2 para preparar plano; G2-A1-007 |
| D29 | BASE TÉCNICA CONSOLIDADA G2 | Conjunto técnico 0.6 adotado no contexto dessa continuidade; alterações materiais voltam a G2 | G2_REVIEW-0.6 e PHYSICAL_MODEL-0.6, limites registrados em G2-A1-007 |
| P20 | PROPOSTO G3 | Plano 0.7 em nove etapas, SDK/pacotes/comandos e verificação por fatia | IMPLEMENTATION_PLAN-0.7; aprovação e execução ainda pendentes |

A2/B/C permanecem comparações históricas, sem reabrir stack. Instalação isolada do SDK 10 e dependências é proposta do plano, não ação já autorizada/executada. Assunções técnicas/versões estão explícitas para revisão, sem mudar requisitos aprovados.

