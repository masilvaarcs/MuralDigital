<#
.SYNOPSIS
    Cria work items (Epics, Issues, Tasks) no Azure DevOps para um repositório.

.DESCRIPTION
    Cria automaticamente a estrutura inicial de Boards para um projeto no ADO:
    - 1 Epic
    - 3 Issues (User Stories) vinculadas ao Epic
    - 5 Tasks vinculadas à primeira Issue (CI pipeline)

    Pré-requisito: Setup-ADOConnection.ps1 já executado.

    IMPORTANTE — Template Basic do ADO:
    - Tipos válidos: "Epic", "Issue", "Task", "Bug"
    - NÃO existe "User Story" nem "PBI" no template Basic
    - Hierarquia: Epic → Issue → Task

.PARAMETER ProjectKey
    Identificador curto do repositório (ex: "mural-digital", "smart-menu").
    Usado como tag e para nomear os work items.

.PARAMETER AreaPath
    Area Path do ADO para este projeto. Padrão: "marcosprogramador\DotNet"
    Opções: Golang | DotNet | Python | JavaScript | DevOps

.PARAMETER EpicTitle
    Título do Epic principal. Se omitido, usa "[EPIC] <AreaPath> — CI/CD Foundation".

.PARAMETER WhatIf
    Simula a execução sem criar nada no ADO (dry-run).

.EXAMPLE
    .\New-WorkItems.ps1 -ProjectKey "mural-digital" -AreaPath "DotNet"
    .\New-WorkItems.ps1 -ProjectKey "mural-digital" -AreaPath "DotNet" -WhatIf
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectKey,

    [Parameter(Mandatory = $false)]
    [ValidateSet("Golang", "DotNet", "Python", "JavaScript", "DevOps")]
    [string]$AreaPath = "DotNet",

    [Parameter(Mandatory = $false)]
    [string]$EpicTitle = "",

    [switch]$WhatIf
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$Project     = "marcosprogramador"
$FullArea    = "$Project\$AreaPath"
$Tag         = $ProjectKey.ToLower()
$DryRun      = $WhatIf.IsPresent

if ($EpicTitle -eq "") {
    $EpicTitle = "[EPIC] $AreaPath — CI/CD Foundation"
}

Write-Host "`n=== New-WorkItems.ps1 ===" -ForegroundColor Cyan
Write-Host "Projeto ADO : $Project"
Write-Host "Area Path   : $FullArea"
Write-Host "Tag         : $Tag"
Write-Host "Epic        : $EpicTitle"
if ($DryRun) { Write-Host "`n[DRY-RUN] Nenhum item sera criado no ADO`n" -ForegroundColor Yellow }
Write-Host ""

# Função auxiliar para criar work item
# Projeto usa template Basic: Epic → Issue → Task
function New-WorkItem {
    param(
        [string]$Type,
        [string]$Title,
        [string]$ParentId = "",
        [string]$Tags = ""
    )

    $allTags = ($Tags + "; " + $Tag).Trim("; ")

    Write-Host "  Criando [$Type] $Title" -ForegroundColor DarkCyan

    if ($DryRun) {
        Write-Host "    [DRY-RUN] az boards work-item create --type '$Type' --title '$Title' --area '$FullArea'" -ForegroundColor DarkYellow
        return "DRY-RUN-ID"
    }

    $json = az boards work-item create `
        --type $Type `
        --title $Title `
        --project $Project `
        --fields "System.AreaPath=$FullArea" "System.Tags=$allTags" `
        --output json

    $item = $json | ConvertFrom-Json
    $id = $item.id

    # Vincular ao parent se informado
    # IMPORTANTE: usar "Parent" (nome de exibição), NÃO o ReferenceName
    # NÃO passar --project no relation add (não é aceito)
    if ($ParentId -ne "" -and $ParentId -ne "DRY-RUN-ID") {
        az boards work-item relation add `
            --id $id `
            --relation-type "Parent" `
            --target-id $ParentId `
            --only-show-errors | Out-Null
    }

    Write-Host "    Criado com ID: $id" -ForegroundColor Green
    return [string]$id
}

# ─────────────────────────────────────────
# EPIC
# ─────────────────────────────────────────
Write-Host "[ 1 ] Criando Epic..." -ForegroundColor Yellow
$epicId = New-WorkItem `
    -Type "Epic" `
    -Title $EpicTitle `
    -Tags "ci-cd"

# ─────────────────────────────────────────
# ISSUES (User Stories no template Basic)
# ─────────────────────────────────────────
Write-Host "`n[ 2 ] Criando Issues (User Stories)..." -ForegroundColor Yellow

$stories = @(
    @{
        Title = "[STORY] Configurar pipeline CI para $AreaPath ($ProjectKey)"
        Tags  = "ci-cd; dotnet"
    },
    @{
        Title = "[STORY] Configurar branch policy — exigir CI verde antes de merge"
        Tags  = "ci-cd; devops"
    },
    @{
        Title = "[STORY] Publicar build Windows self-contained como artifact"
        Tags  = "ci-cd; deploy; dotnet"
    }
)

$storyIds = @()
foreach ($story in $stories) {
    $id = New-WorkItem `
        -Type "Issue" `
        -Title $story.Title `
        -ParentId $epicId `
        -Tags $story.Tags
    $storyIds += $id
}

# ─────────────────────────────────────────
# TASKS para a primeira Issue (CI pipeline)
# ─────────────────────────────────────────
Write-Host "`n[ 3 ] Criando Tasks para a Issue de CI..." -ForegroundColor Yellow

$tasks = @(
    "[TASK] Criar azure-pipelines.yml com build net9.0-windows"
    "[TASK] Configurar Service Connection GitHub para ADO no portal"
    "[TASK] Adicionar dotnet format e Roslyn analyzers ao pipeline"
    "[TASK] Testar pipeline com PR de validacao"
    "[TASK] Documentar pipeline no README"
)

foreach ($taskTitle in $tasks) {
    New-WorkItem `
        -Type "Task" `
        -Title $taskTitle `
        -ParentId $storyIds[0] `
        -Tags "ci-cd" | Out-Null
}

# ─────────────────────────────────────────
# Resultado final
# ─────────────────────────────────────────
Write-Host "`n=== Work items criados com sucesso! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Epic ID  : $epicId"
Write-Host "Issues   : $($storyIds -join ', ')"
Write-Host ""
Write-Host "Ver no Board:" -ForegroundColor Cyan
Write-Host "  https://dev.azure.com/masilvaarcs/$Project/_boards/board/t/$Project%20Team/Issues"
Write-Host ""

if (-not $DryRun) {
    Write-Host "Listar work items criados:" -ForegroundColor Cyan
    az boards query `
        --wiql "SELECT [System.Id], [System.WorkItemType], [System.Title], [System.State], [System.Parent] FROM WorkItems WHERE [System.AreaPath] = '$FullArea' AND [System.Tags] CONTAINS '$Tag' ORDER BY [System.Id]" `
        --project $Project `
        -o table
}
