$mdPath = "docs/API_RESPONSE_MESSAGES_BY_MODULE_VI.md"
$content = Get-Content -Raw -LiteralPath $mdPath

# Find table rows with Example Location path:line
$pattern = '(?m)^\|\s*\d+\s*\|\s*(.*?)\s*\|\s*(src\\CapstoneProject\.API\\Controllers\\[^|:]+\.cs):(\d+)\s*\|$'

function Get-ApiEndpointFromLocation([string]$relPath, [int]$lineNo){
    $fullPath = Join-Path (Get-Location) $relPath.Replace('\\','/')
    if(-not (Test-Path $fullPath)){ return $null }
    $lines = Get-Content -LiteralPath $fullPath
    if($lineNo -lt 1 -or $lineNo -gt $lines.Count){ return $null }

    $controllerRoute = ""
    for($i=0; $i -lt [Math]::Min($lines.Count, 120); $i++){
        if($lines[$i] -match '^\s*\[Route\("([^"]+)"\)\]'){
            $controllerRoute = $Matches[1]
            break
        }
    }

    $method = $null
    $subRoute = ""
    for($i=$lineNo-1; $i -ge 0; $i--){
        $t = $lines[$i].Trim()
        if($t -match '^\[(HttpGet|HttpPost|HttpPut|HttpDelete|HttpPatch)(?:\("([^"]*)"\))?\]'){
            $httpAttr = $Matches[1]
            $method = $httpAttr.Substring(4).ToUpper()
            $subRoute = $Matches[2]
            break
        }
        if($t -match '^\s*public\s+(async\s+)?Task<'){ break }
    }

    if(-not $method){ return $null }

    $base = $controllerRoute
    if([string]::IsNullOrWhiteSpace($base)){
        # fallback from folder + controller name
        $name = [IO.Path]::GetFileNameWithoutExtension($relPath)
        $folder = if($relPath -match 'Controllers\\([^\\]+)\\'){ $Matches[1].ToLower() } else { '' }
        if($folder){ $base = "api/$folder/" + $name.Replace('Controller','').ToLower() }
        else { $base = "api/" + $name.Replace('Controller','').ToLower() }
    }

    $base = $base -replace '\[controller\]', ([IO.Path]::GetFileNameWithoutExtension($relPath).Replace('Controller',''))
    $base = $base -replace '\[action\]',''
    $base = $base.Trim('/')

    $path = if([string]::IsNullOrWhiteSpace($subRoute)) { "/$base" } else { "/$base/" + $subRoute.Trim('/') }
    $path = $path -replace '/+','/'
    return "$method $path"
}

$updated = [regex]::Replace($content, $pattern, {
    param($m)
    $msg = $m.Groups[1].Value.TrimEnd()
    $rel = $m.Groups[2].Value
    $line = [int]$m.Groups[3].Value
    $ep = Get-ApiEndpointFromLocation $rel $line
    if([string]::IsNullOrWhiteSpace($ep)){
        $ep = "N/A"
    }
    return "| " + ($m.Value -replace '^\|\s*(\d+)\s*\|.*$','$1') + " | $msg | $ep |"
})

$updated = $updated -replace 'Example Location','API Endpoint'
Set-Content -LiteralPath $mdPath -Value $updated -Encoding UTF8
Write-Output "done"
