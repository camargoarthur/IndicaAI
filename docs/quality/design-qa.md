# QA visual — IndicaAI 0.8

**Adendo documental, 09/11/2026:** rebranding do [roteiro manual HTML](../TEST_CASES.html) conferido no navegador; [QA específico e evidências](../evidence/test-guide/design-qa.md), `final result: passed`. O produto permanece em 0.9. O relatório original do aplicativo abaixo fica preservado; não foi substituído pelo QA do documento.

final result: passed

Comparação realizada no navegador interno do Codex com aplicação real em `http://127.0.0.1:5273`. Fontes e capas locais carregadas; Source Sans 3 no corpo (18 px/27 px), Libre Baskerville nos títulos. Não foi avaliada apenas a existência dos arquivos: fontes computadas, carregamento das imagens e capturas renderizadas foram consultados.

## Alvos e normalização

Fonte visual: `docs/design/estudo-2.png`, `direcao-2-acompanhamento.png`, `direcao-2-historico.png`, `direcao-2-lote.png`, `direcao-2-csv-preview.png`, `direcao-2-escuro-en.png`. Cada referência mede 1487×1058. Implementação: `docs/evidence/ui/F-library.jpg`, `F-detail.jpg`, `F-history.jpg`, `F-batch.jpg`, `F-csv.jpg`, `F-detail-dark-en.jpg`.

Viewport CSS 1487×1058, DPR 1. O wrapper nativo retornou cinco capturas 1472×1047 e uma 1487×1058; comparação normalizou mecanicamente cada captura ao canvas 1487×1058 (aproximadamente 1%, sem redesenhar conteúdo). As comparações `docs/evidence/ui/compare-{library,detail,history,batch,csv,dark-en}.png` contêm referência à esquerda e implementação à direita, 2994×1058 com intervalo de 20 px. Todas foram abertas em entrada combinada; a prévia CSV também foi aberta no recorte combinado `compare-csv-focus.png` (1980×650), permitindo ler tabela/badges/ações. O recorte foi extraído das imagens reais, sem reconstruir a UI.

Estados alinhados: biblioteca de cinco mídias em claro/PT-BR; livro em Lendo com datas opcionais vazias; histórico do mesmo livro; lote misto jogo/livro/série com último grupo sem novo estado; prévia CSV com inserir/atualizar/sem alteração/rejeitar; acompanhamento de livro Reading em escuro/EN. Conteúdo demonstrativo variável não foi confundido com falha visual: IDs UUID em vez de slugs, um evento em vez de três, listas ainda vazias, nota vazia e datas reais do QA. A segunda prévia usa Mundos imaginários e Aventura épica e descoberta para conservar os mesmos quatro tipos de resultado após a primeira importação. Anotação de revisão/data no topo do mock é metadado do design e não faz parte do produto.

## Comparações e correções

| Iteração | Achado | Correção e evidência posterior |
|---|---|---|
| Inicial | P1: tipografia genérica, seletor de mídia em dropdown e galeria com ordem/seleção divergentes | Fontes locais, abas de mídia, ordem das cinco mídias e seleção explícita; `compare-library.png` |
| Inicial | P1: histórico sem contexto lateral; lote em blocos empilhados | Contexto lateral e tabela de histórico; lote em tabela por mídia com aviso e confirmação bloqueada; `compare-history.png`, `compare-batch.png` |
| Inicial | P2: prévia excessivamente fragmentada | Quatro colunas, diferenças/preservações e badges semânticos; `compare-csv.png` |
| Final | P2: paleta escura fria divergia dos tons quentes escolhidos | Fundo #211b19 e superfície #2c2420; `compare-dark-en.png`, `F-mobile-detail.jpg` |
| Final | P2: botão Add a work ultrapassava largura 320 | Cabeçalho quebra antes de 380; largura útil 305, scrollWidth 305, viewport 320; `F-mobile-320.jpg` |
| Final | P2: altura da prévia empurrava ação principal para fora da primeira tela | Título/ajuda/tabela compactados; base do botão em y=1014 dentro de 1058; `compare-csv.png` e recorte, sem perda de linhas/erros |
| Final | P1 funcional: texto literal de parâmetro no botão CSV | Plurais reais nos dois idiomas, teste RED→GREEN; prévia mostra “Importar 3 registros válidos” |

Após os ajustes, a comparação final não encontrou P0/P1/P2 acionável. Evidências intermediárias anteriores ficam preservadas como histórico; `F-*` e as comparações listadas são a base final. A captura escura anterior de galeria cortada foi descartada como evidência de aprovação e substituída por acompanhamento escuro no mesmo estado da referência.

## Superfícies avaliadas

- **Tipografia:** hierarquia serifada editorial, títulos sem truncamento, corpo e labels sans, fallback disponível, pesos e quebras legíveis. O sans tem traço mais estreito que o mock; diferença óptica residual P3. A interface operacional tem metadados pequenos auxiliares e densidade menor em algumas áreas do que o mock estático; títulos, decisões e ações continuam claros.
- **Ritmo/layout:** margens coerentes, galeria com cinco colunas desktop e duas estreitas, capas 2:3, formulários em duas colunas de datas e reflow em uma; tabelas mantêm semântica e rolagem própria quando necessária. Sidebar/dados e ações do detalhe têm disposição operacional ligeiramente distinta, preservando hierarquia. Sem clipping de controles persistentes nos estados consultados.
- **Cores:** claro em papel quente e terracota; escuro quente com texto creme; badges/error não dependem só de cor. Bordas de controles reforçadas em ambos os temas. Contrastes calculados no relatório técnico, foco visível de 3 px observado em `F-keyboard-focus.jpg`.
- **Imagens:** cinco capas originais em bitmap 1024×1536 mantêm assunto, paleta e enquadramento do estudo; não são cópias pixel a pixel. Sem placeholders, redesenho CSS ou glifos substituindo imagens. Ícones locais de biblioteca, fontes locais e licenças inventariadas.
- **Conteúdo:** estados compatíveis por mídia, anime como atributo, privacidade da nota, fallback sem alegar conclusão, lote integral e CSV parcial explícitos. PT-BR e EN verificados; dados cadastrados permanecem no idioma original. Campos nativos de data usam o navegador/SO, sem promessa de máscara textual fixa.

## Interação e responsividade

Inclusão, alteração, novo consumo, preferências, seleção/lote, filtros e CSV foram usados no navegador; a importação real terminou com três processadas/uma rejeitada. Ensaio com duas abas exigiu revisão de rascunho após mudança concorrente e não sobrescreveu nota automaticamente. Teclado mostrou foco visível no menu, nomes/labels estão presentes na árvore acessível; console consultado sem erros. Isso não substitui auditoria formal de tecnologia assistiva.

Capturas estreitas: `F-mobile-detail.jpg`, viewport 390×844 (captura nativa 375×812); `F-mobile-320.jpg`, 320×844 (captura nativa 305×804). Após correção, sem excesso horizontal no documento em ambas as telas consultadas. As diferenças de canvas nativo foram registradas, sem interpretar a borda/scrollbar como layout incorreto. Override de viewport removido ao final e preview mantido aberto.

## Checklist e acabamento futuro

- [x] Referência e implementação abertas juntas; comparações completas e recorte legível.
- [x] P0/P1/P2 corrigidos, recapturados e comparados.
- [x] Fontes, cores, imagens, conteúdo e responsividade explicitamente avaliados.
- [x] Preview local aberto, sem publicação externa.

P3 opcional: aproximar o peso óptico do sans e o acabamento do seletor nativo; estender o sublinhado do menu principal a todas as subrotas (o título e o link de retorno já identificam o contexto). Não bloqueiam leitura ou operação. Não há pergunta de design pendente para entregar este candidato local.
