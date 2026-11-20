# Preferências e idiomas

Referência técnica adotada na implementação A1. A aprovação e execução estão registradas em [GATES](../GATES.md); código, migrações e testes são a evidência do comportamento atual. Terminologia propositiva no corpo preserva o desenho original e não significa que a implementação esteja pendente.

## Escopo e referência visual

MVP com claro/escuro e interface em Português (Brasil) e English. Ampliar traduções deve ser uma operação documentada de conteúdo/configuração, sem espalhar condicionais por telas ou alterar domínio/banco de obras a cada idioma.

Direção editorial 2 permanece escolhida. [Mock escuro em inglês](../design/direcao-2-escuro-en.png) deriva da [tela clara em PT-BR](../design/direcao-2-acompanhamento.png), anexada efetivamente como referência ao Image Gen. Não é nova alternativa visual. Mantém título da obra, listas e nota em português, com interface em inglês, demonstrando a separação de conteúdo. O mock e as cores escuras continuam propostos, sem contraste medido.

## Preferências e comportamento propostos — G1

| Preferência | Opções iniciais | Default proposto | Aplicação |
|---|---|---|---|
| Aparência / Appearance | Claro / Light; Escuro / Dark | Claro, continuidade do estudo escolhido | Todas as telas, campos, diálogos, erros, foco, seleção e preview |
| Idioma / Language | Português (Brasil); English | Português (Brasil) | Navegação, formulários, ajudas, datas/números de apresentação, acessibilidade, erros e relatórios administrativos |

Usar nomes de idiomas, sem bandeiras ou exigir conhecimento de códigos. Português (Brasil) identifica `pt-BR`; English identifica `en`, sem impor variante regional inglesa não solicitada. Tema e idioma independentes: quatro combinações possíveis. O MVP proposto tem escolhas manuais; seguir automaticamente o sistema não foi solicitado nem acrescentado como requisito.

Entrada persistente de Preferências na navegação da instância local, junto à personalização de rótulos existente. Proposta: alterar controles aplica a preferência imediatamente e salva localmente na identidade interna, sem login. Informar falha de gravação e que a preferência não foi persistida, mantendo a interface utilizável; ao reiniciar, carregar o último valor confirmado. Valores desconhecidos de versão futura usam os defaults suportados com aviso, sem apagar dados.

Trocar tema não muda layout ou conteúdo. Trocar idioma mantém obra, contexto, filtros, seleção e campos editados; não reenvia acompanhamento nem grava formulário pessoal. Se uma tela precisar recarregar, preservar o rascunho antes ou resolver alterações explicitamente. Nunca perder texto digitado pela escolha de idioma. Aplicar a preferência carregada antes de mostrar a tela completa, para evitar clarão no tema errado.

Os seletores aparecem no topo do novo mock para tornar a combinação visível; sua colocação em todas as telas não está proposta. A localização final dos controles é em Preferências, com acesso estável, sem transformar cada formulário em painel de configuração.

## O que traduz e o que permanece conteúdo

| Elemento | Proposta ao trocar idioma |
|---|---|
| Menus, botões, rótulos, ajuda, validação e mensagens de importação | Traduzir integralmente, inclusive área master, diálogos e nomes acessíveis |
| Mídias, estados padrão, ações/eventos e operadores de filtro | Traduzir o rótulo; manter IDs, significado, compatibilidade e consulta |
| Estado renomeado pelo usuário | Preservar o nome personalizado literalmente nos dois idiomas; mostrar significado base traduzido como apoio |
| Restauração do nome padrão do estado | Voltar ao rótulo traduzível do sistema; não criar/excluir estado |
| Títulos, descrições, autores e demais metadados do catálogo | Preservar texto cadastrado; idioma da interface não cria outra obra nem tradução automática |
| Nomes de tags/grupos, listas e anotação privada | Preservar como conteúdo informado, sem reescrever ou traduzir automaticamente |
| Datas e números exibidos | Formatar pela localidade sem alterar valor, data de calendário, instante UTC ou origem inferida/informada |
| CSV: cabeçalhos, IDs, códigos de mídia e delimitador | Manter contrato técnico canônico; traduzir instruções, razões e nomes de colunas da prévia de interface |

Exemplo: `Lendo` padrão vira `Reading`. Se a pessoa o renomeou para `Lendo aos poucos`, esse texto continua igual em English; seu significado base pode ser apresentado como Reading. Título A cidade distante, listas Favoritos/Leituras do ano e nota em português permanecem iguais no mock inglês.

Datas localizadas devem evitar ambiguidade: leitura longa usa mês por nome quando necessário, e entrada usa controle acessível com formato indicado. A ausência de data continua ausência; a referência pela inclusão não passa a data informada por tradução. Não mudar timezone, regras de fallback, ordenação do histórico ou parsing do CSV porque mudou o idioma.

Esta definição é uma delimitação proposta de internacionalização da interface; não presume catálogo com várias traduções de título, editor de tradução, serviço externo ou tradução automática de conteúdo.

## Organização técnica proposta — G2

### Temas

Usar valores semânticos comuns aos dois temas: fundo, superfície, texto principal/secundário, ação, borda, foco, sucesso, aviso e erro. Componentes consultam esses valores; não recebem cores literais distribuídas nem classes de negócio “se escuro”. Adaptar também controles nativos e `color-scheme`, que informa o esquema ao navegador e não substitui a definição das cores da aplicação. [Fonte técnica](https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Properties/color-scheme).

Preservar capas/imagens originais sem inversão automática. Estados, seleção, falhas e compatibilidade continuam identificados por texto/forma, não só pela cor. Contraste deve ser medido em claro e escuro, incluindo erros, badges, campos e foco; objetivo candidato WCAG 2.2 AA. Não tratar imagem gerada como evidência de conformidade. [Critério de contraste](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html).

### Internacionalização

Recursos separados por idioma e catálogo de localidades suportadas com código, nome de exibição e metadados de formato/direção pertinentes. Traduções referenciam chaves semânticas estáveis e mensagens com parâmetros/plurais, sem concatenar frases ou persistir texto traduzido como identificador. Mensagem de erro mantém código estável e parâmetros de domínio; a interface traduz a apresentação. Não fazer com que o idioma esconda nomes técnicos necessários a corrigir uma linha CSV.

Proposta de fallback: localizar a mensagem no idioma selecionado; se ausente, usar PT-BR; nunca mostrar chave crua ou texto vazio. Relatar ausência para manutenção local, sem telemetria externa implícita. Fallback é proteção em execução, não permissão para entregar EN incompleto: exigir cobertura das chaves e placeholders de PT-BR/EN antes de liberar a versão.

Formatação usa mecanismos do framework/ambiente para a localidade selecionada. No cliente, Intl.DateTimeFormat é um candidato nativo para datas; isso não define como converter datas sem horário ou instantes. Datas de calendário devem ser formatadas sem deslocá-las de dia por conversão UTC. [Fonte técnica](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Intl/DateTimeFormat).

| Arquitetura candidata | Encaixe proposto | Cuidado |
|---|---|---|
| A1 — ASP.NET Core | Recursos de localização e cultura do framework; tema em recursos de estilo | Separar idioma da interface de parsing canônico CSV/datas persistidas; alinhar mensagens cliente/servidor |
| A2 — Django | Catálogos gettext, idioma selecionado e formatação do framework | Cobrir também mensagens/controles do cliente; não confundir usar admin padrão com permitir login no MVP |
| B — frontend + API | Recursos por localidade na UI; API com códigos/valores estáveis | Evitar traduzir mensagens em duas camadas com políticas divergentes; biblioteca só após escolha G2 |
| C — navegador | Recursos incluídos no pacote e preferências locais | Mesmos cuidados de tradução/tema; não elimina os riscos de durabilidade do armazenamento |

ASP.NET Core fornece serviços de localização e Django usa catálogos de tradução, mas ambos exigem identificar e traduzir as mensagens do produto; nenhum recurso traduz todo o catálogo automaticamente. Isso é capacidade confirmada das alternativas, não escolha de stack. [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-10.0), [Django](https://docs.djangoproject.com/en/5.2/topics/i18n/translation/).

### Preferências locais

Conceito candidato ligado à identidade interna: modo de aparência e código de idioma. Armazenamento final depende de G2. Manter defaults/versionamento e carregar preferências sem depender de conexão externa. Idioma/tema não são entidades de obra, evento ou estado base. Não duplicar tabelas de acompanhamentos por idioma, nem criar usuários/contas para guardar preferência.

## Como adicionar um idioma depois

1. Criar catálogo de mensagens da nova localidade a partir das chaves de referência, com plurais, parâmetros e nomes acessíveis.
2. Registrar código/nome e metadados pertinentes no catálogo de localidades. Não inserir condições por tela nem migrar obras por causa do idioma.
3. Validar chaves, parâmetros, pluralização e fallback; revisar termos de estados e mensagens administrativas com a pessoa tradutora.
4. Verificar layout com textos mais longos, datas/números, entrada, teclado e as duas aparências, preservando conteúdo pessoal.
5. Distribuir recursos revisados com a aplicação e liberar a localidade no seletor.

Esse é o caminho facilitado proposto para idiomas compatíveis com a estrutura prevista; não é garantia de custo zero. Idiomas com outra direção de escrita, calendário ou fonte podem requerer ajustes de apresentação e verificação específicos. A estrutura deve permitir esses metadados sem exigir implementar agora uma terceira tradução, RTL completo ou editor de idiomas dentro do app.

## Verificação futura e pendências

Executar depois de G3 jornadas essenciais e administração em claro/PT-BR, escuro/PT-BR, claro/EN e escuro/EN; conferir preferência após reinício e mudança durante edição. Verificar nomes personalizados e conteúdo preservados, IDs/estado/histórico invariantes, data efetiva com origem, CSV canônico, plurais e falta de traduções, contraste/foco/reflow e inclusão de um idioma de teste sem mexer no domínio.

Esta rodada fornece requisito, design e contratos candidatos. Não há códigos de recurso implementados, tradução completa entregue, preferência salva no produto ou testes dessas combinações executados. Defaults, fluxo de aplicação, fallback e desenho escuro foram aprovados com G1 0.5 (documento histórico removido na limpeza B14); mecanismos concretos e persistência física aguardam G2 (documento histórico removido na limpeza B14).
