Set-Location "$PSScriptRoot/.."

$enPath = "docs/_api_messages_en.txt"
$viMdPath = "docs/API_RESPONSE_MESSAGES_VI.md"
$moduleMdPath = "docs/API_RESPONSE_MESSAGES_BY_MODULE_VI.md"

$viRows = Get-Content -LiteralPath $viMdPath | Where-Object { $_ -match '^\|\s*\d+\s*\|' }
$map = @{}
foreach ($row in $viRows) {
    if ($row -match '^\|\s*(\d+)\s*\|\s*(.*?)\s*\|\s*(.*?)\s*\|\s*$') {
        $en = $matches[2].Trim() -replace '\\\|', '|'
        $vi = $matches[3].Trim() -replace '\\\|', '|'
        if (-not [string]::IsNullOrWhiteSpace($en) -and -not $map.ContainsKey($en)) {
            $map[$en] = $vi
        }
    }
}

$files = Get-ChildItem -Path "src" -Recurse -Filter "*.cs" | Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\" }
$replacedFiles = 0
$replaceOps = 0
$orderedKeys = $map.Keys | Sort-Object Length -Descending

foreach ($f in $files) {
    $content = Get-Content -Raw -LiteralPath $f.FullName
    if ($null -eq $content) { continue }
    $original = $content

    foreach ($k in $orderedKeys) {
        if ([string]::IsNullOrEmpty($k)) { continue }
        $v = $map[$k]
        $kEsc = [regex]::Escape($k)
        $vEsc = $v.Replace('"', '\"')

        $before = $content
        $content = [regex]::Replace($content, '"' + $kEsc + '"', '"' + $vEsc + '"')
        $content = [regex]::Replace($content, '\$"' + $kEsc + '"', '$"' + $vEsc + '"')

        if ($content -ne $before) { $replaceOps++ }
    }

    if ($content -ne $original) {
        [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
        $replacedFiles++
    }
}

$callPattern = [regex]::new('Result(?:<[^>]+>)?\.(Success|Failure)\((.*?)\);', [System.Text.RegularExpressions.RegexOptions]::Singleline)
$strPattern = [regex]::new('\$?"(?:\\.|[^"])*"|@"(?:[^"]|"")*"')
$occurrences = New-Object System.Collections.Generic.List[object]

function Get-ModuleName([string]$path) {
    $p = $path -replace '\\', '/'
    if ($p -match 'src/CapstoneProject\.API/Controllers/([^/]+)/') { return "API/$($matches[1])" }
    if ($p -match 'src/CapstoneProject\.Application/Features/([^/]+)/') { return "Application/$($matches[1])" }
    if ($p -match 'src/CapstoneProject\.API/') { return 'API/Other' }
    if ($p -match 'src/CapstoneProject\.Application/') { return 'Application/Other' }
    if ($p -match 'src/CapstoneProject\.Infrastructure/') { return 'Infrastructure' }
    if ($p -match 'src/CapstoneProject\.Domain/') { return 'Domain' }
    return 'Other'
}

foreach ($f in $files) {
    $content = Get-Content -Raw -LiteralPath $f.FullName
    if ($null -eq $content) { continue }

    foreach ($m in $callPattern.Matches($content)) {
        $args = $m.Groups[2].Value
        foreach ($s in $strPattern.Matches($args)) {
            $raw = $s.Value
            if ($raw.StartsWith('@"')) { $msg = $raw.Substring(2, $raw.Length - 3).Replace('""', '"') }
            elseif ($raw.StartsWith('$"')) { $msg = $raw.Substring(2, $raw.Length - 3) }
            else { $msg = $raw.Substring(1, $raw.Length - 2) }

            if ([string]::IsNullOrWhiteSpace($msg)) { continue }
            if ($msg -notmatch '[A-Za-zÀ-ỹ]') { continue }

            $rel = ($f.FullName -replace '^.*?src[\\/]', 'src/').Replace('\\', '/')
            $module = Get-ModuleName $rel
            $line = 1 + ($content.Substring(0, $m.Index).Split("`n").Count - 1)
            $occurrences.Add([pscustomobject]@{
                Module = $module
                Message = $msg
                File = $rel
                Line = $line
            }) | Out-Null
        }
    }
}

$groups = $occurrences | Group-Object Module | Sort-Object Name
$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine('# API Response Messages By Module (VI)')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('Nguon: cac chuoi message trong Result.Success/Result.Failure sau khi da Viet hoa.')
[void]$sb.AppendLine('')

foreach ($g in $groups) {
    [void]$sb.AppendLine('## ' + $g.Name)
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine('| STT | Message (VI) | Example Location |')
    [void]$sb.AppendLine('|---:|---|---|')
    $idx = 0
    $unique = $g.Group | Group-Object Message | Sort-Object Name
    foreach ($u in $unique) {
        $idx++
        $sample = $u.Group | Select-Object -First 1
        $msgEsc = $u.Name.Replace('|', '\\|')
        $locEsc = ("$($sample.File):$($sample.Line)").Replace('|', '\\|')
        [void]$sb.AppendLine("| $idx | $msgEsc | $locEsc |")
    }
    [void]$sb.AppendLine('')
}

[System.IO.File]::WriteAllText((Join-Path (Get-Location) $moduleMdPath), $sb.ToString(), [System.Text.Encoding]::UTF8)

Write-Output "replaced_files=$replacedFiles"
Write-Output "replace_ops=$replaceOps"
Write-Output "modules=$($groups.Count)"
Write-Output "module_doc=$moduleMdPath"
