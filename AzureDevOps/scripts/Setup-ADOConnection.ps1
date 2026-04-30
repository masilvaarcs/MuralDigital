<#
.SYNOPSIS
    Configura a conexão com Azure DevOps e cria a estrutura inicial de Area Paths.

.DESCRIPTION
    - Instala extensão azure-devops na Azure CLI (se necessário)
    - Autentica com PAT
    - Define organização e projeto como padrão
    - Cria Area Paths para todos os stacks do projeto marcosprogramador

.PARAMETER Pat
    Personal Access Token do Azure DevOps.
    Gerar em: https://dev.azure.com/masilvaarcs → User Settings → Personal Access Tokens
    Escopos necessários: Work Items (Read & Write), Code (Read), Build (Read & Execute)

.EXAMPLE
    .\Setup-ADOConnection.ps1
    .\Setup-ADOConnection.ps1 -Pat "xxxxxxxxxxxx"
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$Pat = ""
)

# Carregar .env local se PAT não foi passado como argumento
if ($Pat -eq "") {
    $envFile = Join-Path $PSScriptRoot "..\\.env"
    if (Test-Path $envFile) {
        Get-Content $envFile | Where-Object { $_ -match "^ADO_PAT=" } | ForEach-Object {
            $Pat = $_.Split("=", 2)[1].Trim()
        }
    }
    if ($Pat -eq "") {
        Write-Error "PAT nao encontrado. Passe -Pat 'TOKEN' ou crie AzureDevOps/.env com ADO_PAT=..."
        exit 1
    }
}

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$Org     = "https://dev.azure.com/masilvaarcs"
$Project = "marcosprogramador"

Write-Host "`n=== Azure DevOps — Setup Inicial ===" -ForegroundColor Cyan
Write-Host "Org:     $Org"
Write-Host "Project: $Project`n"

# 1. Verificar Azure CLI
Write-Host "[ 1/5 ] Verificando Azure CLI..." -ForegroundColor Yellow
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Error "Azure CLI nao encontrado. Instale em: https://aka.ms/installazurecliwindows"
    exit 1
}
Write-Host "        Azure CLI OK" -ForegroundColor Green

# 2. Instalar extensão azure-devops
Write-Host "[ 2/5 ] Instalando/atualizando extensao azure-devops..." -ForegroundColor Yellow
az extension add --name azure-devops --upgrade --only-show-errors
Write-Host "        Extensao OK" -ForegroundColor Green

# 3. Autenticar
Write-Host "[ 3/5 ] Autenticando com PAT..." -ForegroundColor Yellow
$Pat | az devops login --organization $Org
Write-Host "        Autenticado" -ForegroundColor Green

# 4. Definir defaults
Write-Host "[ 4/5 ] Definindo defaults (org + project)..." -ForegroundColor Yellow
az devops configure --defaults organization=$Org project=$Project
Write-Host "        Defaults configurados" -ForegroundColor Green

# 5. Criar Area Paths (uma por stack)
Write-Host "[ 5/5 ] Criando Area Paths..." -ForegroundColor Yellow

$areaPaths = @("Golang", "DotNet", "Python", "JavaScript", "DevOps")

foreach ($area in $areaPaths) {
    $existing = az boards area project list --project $Project --depth 2 --query "children[?name=='$area'].name" -o tsv 2>$null
    if ($existing -and $existing.Trim() -ne "") {
        Write-Host "        Area Path '$area' ja existe — pulando" -ForegroundColor DarkYellow
    } else {
        $result = az boards area project create --name $area --project $Project 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "        Criado: $Project\$area" -ForegroundColor Green
        } else {
            Write-Host "        Aviso ao criar '$area': $result" -ForegroundColor DarkYellow
        }
    }
}

# Resultado
Write-Host "`n=== Setup concluido! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Configuracao atual:" -ForegroundColor Cyan
az devops configure --list
Write-Host ""
Write-Host "Area Paths criados:" -ForegroundColor Cyan
az boards area project list --project $Project --query "children[].name" -o table
Write-Host ""
Write-Host "Proximo passo:" -ForegroundColor Cyan
Write-Host "  .\New-WorkItems.ps1 -ProjectKey 'mural-digital' -AreaPath 'DotNet'"
