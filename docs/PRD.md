# Requisitos do IndicaAI

Requisitos R01–R21 implementados no MVP local. Autoridade em [GATES](GATES.md); validação atual em [VALIDATION](VALIDATION.md).

## Objetivo

Unificar acompanhamento pessoal de jogos, livros, mangás, filmes e séries: estado, histórico, organização livre e anotações. Começar localmente, com catálogo reduzido, sem antecipar o produto multiusuário.

## Requisitos MVP

| ID | Aprovado | Limite ou consequência |
|---|---|---|
| R01 | Cinco mídias desde o MVP: jogo, livro, mangá, filme e série | Não reduzir escopo a jogos/anime |
| R02 | Anime atributo de filme/série | Sem sexta mídia nem cópia da obra por visualização |
| R03 | Local sem login, com identidade interna | Não implica senha, conta ou seleção multiusuário |
| R04 | Master administra tags globais | Comunidade híbrida só na fase multiusuário |
| R05 | Tags com grupos e mídias aplicáveis | Respeitar aplicabilidade na associação e nos filtros |
| R06 | Listas pessoais livres misturam mídias | Obra pode participar de várias listas; lista não é estado |
| R07 | Um estado atual obrigatório por usuário/obra | Compatível com mídia; um único acompanhamento |
| R08 | Só renomear estados existentes | Não criar/excluir estados nem mudar semântica pelo rótulo |
| R09 | Mudanças livres e reconsumo com histórico | Releitura, replay e reassistir sem duplicar obra |
| R10 | Datas de início/conclusão opcionais; fallback inclusão | Não exibir data inferida como confirmada |
| R11 | Campo de texto livre privado por obra | Anotação pessoal; sem ratings, estrelas ou avaliação pública |
| R12 | Catálogo reduzido via banco, campos comuns/específicos | Uma entrada principal; versões vinculáveis só futuramente |
| R13 | Interface de administração de tags | Cadastro/edição e associação pelo master |
| R14 | CSV administrativo global com ; e MERGE | Preservar dados ausentes; não substituir conjunto de dados |
| R15 | Preview e sucesso parcial no CSV | Importar válidas e reportar rejeitadas; sem registro parcialmente gravado |
| R16 | Filtros contextuais AND, OR e NOT | Critérios relevantes à tela; combinação inteligível |
| R17 | Estados e listas em lote | Validar compatibilidade de estados por mídia |
| R18 | Arquivar/restaurar dados pessoais | Preservar estado, histórico, nota, datas e vínculos |
| R19 | Alternância entre modo claro e escuro no MVP | Preservar a direção visual e legibilidade; fluxo/defaults/cores detalhados em G1 |
| R20 | Interface multilíngue com Português (Brasil) e inglês no MVP | Tradução e formatação sem alterar identidade, estado ou dados das obras; delimitação de conteúdo em G1 |
| R21 | Estrutura e procedimento que facilitem adicionar outros idiomas | Textos separados por idioma e localização extensível; não exigir uma terceira tradução agora |

## Regras relacionadas

Catálogo global e acompanhamento pessoal são distintos. Adicionar obra existente cria ou reativa acompanhamento da identidade interna, sem duplicar catálogo. O estado é pessoal, não atributo global da obra.

Renomear um estado mantém sua identidade e significado. Os nomes históricos não foram recuperados; o conjunto inicial proposto foi agora aprovado na UX 0.5: intenção, em consumo, pausa, conclusão e abandono, com rótulos por mídia da tabela G1. Não impor sequência de transições nem criar/excluir estados pela personalização.

Cada alteração de estado deve manter estado atual e histórico coerentes. Datas retroativas informadas não substituem o instante real do registro. A UX aprovada usa ciclos no mesmo acompanhamento, novo consumo explícito, correção de datas com preservação do valor anterior e repetição comum do estado sem evento fictício. Representação física, idempotência e restrições são contratos candidatos de G2, não permissão para perder histórico.

Quando início/conclusão não forem informados, sua referência efetiva é a data de inclusão. O fallback não conclui a obra automaticamente. Sua origem precisa ser reconhecível; datas aparentemente incoerentes e ciclos precisam de tratamento de UX antes da implementação.

Arquivamento é do acompanhamento pessoal, separado de excluir obra do catálogo. Arquivadas ficam fora da biblioteca ativa por padrão e têm caminho para consulta/restauração, conservando dados e relações. Arquivamento não é um novo estado de consumo.

Modo claro/escuro e PT-BR/EN fazem parte do MVP por instrução posterior explícita. Detalhamento de UX em [PREFERENCES_I18N](architecture/PREFERENCES_I18N.md), aprovado com G1 0.5: seletores independentes, defaults claro/PT-BR, preferência local persistida, proteção de rascunho, textos de interface separados do conteúdo informado e recursos de tradução extensíveis. Fallback PT-BR é proteção em execução, com cobertura completa PT-BR/EN na entrega; rótulos personalizados e conteúdo permanecem literais. Mecanismo físico/framework de localização permanece G2.

## Fora do MVP

Login, múltiplos usuários, sugestões/votos/moderação comunitária, CSV pessoal, exportação, ratings, integrações de catálogo, recomendações, aplicativos móveis e progresso detalhado por episódios/capítulos/horas. Veja [BACKLOG](BACKLOG.md) para diferenciar adiado explicitamente de possibilidades futuras.

## Qualidade

Preservação de dados, coerência do histórico, privacidade das anotações, filtros compreensíveis e persistência local decorrem das regras. G1 aprovou objetivo de UX WCAG 2.2 AA para verificação futura, sem alegar conformidade. Stack A1 aprovada e implementada. Critérios e limites efetivamente verificados estão nos relatórios de qualidade e na rotina de validação.

[Cenários de aceitação](quality/ACCEPTANCE.md) orientarão verificações futuras. [Gates](GATES.md) separam documentação, UX, arquitetura, plano e implementação.

