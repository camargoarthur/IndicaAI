# Read-only package, Markdown links, UTF-8, CSV fixture and PNG verification.
param([switch]$AllowExtra)
$ErrorActionPreference = 'Stop'
$taskRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$taskManifest = Get-Content -LiteralPath (Join-Path $taskRoot 'PACKAGE_MANIFEST.json') -Raw | ConvertFrom-Json
$taskErrors = [Collections.Generic.List[string]]::new()
$taskExpected = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$null = $taskExpected.Add('PACKAGE_MANIFEST.json')
$taskUtf8 = [Text.UTF8Encoding]::new($false, $true)
$taskMarkdownCount = 0
$taskLinksCount = 0
foreach ($entry in $taskManifest.files) {
    $name = [string]$entry.path
    if ($name -match '(^/|(^|/)\.\.(/|$)|\\|:)') { $taskErrors.Add("Unsafe path: $name"); continue }
    $path = [IO.Path]::GetFullPath((Join-Path $taskRoot $name))
    if (-not $path.StartsWith($taskRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { $taskErrors.Add("Outside root: $name"); continue }
    if (-not $taskExpected.Add($name)) { $taskErrors.Add("Duplicate manifest entry: $name") }
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { $taskErrors.Add("Missing: $name"); continue }
    $bytes = [IO.File]::ReadAllBytes($path)
    $hash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($bytes.Length -ne $entry.bytes -or $hash -ne $entry.sha256) { $taskErrors.Add("Hash/size mismatch: $name") }
    if ($name -match '\.(md|json|csv|py|ps1)$') {
        try { $content = $taskUtf8.GetString($bytes) } catch { $taskErrors.Add("Invalid UTF-8: $name"); continue }
    }
    if ($name -match '\.md$') {
        $taskMarkdownCount++
        foreach ($match in [regex]::Matches($content, '\[[^\]\r\n]*\]\(([^)\r\n]+)\)')) {
            $target = $match.Groups[1].Value.Trim().Trim('<','>')
            if ($target -match '^(https?://|mailto:|#)') { continue }
            $target = [Uri]::UnescapeDataString(($target -split '#',2)[0])
            if (-not $target) { continue }
            $taskLinksCount++
            $resolved = Join-Path (Split-Path -Parent $path) $target
            if (-not (Test-Path -LiteralPath $resolved)) { $taskErrors.Add("Broken local link: $name -> $target") }
        }
    }
    if ($name -match '\.png$') {
        if ($bytes.Length -lt 24 -or [BitConverter]::ToString($bytes,0,8) -ne '89-50-4E-47-0D-0A-1A-0A') { $taskErrors.Add("Invalid PNG: $name"); continue }
        $width = [uint32]$bytes[16]*16777216 + [uint32]$bytes[17]*65536 + [uint32]$bytes[18]*256 + $bytes[19]
        $height = [uint32]$bytes[20]*16777216 + [uint32]$bytes[21]*65536 + [uint32]$bytes[22]*256 + $bytes[23]
        Write-Output "PNG $name : $width x $height"
    }
}
if (-not $AllowExtra) {
    foreach ($file in Get-ChildItem -LiteralPath $taskRoot -File -Force -Recurse) {
        $relative = $file.FullName.Substring($taskRoot.Length + 1).Replace('\','/')
        if ($relative -match '^(\.git|\.codex)/') { continue }
        if (-not $taskExpected.Contains($relative)) { $taskErrors.Add("Unlisted file: $relative") }
    }
}
foreach ($fixture in 'tags-proposta.csv','associacoes-proposta.csv') {
    $rows = @(Import-Csv -LiteralPath (Join-Path $taskRoot "examples/$fixture") -Delimiter ';' -Encoding UTF8)
    if ($rows.Count -ne 2) { $taskErrors.Add("Unexpected fixture row count: $fixture") }
    Write-Output "CSV $fixture : $($rows.Count) records"
}
$tagRows = @(Import-Csv -LiteralPath (Join-Path $taskRoot 'examples/tags-proposta.csv') -Delimiter ';' -Encoding UTF8)
if (-not ($tagRows | Where-Object { $_.nome -match ';' })) { $taskErrors.Add('Quoted semicolon fixture not preserved') }
if ($taskErrors.Count) { $taskErrors | ForEach-Object { Write-Output "ERROR $_" }; exit 1 }
Write-Output "OK: $($taskManifest.files.Count) manifest files, $taskMarkdownCount Markdown files, $taskLinksCount local links. UTF-8, fixture CSV and PNG checks completed."
