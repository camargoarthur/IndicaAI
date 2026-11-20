
param(
    [switch]$Executar,
    [string]$Projeto,
    [string]$CodexPath
)

$ErrorActionPreference = "Stop"

# ==========================================================
# MODO 1 - EXECUCAO AUTOMATICA PELO AGENDADOR DO WINDOWS
# ==========================================================

if ($Executar) {

    $logDir = Join-Path $env:LOCALAPPDATA "CodexAgendado"
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null

    $log = Join-Path $logDir "$(Get-Date -Format 'yyyyMMdd_HHmmss').log"

    try {
        Set-Location -LiteralPath $Projeto

        if (-not (Test-Path -LiteralPath $CodexPath -PathType Leaf)) {
            throw "Executavel do Codex nao encontrado: $CodexPath"
        }

        $prompt = @"
Continue a tarefa anterior de onde parou.
Analise o historico da sessao e o estado atual do projeto.
Identifique o que ja foi concluido e o que ainda falta.
Execute todas as tarefas pendentes sem repetir alteracoes concluidas.
Realize os testes necessarios e apresente um resumo final.
"@

        "Inicio: $(Get-Date)" | Out-File $log -Encoding UTF8
        "Projeto: $Projeto" | Out-File $log -Append
        "Modo: Full Access" | Out-File $log -Append

        # Evita que mensagens de stderr interrompam o
        # pipeline de logs no Windows PowerShell 5.1.
        $ErrorActionPreference = "Continue"

        & $CodexPath exec `
            --dangerously-bypass-approvals-and-sandbox `
            resume --last $prompt *>&1 |
            Out-File -FilePath $log -Append -Encoding UTF8

        $codigoSaida = $LASTEXITCODE

        "Fim: $(Get-Date)" | Out-File $log -Append
        "Codigo de saida: $codigoSaida" |
            Out-File $log -Append

        exit $codigoSaida
    }
    catch {
        "ERRO: $_" | Out-File $log -Append -Encoding UTF8
        exit 1
    }
}

# ==========================================================
# MODO 2 - CRIACAO DO AGENDAMENTO
# ==========================================================

$projetoAtual = (Get-Location).Path

# Localiza o executavel do Codex instalado
$codex = Get-Command codex.cmd -ErrorAction SilentlyContinue

if (-not $codex) {
    $codex = Get-Command codex.exe -ErrorAction SilentlyContinue
}

if (-not $codex) {
    throw "Codex CLI nao encontrado. Verifique a instalacao."
}

$codexExecutavel = $codex.Source

Write-Host ""
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "      AGENDAMENTO DO CODEX CLI" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Projeto: $projetoAtual"
Write-Host "Permissao: FULL ACCESS"
Write-Host "Recorrencia: NENHUMA"
Write-Host ""

# Solicita o horario ao usuario
$entrada = Read-Host "Informe o horario (HH:mm)"

try {
    $hora = [datetime]::ParseExact(
        $entrada,
        "HH:mm",
        [System.Globalization.CultureInfo]::InvariantCulture
    )
}
catch {
    throw "Horario invalido. Utilize HH:mm. Exemplo: 15:30"
}

# Define hoje ou amanha automaticamente
$agora = Get-Date
$agendamento = $agora.Date.Add($hora.TimeOfDay)

if ($agendamento -le $agora) {
    $agendamento = $agendamento.AddDays(1)
}

# Cria nome unico para a tarefa
$nomeTarefa = "Codex_Retomar_$(Get-Date -Format 'yyyyMMdd_HHmmss_fff')"

# Define argumentos de execucao
$argumentos = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -File `"$PSCommandPath`" -Executar -Projeto `"$projetoAtual`" -CodexPath `"$codexExecutavel`""

# Configura a acao
$acao = New-ScheduledTaskAction `
    -Execute "powershell.exe" `
    -Argument $argumentos `
    -WorkingDirectory $projetoAtual

# EXECUCAO UNICA - NAO RECORRENTE
$gatilho = New-ScheduledTaskTrigger `
    -Once `
    -At $agendamento

# Executa com a conta atual do Windows
$usuario = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name

$principal = New-ScheduledTaskPrincipal `
    -UserId $usuario `
    -LogonType Interactive `
    -RunLevel Limited

# Configuracoes da tarefa
$configuracoes = New-ScheduledTaskSettingsSet `
    -StartWhenAvailable `
    -ExecutionTimeLimit (New-TimeSpan -Seconds 0) `
    -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries

# Registra no Agendador do Windows
Register-ScheduledTask `
    -TaskName $nomeTarefa `
    -Action $acao `
    -Trigger $gatilho `
    -Principal $principal `
    -Settings $configuracoes | Out-Null

# ==========================================================
# CONFIRMACAO
# ==========================================================

Write-Host ""
Write-Host "=======================================" -ForegroundColor Green
Write-Host "      AGENDAMENTO CONCLUIDO" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green
Write-Host ""
Write-Host "Projeto: $projetoAtual"
Write-Host "Execucao: $($agendamento.ToString('dd/MM/yyyy HH:mm'))"
Write-Host "Permissoes: FULL ACCESS"
Write-Host "Recorrencia: EXECUCAO UNICA"
Write-Host "Tarefa: $nomeTarefa"
Write-Host "Logs: $env:LOCALAPPDATA\CodexAgendado"
Write-Host ""
Write-Host "O Windows executara o Codex automaticamente." -ForegroundColor Green
