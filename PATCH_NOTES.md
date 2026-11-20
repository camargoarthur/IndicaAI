## 0.13 — 20/11/2026

- **Seu acervo** ganha filtro rápido por estado. A seleção em lote funciona pela capa, e o retorno da obra ou do lote mantém o recorte de origem.
- **Histórico** carrega a data registrada no ciclo escolhido e exige uma ação explícita para apagá-la. **Preferências** protege as edições de rótulos da mesma mídia.
- **Adicionar obra** abre o Catálogo na mídia escolhida. A busca de filmes no TMDB aparece nos recortes pertinentes. Limpar filtros avançados preserva a busca e a mídia, e o filtro Anime aparece onde se aplica.
- Obras sem capa ganham uma apresentação legível. A manutenção informa o motivo por obra e permite retomar os downloads pendentes.
- A associação de tags restringe as opções à mídia da obra. O resultado do CSV mantém a linha e o motivo das rejeições.
- A restauração confirma o título e oferece acesso à obra. **Listas** mostra a contagem de obras e abre a renomeação sob demanda.
- O roteiro manual e o mapa de cobertura incluem a verificação do filtro rápido e das melhorias da auditoria.

## 0.12 — 20/11/2026

- A busca principal do Catálogo consulta as obras salvas no dispositivo e funciona offline.
- Os cartões mostram o estado da obra, inclusive quando ela está arquivada ou sem acompanhamento.
- A busca de filmes no TMDB exibe pôster, sinopse curta e as cinco pesquisas mais recentes.
- A importação de filmes e séries salva as novas obras como **Quero assistir**. É possível retomar a carga sem duplicar obras ou sobrescrever acompanhamentos existentes. Títulos sem correspondência segura ficam disponíveis no catálogo local.
- Pôsteres baixados ficam disponíveis offline por até 90 dias. O backup da coleção guarda os dados das obras e dos acompanhamentos, e as capas podem ser baixadas novamente.
- O catálogo pessoal passou a iniciar vazio. A atualização remove os exemplos de demonstração intactos e sem uso após criar uma cópia da base. Exemplos acompanhados ou modificados são preservados.
- Os campos de início e conclusão acompanham o estado escolhido. A confirmação aparece ao informar uma data futura ou uma conclusão anterior ao início. Datas ocultas pelo formulário podem ser consultadas e corrigidas no Histórico.
- O roteiro manual ganhou o caso TM41 para importação em lote. A validação completa passou nas oito etapas, com 81 testes de produto.

## 0.11 — 20/11/2026

- O Catálogo passou a buscar filmes no TMDB por título e ano opcional. A prévia mostra sinopse, pôster e os dados disponíveis antes de adicionar o filme.
- A importação guarda título, ano, duração e direção disponíveis. Ao selecionar novamente um filme já vinculado ao mesmo registro do TMDB, o aplicativo abre a obra existente com seu acompanhamento e histórico.
- O iniciador passou a ler a credencial do TMDB do `.env` local. As obras já salvas continuam acessíveis quando o serviço está indisponível.
- O roteiro manual ganhou o caso TM40. Um ensaio em base isolada confirmou a busca, a inclusão e a reabertura do mesmo filme.
- A carga ampla do catálogo para consulta offline e suas atualizações posteriores entraram no backlog como B15.

## 0.10 — 20/11/2026

- A validação passou a executar os testes de acordo com o impacto da mudança. O arquivo `Validar-IndicaAI.bat` permite rodar a validação completa por duplo clique e consultar os resultados por etapa.
- A suíte ganhou cobertura de listas, tags, filtros, cancelamento de importação CSV, correção de ciclos anteriores, estados inválidos, edições concorrentes e formulários das cinco mídias.
- Os 39 casos do roteiro manual passaram a indicar sua cobertura automatizada. As marcações no navegador passaram a ser separadas por versão.
- O verificador de pacote ganhou compatibilidade com Windows PowerShell 5.1. A validação passou a detectar testes omitidos mesmo quando o executor informa sucesso. A rodada completa passou nas oito etapas, com 61 testes de produto e 23 testes de contrato.
- O README e o índice da documentação foram reorganizados. A limpeza removeu 59 arquivos obsoletos e reposicionou 12 arquivos de arquitetura, qualidade, registros e scripts.
- `docs/BACKLOG.md` passou a concentrar as pendências do projeto. O README passou a apontar diretamente para essa lista.
- A pasta foi preparada para Git. As regras de exclusão protegem credenciais, `.env`, bancos locais e a lista pessoal de obras. As regras de fim de linha preservam a verificação do pacote no Windows.

## 0.9 — 12/11/2026

- O aplicativo passou a abrir por duplo clique em `Iniciar-IndicaAI.bat`. A janela do iniciador controla o servidor: fechar a janela encerra o aplicativo, e fechar apenas o navegador permite voltar à mesma sessão.
- O iniciador ganhou suporte ao Windows PowerShell 5.1 e a caminhos com espaços. A abertura, o encerramento e a reabertura com os mesmos dados foram verificados.
- Os 39 casos manuais foram reunidos em um roteiro HTML que abre diretamente no navegador. O roteiro permite registrar resultados e observações, acompanhar o progresso, retomar uma etapa e imprimir.
- Comandos, caminhos, IDs e exemplos de CSV ganharam botões de cópia. O roteiro confirma **Copiado** após o sucesso e orienta a seleção manual quando o navegador bloqueia a operação.
- Os testes passaram a ter iniciadores próprios por duplo clique para a base isolada e para a base restaurada. O roteiro passou a indicar qual arquivo abrir em cada etapa.
- Foram registradas as regras de manutenção da versão, do roteiro manual, das patch notes e do ponto de retomada do projeto.

## 0.8 — 09/11/2026

- O IndicaAI passou a permitir o acompanhamento local de jogos, livros, mangás, filmes e séries, com estados próprios por mídia e identificação de anime em filmes e séries.
- Cada obra ganhou acompanhamento com estado atual, rótulos renomeáveis, ciclos de consumo, datas opcionais, notas privadas e histórico de correções.
- A biblioteca ganhou listas que misturam mídias, filtros de inclusão e exclusão, alterações em lote e arquivamento com restauração.
- A administração passou a permitir criar grupos e tags, definir as mídias aplicáveis e importar CSV com prévia, cancelamento e aplicação parcial dos registros válidos.
- O aplicativo ganhou temas claro e escuro, interface em português e inglês, armazenamento local em SQLite, backup, restauração e pacote para Windows x64.
- Foram corrigidos o reenvio da inclusão de obras, a perda de rascunhos e da seleção de mídia, a identificação de tags nos filtros, a restauração sobre uma base danificada e a contagem exibida na importação CSV. A entrega passou em 47 testes de produto.
