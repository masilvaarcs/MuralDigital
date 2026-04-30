# Roadmap — MuralDigital

## Epics planejados

| Epic | Título | Prioridade | Status |
|------|--------|-----------|--------|
| EPIC-001 | CI/CD Foundation | P0 | 🔵 Em andamento |
| EPIC-002 | Publicação Microsoft Store | P1 | ⬜ Planejado |
| EPIC-003 | Suporte a Android | P2 | ⬜ Planejado |
| EPIC-004 | Gestão avançada de contatos | P2 | ⬜ Planejado |
| EPIC-005 | Histórico de murais enviados | P3 | ⬜ Planejado |

---

## EPIC-001 — CI/CD Foundation (P0)

**Objetivo:** Pipeline automatizado que valida build, formato de código e publica artifact Windows.

**Issues:**
1. Configurar pipeline CI para .NET MAUI (mural-digital)
2. Configurar branch policy — exigir CI verde antes de merge
3. Publicar build Windows self-contained como artifact

**Critério de conclusão:** PR sem CI verde bloqueado de merge; artifact `.exe` gerado automaticamente.

---

## EPIC-002 — Publicação Microsoft Store (P1)

**Objetivo:** Publicar MuralDigital na Microsoft Store via `msstore` CLI, com pipeline de publicação automatizado.

**Issues:**
1. Configurar identidade de app no Partner Center
2. Adicionar stage de Package (MSIX) ao pipeline
3. Automatizar submissão via msstore CLI

---

## EPIC-003 — Suporte a Android (P2)

**Objetivo:** Garantir que o app funciona no Android com a mesma experiência do Windows.

**Issues:**
1. Validar layout em dispositivos Android (320dp-480dp)
2. Resolver permissões de clipboard e abertura de WhatsApp no Android
3. Publicar na Google Play (opcional)

---

## EPIC-004 — Gestão avançada de contatos (P2)

**Objetivo:** Importar/exportar contatos, grupos de envio e histórico de murais por contato.

---

## EPIC-005 — Histórico de murais enviados (P3)

**Objetivo:** Salvar os últimos N murais gerados com data, contatos e estilo usado.
