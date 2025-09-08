$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$objectSizesFile = Join-Path $repoRoot "..\.git\object_sizes.txt"
if (-not (Test-Path $objectSizesFile)) {
    Write-Error "object_sizes.txt not found at $objectSizesFile. Run the git rev-list pipeline first."
    exit 1
}
Get-Content $objectSizesFile | ForEach-Object {
    if ($_ -match '^(?<hash>\S+)\s+(?<type>\S+)\s+(?<size>\d+)\s*(?<rest>.*)$') {
        [PSCustomObject]@{
            Hash = $matches['hash']
            Type = $matches['type']
            Size = [int]$matches['size']
            Name = $matches['rest'].Trim()
        }
    }
} | Sort-Object -Property Size -Descending | Select-Object -First 20 | Format-Table -AutoSize
