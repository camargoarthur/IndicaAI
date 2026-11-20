# Direção 2 — desdobramento de UX 0.3

Atualização 0.6: a UX 0.5 consolidada foi aprovada em [G0-G1-APPROVED-006](../records/G0-G1-APPROVED-006.md), incluindo os desdobramentos e preferências de aparência/idioma. As propostas e pendências 0.3–0.5 abaixo ficam como histórico. Mecânica técnica e execução seguem nos gates posteriores.

Data: 08/11/2026. Direção visual ESCOLHIDA pelo usuário; aplicação aos demais fluxos PROPOSTA. [Evidência](../records/G1-VISUAL-002.md). Complementa UX_PROPOSAL (documento histórico removido na limpeza B14), sem mudar requisitos ou regras ainda pendentes.

Continuidade 0.4: acompanhamento revisado sem ajuste solicitado, conforme [registro](../records/G1-REVIEW-004.md). Novos mocks de [histórico](direcao-2-historico.png), [lote](direcao-2-lote.png) e [preview CSV](direcao-2-csv-preview.png) concretizam fluxos antes apenas textuais. Revisão conjunta G1 (documento histórico removido na limpeza B14) é a referência atual para aprovação; demais seções 0.3 abaixo permanecem como base histórica.

Atualização 0.5: os requisitos aprovados de tema/idioma ampliam a direção editorial às quatro combinações claro/escuro e PT-BR/EN. A revisão atual é G1 0.5 (documento histórico removido na limpeza B14); [mock escuro/English](direcao-2-escuro-en.png) e [contrato candidato de preferências](../architecture/PREFERENCES_I18N.md) preservam dados e hierarquia. Valores semânticos de cor por tema são propostos, sem aprovar tokens/contraste por imagem. Não reabrir a escolha da direção 2.

## Referência e novo artefato

Referência escolhida: [biblioteca editorial em galeria](estudo-2.png). Novo artefato: [acompanhamento de uma obra](direcao-2-acompanhamento.png), gerado usando a imagem escolhida como referência visual real. Não é uma quarta alternativa nem um protótipo executável. O desdobramento está aberto à revisão.

Preservar da referência: fundo marfim, texto escuro, títulos editoriais com serifa, controles em terracota, navegação superior e obras em galeria. Dimensões, fontes específicas, cores exatas, contraste e comportamento responsivo ainda são detalhamento proposto, sem tokens medidos ou design system implementado.

## Biblioteca e galeria

Título “Seu acervo”, busca, ação “Adicionar obra” e recortes das cinco mídias mantêm a hierarquia escolhida. Cada item apresenta título, mídia real e um estado pessoal; anime é atributo adicional apenas de filme/série. Abrir item leva ao detalhe da obra e conserva busca/filtros ao voltar.

Capas continuam opcionais. Sem capa cadastrada, propor apresentação editorial centrada em título, mídia e estado, conservando a área clicável e a ordem de leitura. Não buscar imagem externa, não inventar capa como dado real e não bloquear acompanhamento por falta de imagem. A galeria escolhida é viável com metadados mínimos; o contrato de armazenamento de imagem, se necessário, permanece em G2.

Filtros avançados abrem área dedicada com modo Todos/Qualquer e exclusões explícitas. Exibir resumo textual dos critérios junto ao resultado. Seleção em lote é um modo explícito acionado por “Selecionar obras”; caixas e barra de alcance aparecem nesse modo, evitando ocupar a tela principal permanentemente. Ao sair do modo, resolver seleção pendente. Sem ações dependentes apenas de hover.

## Obra e acompanhamento

| Região | Proposta no desdobramento | Regra preservada |
|---|---|---|
| Cabeçalho | Navegação e retorno à biblioteca | Sem login nem troca de identidade |
| Coluna de referência | Capa opcional, mídia e dados globais somente leitura | Sem editor completo de catálogo |
| Acompanhamento | Estado obrigatório, datas opcionais, listas e anotação privada | Um estado compatível, listas independentes, sem ratings |
| Datas | Campos vazios e ajuda sobre referência na inclusão | Fallback não é dado informado nem conclusão automática |
| Ações | Salvar como principal, histórico como apoio, novo consumo separado | Reconsumo explícito e histórico preservado |
| Arquivo | Ação textual separada dos dados globais | Arquivar acompanhamento pessoal reversível |

O mock mostra edição de um livro já incluído, estado Lendo, duas listas e uma nota fictícia. Não mostra estado obrigatório vazio na inclusão; essa variação continua descrita no handoff. Remover um chip de lista altera apenas a seleção em edição; persistir junto ao formulário com Salvar, sem mudar estado. Cancelar/voltar com alterações apresenta aviso. Abrir Histórico a partir de formulário alterado resolve essas alterações antes de navegar.

Início/conclusão permanecem vazios quando não informados. O texto “referência na inclusão” explica ambos; a ajuda adicional explicita que referência de conclusão não significa conclusão. Instante de registro aparece no histórico, não no lugar de uma data informada. “Registrar novo consumo” abre revisão explícita do novo ciclo, sem duplicar obra ou apagar anterior.

## Aplicação aos demais fluxos

| Fluxo | Aplicação proposta da linguagem | Informação que deve permanecer explícita |
|---|---|---|
| Histórico | Título editorial e eventos em sequência com divisórias discretas | Registro real separado de data informada, ciclo e correções |
| Listas mistas | Galeria com o mesmo padrão da biblioteca, contexto da lista no título | Retirar da lista não altera estado/arquivo |
| Lote | Área de revisão com grupos por mídia, tipografia e controles do mesmo sistema | Alcance selecionado, destino de cada grupo e confirmação integral |
| Arquivadas | Galeria identificada como arquivo e ação Restaurar | Estado/dados preservados, sem aparência de exclusão definitiva |
| Tags | Formulário administrativo simples, campos explícitos | Grupo, mídias aplicáveis, incompatibilidade e alcance global |
| CSV | Tabela de diferenças e resultados com separadores leves | Antes/depois/preservados, rejeições e número de válidas; não forçar preview em galeria |

O uso de tabela administrativa continua adequado dentro da direção editorial. Escolher galeria para obras não obriga transformar dados de importação em cartões ou acrescentar filtros sem função.

## Estados e avaliação futura

Em largura estreita, propor navegação adaptada, galeria com menos colunas e detalhe em uma coluna com dados globais antes do acompanhamento. Capa não deve empurrar estado/salvar para distância desnecessária. Preservar rótulos, foco, ordem de leitura e ações de seleção em teclado.

Inspeção visual do novo artefato: identidade editorial mantida, dados globais/pessoais separados, datas vazias com ajuda, listas separadas do estado e anotação privada visível. Não há login, rating, exportação ou comunidade. Não se verificaram contraste, reflow, teclado, persistência ou navegação por ser imagem estática. O histórico, lote e CSV têm especificação textual, mas ainda não têm novos mocks nesta revisão.

## O que continua em revisão G1

A direção visual já foi escolhida. A revisão funcional continua concentrada em três grupos concretos do handoff: conjunto/semântica de estados; ciclos, datas e correções; gramática de filtros e confirmação integral de lote por mídia. O desdobramento visual e esses comportamentos precisam de revisão de G1; nenhuma nova pergunta bloqueia a preparação documental.
