# Modelo conceitual

Referência técnica adotada na implementação A1. A aprovação e execução estão registradas em [GATES](../GATES.md); código, migrações e testes são a evidência do comportamento atual. Terminologia propositiva no corpo preserva o desenho original e não significa que a implementação esteja pendente.

## Invariantes aprovadas

1. Visualização anime não muda identidade nem duplica a obra. Mangá é mídia própria (R01–R02).
2. No máximo um acompanhamento por usuário/obra; exatamente um estado obrigatório compatível, inclusive se arquivado (R07).
3. Renomear mantém identidade/semântica, sem alterar quantidade de estados (R08).
4. Transições livres compatíveis e reconsumo preservam histórico (R09).
5. Data informada é opcional; efetiva usa inclusão na ausência. Registro real é separado de data retroativa (R10).
6. Listas são independentes de estado e mistas (R06).
7. Anotação pertence à identidade/acompanhamento, privada e sem rating (R11).
8. Arquivo preserva dados e vínculos necessários à restauração (R18).
9. MERGE preserva dados ausentes e não deixa linha inválida parcialmente gravada (R14–R15).

## Datas: representação candidata

PROPOSTO: guardar início/conclusão informados nulos quando ausentes e derivar cada data efetiva da informada ou inclusão, com indicação da origem. Guardar instante de inclusão separado de datas de calendário. Não preencher o campo informado com fallback, pois apagaria a distinção.

Fallback de conclusão não significa conclusão automática. Ciclos de reconsumo, repetir o mesmo estado e edição de datas passadas precisam de desenho de interação antes da implementação. Não criar vários acompanhamentos por obra para representar ciclos.

## Campos de catálogo

Aprovado: campos comuns e alguns específicos; entrada inicial via banco. Propostas ilustrativas: título/descrição comuns, autoria para livros/mangás, plataforma para jogos, duração para filmes, contagem de episódios para séries. Não tornam progresso por episódio, capas externas, integrações ou obrigatoriedade desses campos parte do MVP.

Não incluir votos, ratings, versões, credenciais ou contas remotas como entidades implementadas do MVP.

## Revisão 0.2 — candidato relacional para G2

PROPOSTO, condicionado à UX e à escolha arquitetural. Detalha a proposta anterior, não é DDL, migração ou banco criado. Se C for escolhida, as garantias precisam de representação equivalente no armazenamento do navegador.

| Estrutura candidata | Chave/restrição e conteúdo |
|---|---|
| Identidade local | ID estável único da instância pessoal, sem credencial |
| Obra | ID estável opaco; mídia obrigatória em cinco códigos; título obrigatório; descrição e ano opcionais; anime booleano permitido apenas em filme/série |
| Detalhe por mídia | Uma relação 1:0..1 com obra, compatível com sua mídia; jogo: plataformas/estúdio; livro: autoria; mangá: autoria; filme: direção/duração em minutos; série: criadores; todos opcionais |
| Estado base | Chave por mídia + código semântico; cinco significados candidatos definidos em UX_PROPOSAL; rótulo inicial; conjunto fechado |
| Rótulo pessoal | Par identidade/estado base único; texto não vazio; não muda código semântico |
| Acompanhamento | ID; unicidade identidade/obra; mídia vinculada à obra; estado atual obrigatório com chave composta mídia/estado; inclusão UTC + data local de inclusão imutável; ciclo atual; nota; arquivo; versão inteira |
| Ciclo | ID e acompanhamento; sequência única por acompanhamento; início/conclusão informados como datas de calendário nulas; instante de criação; anteriores preservados |
| Evento | ID, acompanhamento/ciclo, sequência de registro única, estado origem/destino quando transição, tipo, instante UTC, data informada quando houver, dados da correção e referência ao corrigido |
| Lista/item | Lista por identidade, item único lista/acompanhamento; garantir o mesmo proprietário |
| Grupo/tag | IDs estáveis; grupo obrigatório por tag como proposta; nome preservado, normalizado para detectar ambiguidades |
| Aplicabilidade/associação | Pares únicos tag/mídia e obra/tag; validar compatibilidade em toda mutação, inclusive edição administrativa |
| Importação/linha | ID de operação, hash/contrato/tipo/revisão do preview, ação/resultado por registro; sem armazenar CSV pessoal; retenção a consolidar |

Campo de capa não é obrigatório nem depende de URL externa. Contagem/progresso de episódios ou capítulos não entra por causa desta proposta. Coleções descritivas como autoria/plataformas pertencem ao seed/catalogação via banco; CSV de tags não edita esses dados. IDs opacos não são criados por dedução de título, evitando juntar homônimos.

Estado e evento devem ser persistidos juntos. Propor uma chave de operação para tornar reenvio idempotente; constraints de unicidade são a defesa final contra repetição. Repetir estado no formulário comum não cria evento, mas novo ciclo é evento explícito. Correções preservam valores anteriores; eventos de estado não são apagados nem reordenados pela data retroativa.

Datas efetivas por ciclo: informado quando presente; senão, data local da inclusão original do acompanhamento. O ciclo não fabrica data de início ou conclusão. Guardar a data local da inclusão evita mudança de referência por conversão posterior de fuso; o instante UTC serve à auditoria. Manter apenas o estado atual do acompanhamento não basta para reconstruir ciclos e datas anteriores.

Unicidade/foreign keys garantem existência e pares básicos; validação transacional de domínio e, se necessário, restrições adicionais garantem compatibilidade cruzada e propriedade. Não apresentar validação só na UI como proteção suficiente. Exclusão em cascata de acompanhamento arquivado não é proposta; arquivar não exclui registro.

Campos e restrições finais, representação de coleções, ciclo/evento corrigido e retenção de importação precisam da aprovação G2. Não há seed real, SQL, ORM ou decisão física aprovada.

## Adendo 0.5 — preferências e textos localizados

R19–R21 aprovados; estrutura física ainda PROPOSTA. Acrescentar preferências ligadas à identidade interna com modo de aparência (`light`/`dark`) e localidade suportada (`pt-BR`/`en` inicialmente), defaults/versionamento candidatos em PREFERENCES_I18N. Não criar identidade/conta por idioma ou tema.

Estados base conservam IDs/mídia/significado; seus rótulos padrão vêm dos recursos de tradução, não são chave de relação ou do histórico. Rótulo personalizado continua conteúdo do usuário e sua ausência permite mostrar o padrão no idioma atual. Preferência de idioma não migra/duplica obra, evento, acompanhamento, lista, tag, nota ou datas; o catálogo de mensagens da interface é separado do conteúdo cadastrado.

Persistência final das preferências depende de G2; não foram criadas tabelas, recursos traduzidos de produção ou migrações nesta rodada. Datas de calendário/instantes mantêm os tipos/valores canônicos, variando somente apresentação por localidade.
