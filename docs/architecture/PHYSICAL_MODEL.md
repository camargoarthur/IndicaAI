# Modelo físico

Referência técnica adotada na implementação A1. A aprovação e execução estão registradas em [GATES](../GATES.md); código, migrações e testes são a evidência do comportamento atual. Terminologia propositiva no corpo preserva o desenho original e não significa que a implementação esteja pendente.

## Convenções candidatas

IDs gerados pela aplicação: UUID em representação textual canônica; chaves de referência existentes no CSV continuam texto opaco válido, sem exigir UUID. Códigos de mídia/estado e chaves de tradução são estáveis e independentes do idioma. `TEXT` para texto Unicode, códigos, IDs, datas e instantes; `INTEGER` para versões, sequências, contagens e booleanos 0/1. Datas de calendário em `YYYY-MM-DD`; instantes em UTC com formato ISO 8601 normalizado e precisão fixa. Formatar idioma apenas na apresentação.

`PK` identifica chave primária, `FK` referência, `UQ` unicidade. Campos são obrigatórios salvo `?`. Não aplicar exclusão em cascata como forma de arquivar. Habilitar/verificar foreign keys em toda conexão. Banco usa restrições e a camada de domínio valida operações cruzadas em transação; UI não é proteção suficiente.

## Catálogo e classificação global

| Estrutura candidata | Colunas principais e restrições |
|---|---|
| `media` | `code TEXT PK`: somente jogo, livro, manga, filme, serie |
| `work` | `id TEXT PK`, `media_code FK`, `title TEXT`, `description TEXT?`, `release_year INTEGER?`, `is_anime INTEGER`, `cover_asset TEXT?`, `version INTEGER`; título não vazio, booleano 0/1, anime=1 somente filme/série; `UQ(id,media_code)` para referências compostas |
| `work_credit` | `work_id FK`, `role TEXT`, `position INTEGER`, `name TEXT`; `PK(work_id,role,position)`, posição positiva, nome não vazio; papéis compatíveis validados por mídia |
| `game_detail` | `work_id PK/FK`, `studio TEXT?`; permitir apenas obra jogo |
| `game_platform` | `work_id FK`, `name TEXT`, `name_key TEXT`; `PK(work_id,name_key)`, somente jogo |
| `film_detail` | `work_id PK/FK`, `duration_minutes INTEGER?`, duração positiva quando informada; somente filme |
| `tag_group` | `id TEXT PK`, `name TEXT`, `name_key TEXT UQ`, `version INTEGER` |
| `tag` | `id TEXT PK`, `group_id FK`, `name TEXT`, `name_key TEXT`, `version INTEGER`; `UQ(group_id,name_key)` |
| `tag_media` | `tag_id FK`, `media_code FK`; `PK(tag_id,media_code)`; representar “todas” pelos cinco pares explícitos |
| `work_tag` | `work_id`, `tag_id`, `media_code`; `PK(work_id,tag_id)`; FKs compostas para `work(id,media_code)` e `tag_media(tag_id,media_code)` |

Campos mínimos candidatos: título/mídia; descrição/ano/anime opcionais nas mídias válidas. Créditos: autoria em livro/mangá, direção em filme, criação em série; jogos podem informar estúdio/plataformas. Não criar tabelas específicas vazias só para simetria; livros/mangás/séries usam créditos com papel apropriado. Papéis técnicos não viram tags/listas automaticamente. Não incluir progresso, versões/edições ou integrações.

Capa é opcional: `cover_asset` identifica arquivo local controlado, relativo ao diretório de recursos da instalação pessoal; não aceitar caminho arbitrário/URL externa como aquisição implícita. Sem capa, manter título/mídia/estado utilizáveis. Se houver arquivos locais, backup operacional deve incluí-los no mesmo conjunto restaurável.

`name_key` é derivada pela mesma normalização candidata do contrato CSV: aparar espaços externos, Unicode NFC e casefold invariável, sem remover acentos. Persistir a chave normalizada e usar comparação binária para unicidade, sem depender da cultura de execução ou da collation NOCASE do SQLite para Unicode completo. Algoritmo/biblioteca e versão serão fixados e testados no G3; não mudar regra silenciosamente entre interface e importador.

Reduzir `tag_media` com associações dependentes deve ser rejeitado pelas referências e pelo domínio; edição administrativa deve explicar as obras afetadas antes de tentar salvar. Grupo único por tag já faz parte da UX aprovada; normalização/unicidade e representação física são candidatas G2.

## Identidade e acompanhamento

| Estrutura candidata | Colunas principais e restrições |
|---|---|
| `local_identity` | `id TEXT PK`, `created_at_utc TEXT`; uma identidade ativa inicial, sem credencial/conta |
| `user_preferences` | `identity_id PK/FK`, `theme TEXT`, `locale TEXT`, `version INTEGER`; theme light/dark; locale disponível no registro de idiomas da aplicação, pt-BR/en inicialmente |
| `base_state` | `media_code FK`, `code TEXT`; `PK(media_code,code)`; cinco significados por mídia aprovados, rótulo padrão em recurso localizado |
| `state_label` | `identity_id FK`, `media_code`, `state_code`, `label TEXT`, `label_key TEXT`; `PK(identity_id,media_code,state_code)`, FK ao estado base, rótulo não vazio e `UQ(identity_id,media_code,label_key)` |
| `tracking` | `id TEXT PK`, `identity_id FK`, `work_id`, `media_code`, `state_code`, `included_at_utc TEXT`, `included_on TEXT`, `note TEXT`, `archived_at_utc TEXT?`, `version INTEGER`; `UQ(identity_id,work_id)`; FKs compostas para obra/mídia e estado/mídia; `UQ(identity_id,id)` |
| `consumption_cycle` | `id TEXT PK`, `tracking_id FK`, `ordinal INTEGER`, `created_at_utc TEXT`, `start_on TEXT?`, `completion_on TEXT?`, `version INTEGER`; `UQ(tracking_id,ordinal)` e `UQ(tracking_id,id)` |
| `tracking_event` | `id TEXT PK`, `tracking_id FK`, `cycle_id`, `sequence INTEGER`, `type TEXT`, `recorded_at_utc TEXT`, `media_code TEXT`, `from_state_code TEXT?`, `to_state_code TEXT?`, `corrected_event_id TEXT?`, `field TEXT?`, `old_value TEXT?`, `new_value TEXT?`, `operation_id TEXT`; `UQ(tracking_id,sequence)`, FK composta ciclo/acompanhamento e estados/mídia quando presentes |
| `personal_list` | `id TEXT PK`, `identity_id FK`, `name TEXT`, `version INTEGER`; `UQ(identity_id,id)`; sem semântica de estado |
| `list_item` | `identity_id`, `list_id`, `tracking_id`; `PK(list_id,tracking_id)`; FKs compostas identidade/lista e identidade/acompanhamento |
| `operation_receipt` | `identity_id FK`, `operation_id TEXT`, `request_hash TEXT`, `kind TEXT`, `result TEXT`, `recorded_at_utc TEXT`; `PK(identity_id,operation_id)`; resultado mínimo com IDs/versões, sem copiar notas desnecessariamente |

Conjunto de estados: intenção/em consumo/pausa/concluído/abandono por mídia, sem criação/exclusão pela interface. Ao renomear, validar também conflito de rótulo personalizado com os padrões efetivos da mesma mídia; defaults traduzidos não são materializados como personalizações. Trocar idioma não altera relações nem grava eventos. Se um nome literal já personalizado coincidir com padrão em outro idioma, mostrar significado base para distinguir e não apagar o dado; o tratamento de conflito não pode tornar a troca de idioma impossível.

O ciclo atual é derivado do maior ordinal, não de data retroativa. Incluir cria acompanhamento + ciclo 1 + evento inicial juntos. Novo ciclo incrementa ordinal e grava transição/evento no mesmo commit. O estado atual é o de `tracking`, mantido consistente com a última transição confirmada por sequência; correção de data não conta como transição de estado. Não preencher data informada com fallback. Eficaz = informada ou `tracking.included_on` imutável, em qualquer ciclo.

FK de correção deve apontar evento do mesmo acompanhamento/ciclo e campo pertinente; tipos/shape validados em domínio. Eventos não são alterados/apagados pelo produto: correção acrescenta registro, preservando valores anteriores. Campos `from`/`to` são requeridos conforme tipo de evento, não para todo evento. Arquivar/restaurar altera marca pessoal e versão, sem criar evento de consumo, sem mudar inclusão e sem apagar relações.

Reenvio usa `operation_id` e hash do pedido: mesma chave + mesmo pedido devolve recibo; mesma chave + conteúdo diferente rejeita. A transação grava recibo e mudanças juntas. Um lote pessoal pode gerar eventos de vários acompanhamentos, com o mesmo ID de operação e sequências próprias; não exigir `operation_id` único na tabela de eventos. Recibos pessoais não são apagados automaticamente no MVP proposto.

## Preview e resultado CSV

| Estrutura candidata | Colunas principais e restrições |
|---|---|
| `classification_revision` | Registro único, contador inteiro incrementado a cada alteração global relevante ao preview |
| `import_batch` | `id TEXT PK`, `file_hash TEXT`, `contract_version TEXT`, `type TEXT`, `baseline_revision INTEGER`, `status TEXT`, instantes de preview/confirmação/finalização; hash do plano revisado; sem arquivo integral bruto |
| `import_row` | `batch_id FK`, `record_number INTEGER`, `physical_start_line INTEGER`, `key TEXT`, `action TEXT`, `status TEXT`, `reason_code TEXT?`, `reason_args TEXT?`, `before TEXT?`, `after TEXT?`, `preserved TEXT?`; `PK(batch_id,record_number)` |

Preview guarda plano/diferenças normalizadas necessárias à confirmação; nenhuma mutação de tags/associações. Confirmar revalida hash/versão/revisão antes de aplicar e serializa alterações administrativas durante o lote. Cada linha válida é commit independente com resultado persistido na mesma transação; falha semântica não grava parte da linha. Interrupção distingue já aplicadas e não processadas e exige nova validação para repetir, sem reaplicar preview antigo.

Retenção candidata: 30 dias para resumo/linhas de lote terminado, com exclusão apenas de diagnóstico temporário, nunca de tags/vínculos gravados. Lotes interrompidos são reconciliados ao iniciar antes de qualquer expurgo. Não usar essa retenção para apagar recibos pessoais ou histórico. Dados de diagnóstico permanecem locais, sem envio externo.

## Limites de integridade e índices

Restrições compostas cobrem mídia/propriedade/applicabilidade; domínio transacional cobre sequência, tipos de evento, coerência do estado e revisões. Constraints não substituem testes de falha entre gravações. Guardar versões inteiras da aplicação e rejeitar edição obsoleta sem sobrescrever silenciosamente. Chaves/indexes iniciais são os PK/UQ citados e: tracking por identidade/arquivo/mídia, eventos por acompanhamento/sequência, ciclos por acompanhamento/ordinal e associações pelos sentidos de consulta necessários. Não antecipar mecanismo de busca ou índices sem medição.

Fonte de capacidades/limites: [SQLite FKs](https://www.sqlite.org/foreignkeys.html), [EF Core SQLite](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations). Tipos, restrições e migrações finais precisam da escolha G2 e de comprovação em G3/G4; não afirmar que o desenho foi executado ou que todos os constraints já passaram no SQLite real.
