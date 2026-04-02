<#
.SYNOPSIS
    Publica o MuralDigital como self-contained para Windows x64 e gera um pacote .7z.

.DESCRIPTION
    Este script automatiza todo o processo de publicação:
    1. Restaura pacotes NuGet
    2. Publica em modo self-contained (sem dependência de .NET Runtime)
    3. Comprime o resultado com 7-Zip (.7z)
    4. Copia o arquivo para a pasta VersaoAtual/

.EXAMPLE
    .\Scripts\Publish.ps1
    Publica com a versão padrão (1.0.0).

.EXAMPLE
    .\Scripts\Publish.ps1 -Version "1.2.0"
    Publica com versão customizada.
#>

param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

# ── Paths ──────────────────────────────────────────────
$ProjectRoot   = Split-Path -Parent $PSScriptRoot
$CsprojFile    = Join-Path $ProjectRoot "MuralDigital.csproj"
$PublishDir    = Join-Path $ProjectRoot "publish"
$VersaoAtualDir = Join-Path $ProjectRoot "VersaoAtual"
$DateStamp     = Get-Date -Format "yyyy-MM-dd"
$ArchiveName   = "MuralDigital_v${Version}_${DateStamp}.7z"
$ArchivePath   = Join-Path $VersaoAtualDir $ArchiveName

# ── Validations ────────────────────────────────────────
if (-not (Test-Path $CsprojFile)) {
    Write-Error "Arquivo .csproj nao encontrado em: $CsprojFile"
    exit 1
}

# Verificar 7-Zip
$SevenZip = $null
$candidates = @(
    "C:\Program Files\7-Zip\7z.exe",
    "C:\Program Files (x86)\7-Zip\7z.exe"
)
foreach ($c in $candidates) {
    if (Test-Path $c) { $SevenZip = $c; break }
}
if (-not $SevenZip) {
    $SevenZip = Get-Command "7z" -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
}
if (-not $SevenZip) {
    Write-Error "7-Zip nao encontrado. Instale em https://www.7-zip.org/"
    exit 1
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  MuralDigital - Publish Self-Contained" -ForegroundColor Cyan
Write-Host "  Versao: $Version  |  Data: $DateStamp" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# ── Step 1: Clean ──────────────────────────────────────
Write-Host "[1/5] Limpando publicacao anterior..." -ForegroundColor Yellow
if (Test-Path $PublishDir) {
    Remove-Item $PublishDir -Recurse -Force
}
Write-Host "      OK" -ForegroundColor Green

# ── Step 2: Restore ────────────────────────────────────
Write-Host "[2/5] Restaurando pacotes NuGet..." -ForegroundColor Yellow
dotnet restore $CsprojFile
if ($LASTEXITCODE -ne 0) {
    Write-Error "Falha no restore."
    exit 1
}
Write-Host "      OK" -ForegroundColor Green

# ── Step 3: Publish ────────────────────────────────────
Write-Host "[3/5] Publicando self-contained (Windows x64)..." -ForegroundColor Yellow
dotnet publish $CsprojFile `
    -f net9.0-windows10.0.19041.0 `
    -c Release `
    --self-contained true `
    -p:WindowsPackageType=None `
    -p:Version=$Version `
    -o $PublishDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "Falha no publish."
    exit 1
}
Write-Host "      OK" -ForegroundColor Green

# ── Step 4: Compress ───────────────────────────────────
Write-Host "[4/5] Comprimindo com 7-Zip..." -ForegroundColor Yellow

# Criar pasta VersaoAtual se não existir
if (-not (Test-Path $VersaoAtualDir)) {
    New-Item -ItemType Directory -Path $VersaoAtualDir -Force | Out-Null
}

# Remover arquivo antigo com mesmo nome se existir
if (Test-Path $ArchivePath) {
    Remove-Item $ArchivePath -Force
}

& $SevenZip a -t7z -mx=7 $ArchivePath "$PublishDir\*" | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Error "Falha ao comprimir com 7-Zip."
    exit 1
}

$SizeMB = [math]::Round((Get-Item $ArchivePath).Length / 1MB, 2)
Write-Host "      OK - $ArchiveName ($SizeMB MB)" -ForegroundColor Green

# ── Step 5: Cleanup ────────────────────────────────────
Write-Host "[5/5] Limpando pasta publish temporaria..." -ForegroundColor Yellow
Remove-Item $PublishDir -Recurse -Force
Write-Host "      OK" -ForegroundColor Green

# ── Summary ────────────────────────────────────────────
Write-Host ""
Write-Host "=============================================" -ForegroundColor Green
Write-Host "  Publicacao concluida com sucesso!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Arquivo: $ArchivePath" -ForegroundColor White
Write-Host "  Tamanho: $SizeMB MB" -ForegroundColor White
Write-Host ""
Write-Host "  Para executar:" -ForegroundColor Gray
Write-Host "  1. Extraia o .7z em qualquer pasta" -ForegroundColor Gray
Write-Host "  2. Execute MuralDigital.exe" -ForegroundColor Gray
Write-Host ""
