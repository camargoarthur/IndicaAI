# Contrato CSV

Referência técnica adotada na implementação A1. A aprovação e execução estão registradas em [GATES](../GATES.md); código, migrações e testes são a evidência do comportamento atual. Terminologia propositiva no corpo preserva o desenho original e não significa que a implementação esteja pendente.

## APROVADO

CSV administrativo global, separador ;, MERGE, dados ausentes preservados, preview sem gravação e sucesso parcial. Importar linhas válidas e reportar rejeitadas; sem gravação parcial de dados/vínculos da mesma linha.

O contexto é administração de tags. CSV pessoal é backlog. Não alterar estado, notas ou listas pessoais por esse fluxo. Catálogo inicial é via banco: não presumir importador completo de metadados de obras. Tipos/colunas definitivos de CSV não vieram no material recuperado.

## PENDENTE para G2

Chave estável; registros/colunas; codificação; normalização; IDs versus nomes; célula vazia versus coluna ausente; conflitos na mesma chave; dependências; revalidação do preview. Apresentar contrato concreto antes de implementar.

## PROPOSTA ilustrativa — não é formato de produção

Um tipo por arquivo, reduzindo dependências entre linhas:

| Exemplo | Chave candidata | Ação candidata |
|---|---|---|
| tags-proposta.csv | tag_id | Inserir/atualizar tag; grupo já existente identificado por ID |
| associacoes-proposta.csv | obra_id + tag_id | Acrescentar associação compatível, sem remover existentes |

Fixtures em [examples](../../examples/README.md): IDs fictícios, não dados iniciais. Proposta de sintaxe: UTF-8, cabeçalho, ; externo, aspas duplas para valores com ;, lista de mídias separada por |. Códigos candidatos: jogo, livro, manga, filme, serie. Anime não é código de mídia.

### MERGE candidato

- Coluna ausente preserva valor existente (regra aprovada).
- Propor célula vazia como preservação também; confirmar antes de implementar. Não apagar implicitamente.
- Campo presente válido atualiza só o que o contrato permite.
- Propor união incremental para coleções, sem substituir associações/mídias existentes por uma lista incompleta.
- Propor duplicata idêntica como “sem alteração”; linhas conflitantes na mesma chave rejeitadas, sem “última vence” oculto.
- Referência inexistente ou mídia inaplicável rejeita a linha. Não criar obra/grupo silenciosamente; eventual criação de grupo exige contrato próprio.
- Colunas desconhecidas devem ser sinalizadas, não interpretadas como comando.

Preservação é aprovada; vazio, união, duplicatas e normalização acima são propostas. MERGE sozinho não decide todos os casos.

## Fluxo

1. Ler estrutura; erro que impeça separar registros não produz importação arbitrária.
2. Validar contrato, resolver referências e classificar ações.
3. Preview por linha: ID, ação, antes/depois, campos preservados e erros, com contagens; sem gravar.
4. Confirmar lote exibido. Cancelar não altera dados.
5. Revalidar mudanças desde o preview; detalhar como invalidá-lo/atualizá-lo na arquitetura.
6. Aplicar cada registro válido atomicamente, incluindo vínculos. Uma falha independente não impede as outras linhas válidas.
7. Mostrar inseridas, atualizadas, sem alteração e rejeitadas com motivos. Todas inválidas: nenhuma gravação.

Apresentação/armazenamento do relatório e repetição do lote são propostas. Não acrescentar exportação para exibir relatório.

## Casos para revisar

| Situação | Resultado |
|---|---|
| Campo existente não veio no arquivo | Preservar (aprovado) |
| Associação existente ausente do arquivo | Preservar (aprovado) |
| Tag de jogo associada a livro | Rejeitar sem vínculo parcial, continuar válidas |
| Nome com ; entre aspas | Proposta: uma célula |
| Nome vazio para tag existente | Proposta: preservar; para tag nova, rejeitar ausência de nome |
| Duas linhas com mesma chave e nomes diferentes | Proposta: rejeitar conflito |
| Dado mudou depois do preview | Revalidar sem aplicar diferenças não revisadas |

## Revisão 0.2 — contrato candidato para G2

PROPOSTO. Detalha o contrato ilustrativo 0.1 acima, preservado para rastreabilidade; não é formato de produção aprovado. UX (documento histórico removido na limpeza B14) e [ADR-001](../records/ADR-001-local-architecture.md) descrevem a interação e os limites de gravação.

### Estrutura e interpretação

- Um tipo por arquivo: tags ou associações. UTF-8, BOM opcional, cabeçalho obrigatório, separador `;`, CRLF/LF aceitos, aspas duplas com escape por duplicação. Quoted newline permanece na célula; relatório identifica registro lógico e linha física inicial.
- Tags: `tag_id` obrigatório; `nome`, `grupo_id`, `midias` opcionais em atualização. Inserção exige nome, grupo existente e pelo menos uma mídia válida. Associações: exatamente `obra_id;tag_id`, ambos obrigatórios. Não cria obras ou grupos. Grupos são mantidos na interface administrativa.
- IDs: texto opaco, estável e sensível a maiúsculas; não inferir ID por nome. Propor limitar IDs a letras ASCII, números, ponto, sublinhado e hífen; não limitar a UUID, pois fixtures usam IDs legíveis. Remover espaços externos antes da validação. Nome em Unicode NFC, aparar apenas espaços externos; preservar capitalização/acento para exibição.
- Códigos de mídia: `jogo|livro|manga|filme|serie`, na célula `midias` separados por `|`, sem código anime. Duplicatas de mídia são deduplicadas. Propor nomes de tag únicos dentro de grupo após normalização NFC/casefold para detectar homônimos ambíguos; conflito com outro ID rejeita, sem fusão silenciosa.
- Cabeçalho duplicado, coluna desconhecida, tipo errado ou estrutura ilegível impedem o preview aplicável do arquivo inteiro; informar o motivo, sem ignorar silenciosamente. Linha vazia é ignorada com contagem, não importada. Erro semântico de registro rejeita só aquele registro.

### Semântica MERGE candidata

| Caso | Proposta |
|---|---|
| Coluna ausente ou célula vazia/espaços em atualização | Preservar valor; vazio não é comando de limpeza |
| Nova tag incompleta | Rejeitar registro; não criar tag parcialmente preenchida |
| Campo escalar válido presente | Atualizar somente nome/grupo informado, validando unicidade e referências |
| Mídias presentes | União com aplicabilidades existentes; não remover mídia ausente do arquivo |
| Associação existente | Sem alteração; outra associação ausente no arquivo fica preservada |
| Limpeza/remoção | Não há sintaxe CSV de remoção nesta revisão; edição administrativa segue validação explícita |
| Mesmo ID com payload normalizado idêntico | Primeira ocorrência pode aplicar; seguintes “duplicada sem alteração”, referenciando o primeiro registro |
| Mesmo ID com payloads diferentes, mesmo se campos forem complementares | Rejeitar todas as ocorrências dessa chave antes de gravar; sem “última vence” |
| Par obra/tag idêntico repetido | Deduplicar; registrar ocorrências extras sem alteração |
| IDs distintos convergem ao mesmo nome normalizado/grupo | Rejeitar todos os registros envolvidos no conflito; sem depender da ordem |
| Referência inexistente, mídia inválida ou inaplicável | Rejeitar registro inteiro, sem gravar nome/vínculo parcialmente |

Nenhuma interpretação de célula como comando, fórmula ou autorização. Conteúdo de CSV permanece dado. União incremental não permite reduzir aplicabilidade; a interface administrativa bloqueia redução que invalidaria vínculos.

### Revisão e execução

Preview só leitura apresenta hash do arquivo, versão de contrato, tipo, revisão dos dados consultados, contagens e diferenças exatas/preservadas por registro. Confirmar apenas as válidas desse preview; arquivo alterado exige nova validação. Mudança de taxonomia/associações entre preview e confirmação invalida preview e exige revisão, não confirmação invisível de novo resultado.

Durante aplicação, controlar concorrência administrativa e usar uma transação por registro, incluindo tag + aplicabilidades quando for o caso. Falha em uma linha não desfaz outras válidas. Se ocorrer erro de infraestrutura que impeça continuar, parar, informar commits concluídos e registros não processados, sem chamar estes de rejeição semântica. Um diário mínimo permite consultar resultado após reinício sem reutilizar preview antigo. Reimportação valida novamente e deve resultar em sem alteração para o já aplicado.

Propor, para dimensionar o MVP, limite inicial de 5 MiB e 10.000 registros lógicos por arquivo, rejeitado antes de confirmar. São limites de contrato candidatos, sem medição e ajustáveis no G2/G3. Propor manter resumo/resultado de importação local por 30 dias, sem guardar cópia integral do arquivo; política final e alcance do diário aguardam G2.

### Fixtures de decisão — não executadas contra importador

| Fixture conceitual | Pré-condição | Resultado esperado se contrato aprovado |
|---|---|---|
| Atualizar nome sem grupo/mídias | Tag existente, grupo G, mídias jogo+livro | Só nome muda; G e ambas as mídias permanecem |
| Nome vazio + mídia filme | Tag existente, nome N, mídia série | Preservar N, unir série+filme |
| Nova tag sem nome | Referência de grupo válida | Rejeitar sem criar tag/aplicabilidade |
| Nome com `;` entre aspas | Estrutura válida | Uma célula, nome íntegro |
| Duas linhas da mesma tag com nomes diferentes | Tag existente ou nova | Ambas rejeitadas, zero gravação para a chave |
| Atualização válida + vínculo inaplicável em arquivo de associações separado | Obra livro, tag só jogo; outro par compatível | Par inválido não gravado; válido pode gravar |
| Interrupção após commit de duas linhas | Terceira ainda não confirmada | Duas aplicadas; demais não processadas; relatório recuperável |
| Associação já existente repetida | Par válido existente | Sem alteração, sem duplicata |
| Revisão global mudou | Preview antigo | Zero gravação; renovar preview e revisar |

As fixtures CSV 0.1 existentes continuam ilustrativas; não são contrato aprovado nem prova de parser/importador. Limites, chaves, normalização e retenção acima são propostas adicionais de G2.

## Adendo 0.5 — idioma da interface

R20/R21 adicionam localização da interface, incluindo instruções, resumo e razões por registro. Proposta: idioma não altera cabeçalhos, chaves, códigos de mídia, delimitador `;`, referências ou normalização do contrato candidato; o mesmo arquivo gera as mesmas ações em PT-BR/EN. Preservar valores do arquivo literalmente conforme validação, sem traduzir nomes de tag/grupo ou tratar texto traduzido como identificador. Mensagens podem ser localizadas a partir de código estável e parâmetros; erro deve conservar chave/registro e informação técnica necessária à correção. Idioma não converte datas/números do contrato automaticamente. Layout/contraste do preview deve ser avaliado também no tema escuro.

