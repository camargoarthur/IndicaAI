# Estudos visuais — revisão 0.2

Status atual 0.6: G0/G1 0.5 e os desdobramentos da direção 2 foram aprovados no alcance de UX ([registro](../records/G0-G1-APPROVED-006.md)). Rótulos de pendência anteriores abaixo são históricos. Imagens continuam estáticas, sem prova de contraste, tradução integral ou funcionamento; a aprovação visual não é evidência de teste nem escolha de stack.

Status: PROPOSTO, G1 pendente. Gerados em 08/11/2026 pelo Image Gen integrado, em três chamadas independentes, sem referência visual fornecida. Prompts completos (documento histórico removido na limpeza B14). Ordem confirmada pela apresentação no chat: estudo 1, estudo 2, estudo 3.

Atualização 0.3: usuário selecionou o estudo 2; [registro de alcance](../records/G1-VISUAL-002.md). Estudos 1 e 3 ficam como alternativas históricas. [Desdobramento da direção escolhida](DIRECTION-2.md) e [novo mock de acompanhamento](direcao-2-acompanhamento.png), ainda propostos, com a imagem 2 como referência efetivamente anexada à geração.

Atualização 0.4: acompanhamento revisado sem ajustes solicitados; continuidade documental registrada em [G1-REVIEW-004](../records/G1-REVIEW-004.md). Revisão conjunta G1 (documento histórico removido na limpeza B14) reúne os comportamentos para decisão. Novos artefatos complementares, não opções de estilo:

| Fluxo | Imagem | Evidência visual observada |
|---|---|---|
| Histórico/reconsumo | [direcao-2-historico.png](direcao-2-historico.png) | Registro real separado de data informada, correção preservada e ciclos identificados |
| Lote misto | [direcao-2-lote.png](direcao-2-lote.png) | Três itens, grupos por mídia, estado de série ausente e confirmação bloqueada |
| Preview administrativo | [direcao-2-csv-preview.png](direcao-2-csv-preview.png) | Quatro registros, preservações, rejeição por referência inexistente e três válidos |

Prompts exatos 0.4 (documento histórico removido na limpeza B14). As duas imagens da direção escolhida foram anexadas como referência em cada geração. Cada mock é estático e foi inspecionado visualmente na saída do Image Gen. Contraste/reflow/teclado e funcionamento não foram testados. No CSV, “3 linhas válidas” inclui uma sem alteração, portanto não promete três gravações. No histórico, o evento antigo mostra o valor original e a correção fica em registro próprio. No lote, nenhum item foi efetivamente alterado. Telas permanecem PROPOSTAS.

Atualização 0.5: claro/escuro e PT-BR/EN entraram explicitamente no MVP. [Mock escuro/English](direcao-2-escuro-en.png), com prompt exato (documento histórico removido na limpeza B14), usa a tela clara de acompanhamento como referência visual anexada. Inspeção: identidade editorial e hierarquia preservadas, campos vazios e textos da interface em inglês; título, listas e nota pessoal permanecem em português intencionalmente. Os seletores no topo demonstram a combinação; a localização proposta no produto é Preferências, conforme [detalhamento](../architecture/PREFERENCES_I18N.md). Não foi medido contraste nem verificada tradução integral/funcionamento; aparência escura é proposta para revisão. G1 atual 0.5 (documento histórico removido na limpeza B14) sucede 0.4.

| Estudo | Artefato local | Leitura proposta |
|---|---|---|
| 1 | estudo-1.png (documento histórico removido na limpeza B14) | Biblioteca em tabela, estado/listas visíveis |
| 2 | [estudo-2.png](estudo-2.png) | Galeria editorial, reconhecimento por capa |
| 3 | estudo-3.png (documento histórico removido na limpeza B14) | Lista/detalhe com datas e anotação privada |

Inspeção visual realizada nas três saídas do gerador: cinco mídias representadas, anime como atributo de série, nenhum login/rating/exportação/comunidade visível. Cada imagem é um único estudo. As imagens foram copiadas para o projeto; não dependem do caminho de cache para revisão. Dimensão solicitada: 1440 × 1024; dimensão real conferida nos cabeçalhos PNG: 1487 × 1058 nas três imagens. Mantiveram-se os arquivos originais, sem esticar ou redimensionar.

Limitações observadas: estudos 1/2 não mostram fluxos de histórico, CSV ou lote; estudo 3 mostra apenas uma etapa de acompanhamento. Rótulos ilustrativos como “Planejando” na imagem 3 não são novos estados aprovados; tabela candidata em UX_PROPOSAL governa a proposta. Lista com nome semelhante a estado na imagem 1 continua lista livre e requer distinção textual. Capas geradas da imagem 2 são ilustração do estudo e não aprovam campo obrigatório, acervo real ou aquisição de capas. Azul/cinza, contraste e tamanho de texto ainda precisam medição numa interface futura.

São imagens estáticas: controles não funcionam, não há persistência ou navegação testadas. Não foi criado protótipo executável, servidor ou arquivo Figma. Jornadas e comportamentos completos: UX_PROPOSAL (documento histórico removido na limpeza B14). Seleção visual não equivale à aprovação do handoff inteiro nem autoriza implementação.
