# Operação local — MVP A1

Execução autorizada em [G3-EXECUTION-008](records/G3-EXECUTION-008.md). O pacote é local, Windows x64, autocontido: não exige SDK instalado para usar. Não houve deploy ou distribuição externa.

## Iniciar e encerrar

Para seguir o [roteiro manual](TEST_CASES.html), dê duplo clique em [Iniciar-Testes-IndicaAI.bat](../Iniciar-Testes-IndicaAI.bat) na raiz. Ele já informa a pasta isolada `.local/teste-manual-01` e a porta 5273 ao mesmo iniciador, sem digitar comandos. Reabra esse mesmo arquivo para preservar os dados da rodada. O auxiliar [Iniciar-Testes-Restaurados-IndicaAI.bat](../Iniciar-Testes-Restaurados-IndicaAI.bat) serve somente ao TM39, após restaurar em `.local/teste-manual-restaurado-01`, na porta 5274. Ambos mantêm a regra de fechar a janela para encerrar o servidor. Feche outra instância na mesma porta antes de iniciar.

Na raiz do projeto, dar duplo clique em **`Iniciar-IndicaAI.bat`**. No pacote distribuído, usar **`Start-IndicaAI.bat`**. O iniciador abre a biblioteca no navegador e mantém a janela de controle aberta. **Fechar essa janela encerra o servidor; Ctrl+C também pode encerrá-lo.** Fechar somente a aba do navegador não encerra. A porta padrão é 5273, somente `127.0.0.1`; outra instância na mesma raiz de dados é recusada.

O `.bat` usa o Windows PowerShell 5.1 já presente no Windows, sem exigir PowerShell 7 ou SDK. O script associa o processo do servidor a um Job Object privado antes de executá-lo: fechar/encerrar o host elimina também o servidor, sem depender de `finally` executar. O navegador fica fora desse job. Manter juntos `Start-IndicaAI.bat`, `Start-IndicaAI.ps1` e `LauncherProcess.cs` no pacote. A política RemoteSigned é solicitada somente para esse processo, sem alteração global; políticas corporativas podem restringir scripts. [Escopo e teste](quality/LAUNCHER-0.9.md).

```powershell
./Iniciar-IndicaAI.bat
# Raiz separada para experimentar, sem alterar a coleção principal:
./scripts/Start-IndicaAI.ps1 -DataRoot E:/ProjetosPessoais/IndicaAI/.local/experimento -Port 5274
```

O script `scripts/Start-IndicaAI.ps1` continua disponível para parâmetros e automação. Prefira o `.bat` para o comportamento vinculado à janela. Abrir diretamente o executável pula esse controle do iniciador. Manter a pasta inteira do pacote, inclusive `wwwroot` e diretórios de idiomas. Encerrar o servidor antes de atualizar os binários.

Dados reais ficam por padrão em `%LOCALAPPDATA%/IndicaAI`, separados dos binários. `INDICAAI_DATA_ROOT` e `INDICAAI_PORT` permitem informar outra raiz/porta no processo; o iniciador restaura as variáveis da sua sessão ao sair. O preview de desenvolvimento usa `.local/qa`, com dados demonstrativos, sem acessar a coleção principal.

## Dados, cópias e atualização

- `data/indicaai.db`: SQLite com identidade interna, catálogo, acompanhamento, ciclos, histórico, notas, listas, tags, preferências, recibos e diagnósticos CSV.
- `assets/covers`: capas locais referenciadas pelo banco. Capas incluídas no pacote são copiadas apenas quando ausentes; dados existentes não são substituídos por seed.
- `keys`: chaves locais de proteção dos tokens web. O banco e essas chaves não têm criptografia própria; a proteção de acesso é a da conta/pasta do Windows.
- `backups`: bundles com banco consistente pela API SQLite, assets, migrações e manifesto SHA-256. Prefixos `b-d` (diário), `b-u` (pré-migração), `b-m` (manual), `b-r` (antes de restaurar). O manifesto é a descrição verificável de cada conjunto, não um CSV pessoal.

Ao reabrir um banco, a aplicação verifica o schema, cria no máximo um backup diário UTC antes das escritas e um snapshot verificado antes de migração pendente. Mantém os sete diários verificados mais recentes. Bundles inválidos, manuais e de migração são preservados; pastas `.p-*` indicam preparação incompleta e não são candidatas a restauração. Schema desconhecido/mais novo, falha de snapshot ou dados inválidos bloqueiam a atualização; não há reset automático.

Para atualizar, encerrar o servidor e substituir somente a pasta dos binários por outra versão compatível. Manter intacta a raiz de dados. Uma cópia em outro dispositivo protege contra perda física do disco: copiar um **bundle já verificado e completo**, com seu manifesto. Uma cópia do arquivo SQLite aberto, isoladamente, não é um backup operacional consistente.

## Backup, validação e restauração

Executar na pasta do pacote com o servidor encerrado. Informar explicitamente a raiz usada no iniciador:

```powershell
./IndicaAI.Web.exe --maintenance backup --data-root 'C:/caminho/IndicaAI'
# O comando anterior imprime a pasta do bundle.
./IndicaAI.Web.exe --maintenance validate --data-root 'C:/caminho/IndicaAI' --bundle 'D:/copias/b-m-...'
./IndicaAI.Web.exe --maintenance restore --data-root 'C:/caminho/IndicaAI' --bundle 'D:/copias/b-m-...'
```

`validate` é somente leitura. `restore` exige ausência de instância ativa; valida formato, limites de caminho, ausência de links/reparse points, hashes, integridade/FKs do SQLite, migrações conhecidas e presença das capas. Prepara o conjunto completo e tenta salvar backup verificado do ativo. Se o ativo estiver danificado (banco corrompido ou capa ausente), essa tentativa pode falhar: o candidato continua sujeito a toda a validação, e os diretórios brutos anteriores são preservados em `recovery-*` pela troca com journal. Não copia arquivos arbitrários do bundle para fora da raiz.

Uma falha durante preparação conserva o ativo. Uma interrupção durante a troca deixa `restore-pending.json`: no próximo início, o app recupera os diretórios anteriores antes de abrir/migrar/seedar qualquer banco. Cópias preparadas e anteriores são mantidas para diagnóstico; não remover esse marcador ou essas pastas enquanto uma recuperação estiver pendente. Em falha de acesso ao disco, manter todas as cópias e corrigir o acesso antes de tentar iniciar novamente.

## CSV e catálogo

Administração → Importar CSV. UTF-8/BOM opcional, `;`, até 5 MiB/10.000 registros. Tipos canônicos:

```text
tag_id;nome;grupo_id;midias
aventura;Aventura;ID-DO-GRUPO;jogo|livro

obra_id;tag_id
alem-do-vale;aventura
```

Os IDs de grupos, tags e obras são literais, sensíveis a maiúsculas. A interface administrativa mostra os IDs globais. Coluna ausente/vazia preserva valor existente; mídias fazem união. Criar grupos pela interface antes de importar tags. O preview mostra antes/depois/preservados; só sua confirmação aplica as válidas. Alteração global invalida preview antigo. Falha em um registro desfaz todas as partes dele; os já confirmados continuam. Após interrupção, consultar o resultado e gerar nova prévia para repetir. Diagnósticos terminados expiram após 30 dias; tags, vínculos e recibos pessoais permanecem.

Catálogo inicial contém cinco obras fictícias e capas originais demonstrativas. Não há editor completo de obras. Novas obras entram via banco, respeitando constraints e triggers. Papéis de crédito: `author` em livro/mangá, `director` em filme, `creator` em série; `Games`/`Platforms` somente jogos, `Films` somente filmes. Mudanças operacionais no catálogo exigem servidor parado e backup verificado. Futuras migrações que reconstruam essas tabelas precisam recriar/verificar os triggers de compatibilidade.

## Desenvolvimento e novos idiomas

SDK 10.0.403 está isolado em `.tools/dotnet`; não altera PATH global. Dependências fixadas e lock files no repositório:

```powershell
. ./scripts/Use-Dotnet.ps1
dotnet restore IndicaAI.slnx --locked-mode
dotnet run --project tests/IndicaAI.Tests --no-restore --disable-build-servers
node tests/draft-model.test.cjs
dotnet publish src/IndicaAI.Web -c Release -r win-x64 --self-contained true -o artifacts/IndicaAI-win-x64 --disable-build-servers
./scripts/Verify-Release.ps1
```

A suíte xUnit roda standalone porque o coordenador de `dotnet test` usa named pipes indisponíveis no sandbox desta execução. Não confundir essa limitação de ferramenta com falha de teste.

Para adicionar idioma: copiar as chaves de `Resources/UiText.pt-BR.resx` para `UiText.<código>.resx`, traduzir preservando parâmetros/plurais, registrar código/nome/padrões de data em `LocaleRegistry`, validar igualdade de chaves/parâmetros e testar as telas nos dois temas. Não alterar IDs/dados/domínio ou migrar obras. `fr` é somente fixture parcial não selecionável, usada para provar carregamento de recurso e fallback; não é terceira tradução entregue. PT-BR e EN são as localidades selecionáveis. O idioma nunca traduz conteúdo cadastrado ou rótulos personalizados.

Ver [QA-REPORT](quality/QA-REPORT.md), [THIRD_PARTY](THIRD_PARTY.md) e [EXECUTION_LOG](quality/EXECUTION_LOG.md) para evidências e decisões operacionais.
