# 🚨 MuralDigital

**Gerador de Mural On-Line para WhatsApp** — Aplicativo .NET MAUI que cria mensagens formatadas de mural congregacional com links encurtados e envio direto via WhatsApp.

---

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [Funcionalidades](#-funcionalidades)
- [Tecnologias](#-tecnologias)
- [Arquitetura](#-arquitetura)
- [Como Usar o App](#-como-usar-o-app)
  - [Tela Principal](#1-tela-principal)
  - [Preenchendo os Dados](#2-preenchendo-os-dados)
  - [Encurtando URLs](#3-encurtando-urls)
  - [Visualizando o Mural](#4-visualizando-o-mural)
  - [Copiando o Texto](#5-copiando-o-texto)
  - [Enviando via WhatsApp](#6-enviando-via-whatsapp)
- [Estilos de Formatação](#-estilos-de-formatação)
- [Gerenciamento de Contatos](#-gerenciamento-de-contatos)
- [Publicação e Execução](#-publicação-e-execução)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Dados Persistidos](#-dados-persistidos)

---

## 🎯 Visão Geral

O **MuralDigital** automatiza a criação de murais informativos formatados para WhatsApp. Ele transforma links do Google Drive em URLs curtas descritivas e gera textos prontos com formatação (negrito, itálico, emojis) que podem ser copiados e colados diretamente em conversas do WhatsApp.

### Fluxo resumido:

```
Preencher Dados → Encurtar URLs → Escolher Estilo → Copiar/Enviar WhatsApp
```

---

## ✨ Funcionalidades

| Funcionalidade | Descrição |
|---|---|
| **Grupos dinâmicos** | Adicione/remova grupos e itens livremente |
| **URLs descritivas** | Links encurtados com nomes legíveis (ex: `tinyurl.com/Atual-Abril-2026`) |
| **4 estilos de texto** | Clássico, Compacto, Destaque e Formal |
| **Preview formatado** | Visualização fiel ao WhatsApp com **negrito**, _itálico_ e links |
| **Copiar com 1 clique** | Texto copiado automaticamente para a área de transferência |
| **Envio multi-contato** | Abre WhatsApp sequencialmente para cada contato selecionado |
| **Persistência JSON** | Dados salvos localmente, carregados automaticamente |
| **Feedback visual** | Barra de status com emojis indicando progresso e resultados |

---

## 🛠 Tecnologias

| Componente | Tecnologia |
|---|---|
| Framework | **.NET 9** + **.NET MAUI** |
| MVVM | **CommunityToolkit.Mvvm 8.4.2** |
| Serialização | **System.Text.Json** |
| Encurtador | **TinyURL API** (alias descritivo) com fallback **is.gd** |
| Plataforma alvo | **Windows 10/11** (self-contained) |

---

## 🏗 Arquitetura

O projeto segue o padrão **MVVM** (Model-View-ViewModel):

```
MuralDigital/
├── Models/           ← Entidades de dados (MuralConfig, MuralGroup, MuralItem, WhatsAppContact)
├── ViewModels/       ← Lógica de apresentação (MainViewModel, PreviewViewModel)
├── Views (*.xaml)    ← Páginas XAML (MainPage, PreviewPage)
├── Services/         ← Serviços de negócio
│   ├── MuralDataService        ← Persistência JSON
│   ├── UrlShortenerService     ← Encurtamento de URLs (TinyURL + is.gd)
│   └── WhatsAppTextGenerator   ← 4 geradores de estilo
├── Converters/       ← Conversores XAML (WhatsAppFormattedText, InvertBool, etc.)
└── Scripts/          ← Scripts de automação (Publish.ps1)
```

---

## 📖 Como Usar o App

### 1. Tela Principal

Ao abrir o app, a **tela principal** carrega automaticamente os dados salvos (ou cria um mural padrão na primeira execução).

**Campos do cabeçalho:**
- **Congregação**: Nome da congregação (ex: "Cong. Auxiliadora")
- **Título**: Título do mural (ex: "MURAL ON-LINE")
- **Visita (opcional)**: Nota especial exibida no topo do mural

**Barra de ações (rodapé):**

| Botão | Ação |
|---|---|
| `+ Grupo` | Adiciona um novo grupo ao mural |
| `🔗 Encurtar Tudo` | Encurta todas as URLs de todos os grupos |
| `👁 Visualizar` | Abre a tela de pré-visualização |
| `📋 Copiar` | Gera o texto (estilo Clássico) e copia para a área de transferência |
| `💾 Salvar` | Salva todos os dados no disco |

---

### 2. Preenchendo os Dados

Cada **grupo** possui:
- **Emoji**: Ícone do grupo (editável)
- **Título**: Nome do grupo (ex: "Programação Arranjo de Campo")
- **Subtítulo**: Texto adicional (opcional)
- **Itens**: Lista de links com label

Cada **item** possui:
- **Label**: Descrição (ex: "Semana 1", "Atual (Abril/2026)")
- **URL original**: Link do Google Drive
- **URL encurtada**: Gerada automaticamente (exibida em verde ✅)

> 💡 **Dica**: Use o botão `+ Item` dentro de cada grupo para adicionar novos itens.

---

### 3. Encurtando URLs

Há duas formas de encurtar:

1. **Individual**: Botão "Encurtar" ao lado de cada item
2. **Em massa**: Botão `🔗 Encurtar Tudo` na barra de ações

O serviço gera **aliases descritivos** baseados no label:
- Label "Atual (Abril/2026)" → `tinyurl.com/Atual-Abril-2026`
- Label "Semana 1" → `tinyurl.com/Semana-1`

**Prioridade de encurtamento:**
1. TinyURL com alias descritivo
2. TinyURL com alias + sufixo numérico (se alias já existir)
3. TinyURL aleatório
4. is.gd com slug
5. is.gd aleatório
6. URL original (último recurso)

---

### 4. Visualizando o Mural

Ao clicar em `👁 Visualizar`:

1. URLs alteradas são re-encurtadas automaticamente
2. A **tela de preview** abre com o texto formatado
3. Escolha entre **4 estilos** no seletor superior
4. O texto muda em tempo real conforme o estilo selecionado

---

### 5. Copiando o Texto

**Na tela principal** → `📋 Copiar`:
- Gera com estilo **Clássico** (padrão)
- Copia automaticamente
- Salva os dados

**Na tela de preview** → `📋 Copiar`:
- Usa o **estilo selecionado** no Picker
- Copia automaticamente

---

### 6. Enviando via WhatsApp

Na tela de preview, selecione os contatos desejados e clique em `✅ Enviar via WhatsApp`:

1. O texto é copiado para a área de transferência
2. O WhatsApp Web/Desktop é aberto para **cada contato selecionado**
3. **Cole** o texto (`Ctrl+V`) na conversa e envie

> ⚠️ O texto é enviado via **clipboard** (Ctrl+V) para preservar emojis e formatação.

---

## 🎨 Estilos de Formatação

| Estilo | Descrição | Exemplo |
|---|---|---|
| **Clássico** | Emojis e negrito, espaçamento confortável | `📌 *Semana 1*` + `👉 link` |
| **Compacto** | Menos espaço, direto ao ponto | `• *Semana 1:* link` |
| **Destaque** | Bordas decorativas, emojis chamativos | `╔══╗` + `✨ *Título* ✨` |
| **Formal** | Limpo e organizado, poucos emojis | `■ Título` + `→ link` |

---

## 👥 Gerenciamento de Contatos

A tela de preview permite:
- **Selecionar/desmarcar** contatos existentes (checkbox)
- **Adicionar** novos contatos (nome + telefone)
- **Remover** contatos não-padrão (botão ✕)

**Contatos padrão** (não removíveis):
- Marcos Silva — (51) 98422-8067
- Charlie Silva — (51) 8447-2509

> Os contatos são salvos junto com a configuração do mural.

---

## 🚀 Publicação e Execução

### Executando a partir da publicação self-contained

1. Extraia o arquivo `.7z` da pasta `VersaoAtual/`
2. Execute `MuralDigital.exe`
3. **Não requer** .NET Runtime instalado — tudo está incluso

### Gerando nova publicação

Use o script de automação:

```powershell
.\Scripts\Publish.ps1
```

O script irá:
1. Publicar em modo **self-contained** para Windows x64
2. Comprimir o resultado em `.7z`
3. Copiar o arquivo para `VersaoAtual/`

Veja mais detalhes em [`Scripts/Publish.ps1`](Scripts/Publish.ps1).

---

## 📁 Estrutura do Projeto

```
MuralDigital/
│
├── App.xaml / App.xaml.cs           # Entry point, recursos globais
├── AppShell.xaml                    # Navegação Shell (rota "preview")
├── MauiProgram.cs                   # DI container, registro de serviços
│
├── MainPage.xaml / .cs              # Tela principal (editor do mural)
├── PreviewPage.xaml / .cs           # Tela de preview + envio WhatsApp
│
├── Models/
│   ├── MuralConfig.cs               # Configuração completa do mural
│   ├── MuralGroup.cs                # Grupo (ex: "Arranjo de Campo")
│   ├── MuralItem.cs                 # Item com label + URLs
│   └── WhatsAppContact.cs           # Contato WhatsApp (nome + fone)
│
├── ViewModels/
│   ├── MainViewModel.cs             # VM da tela principal
│   ├── PreviewViewModel.cs          # VM do preview + envio
│   ├── MuralGroupViewModel.cs       # VM de grupo (encurtar, add item)
│   ├── MuralItemViewModel.cs        # VM de item (dirty tracking)
│   └── ContactViewModel.cs          # VM de contato
│
├── Services/
│   ├── MuralDataService.cs          # Persistência JSON local
│   ├── UrlShortenerService.cs       # TinyURL + is.gd
│   └── WhatsAppTextGenerator.cs     # 4 estilos de geração
│
├── Converters/
│   └── Converters.cs                # WhatsAppFormattedText, InvertBool, etc.
│
├── Scripts/
│   └── Publish.ps1                  # Script de publicação automatizada
│
├── VersaoAtual/                     # Publicação mais recente (.7z)
│
├── Resources/
│   ├── AppIcon/                     # Ícone do app
│   ├── Fonts/                       # Fontes
│   ├── Images/                      # Imagens
│   ├── Splash/                      # Splash screen
│   └── Styles/                      # Colors.xaml, Styles.xaml
│
└── Platforms/                       # Código específico por plataforma
    ├── Android/
    ├── iOS/
    ├── MacCatalyst/
    ├── Tizen/
    └── Windows/
```

---

## 💾 Dados Persistidos

Os dados são salvos em JSON no diretório local do app:

```
%LOCALAPPDATA%\<User>\com.companyname.muraldigital\Data\mural_config.json
```

O arquivo contém:
- Cabeçalho (congregação, título, nota de visita)
- Grupos e itens (com URLs originais e encurtadas)
- Contatos WhatsApp
- Rodapé

---

## 📄 Licença

Projeto de uso interno. Todos os direitos reservados.
