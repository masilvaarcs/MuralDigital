# EPIC-001 — CI/CD Foundation

> **ADO:** `marcosprogramador` · Area Path: `marcosprogramador\DotNet`  
> **Tags:** `dotnet` · `ci-cd` · `mural-digital`  
> **Prioridade:** P0 — bloqueia publicação na Store  
> **Status:** 🔵 Em andamento

---

## Visão

Criar um pipeline de CI no Azure DevOps que valide automaticamente cada Pull Request: build .NET MAUI para Windows, verificação de formato de código e publicação do artifact `.exe` self-contained. Objetivo: nenhum PR é mergeado sem CI verde.

---

## STORY-001 — Configurar pipeline CI para .NET MAUI (mural-digital)

> **Tags:** `ci-cd` · `dotnet`  
> **Estimativa:** M (1 dia)

**Como** desenvolvedor do MuralDigital  
**Quero** um pipeline CI que rode automaticamente a cada PR  
**Para** garantir que o código compila e está formatado antes de chegar à `main`

### Acceptance Criteria

- [ ] **AC-1:** Dado um PR aberto para `main`, quando o pipeline rodar, então `dotnet build -f net9.0-windows10.0.19041.0` deve passar sem erros
- [ ] **AC-2:** Dado um PR com código mal formatado, quando o pipeline rodar, então `dotnet format --verify-no-changes` deve falhar e bloquear o merge
- [ ] **AC-3:** Dado um PR com todos os checks verdes, então o merge é liberado automaticamente

### Tasks

| ID ADO | Título | Status |
|--------|--------|--------|
| (criado pelo script) | [TASK] Criar azure-pipelines.yml com build net9.0-windows | To Do |
| (criado pelo script) | [TASK] Configurar Service Connection GitHub para ADO no portal | To Do |
| (criado pelo script) | [TASK] Adicionar dotnet format e Roslyn analyzers ao pipeline | To Do |
| (criado pelo script) | [TASK] Testar pipeline com PR de validacao | To Do |
| (criado pelo script) | [TASK] Documentar pipeline no README | To Do |

### Detalhes técnicos

**Arquivo a criar:** `azure-pipelines.yml` na raiz do projeto

```yaml
# Estrutura esperada
trigger:
  - main

pool:
  vmImage: windows-latest

steps:
  - task: UseDotNet@2
    inputs:
      version: '9.x'

  - script: dotnet workload install maui-windows
    displayName: 'Install MAUI workload'

  - script: dotnet build MuralDigital.csproj -f net9.0-windows10.0.19041.0 --configuration Release
    displayName: 'Build Windows'

  - script: dotnet format --verify-no-changes
    displayName: 'Check code format'
```

**Service Connection:** GitHub → ADO (criar no portal: Project Settings → Service Connections → GitHub)

---

## STORY-002 — Configurar branch policy — exigir CI verde antes de merge

> **Tags:** `ci-cd` · `devops`  
> **Estimativa:** S (4 horas)

**Como** desenvolvedor  
**Quero** que o Azure DevOps bloqueie merges com CI com falha  
**Para** garantir que `main` sempre esteja em estado compilável

### Acceptance Criteria

- [ ] **AC-1:** Dado um PR com pipeline falhando, quando tentar fazer merge, então o botão "Complete" fica bloqueado
- [ ] **AC-2:** Dado um PR com todos os checks verdes, então o merge é liberado

### Detalhes técnicos

Configurar em: ADO → Repos → Branches → `main` → Branch policies → Build validation → selecionar pipeline

---

## STORY-003 — Publicar build Windows self-contained como artifact

> **Tags:** `ci-cd` · `deploy` · `dotnet`  
> **Estimativa:** S (4 horas)

**Como** usuário que quer instalar o app  
**Quero** um artifact `.exe` gerado automaticamente a cada merge na `main`  
**Para** não precisar buildar o projeto localmente para instalar

### Acceptance Criteria

- [ ] **AC-1:** Dado um merge na `main`, quando o pipeline rodar, então um artifact `MuralDigital-win-x64.exe` é publicado no ADO
- [ ] **AC-2:** O artifact pode ser baixado diretamente pelo painel do ADO

### Detalhes técnicos

```yaml
# Adicionar ao pipeline após o build
- script: dotnet publish MuralDigital.csproj -f net9.0-windows10.0.19041.0 -c Release --self-contained true -r win-x64 -o ./publish/win-x64
  displayName: 'Publish self-contained'

- task: PublishBuildArtifacts@1
  inputs:
    pathToPublish: './publish/win-x64'
    artifactName: 'MuralDigital-win-x64'
```

---

## Critérios de saída do Epic

- [ ] Pipeline CI rodando a cada PR para `main`
- [ ] Branch policy ativa — merge bloqueado sem CI verde
- [ ] Artifact `.exe` publicado automaticamente no merge
- [ ] Tempo médio do pipeline < 8 minutos
- [ ] README atualizado com badge de status do pipeline
