$docPath = "docs/API_RESPONSE_MESSAGES_BY_MODULE_VI.md"
$controllersRoot = "src/CapstoneProject.API/Controllers"

function Get-EndpointFromHttpAttr([string]$httpAttr,[string]$controllerRoute){
    $m = [regex]::Match($httpAttr,'\[(HttpGet|HttpPost|HttpPut|HttpDelete|HttpPatch)(?:\("([^"]*)"\))?\]')
    if(-not $m.Success){ return $null }
    $method = $m.Groups[1].Value.Substring(4).ToUpper()
    $sub = $m.Groups[2].Value
    $base = $controllerRoute.Trim('/')
    $path = if([string]::IsNullOrWhiteSpace($sub)){ "/$base" } else { "/$base/" + $sub.Trim('/') }
    $path = $path -replace '/+','/'
    return "$method $path"
}

# Build requestType -> endpoints from API controllers
$map = @{}
$controllerFiles = Get-ChildItem $controllersRoot -Recurse -Filter *.cs
foreach($f in $controllerFiles){
    $lines = Get-Content -LiteralPath $f.FullName
    $controllerRoute = ""
    foreach($ln in $lines){
        if($ln -match '^\s*\[Route\("([^"]+)"\)\]'){ $controllerRoute = $Matches[1]; break }
    }
    if([string]::IsNullOrWhiteSpace($controllerRoute)){ continue }

    for($i=0; $i -lt $lines.Count; $i++){
        $trim = $lines[$i].Trim()
        if($trim -match '^\[(HttpGet|HttpPost|HttpPut|HttpDelete|HttpPatch)(?:\("([^"]*)"\))?\]'){
            $endpoint = Get-EndpointFromHttpAttr $trim $controllerRoute
            if(-not $endpoint){ continue }

            # Capture method block until next http attr or next method signature boundary
            $block = New-Object System.Collections.Generic.List[string]
            for($j=$i; $j -lt [Math]::Min($lines.Count, $i+220); $j++){
                $t = $lines[$j].Trim()
                if($j -gt $i -and $t -match '^\[(HttpGet|HttpPost|HttpPut|HttpDelete|HttpPatch)'){ break }
                $block.Add($lines[$j])
            }
            $joined = ($block -join "`n")
            $matches = [regex]::Matches($joined,'new\s+([A-Za-z0-9_]+(?:Command|Query))\b')
            foreach($mm in $matches){
                $type = $mm.Groups[1].Value
                if(-not $map.ContainsKey($type)){ $map[$type] = New-Object System.Collections.Generic.HashSet[string] }
                [void]$map[$type].Add($endpoint)
            }
        }
    }
}

$lines = Get-Content -LiteralPath $docPath
$out = New-Object System.Collections.Generic.List[string]
$updated = 0

foreach($line in $lines){
    if($line -notmatch '^\|\s*\d+\s*\|'){ $out.Add($line); continue }

    $parts = $line.Trim().Trim('|').Split('|') | ForEach-Object { $_.Trim() }
    if($parts.Count -ne 4){ $out.Add($line); continue }

    $stt = $parts[0]
    $vi = $parts[1]
    $en = $parts[2]
    $loc = $parts[3]

    # Skip rows already endpoint/global
    if($loc -match '^(GET|POST|PUT|DELETE|PATCH)\s+/api/' -or $loc -match '^Global\s'){ $out.Add($line); continue }

    # Try convert source-file location to endpoint
    if($loc -match '^src/.+\.cs:\d+$'){
        $fileOnly = [IO.Path]::GetFileNameWithoutExtension(($loc -split ':')[0])
        $requestType = if($fileOnly -match 'Handler$'){ $fileOnly -replace 'Handler$','' } else { $null }

        if($requestType -and $map.ContainsKey($requestType) -and $map[$requestType].Count -gt 0){
            $eps = @($map[$requestType]) | Sort-Object
            $endpointText = ($eps -join ' ; ')
            $out.Add("| $stt | $vi | $en | $endpointText |")
            $updated++
            continue
        }
    }

    $out.Add($line)
}

Set-Content -LiteralPath $docPath -Value $out -Encoding UTF8
"updated_rows=$updated"
