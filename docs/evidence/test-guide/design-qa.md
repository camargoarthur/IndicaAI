# QA visual do roteiro HTML — 09/11/2026

final result: passed

## Ajuste de cópia — 09/11/2026

Pedido do usuário sobre a etapa 0 aplicado aos demais trechos de cópia: nove botões (quatro blocos, um caminho e quatro IDs). Comando, caminho, ambos os CSVs, ID preenchido e backup comparados com a área de transferência no navegador, após aguardar o estado de sucesso; conteúdo correto, incluindo quebras de linha dos CSVs. Campo vazio exibiu instrução sem alegar cópia. Console consultado sem erros. Em 320 px, scrollWidth 305 e botão acessível. [Desktop](copy-desktop.jpg), mobile (documento histórico removido na limpeza B14), [comparação da área indicada](copy-comparison.png). A mudança preserva as definições dos 39 casos e a identidade visual. Não houve simulação forçada de todas as políticas de clipboard; fallback de seleção orienta o usuário se a cópia não for permitida. A API moderna e suas restrições são documentadas em [MDN — writeText](https://developer.mozilla.org/en-US/docs/Web/API/Clipboard/writeText); a tentativa de compatibilidade usa o recurso legado [execCommand](https://developer.mozilla.org/en-US/docs/Web/API/Document/execCommand), somente como alternativa.

## Direção e escopo

Pedido atual: melhorar a apresentação do documento manual no navegador, sem decoração excessiva, preservando testes e percurso. A referência é a direção 2 já aprovada do IndicaAI: [histórico](../../design/direcao-2-historico.png), tokens e fontes do aplicativo. Trata-se de adaptação desse sistema visual para documentação: conteúdo, rotas, densidade e hierarquia são próprios do roteiro. A comparação avalia identidade, leitura e interação; não promete reproduzir a tela de histórico como documento.

HTML canônico: [TEST_CASES.html](../../TEST_CASES.html). Arquivo único com CSS, JavaScript, duas fontes e licenças incorporados; sem CDN, instalação ou dependência do servidor do aplicativo. O Markdown anterior virou apontador, sem segunda cópia dos casos.

## Evidência visual

Fonte 1487×1058; viewport CSS 1487×1058, DPR 1. Captura do navegador 1472×1047, normalizada mecanicamente a 1487×1058 no canvas de comparação (aproximadamente 1%). [Comparação completa](compare-identity.png), 2994×1058, e [recorte de tipografia](compare-type.png), 1800×430, abertos juntos para julgar a identidade real, sem concluir por nomes de arquivos. [Desktop final](desktop-final.jpg), [preparação](preparation-final.jpg), [mobile final 320](mobile-320-final.jpg). Capturas mobile anteriores e primeiro desktop preservados como intermediários.

## Iterações

| Achado | Tratamento | Evidência posterior |
|---|---|---|
| P2: corpo auxiliar dos casos pequeno para leitura prolongada | Base de rem ajustada para 18 px; instruções a 17,1 px com entrelinha confortável | Desktop final e comparação de tipografia |
| P2: etapas em faixa horizontal cortavam nomes na tela estreita | Navegação móvel por select nativo com todas as etapas; conteúdo em uma coluna | Mobile final 320, select usado no navegador |
| Rótulos auxiliares continham marcadores Markdown | Removidos os marcadores do nome acessível dos campos de IDs | Campo localizado pelo nome e preenchido/esvaziado pela UI |

Após as correções, não restou P0/P1/P2 acionável nos estados conferidos. Não houve geração de imagem ou ícone: esta composição editorial não pede ilustração; reutiliza fontes e linguagem visual existentes. P3 opcional: acabamento nativo dos selects varia entre navegadores.

## Superfícies avaliadas

- **Tipografia:** Libre Baskerville para marca/títulos; Source Sans 3 no corpo; fontes carregadas de dados incorporados. Pesos, quebras e hierarquia conferidos em desktop e mobile. Instruções e resultado ficam separados e legíveis; caminhos recebem faixa própria.
- **Layout:** navegação por etapa, uma seção de cada vez, blocos de teste sem tabela gigante. Colunas execução/esperado em desktop e empilhamento em mobile. Bordas discretas e ritmo editorial consistente com a referência.
- **Cores:** papel quente #faf5ed, texto #202132 e terracota #a4472a; estados também têm texto, não dependem só de cor. Foco azul de 3 px observado por teclado. Bordas dos controles #9a8173.
- **Imagens/recursos:** sem placeholders ou arte CSS substituindo imagens; o documento é tipográfico. Fontes e suas licenças locais preservadas no próprio HTML.
- **Conteúdo:** comparação das 39 definições renderizadas contra o Markdown original: IDs, títulos, caminhos, execução e esperado preservados. Nenhum caso foi marcado aprovado por migração. Rotas do app são instruções, não chamadas automáticas.
- **Responsividade:** 390 e 320×844 conferidos; no viewport 320, scrollWidth 305 (largura útil sem scrollbar), sem overflow do documento. O seletor de etapa preserva acesso a todo o percurso. Tabelas auxiliares e comandos têm rolagem própria quando necessária.

## Interações verificadas

Navegação desktop e mobile; avançar/voltar; seleção de resultado; uma falha contada e “Não executado” excluído da quantidade executada; nota e campo de ID preservados após reload; foco por teclado; console sem erros nos estados consultados. Marcações temporárias e campos foram revertidos: **39 pendentes, nenhuma anotação preenchida** ao entregar.

Limites: o navegador interno aceita HTTP/HTTPS e bloqueou file://. O QA usou um preview temporário somente leitura, expondo apenas este HTML em loopback, sem filesystem genérico, sem API e sem iniciar o IndicaAI. Esse preview foi encerrado ao finalizar. O arquivo foi preparado para abertura direta em navegador normal; não se alegou testar file:// neste navegador interno. A impressão tem estilos dedicados e ação nativa; não houve inspeção de PDF impresso nem auditoria formal com leitor de tela. Se o navegador negar armazenamento local, o documento informa a limitação e mantém a leitura disponível.
