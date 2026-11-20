# Patch notes — IndicaAI

Histórico único e contínuo. Versão funcional atual em [VERSION](VERSION): **0.10**. Próxima alteração funcional/correção de bug: **0.11**. Permanecer em `0.xx` até o usuário autorizar explicitamente lançar **1.0**. Versões não são números decimais: 0.10 sucede 0.9. Os rótulos anteriores não serão renumerados retroativamente.

Mudanças exclusivamente visuais e pedidos de documentação não incrementam a versão funcional; são registrados por data no bloco documental/visual abaixo. Mudanças mistas que também alterem comportamento funcional incrementam a versão. Ao manter estas notas, descrever o que mudou e avaliar o impacto no [roteiro manual único](docs/TEST_CASES.html). Políticas permanentes em [AGENTS](AGENTS.md).

## Documentação e visual — sem nova versão funcional

### 20/11/2026 — preparação local para Git

- `.gitignore` cobre configuração privada, credenciais, arquivos de banco e a lista pessoal `docs/Assistir.txt`. `.env` local reservado sem credenciais. `.gitattributes` preserva os bytes do checkout para a validação do manifesto.
- Escopo autorizado pelo usuário: preparação da pasta. Inicialização do Git, conexão com o remoto e publicação ficam sob controle do usuário.
- Roteiro manual, mapa TM e testes de produto avaliados. A mudança é de versionamento de arquivos e não afeta casos ou comportamento do aplicativo. Versão **0.10**.

### 15/11/2026 — B14: estrutura e limpeza concluídas

- Eliminados 48 arquivos obsoletos/redundantes, além dos dez auxiliares temporários anteriores, sem backup. Doze arquivos reorganizados em arquitetura, qualidade, registros e scripts; referências locais corrigidas. Detalhamento e critérios em [STRUCTURE](docs/STRUCTURE.md).
- README e documentos de orientação consolidados para o estado implementado; backlog único preservado e B14 removido das pendências. Aprovações, requisitos, dados locais, licenças e evidências finais mantidos. Nenhum upload realizado.
- Roteiro avaliado: casos, IDs, fontes e fluxo de início mantidos. A organização não altera funcionalidade; versão permanece **0.10**.
- Encerramento: removido também o ZIP do SDK instalado, totalizando **59 arquivos antigos excluídos**, sem backup; **12 arquivos movidos**. Full passou **8/8**, com **61/61 testes de produto**, **23/23 contratos**, pacote/iniciador e documentos. [Resumo](docs/evidence/B14-validation-full.json). Servidor de teste encerrado; revisão documental final pelo perfil Docs.


### 15/11/2026 — organização inicial da documentação

- README reescrito para apresentar uso, testes, desenvolvimento e documentação atual, retirando instruções iniciais obsoletas. Criado índice de documentação em `docs/README.md`.
- Removidos dez arquivos temporários de construção e edição de guias em `.tools`, já substituídos pelo HTML canônico e testes atuais. Dados, aplicativo e ferramentas efetivamente usadas permanecem. A auditoria B14 continua em andamento.
- Roteiro manual avaliado: sem alteração de casos ou caminhos operacionais. Versão funcional permanece 0.10.

### 15/11/2026 — auditoria de organização em andamento

- Corrigida no CODEX_START a orientação sobre backlog: a lista única fica em `docs/BACKLOG.md` e o README aponta para ela. Auditoria de B14 iniciada e ainda não concluída; nenhuma exclusão ou movimentação realizada.
- Roteiro de testes avaliado: nenhuma mudança de funcionalidade ou caminho de tela nesta correção documental. Versão funcional permanece 0.10.

### 15/11/2026 — backlog com fonte única

- Por instrução direta do usuário, `docs/BACKLOG.md` passou a ser a única lista de pendências; o README agora explica o propósito do backlog e direciona ao documento, sem repetir IDs ou itens.
- Regra permanente e introdução do backlog atualizadas. O registro anterior de espelhamento abaixo permanece como histórico e foi substituído por esta decisão. Nenhuma funcionalidade ou caso manual mudou; `VERSION` permanece **0.10**.

### 15/11/2026 — validação dos itens manuais do backlog

- B12–B14, incluídos manualmente pelo usuário em `docs/BACKLOG.md`, foram conferidos e espelhados no README com os mesmos IDs. B12 depende da definição de avaliações (B10); B13 exige métricas efetivamente registradas e tem relação com B08; B14 inclui documentos e toda a estrutura de arquivos para futura publicação no GitHub, sem autorizar upload agora.
- O texto foi esclarecido preservando a intenção original e os itens como pendentes, sem prioridade ou implementação. O roteiro manual de casos de teste não muda: nenhuma funcionalidade ou caminho de tela foi alterado. Versão funcional permanece **0.10**.

### 12/11/2026 — abertura do roteiro por duplo clique

- Etapa 00 orienta abrir `Iniciar-Testes-IndicaAI.bat` pelo Explorador de Arquivos, sem abrir terminal ou digitar o comando de início. O arquivo auxiliar do roteiro encapsula os mesmos parâmetros já documentados e usa `.local/teste-manual-01`, separada da coleção pessoal; o iniciador cotidiano permanece `Iniciar-IndicaAI.bat`.
- Regra destacada: após iniciar com sucesso, janela do `.bat` aberta mantém o servidor; fechar a janela encerra; fechar somente o navegador não encerra. TM01 e TM37 ajustados para abertura/reabertura por duplo clique.
- TM39 usa `Iniciar-Testes-Restaurados-IndicaAI.bat` após o restore, na porta 5274; ele recusa a abertura se o banco restaurado não existir. Os comandos de manutenção continuam exclusivos da etapa opcional de backup.
- Impacto no roteiro: preparação e instruções operacionais atualizadas, preservando os 39 IDs, campos e marcações locais. Correção documental com arquivos auxiliares do roteiro; implementação do servidor e seu controle de vida permanecem os existentes da **0.9**, sem nova versão funcional.
- Verificação estática do HTML, instruções e encaminhamento dos arquivos auxiliares; integridade documental conferida. Servidor não iniciado e casos do produto não executados nesta rodada.

### 09/11/2026 — botões de copiar no roteiro

- Botões junto ao comando inicial, caminho da pasta, dois modelos CSV, comandos de backup e quatro campos de IDs; confirmação “Copiado” após sucesso.
- Campos vazios pedem preenchimento. A cópia usa a API do navegador e tentativa de compatibilidade para arquivo local; se ambas forem negadas, seleciona o trecho e orienta a cópia manual, sem informar sucesso falso.
- Regra de oferecer botão em instruções de cópia registrada na memória permanente. Preservados os 39 casos, suas marcações/anotações e a versão funcional **0.9**.
- Conferidos no navegador comando, caminho, ambos os CSVs, ID e backup pela comparação do conteúdo copiado; viewport 320 sem overflow do documento. Preview temporário encerrado ao finalizar; aplicativo não foi iniciado.

### 09/11/2026 — roteiro visual para abrir no navegador

- Roteiro manual migrado para **HTML canônico**, seguindo a identidade editorial aprovada: papel claro, títulos serifados, terracota e navegação por etapas.
- Preservados TM01–TM39, incluindo caminhos das telas, instruções, resultados e sequência natural. O Markdown passou a ser apenas um apontador para evitar duas versões editáveis dos testes.
- Adicionados controles de resultado/observação, progresso e retomada local no navegador, campos para registrar IDs, seletor de etapa em mobile e estilos de impressão. Isso pertence ao documento; o comportamento do aplicativo não mudou.
- HTML com estilos, scripts e fontes incorporados para abrir sem servidor/CDN. Limitações do armazenamento local são informadas na própria página. QA visual e de interação em [evidências](docs/evidence/test-guide/design-qa.md); marcações de conferência removidas antes da entrega.
- Links e memória atualizados para o HTML; versão funcional mantida em **0.9**. O servidor do aplicativo permaneceu desligado; preview documental temporário encerrado após o QA.

### 09/11/2026 — roteiro de testes e regras de manutenção

- Criado [roteiro único de testes manuais](docs/TEST_CASES.html), com 37 casos principais e dois opcionais de backup/restauração, caminhos das telas, execução e resultado esperado. Casos organizados por percurso para aproveitar os mesmos dados e reduzir retornos entre setores. Documento preparado, ainda sem execução pelo usuário.
- Registradas regras de versão `0.xx`, exceções visuais/documentais, atualização do roteiro, patch notes contínuos e checkpoint de retomada.
- Criada seção de backlog no README, sincronizada com [BACKLOG](docs/BACKLOG.md), incluindo ampliação/consolidação futura dos testes automatizados. A suíte já existente não foi apagada nem apresentada como inexistente.
- Nenhum comportamento do aplicativo foi alterado; versão permanece **0.9** e servidor permanece desligado.

## 0.10 — 12/11/2026 — testes e validação de entregas

- **B11 entregue:** ampliada a suíte existente com cenários do roteiro e consolidada a automação. Item removido dos backlogs do README e BACKLOG; atualização dos testes passa a obrigação permanente a cada mudança/nova adição.
- Executor por impacto com perfis Docs, Visual, Functional, Module e Full; Auto usa maior impacto e amplia para Full em caso desconhecido. Logs completos em disco, resumo curto/JSON, código de falha e rejeição de relatórios vazios ou com omissões. `Validar-IndicaAI.bat` executa Full por duplo clique.
- Testes complementares verificam listas, associações/tags, filtros, cancelamento de CSV, correção de ciclo antigo, estados inválidos, concorrência HTTP e formulários das quatro outras mídias. JS cobre texto apagado e seleção/desmarcação de checkboxes.
- Roteiro avaliado: preservados os 39 IDs/casos; incluída orientação da validação automatizada e mapa técnico por TM, com cobertura parcial e limites manuais explícitos. Versão exibida/chave de marcações atualizadas para 0.10, sem transportar aprovações da 0.9.
- Sem alteração nas regras ou telas do aplicativo; a nova funcionalidade é a rotina de validação. Resultados e limites reais ficam em [VALIDATION-0.10](docs/evidence/VALIDATION-0.10.md).
- Corrigida compatibilidade do verificador de pacote com Windows PowerShell 5.1 e isolamento dos módulos nativos quando o executor é chamado via Node/PowerShell 7. O teste operacional verifica acesso CIM antes de iniciar processos; seus logs agora são exclusivos por rodada.
- Verificação: **61/61 xUnit**, **23/23 contratos Node**, modelo JS, build/publicação, 22 recursos do pacote, ciclo do iniciador e documentos. Full passou 8/8 etapas; perfil Module/Preferences passou 30/30 testes de produto. Revisão independente apontou omissões Node que poderiam passar com exit 0; corrigido com regressão RED→GREEN. Servidor do ensaio encerrado; dados pessoais não usados.

## 0.9 — 09/11/2026 — servidor controlado pela janela

**Alterado**

- Entrada por duplo clique em `Iniciar-IndicaAI.bat` na raiz; `Start-IndicaAI.bat` no pacote distribuído.
- Servidor vinculado ao host do iniciador por Job Object privado. Fechar a janela encerra o servidor; fechar somente o navegador não encerra.
- Script compatível com Windows PowerShell 5.1 e caminhos de dados com espaços. Instruções de operação e verificações do pacote atualizadas.

**Verificação registrada**

- Iniciador direto, `.bat` do pacote e `.bat` da raiz: HTTP 200, encerramento abrupto do host seguido da saída do servidor/HTTP indisponível e reabertura dos mesmos dados. Processos encerrados ao entregar. [Evidência](docs/quality/LAUNCHER-0.9.md).
- Revisão estática específica sem achados acionáveis; 22 arquivos do pacote conferidos por hash. Não se alegou novo ensaio visual ou execução completa da suíte do produto nesta alteração operacional.

**Roteiro manual:** TM01 e TM35–TM37, acrescentados agora retrospectivamente ao documento único.

## 0.8 — 09/11/2026 — candidato local do MVP

**Adicionado**

- Acompanhamento local sem login para jogos, livros, mangás, filmes e séries; Anime como atributo de filme/série.
- Estados compatíveis e rótulos renomeáveis, ciclos de consumo, datas opcionais e correções com histórico, notas privadas e listas mistas.
- Administração de grupos/tags, aplicabilidade por mídia e CSV administrativo MERGE com prévia, cancelamento, preservação de campos e resultado parcial.
- Filtros contextuais AND/OR/NOT, alterações em lote, arquivamento/restauração pessoal, claro/escuro e PT-BR/EN com recursos extensíveis.
- Persistência SQLite, backups verificados, proteção pré-migração, restauração e recuperação operacional; pacote Windows x64 autocontido.

**Corrigido no fechamento**

- Reenvio da inclusão, perda de rascunhos e recorte de mídia, identificação/aplicabilidade das tags nos filtros, restauração sobre ativo danificado e texto de contagem do botão CSV. [Revisão final](docs/quality/FINAL_REVIEW-0.8.md).

**Verificação registrada**

- 47/47 testes do produto, teste JavaScript de rascunhos, QA de navegador/visual e backup/restauração em cópia. Limites reais em [QA-REPORT](docs/quality/QA-REPORT.md). Essas evidências históricas não significam execução pelo usuário do novo roteiro manual.

Registros 0.1–0.7 anteriores são rodadas de requisitos, design e plano, preservadas nos documentos de decisão; não foram transformadas em releases executáveis retroativas. A aprovação G4/lançamento 1.0 não foi presumida.
