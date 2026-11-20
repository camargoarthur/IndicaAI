# ADR-001 — arquitetura local e persistência

Atualização 0.7: usuário escolheu A1, [G2-A1-007](G2-A1-007.md). ADR preserva alternativas e recomendações históricas; contexto/contratos 0.6 alimentam o plano G3 proposto. Essa escolha não significa build/testes, banco criado ou autorização de execução.

Status: PROPOSTO. Gate G2. Data: 08/11/2026. Revisão 0.2.
Requisitos: R01–R18, especialmente R03/R07/R09/R12/R14/R15/R18. Fontes: PRD, UX_PROPOSAL, DATA_MODEL, CSV_CONTRACT e fontes oficiais T01–T09 em SOURCES.

Situação 0.6: G0/G1 0.5 aprovados, incluindo R19–R21. Este ADR permanece comparação histórica com adendos; G2_REVIEW-0.6 (documento histórico removido na limpeza B14) é o conjunto atual para escolha de A1/A2/B/C e contratos, com [modelo físico](../architecture/PHYSICAL_MODEL.md). Nenhuma alternativa selecionada ainda. Não confundir aprovação da UX com escolha técnica.

## Problema

Guardar catálogo e acompanhamento localmente, com estado/histórico coerentes e importação incremental recuperável. O usuário ainda não escolheu linguagem, framework ou distribuição. A possibilidade multiusuário não justifica API remota, contas ou infraestrutura antecipadas.

## Alternativas

Custos abaixo são avaliação qualitativa do agente, não benchmarks ou preferência do usuário. A/B/C descrevem topologia; linguagem é escolha separada.

| Opção | Candidato concreto | Benefício | Custo/risco | Reversão |
|---|---|---|---|---|
| A1 — integrada | ASP.NET Core / Razor Pages, C#, EF Core, SQLite em arquivo | Um serviço local e um banco; formulários e transações no mesmo limite; distribuição com runtime embutido é suportada | JavaScript pontual para filtros/preview; testar migrações SQLite, que têm limitações de reconstrução e tipos; pacote por SO/arquitetura | Moderada: domínio/dados relacionais reaproveitáveis; UI e persistência não migram automaticamente |
| A2 — integrada | Django / templates, Python, ORM, SQLite | Formulários e transações disponíveis; organização de domínio semelhante a A1 | Distribuir Python/servidor e manter ambiente; admin padrão não atende o local sem login sem redesenhar interface; SQLite continua limitado em concorrência | Moderada; templates/ORM específicos precisam adaptação |
| B — UI + API | React/TypeScript com build Vite; API ASP.NET Core/C#; SQLite | Interações ricas e contrato de API explícito; útil se múltiplos clientes se tornarem requisito real | Dois componentes lógicos e cadeia de build adicional; contratos, erros e estado duplicam trabalho; pode servir arquivos compilados pelo mesmo processo local | Moderada/alta para eliminar API; boa separação não elimina custo de evolução multiusuário |
| C — navegador | TypeScript + IndexedDB | Dispensa serviço de dados instalado; transações locais no navegador | Armazenamento depende de origem/perfil, quotas e limpeza; carga de catálogo via banco e recuperação precisam prova concreta; persistência solicitada pode não ser concedida | Alta para migrar armazenamento e operação; não presumir banco relacional nem simples cópia de arquivo |

SQLite foi concebido para armazenamento local de aplicações; muitos escritores simultâneos ou arquivo em rede mudam essa adequação. Isso sustenta A/B para o MVP local, sem prometer que servirá para a fase multiusuário. [T01](https://www.sqlite.org/whentouse.html).

## Recomendação

Recomendar A, com SQLite em arquivo e módulos internos bem separados. A1 é candidato preferencial desta revisão pela distribuição autocontida suportada e pelo encaixe de formulários/transações; A2 continua alternativa válida se manutenção em Python for preferível. Essa preferência é inferência do agente, sem benchmark ou conhecimento da experiência técnica do usuário. Não seleciona stack em seu nome.

Para A1, avaliar a linha .NET 10 LTS, atualmente listada na política oficial, e fixar patch/compatibilidade no G3. Para A2, avaliar linha Django 5.2 LTS ainda suportada conforme tabela oficial; não instalar ou fixar versões agora. Publicação autocontida inclui runtime e requer artefato específico da plataforma. [T02](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core), [T03](https://learn.microsoft.com/en-us/dotnet/core/deploying/), [T08](https://www.djangoproject.com/download/).

Escolher B se houver necessidade aprovada de cliente independente ou se a direção de interação justificar o custo; escolha visual isolada não exige SPA. Não recomendar C sem demonstrar durabilidade/recuperação e como inserir catálogo via banco. A documentação de armazenamento do navegador confirma políticas de quota/evicção; IndexedDB não é sinônimo de perda inevitável, mas exige decisão operacional explícita. [T09](https://developer.mozilla.org/en-US/docs/Web/API/Storage_API/Storage_quotas_and_eviction_criteria).

## Operação proposta para A/B

1. Uma instância local inicia serviço limitado ao loopback e abre navegador; encerrar termina o serviço, preservando arquivo. Windows é alvo candidato por ser o ambiente atual, não requisito aprovado de distribuição.
2. Banco e backups em diretório de dados do usuário separado dos binários. Atualização não troca banco por seed nem redefine identidade. Caminho exato e formato de pacote são entregáveis G3 após G2.
3. Uma identidade local estável, criada uma vez; master é capacidade da instância. Sem credenciais, tela de login ou seleção multiusuário. Notas são privadas no produto, mas pessoas com acesso ao mesmo arquivo/conta do SO não ficam isoladas por autenticação inexistente.
4. Recursos necessários ao MVP servidos localmente, sem CDN obrigatória ou chamadas externas. Offline integral é critério proposto a verificar na distribuição, não resultado medido.
5. Rejeitar origens/hosts inesperados e proteger mutações contra requisições de outras páginas. Loopback sozinho não substitui proteção de formulários/API. Sem exposição LAN como atalho para multiusuário.

## Contratos de integridade propostos

| Operação | Limite transacional e comportamento em falha |
|---|---|
| Incluir/reativar | Unicidade identidade/obra; estado compatível obrigatório; recuperar ID existente em disputa |
| Alterar estado | Evento + estado atual + versão do acompanhamento no mesmo commit; ordem por sequência de registro, não data informada |
| Novo ciclo | Criar ciclo e evento, apontar ciclo atual e estado juntos; preservar ciclos anteriores |
| Renomear | Apenas rótulo por identidade/estado base; nenhum histórico ou significado alterado |
| Lote pessoal | Uma transação para toda seleção revisada; conflito aborta lote, informando recarregar |
| Arquivar/restaurar | Alterar marca pessoal; nenhuma exclusão de eventos/datas/notas/vínculos |
| CSV global | Preview só leitura; confirmar snapshot revisado; transação independente por registro incluindo vínculos; sem transação única que desfaça todas as válidas |

EF Core e Django possuem mecanismos transacionais, mas a aplicação deve definir os limites acima; usar ORM não garante contratos sozinho. [T04](https://learn.microsoft.com/en-us/ef/core/saving/transactions), [T07](https://docs.djangoproject.com/en/5.2/topics/db/transactions/).

Habilitar/verificar chaves estrangeiras em cada conexão SQLite; unicidade e verificações no banco complementam regras de domínio. Usar versão inteira controlada pela aplicação para conflitos, pois token gerado pelo banco não é suportado pelo provider SQLite do EF. Migrações podem exigir reconstrução de tabelas. [T05](https://www.sqlite.org/foreignkeys.html), [T06](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations).

Preview CSV deve guardar hash do conteúdo, contrato, tipo, ações e revisão global de classificação consultada. Na confirmação, comparar revisão e revalidar sob controle de escrita; divergência anterior à aplicação invalida preview. Durante aplicação, serializar alterações administrativas concorrentes, mantendo commits por linha. Interrupção conserva commits concluídos e marca demais como não processadas; repetição faz MERGE idempotente. Propor diário local mínimo do lote para recuperar resultado após reinício; retenção ainda precisa constar do contrato G2.

Consultas usam contexto do proprietário e arquivo, predicados parametrizados para AND/OR/NOT e deduplicação de obras em junções de tags/listas. Índices iniciais candidatos: unicidade identidade/obra, pares lista/acompanhamento e obra/tag, histórico por acompanhamento/sequência, filtros de identidade/arquivo/mídia. Medir antes de adicionar mecanismo de busca/índices especulativos.

## Recuperação e verificações futuras

Backup operacional não é exportação pessoal do produto. Propor cópia consistente pela API de backup SQLite, antes de migração, em destino versionado sem sobrescrever única cópia. Com banco aberto, não presumir que copiar só o arquivo principal basta. A API oficial oferece snapshot consistente. [T10](https://www.sqlite.org/backup.html).

Restaurar com serviço parado, conferir integridade/referências e versão, preservar cópia do banco atual e abrir banco restaurado; nunca testar recuperação sobre os únicos dados do usuário. Política de retenção/capacidade e procedimento final entram no G2/G3. Nesta rodada não se cria banco nem backup real.

G3 deve planejar testes de falha entre evento/estado, compatibilidade por mídia, datas/ciclos, reenvio de operação, MERGE repetido, rejeição sem vínculos parciais, preview desatualizado, interrupção, restauração e persistência após reinício. UX com teclado e largura estreita também precisa verificação. Nenhum teste de aplicação ou medição de desempenho ocorreu.

## Decisão

Escolha do usuário: AINDA NÃO REGISTRADA. Evidência: ausente; esta recomendação não é aprovação.
Revisão dos artefatos: documentos 0.2 de 08/11/2026, sem commit existente informado.
Ressalvas: revisar dependências de estados/ciclos e CSV após G1. Substitui: não substitui escolha aprovada; detalha P03 e preserva comparação 0.1 em ARCHITECTURE.

## Consequências

Se A1/A2 for aprovado, detalhar esquema físico, contrato CSV final, distribuição/recuperação e plano G3. Se B/C for escolhido, revisar os mesmos contratos, sem sacrificar preservação. G2 permanece pendente; G3 e execução não foram autorizados. Documentos afetados: ARCHITECTURE, DATA_MODEL, CSV_CONTRACT e OPEN_QUESTIONS.

## Adendo 0.5 — temas e internacionalização

Requisitos adicionais aprovados R19–R21, com evidência em SCOPE-005. [PREFERENCES_I18N](../architecture/PREFERENCES_I18N.md) define proposta independente de stack: recursos de idioma separados, chaves/mensagens parametrizadas, formato localizado só na apresentação, rótulos personalizados e conteúdo preservados, cores semânticas claro/escuro e preferências ligadas à identidade local.

A1 tem serviços/recursos de localização do ASP.NET Core; A2 tem catálogos de tradução Django; B/C precisam camada de localização da interface e contratos canônicos. Fontes T11–T15 em SOURCES. Esses mecanismos não são tradução automática, e todos os candidatos precisam cobertura de mensagens, layout e contraste. O novo requisito não impõe SPA nem muda sozinho a recomendação A; comparar custo de manutenção de traduções e persistência da preferência em G2.

Contrato candidato para novos idiomas: adicionar/revisar recursos e registro de localidade, sem refazer telas/domínio/migrar obras. Idiomas com direção/fonte/calendário diferentes podem exigir adaptação de apresentação. Patches, ferramentas e biblioteca/formato de recursos só após escolha técnica; não foram instalados.
