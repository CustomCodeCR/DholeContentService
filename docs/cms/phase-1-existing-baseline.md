# FASE 1 — Línea base existente del CMS

## Objetivo

Preservar y reutilizar lo que ya existe en `DholeContentService` antes de extender el CMS. Esta fase no crea nuevas tablas funcionales ni reemplaza entidades existentes.

## Inventario verificado

| Concepto requerido | Implementación existente |
| --- | --- |
| ContentItem | `Dhole.Content.Domain.ContentItems.Entities.ContentItem` / `ServiceDbContext.ContentItems` |
| ContentRevision | `Dhole.Content.Domain.ContentItems.Entities.ContentRevision` / `ServiceDbContext.ContentRevisions` |
| Taxonomy | `TaxonomyTerm` + `ContentTaxonomy` / `ServiceDbContext.TaxonomyTerms` y `ContentTaxonomies` |
| MediaReference | `Dhole.Content.Domain.Media.Entities.MediaReference` / `ServiceDbContext.MediaReferences` |
| NavigationMenu | `NavigationMenu` / `ServiceDbContext.NavigationMenus` |
| NavigationMenuItem | `NavigationMenuItem` / `ServiceDbContext.NavigationMenuItems` |
| SiteSettings | `SiteSetting` / `ServiceDbContext.SiteSettings` |
| Inbox | `InboxMessage` / `ServiceDbContext.InboxMessages` |
| Outbox | `OutboxMessage` / `ServiceDbContext.OutboxMessages` |

## Tipos de contenido existentes

Se mantienen como línea base:

- `Page`
- `News`
- `Post`
- `Announcement`
- `Banner`
- `Video`
- `ReusableBlock`

## Reglas para las siguientes fases

1. No volver a crear una entidad o tabla si la capacidad ya existe.
2. Extender `ContentItem`, `ContentRevision`, taxonomías, multimedia, navegación y settings sobre sus implementaciones actuales.
3. Mantener Inbox/Outbox existentes para integración y mensajería.
4. Cualquier migración futura debe modificar o relacionar el esquema actual de forma incremental; no debe reemplazarlo.
5. Las pruebas `Phase1ExistingCmsBaselineTests` funcionan como protección mínima contra la eliminación accidental de esta línea base.

## Cambios realizados en FASE 1

- Se corrigieron los unit tests existentes para usar los namespaces y responsabilidades actuales del dominio.
- Se agregaron pruebas de regresión para verificar que los conceptos CMS existentes continúan expuestos por `ServiceDbContext`.
- Se agregaron pruebas que preservan los tipos de contenido actuales.
- Se agregó CI para compilar y ejecutar tests en pull requests hacia `develop`.

No se agregó ninguna tabla funcional nueva en esta fase.
