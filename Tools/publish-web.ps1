# Publica metaverso-web/ en la rama gh-pages (GitHub Pages).
# La rama guarda un solo commit que se reemplaza en cada publicacion, para que los
# builds de Unity (decenas de MB) no se acumulen en el historial del repositorio.
#
# Uso, desde la raiz del proyecto:
#   .\Tools\publish-web.ps1
#   .\Tools\publish-web.ps1 -Message "deploy: fachada nueva"

param([string]$Message)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $PSCommandPath)
$web = Join-Path $root "metaverso-web"

if (-not (Test-Path (Join-Path $web ".git"))) {
    throw "metaverso-web no es un worktree de git. Ver Docs/DEPLOY.md, seccion 'Primera vez'."
}

$versionFile = Join-Path $web "docs/version.json"
$label = (Get-Content $versionFile -Raw | ConvertFrom-Json).label
if (-not $Message) { $Message = "deploy web $label" }

$branch = git -C $web rev-parse --abbrev-ref HEAD
if ($branch -ne "gh-pages") {
    throw "metaverso-web esta en la rama '$branch', se esperaba gh-pages."
}

git -C $web add -A

git -C $web rev-parse --verify HEAD 2>$null | Out-Null
if ($LASTEXITCODE -eq 0) {
    git -C $web commit --amend -m $Message
} else {
    git -C $web commit -m $Message
}
if ($LASTEXITCODE -ne 0) { throw "El commit fallo." }

git -C $web push --force origin gh-pages
if ($LASTEXITCODE -ne 0) { throw "El push fallo." }

Write-Host "Publicado $label. En 1-2 minutos: https://juliojulioso.github.io/metaverso-mvp/"
