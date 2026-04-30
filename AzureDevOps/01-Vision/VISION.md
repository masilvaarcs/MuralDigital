# Vision — MuralDigital

## Problema

Profissionais e organizações que compartilham conteúdo semanal por WhatsApp precisam montar murais com múltiplos links. Os links gerados por ferramentas como Google Drive têm 80-100+ caracteres, resultando em mensagens ilegíveis, difíceis de copiar e pouco profissionais.

## Solução

**MuralDigital** é um aplicativo .NET 9 MAUI para Windows (com suporte a Android/iOS) que transforma links longos em murais elegantes prontos para colar no WhatsApp — com encurtamento automático de URLs, 4 estilos de formatação e envio sequencial para múltiplos contatos.

## Proposta de valor

> *"Adeus, links monstruosos. Olá, mural bonito."*

Montando um mural de 6 links que antes levava 10 minutos de formatação manual, o app entrega o resultado em menos de 2 minutos.

## Arquitetura

```
MuralDigital/
├── ViewModels/           ← MVVM (CommunityToolkit.Mvvm)
│   ├── MainViewModel     ← orquestra grupos/itens e encurtamento
│   ├── PreviewViewModel  ← visualização + cópia do mural formatado
│   ├── MuralGroupViewModel / MuralItemViewModel
│   └── ContactViewModel
├── Services/
│   ├── UrlShortenerService   ← integração TinyURL API
│   ├── MuralDataService      ← persistência local (Preferences + JSON)
│   └── WhatsAppTextGenerator ← 4 estilos de formatação
├── Models/
├── Converters/
└── Platforms/            ← Windows / Android / iOS / macOS
```

**Padrões aplicados:**
- **MVVM** via CommunityToolkit.Mvvm
- **Adapter** — `UrlShortenerService` isola a dependência da API TinyURL
- **SRP** — cada ViewModel tem responsabilidade única
- **DI** via `MauiProgram.cs`

## Público-alvo

- Organizações religiosas, educacionais e comunitárias
- Qualquer pessoa que envie boletins/murais recorrentes por WhatsApp

## Métricas de sucesso

- [ ] App publicado na Microsoft Store
- [ ] Build CI verde em < 5 minutos
- [ ] 0 crashes críticos reportados
- [ ] Suporte a Android comprovado (testes manuais)

## Constraints

- Plataforma primária: **Windows 10/11** (já publicado como `.exe` unpackaged)
- API TinyURL: rate limit da conta gratuita
- .NET 9 MAUI — sem Blazor Hybrid
- Sem backend próprio (app 100% client-side)
