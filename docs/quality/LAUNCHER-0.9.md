# Iniciador vinculado à janela — 09/11/2026

Autorização direta: “Primeiro finalize o processo. Depois disso altere o funcionamento dele para uma bat ou algo similar, enquanto ela estiver aberta o serviço roda mas quando eu fecho o serviço se encerra.” Escopo pequeno e determinado; sem nova aprovação abstrata ou alteração de requisitos funcionais.

O processo anterior foi encerrado antes das mudanças. Entrega: `.bat` para duplo clique, host Windows PowerShell 5.1 e servidor dentro de um Job Object com `JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE`. O processo nasce suspenso, é associado ao job antes de executar e é eliminado se a associação falhar. O handle do job não é herdado; encerrar o host fecha o handle e o Windows encerra o servidor. Não há serviço do Windows ou tarefa de inicialização.

Referência: [Microsoft — Job Objects](https://learn.microsoft.com/en-us/windows/win32/procthread/job-objects). Abertura do navegador continua sendo feita pelo iniciador, fora do job do servidor. Fechar só a aba não encerra o serviço; fechar a janela do iniciador encerra. Dados ficam separados e persistem. Fechamento abrupto não aguarda solicitações em andamento; SQLite mantém sua recuperação transacional.

Plano de execução:

1. Teste de ciclo real isolado: iniciar, HTTP 200 somente loopback, matar o host abruptamente, comprovar processo/porta encerrados, reabrir a mesma raiz e repetir encerramento.
2. Criar `.bat` e helper de job; adaptar script existente preservando parâmetros, ambiente e comportamento do navegador. Uma entrada na raiz do projeto e outra dentro do pacote.
3. Executar teste em Windows PowerShell 5.1 contra fontes e cópia distribuída; verificar pacote, documentos e hashes. Deixar todos os servidores de teste desligados ao entregar.

## Resultados reais

- RED: [iniciador ausente](../evidence/11-launcher-red.log). Depois da implementação, [Windows PowerShell 5.1 direto](../evidence/11-launcher-green.log) e [`.bat` distribuído](../evidence/11-package-launcher-green.log) passaram. A raiz de dados do segundo ensaio contém espaço.
- A [entrada da raiz `Iniciar-IndicaAI.bat`](../evidence/11-root-launcher-green.log) também passou pelo mesmo teste. O script distribuído usa UTF-8 com BOM para que o Windows PowerShell 5.1 leia corretamente mensagens em português.
- O teste chama o iniciador, espera HTTP 200, identifica seu filho pelo PID pai, encerra abruptamente somente o host de teste e comprova que o servidor sai e HTTP para. Reabre os mesmos dados e repete. Os servidores dos ensaios foram encerrados. O inventário CIM de portas se mostrou inconsistente no ambiente; a prova de encerramento usa PID e resposta HTTP real.
- Não houve clique automatizado no X de uma janela visível: foi encerramento abrupto do host, exercitando a liberação de handles mesmo sem executar `finally`. O vínculo de fechamento do console depende do mecanismo de Job Objects documentado pela Microsoft acima.
- Revisão estática independente e específica dos seis arquivos do iniciador/teste: nenhum achado Critical/Important/Minor; o revisor não executou testes. Não foi repetida a revisão global do MVP.
- Pacote conferido com 22 arquivos iguais (19 recursos anteriores + três arquivos do iniciador), nenhum banco incluído. A alteração é exclusivamente operacional; não exige recompilar o aplicativo.

Comandos de reprodução, executados da raiz e usando dados isolados gerados pelo teste:

```powershell
./tests/launcher-lifetime.test.ps1
./tests/launcher-lifetime.test.ps1 -ThroughBat -LauncherRoot artifacts/IndicaAI-win-x64
./tests/launcher-lifetime.test.ps1 -ThroughBat -BatchPath ./Iniciar-IndicaAI.bat
./scripts/Verify-Release.ps1
```
