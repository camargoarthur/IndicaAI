# Validação antes da entrega

Rotina autorizada em 12/11/2026 e entregue com a versão 0.10. O [roteiro HTML](TEST_CASES.html) continua sendo o documento único de execução manual; esta página descreve apenas a automação técnica.

## Uso

Para uma conferência completa, dar duplo clique em [Validar-IndicaAI.bat](../Validar-IndicaAI.bat). A janela informa aprovação/falha e o local dos logs. Essa validação pode iniciar um servidor temporário isolado e o encerra ao terminar; não usar essa janela como iniciador cotidiano. O SDK local em `.tools/dotnet` e Node são necessários para desenvolvimento/testes, mas continuam dispensáveis para usar o aplicativo publicado.

No PowerShell, a partir da raiz:

```powershell
# Entrega funcional: todos os testes de produto e verificações básicas.
./scripts/Validate.ps1 -Profile Functional

# Escolher pelo maior impacto dos arquivos alterados, inclusive arquivos novos.
./scripts/Validate.ps1 -ChangedPath README.md,docs/TEST_CASES.html

# Durante desenvolvimento: módulos e dependências, com smoke compartilhado.
./scripts/Validate.ps1 -Profile Module -Module Tracking,Lists

# Conferir a seleção sem executar testes.
./scripts/Validate.ps1 -ChangedPath src/IndicaAI.Web/Features/Tracking/TrackingService.cs -PlanOnly

# Candidato a lançamento ou alteração estrutural/operacional.
./scripts/Validate.ps1 -Profile Full
```

## Critérios obrigatórios

| Perfil | Quando | O que executa |
|---|---|---|
| Docs | Apenas documentação | Contratos do guia, referências da cobertura, testes do seletor/executor, hashes/links/UTF-8/fixtures documentais. |
| Visual | CSS, capas, fontes ou ícones | Base documental, modelo JS, build e HTTP de entrada/temas/idiomas/rótulos/formulários. Complementar com inspeção das telas afetadas. |
| Functional | Funcionalidade, bug, formulário, JS ou testes de produto | Base documental, JS, build e **toda a suíte xUnit**, incluindo unidade, SQLite e HTTP. |
| Module | Iteração durante desenvolvimento | Base documental/JS/build, classes dos módulos escolhidos, domínio e smoke/percursos compartilhados. Não substitui Functional na entrega funcional. |
| Full | Lançamento, persistência, operação, dependências, infraestrutura ou impacto desconhecido | Functional + publicação isolada, verificação dos recursos do pacote e teste real do iniciador pelo BAT. |
| Auto | Perfil padrão | Seleciona o maior impacto dos caminhos informados. Sem caminhos ou arquivo desconhecido, escolhe Full. |

Módulos disponíveis: Tracking, Lists, Tags, Imports, Preferences, Library, Recovery. O seletor rejeita nomes inválidos e perfis explicitamente inferiores ao impacto dos caminhos fornecidos. Informe os caminhos alterados, inclusive os novos; sem caminhos, use Full. Alterações mistas nunca ficam apenas no perfil documental. Mudanças em Razor executam Functional, pois podem alterar formulários e comportamento.

A suíte inicial tinha 47 testes em aproximadamente 8 segundos: executar o conjunto funcional em cada entrega custa pouco e reduz risco de omissões. Medir novamente se o custo crescer; não reduzir cobertura só para encurtar o chat.

## Manutenção em cada alteração

1. Identificar requisitos, casos TM e dependências afetados. Atualizar/criar os testes correspondentes; para bugs, reproduzir a falha antes da correção. Testes retrospectivos de comportamento existente podem passar na primeira execução e devem ser descritos assim.
2. Atualizar [mapa de cobertura](../tests/manual-coverage.json) e roteiro quando necessário. Os IDs antigos são preservados. O mapa aponta métodos/arquivos reais; não é uma segunda cópia do roteiro.
3. Atualizar VERSION quando aplicável, patch notes e memória. Atualizar conscientemente o manifesto para os arquivos autorizados; hash divergente deve ser investigado, não ignorado pelo validador.
4. Executar o perfil adequado sobre os arquivos finais. Falha, teste omitido, relatório ausente ou zero testes impedem declarar a validação aprovada. Corrigir e repetir o conjunto afetado; ampliar se houver risco compartilhado.
5. Relatar perfil, totais, resultado, limitações e caminho do resumo. Após alterações somente documentais no fechamento, repetir Docs; não repetir produto aprovado se código/testes/dependências não mudaram.

## Evidência e custo de contexto

Cada execução cria uma pasta única em `.tools/validation/<data-hora-pid>/`: `summary.json`, relatório estruturado de produto e um log por etapa. O terminal apresenta somente PASS/FAIL, duração e totais. Em caso de falha, abrir o log daquela etapa. Uma execução reprovada não é sobrescrita pela próxima; não se reutiliza resultado antigo como validação de código novo.

O teste do iniciador usa dados `.local/launcher test-<GUID>`, porta 5276, sem abrir navegador; fecha seus próprios hosts/servidores e reabre os mesmos dados para verificar lock/persistência. Testes de serviço/HTTP usam diretórios temporários `IndicaAI-tests` e `IndicaAI-web-tests`, cada um com GUID. Esses dados são isolados e preservados para diagnóstico. Nenhum ensaio usa a coleção pessoal ou `.local/teste-manual-01`.

O ensaio operacional precisa consultar processos via CIM e acessar loopback. Se o isolamento do agente negar isso, Full deve falhar e ser executado com a permissão apropriada para esse ensaio; não substituir silenciosamente por aprovação parcial. O teste confere acesso CIM antes de iniciar processos. Os scripts usam Windows PowerShell 5.1 com módulos nativos, mesmo quando a rotina é chamada por PowerShell 7/Node.

## Limites

Os casos TM01–TM43 têm rastreabilidade para verificações automatizadas parciais. TestServer envia requisições à aplicação em memória e inspeciona HTML e dados, sem executar JavaScript ou cliques de navegador. Os testes do TMDB, do lote e da retomada de capas usam respostas simuladas. Os testes JS cobrem rascunhos, os três estados da seleção geral e buscas recentes. O ensaio do iniciador encerra abruptamente seu host; não simula o clique real no X.

Aparência, foco, navegação por teclado, clipboard, diálogos e restauração de rascunhos ou seleção na troca real de preferência continuam no roteiro manual. A automação cobre parte dos 43 percursos e deixa suas marcações sob o controle do usuário.
