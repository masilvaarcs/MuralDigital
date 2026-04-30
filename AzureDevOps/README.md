# Azure DevOps — MuralDigital

Integração do repositório **MuralDigital** (.NET 9 MAUI) com Azure DevOps Boards e Pipelines.

## Setup (primeira vez)

```powershell
# 1. Criar .env com o PAT (já feito — não versionar)
# 2. Autenticar e criar Area Paths (apenas uma vez por máquina)
.\scripts\Setup-ADOConnection.ps1

# 3. Criar a estrutura inicial de Boards
.\scripts\New-WorkItems.ps1 -ProjectKey "mural-digital" -AreaPath "DotNet"
```

## Hierarquia de work items (template Basic)

```
Epic
└── Issue  (= User Story no template Basic)
    └── Task
```

| Tipo no ADO | Equivalente | Prefixo no título |
|---|---|---|
| Epic | Epic | `[EPIC]` |
| Issue | User Story | `[STORY]` |
| Task | Task | `[TASK]` |
| Bug | Bug | `[BUG]` |

## Area Path

Todos os work items deste repositório usam:
```
marcosprogramador\DotNet
```

## Estrutura de pastas

```
AzureDevOps/
├── README.md              ← este arquivo
├── .env                   ← PAT local (nunca commitar)
├── 01-Vision/
│   ├── VISION.md
│   └── ROADMAP.md
├── 02-Templates/
│   ├── epic.template.md
│   ├── user-story.template.md
│   ├── task.template.md
│   └── bug.template.md
├── 03-Backlog/
│   └── EPIC-001-CI-Foundation.md
└── scripts/
    ├── Setup-ADOConnection.ps1
    └── New-WorkItems.ps1
```

## CI/CD — Fases planejadas

| Fase | O que faz | Status |
|---|---|---|
| Phase 1 — Validate | dotnet build + testes + format check | Planejado |
| Phase 2 — Quality | cobertura + Roslyn analyzers | Futuro |
| Phase 3 — Artifacts | Self-contained publish Windows x64 | Futuro |
| Phase 4 — Staging | Deploy automático (aprovação manual) | Futuro |

## Links úteis

- [Board](https://dev.azure.com/masilvaarcs/marcosprogramador/_boards/board/t/marcosprogramador%20Team/Issues)
- [Pipelines](https://dev.azure.com/masilvaarcs/marcosprogramador/_build)
- [Gerar PAT](https://dev.azure.com/masilvaarcs/_usersSettings/tokens)
