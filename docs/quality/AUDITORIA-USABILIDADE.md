# Auditoria de usabilidade — 20/11/2026

A inspeção usou o código com o filtro rápido integrado, as capturas do usuário e o Edge numa cópia da base de ensaio. O menu de Filmes foi observado em desktop claro e tela estreita escura. Catálogo em Jogos, Listas, Arquivadas, Preferências, Tags, Associações e Importar CSV foram abertas nesse ambiente. Os cenários de perda de dados abaixo foram rastreados no código, sem reproduzir alterações na coleção pessoal.

| Prioridade | Caminho e problema | Mudança proposta |
|---|---|---|
| Alta | Histórico → corrigir data. O campo abre vazio mesmo para uma data existente. Salvar o vazio pode apagar a data do ciclo escolhido. | Carregar a data do ciclo e do campo selecionados. Oferecer uma ação explícita para limpar. |
| Alta | Preferências → rótulos. Ao editar dois rótulos e salvar um deles, o redirecionamento pode descartar a outra edição. Cada rótulo tem seu próprio formulário. | Salvar as alterações da mídia juntas ou sinalizar a edição pendente antes de sair. |
| Alta | Biblioteca → Jogos → Adicionar obra. O link perde a mídia. No Catálogo, o cabeçalho oferece TMDB até em Jogos. | Levar a mídia para o catálogo e oferecer a pesquisa externa de filmes em Filmes ou Todas. |
| Alta | Lista ou catálogo filtrado → obra → voltar. O detalhe volta sempre à Biblioteca. A revisão em lote também mostra biblioteca ativa e volta ao acervo geral ao salvar ou cancelar. | Preservar lista, mídia, busca, filtros e página no retorno. |
| Alta | Biblioteca → Selecionar obras. Só o checkbox seleciona. Clicar na capa pode abrir o detalhe. A seleção geral também pode continuar marcada após desmarcar um item. | Usar toda a capa para seleção nesse modo e derivar a marcação geral dos itens, inclusive o estado parcial. |
| Média | Biblioteca e Catálogo → Filtros. Anime ocupa a primeira linha fixa, inclusive fora das mídias aplicáveis. Limpar filtros avançados também descarta busca e mídia. | Agrupar Anime com as tags de filmes e séries e limitar a limpeza aos critérios correspondentes. |
| Média | Galeria → capa ausente. Das 78 obras da carga registrada, 9 ficaram sem vínculo e 48 tinham vínculo, mas sem arquivo de imagem. O lote não registra a causa individual. O cartão reserva a área inteira mesmo quando vazia. | Registrar a causa por obra, permitir retomar downloads pendentes e oferecer uma apresentação legível na falta de imagem. |
| Média | Administração → Associar tags. Os seletores oferecem tags incompatíveis com a mídia e permitem tentar salvar com opções vazias. | Filtrar as tags pela obra escolhida e indicar como cadastrar uma tag aplicável. |
| Média | Arquivadas → Restaurar. A obra desaparece da grade sem mensagem específica nem caminho direto à obra restaurada. | Confirmar a restauração pelo título e oferecer acesso à obra. |
| Média | Administração → CSV → Resultado. A prévia informa a linha física e detalhes do erro que deixam de aparecer no resultado final. | Manter a linha e o motivo detalhado, com acesso às rejeições. |
| Baixa | Listas. Cada lista exibe um formulário de renomeação aberto e não indica quantas obras contém. | Priorizar abrir a lista, exibir a contagem e revelar a edição ao escolher Renomear. |

O filtro rápido já permite voltar a Todos num recorte vazio. Para as próximas correções, começar por Histórico e Preferências pelo risco de perda de informação, depois navegação e seleção por afetarem a consulta diária. A inspeção não incluiu teste com leitores de tela nem sessões com usuários externos.

## Correções integradas à 0.13

O usuário autorizou aplicar os 11 pontos em D39. Cada linha abaixo liga o ajuste entregue ao cenário de aceitação do roteiro.

| Área | Comportamento entregue | Cenário |
|---|---|---|
| Histórico | A data selecionada aparece preenchida. A remoção exige **Limpar data** e preserva o registro anterior no histórico. | [TM21](../TEST_CASES.html#TM21) |
| Preferências | Os rótulos de cada mídia são gravados juntos. Um erro de validação mantém os valores digitados. | [TM03](../TEST_CASES.html#TM03) |
| Adicionar obra | O Catálogo abre na mídia escolhida. A pesquisa de filmes no TMDB fica em Filmes e Todas. | [TM43](../TEST_CASES.html#TM43) |
| Navegação | O detalhe e a revisão em lote mantêm a origem. Se a última página esvaziar, o resultado exibe a última página disponível. | [TM43](../TEST_CASES.html#TM43) |
| Seleção | A capa ativa o checkbox, e a seleção geral acompanha os estados vazio, parcial e completo. | [TM30](../TEST_CASES.html#TM30) |
| Filtros | Anime aparece nas tags de filmes e séries. Limpar preserva a busca, a mídia e o filtro rápido. | [TM29](../TEST_CASES.html#TM29), [TM42](../TEST_CASES.html#TM42) |
| Capas | A galeria apresenta **Capa indisponível**. A manutenção informa o motivo por linha e permite retomar os downloads pendentes. | [TM41](../TEST_CASES.html#TM41) |
| Associações | As tags exibidas são aplicáveis à mídia escolhida, com acesso ao cadastro se nenhuma estiver disponível. | [TM09](../TEST_CASES.html#TM09), [TM10](../TEST_CASES.html#TM10) |
| CSV | O resultado conserva linha e motivo e oferece uma visualização das rejeições. | [TM12](../TEST_CASES.html#TM12) |
| Arquivadas | A confirmação traz o título e abre o acompanhamento restaurado. | [TM22](../TEST_CASES.html#TM22) |
| Listas | As obras ativas são contadas, a abertura é a ação principal e a renomeação fica recolhida. | [TM05](../TEST_CASES.html#TM05) |
