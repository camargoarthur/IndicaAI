# MVP versus backlog

Lista canônica e única de pendências, referenciada na seção **Backlog** do [README](../README.md). Registrar novos pedidos somente aqui. Ao implementar um item, removê-lo desta lista de pendentes e registrar a entrega nas [patch notes](../PATCH_NOTES.md). Inclusão aqui não autoriza implementação nem define prioridade.

## MVP aprovado

Cinco mídias, local sem login com identidade interna, master de tags globais, listas mistas, estado obrigatório/renomeável por mídia, transições livres, histórico, datas com fallback, notas privadas, catálogo reduzido via banco, interface de tags, CSV administrativo com MERGE/preview/sucesso parcial, filtros contextuais, lote e arquivo reversível.

## Futuro separado

| ID | Item | Evidência de futuro | Dependência / limite |
|---|---|---|---|
| B01 | Múltiplos usuários e autenticação | Fase futura em S1/S2 | Sem provedor ou modelo de conta definido |
| B02 | Sugestões/votação híbrida e moderação | Explicitamente fase multiusuário, E3/S1 | Definir papéis, critérios, abuso e recurso antes de construir |
| B03 | CSV de biblioteca pessoal | Explicitamente adiado, E2 | Estrutura conceitual preparada não é importador antecipado |
| B04 | Exportação | Explicitamente adiada, E2 | Formatos e alcance a definir |
| B05 | Versões/edições vinculadas | Explicitamente futuro, E2 | Manter obra principal no MVP |
| B06 | Ampliar a integração de catálogo | Futuro citado em S3 | Busca individual de séries na interface, fontes para jogos/livros/mangás e revisão dos vínculos pendentes. Definir escopo e fonte antes de cada etapa |
| B07 | Recomendações | Futuro citado em S3 | Sem algoritmo ou prioridade aprovados |
| B08 | Progresso detalhado | Futuro citado em S3 | Métricas por mídia a definir |
| B09 | Aplicativos móveis | Possibilidade citada em S3 | Não pressupõe API separada agora |
| B10 | Ratings/avaliações | Fora do MVP; futuro apenas mencionado em S3 | Requer nova validação; anotações não autorizam ratings |
| B12 | Imagem compartilhável após avaliação | Gerar imagem de uma avaliação para publicar em Instagram, X/Twitter ou redes similares | Depende de definir/autorizar avaliações (B10); formato, conteúdo e privacidade da imagem serão decididos antes da implementação. Não implica postagem automática |
| B13 | Recap anual | Resumo do ano por mídia, categoria e tags, com exemplos como capítulos/páginas lidos e horas assistidas | Definir métricas e origem dos dados; detalhes de progresso por mídia dependem de B08. Não inferir páginas/horas não registradas |
| B15 | Catálogo local amplo de filmes e séries | Pedido de planejamento em 20/11/2026: estudar carga inicial e atualizações posteriores para busca offline | Confirmar fonte, direitos de armazenamento e uso de capas, volume e estratégia incremental |

PROPOSTA de continuidade comunitária baseada em S3: associações pendentes visíveis nas obras, mas fora dos filtros oficiais até aprovação. O caráter híbrido foi reafirmado; quórum, peso, prazos e poderes de moderação não foram definidos.

Não há prioridade aprovada entre esses itens, datas ou promessa de entrega. Evitar tabelas, filas e telas antecipadas para possibilidades futuras. Manter fronteiras conceituais e propriedade de dados explícita basta nesta etapa.
