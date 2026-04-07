$mapPath = "docs/API_RESPONSE_MESSAGES_VI.md"
$targetPath = "docs/API_RESPONSE_MESSAGES_BY_MODULE_VI.md"

$mapText = Get-Content -Raw -LiteralPath $mapPath
$dict = @{}
foreach($line in ($mapText -split "`r?`n")){
    if($line -match '^\|\s*\d+\s*\|\s*(.*?)\s*\|\s*(.*?)\s*\|\s*$'){
        $en = $Matches[1].Trim()
        $vi = $Matches[2].Trim()
        if(-not [string]::IsNullOrWhiteSpace($vi) -and -not $dict.ContainsKey($vi)){
            $dict[$vi] = $en
        }
    }
}

$lines = Get-Content -LiteralPath $targetPath
$out = New-Object System.Collections.Generic.List[string]
$miss = New-Object System.Collections.Generic.List[string]

foreach($line in $lines){
    if($line -match '^\|\s*STT\s*\|\s*Message \(VI\)\s*\|\s*(.*?)\s*\|\s*$'){
        $locHeader = $Matches[1].Trim()
        $out.Add("| STT | Message (VI) | Message (EN) | $locHeader |")
        continue
    }

    if($line -match '^\|\s*--:\s*\|'){
        $parts = $line.Trim().Trim('|').Split('|') | ForEach-Object { $_.Trim() }
        if($parts.Count -eq 3){
            $out.Add("| --: | --- | --- | --- |")
            continue
        }
    }

    if($line -match '^\|\s*\d+\s*\|'){
        $parts = $line.Trim().Trim('|').Split('|') | ForEach-Object { $_.Trim() }
        if($parts.Count -eq 3){
            $stt = $parts[0]
            $vi = $parts[1]
            $loc = $parts[2]
            $en = if($dict.ContainsKey($vi)){ $dict[$vi] } else { "N/A" }
            if($en -eq "N/A"){ $miss.Add($vi) }
            $out.Add("| $stt | $vi | $en | $loc |")
            continue
        }
    }

    $out.Add($line)
}

Set-Content -LiteralPath $targetPath -Value $out -Encoding UTF8
"added_en_column"
"unmapped_count=$($miss.Count)"
$miss | Select-Object -First 20 | ForEach-Object { "UNMAPPED: $_" }
