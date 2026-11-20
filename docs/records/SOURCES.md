# Fontes e limites

Registro de proveniência — 08/11/2026.

## Produto

- S1: pedido atual do usuário, com as regras explícitas e a criação do pacote. Fonte mais recente; cobre também regras iniciais não recuperadas.
- S2: conversa “Encontrar Skills para Projetos”, ID 6af0690b-582c-83e9-892b-3276e141ab03, recuperada pela ferramenta de histórico com limite de 10 turnos. Foram devolvidos 6 turnos, sem próxima página e sem anexos listados. Não se afirma acesso aos turnos iniciais.
- S3: consolidações do assistente no histórico recuperado. São contexto e implicações; sugestões não viram decisões do usuário. A tabela histórica de governança veio sem marcas nas células de fase: as fases foram determinadas pelas respostas explícitas.

| Evidência | Turno da conversa anterior | Decisões diretas |
|---|---|---|
| E1 | c2cd7355-1e88-47be-a606-e17b4a0095bc | Datas opcionais com fallback inclusão; perguntas agrupadas |
| E2 | f7b0fa75-1eac-4078-a1b2-f833fbce5303 | 12 respostas: só renomear estados, transições livres, repetição com histórico, anotações, campos comuns/específicos, obra principal, filtros E/OU/exclusão, lote, CSV pessoal futuro, sucesso parcial, exportação futura, arquivamento |
| E3 | 936e1823-a63e-4616-a4ec-adc7d183e66f | Master MVP e comunidade futura; anime atributo; tags por grupos/mídias |
| E4 | 4e3d89cf-5a9c-4cdf-aa48-e5ddb5fec575 e 5abfd122-6b2b-4597-b768-b1b522098cad | MDs, ZIP, AGENTS, skills e prompt |

O sistema comunitário híbrido foi reafirmado em S1. A regra “associação pendente aparece na obra, mas fica fora dos filtros oficiais” só aparece em S3 no material recuperado. Foi preservada como proposta de continuidade do backlog, sem ser apresentada como aprovação histórica detalhada.

## Convenções

APROVADO: regra explícita do usuário em S1 ou S2. Não significa aprovação formal de toda a redação deste pacote.
PROPOSTO: elaboração ou recomendação ainda não confirmada.
PENDENTE: escolha necessária sem solução consolidada.

Não foram reconstruídos nomes/quantidade dos estados, campos definitivos por mídia, pesos de votos, telas, cores ou stack.

## Orientação oficial do Codex

Consultada em 08/11/2026; não é fonte dos requisitos do produto.

- [Build skills](https://learn.chatgpt.com/docs/build-skills): formato, descoberta local e instalação.
- [Custom instructions with AGENTS.md](https://learn.chatgpt.com/docs/agent-configuration/agents-md): instruções por diretório.

Product Design e Superpowers estavam disponíveis no catálogo do ambiente autor. Isso não comprova disponibilidade no destino. Nenhuma distribuição externa foi embutida ou instalada pelo pacote.

## Rodada 0.2 — evidência da sessão atual

- S4: mensagem atual do usuário em 08/11/2026: “Ola, dentro de CODEX_START na pasta do projeto tem a conversa inicial e orientações, comece.” Escopo interpretado e registrado em [AUTH-001](AUTH-001.md). Não é aprovação dos novos artefatos.
- S5: catálogo efetivamente exposto na sessão: skills Superpowers e Product Design disponíveis; arquivos de `using-superpowers`, `brainstorming`, `verification-before-completion`, Product Design `index`, `user-context`, `get-context`, `ideate`, e `imagegen` lidos. Templates do projeto consultados. Nenhum plugin/dependência instalado.
- S6: três resultados Image Gen independentes, prompts e imagens em [design](../design/README.md), exibidos no chat na ordem 1–3. Não há design system, screenshots ou app anterior no pacote; nenhum foi inventado como referência.
- S7: baseline de tamanho/SHA-256 dos 27 arquivos conferido com PowerShell antes das edições; manifesto original preservado em evidence (documento histórico removido na limpeza B14). Python e launcher tentados, sem runtime instalado. Script de preflight Product Design lido, mas não executável neste ambiente; checagem equivalente confirmou ausência de `user-context.md`. Não se salvou contexto global.
- S8: Superpowers `systematic-debugging` consultada ao diagnosticar a diferença de execução do verificador entre PowerShell 7 e Windows PowerShell antigo. O mesmo script passou em PowerShell 7; não se alterou política global. Comando reproduzível e limite registrados em PACKAGE_VERIFICATION.
- S9: seleção atual do usuário: “2”, em resposta à pergunta explícita sobre direção visual. [G1-VISUAL-002](G1-VISUAL-002.md) registra alcance; [DIRECTION-2](../design/DIRECTION-2.md) detalha continuidade proposta. Um novo mock foi gerado por Image Gen usando estudo-2.png como referência visual anexada; não é nova rodada de alternativas, nem aprovação de comportamento ou implementação.
- S10: mensagens posteriores “Nesse momento nada” (ao pedido de ajustes da tela de acompanhamento) e “Então seguimos” (após confirmação de pendências). Revisão sem ajustes e continuidade documental registradas em [G1-REVIEW-004](G1-REVIEW-004.md). Três novos mocks complementares gerados para histórico, lote e CSV, com as duas imagens da direção 2 efetivamente anexadas como referências. Prompts 0.4 (documento histórico removido na limpeza B14); proposta conjunta (documento histórico removido na limpeza B14). Nenhuma aprovação funcional/arquitetural foi inferida dessas mensagens.
- S11: mensagem atual “Sim, e tambem possiblidade de multi lingua, atualmente com PTBR e EN mas com possibilidade e maneira facilitada de outros”, após pergunta sobre alternância claro/escuro no MVP. [SCOPE-005](SCOPE-005.md) registra aprovação de R19–R21, sem aprovação global G0/G1/G2/G3. [Preferências/localização](../architecture/PREFERENCES_I18N.md) e G1 0.5 (documento histórico removido na limpeza B14) são elaborações propostas; mock escuro/English gerado com a tela clara da direção escolhida efetivamente anexada, prompt preservado.
- S12: resposta atual “Sim” à pergunta explícita “Para avançar conforme os gates do projeto, você aprova a consolidação funcional e a UX 0.5 (G0/G1)?”. [G0-G1-APPROVED-006](G0-G1-APPROVED-006.md) registra aprovação real G0/G1, sem ressalvas fornecidas. G2 0.6 (documento histórico removido na limpeza B14) e [modelo físico](../architecture/PHYSICAL_MODEL.md) são novas propostas para escolha, não aprovadas por essa mensagem.
- S13: na preparação G2, política .NET e documentação oficial SQLite/EF Core (T02/T05/T06/T10) reconferidas em 08/11/2026. Skill writing-plans consultada após a aprovação do design; não se produziu plano detalhado porque GATES, instrução do usuário, exige primeiro escolha G2. Nenhuma skill permite antecipar código ou instalação. Revisão independente somente leitura do G2 pelo subagente revisao_g2_06 não encontrou lacuna material; restrições/contratos ainda precisam comprovação executável G3/G4.
- S14: mensagem atual “Siga pela A1”, em resposta à pergunta de A1/contratos G2 para preparar plano. [G2-A1-007](G2-A1-007.md) explicita escolha/contexto e limites. `writing-plans` aplicada ao plano 0.7 (documento histórico removido na limpeza B14), com autorrevisão, sem subagente de planejamento ou código de produto.
- S15: comandos somente leitura `dotnet --info`, `dotnet --list-sdks`, `dotnet --list-runtimes`: SDK 6.0.428 único; runtimes 6/8/9, nenhum SDK/runtime 10. Host x64/Windows. SDK 10 não instalado nesta rodada; obtenção isolada é etapa futura do plano. Nenhum restore/build/teste de produto realizado.

### Fontes técnicas oficiais consultadas em 08/11/2026

Sustentam capacidades/limites técnicos, não requisitos nem aprovação de stack. Recomendações de arquitetura são inferências do agente. Versões devem ser reconferidas no G3; sem fixar patch ou instalar ferramentas.

| ID | Fonte | Uso nesta revisão |
|---|---|---|
| T01 | [SQLite — Appropriate Uses](https://www.sqlite.org/whentouse.html) | Adequação a dados locais e limites de concorrência/arquivo em rede |
| T02 | [.NET — Support Policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core) | Linha .NET 10 LTS como candidata suportada |
| T03 | [.NET — Publishing overview](https://learn.microsoft.com/en-us/dotnet/core/deploying/) | Distribuição autocontida ou dependente de runtime, artefato por plataforma |
| T04 | [EF Core — Transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions) | Atomicidade e transações explícitas |
| T05 | [SQLite — Foreign Keys](https://www.sqlite.org/foreignkeys.html) | Habilitar/verificar integridade referencial por conexão |
| T06 | [EF Core — SQLite limitations](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations) | Migrações, tipos e tokens de concorrência |
| T07 | [Django 5.2 — Transactions](https://docs.djangoproject.com/en/5.2/topics/db/transactions/) e [SQLite notes](https://docs.djangoproject.com/en/5.2/ref/databases/#sqlite-notes) | Transações e limites de concorrência SQLite na alternativa A2 |
| T08 | [Django — Supported versions](https://www.djangoproject.com/download/) | Django 5.2 LTS ainda listado com suporte estendido; não é a versão mais recente |
| T09 | [MDN — Storage quotas and eviction](https://developer.mozilla.org/en-US/docs/Web/API/Storage_API/Storage_quotas_and_eviction_criteria) | Persistência por origem e quotas/evicção da alternativa C |
| T10 | [SQLite — Online Backup API](https://www.sqlite.org/backup.html) | Snapshot consistente e procedimento operacional candidato |
| T11 | [ASP.NET Core — Globalization and localization](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-10.0) | Serviços/recursos de localização da alternativa A1; não equivale a tradução pronta |
| T12 | [Django 5.2 — Translation](https://docs.djangoproject.com/en/5.2/topics/i18n/translation/) | Catálogos de tradução e mensagens marcadas na alternativa A2 |
| T13 | [MDN — color-scheme](https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Properties/color-scheme) | Esquema de controles do navegador, complementando temas semânticos |
| T14 | [MDN — Intl.DateTimeFormat](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Intl/DateTimeFormat) | Formatação por localidade, sem alterar representação persistida |
| T15 | [W3C — Understanding Contrast Minimum](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html) | Verificação candidata de contraste nos dois temas, sem alegar conformidade do mock |
| T16 | [.NET new](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-new), [templates](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-new-sdk-templates), [test](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test), [publish](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-publish), [EF CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) | Sintaxe de comandos futuros do plano; não execução no SDK instalado |
| T17 | [SDK .NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0), [instalação Windows](https://learn.microsoft.com/en-us/dotnet/core/install/windows) | SDK 10.0.403 x64 confirmado; planejar obtenção isolada sem rebaixar para SDK 6 |
| T18 | [EF SQLite 10.0.14](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite/10.0.14), [EF Design 10.0.14](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Design/10.0.14), [dotnet-ef 10.0.14](https://www.nuget.org/packages/dotnet-ef/10.0.14), [Mvc.Testing 10.0.14](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Testing/10.0.14) | Versões propostas e ferramenta local compatíveis por linha; restore/teste ainda não provados |
| T19 | [CsvHelper 34.0.0](https://www.nuget.org/packages/CsvHelper/34.0.0), [guia oficial](https://joshclose.github.io/CsvHelper/getting-started.html), [SmartFormat.NET 3.6.2](https://www.nuget.org/packages/SmartFormat.NET/3.6.2) | Parsing e pluralização sem implementar essas ferramentas do zero |
| T20 | [xUnit v3 4.0.2](https://www.nuget.org/packages/xunit.v3/4.0.2), [guia v3](https://xunit.net/docs/getting-started/v3/getting-started), [runner 4.0.1](https://www.nuget.org/packages/xunit.runner.visualstudio/4.0.1), [Test.Sdk 18.11.0](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk/18.11.0), [integração ASP.NET](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0) | Runner VSTest explícito, projeto de testes e SQLite real; evitar pacote xunit v2 legado |
| T21 | [Unicode CaseFolding 17.0.0](https://www.unicode.org/Public/17.0.0/ucd/CaseFolding.txt), [licença de dados](https://www.unicode.org/license.txt) | Full casefold C+F e renormalização; recurso/atribuição a obter na execução, nenhum arquivo de dados copiado agora |

As consultas trouxeram conteúdo legível; algumas páginas Microsoft também continham avisos de interface/autorização/versão. A análise se limita às seções efetivamente devolvidas, sem afirmar validação de build ou comandos. Não se usaram artigos de terceiros ou snippets de busca como prova de implementação.
