# FASE 4 — Content Routes

## Objetivo

Separar la URL pública del `Slug` del contenido y hacer que la resolución canónica dependa de una tabla de rutas.

## Tabla

`content.content_routes` contiene:

- `Id`
- `SiteKey`
- `ContentId`
- `Locale`
- `Path`
- `IsPrimary`
- `IsActive`
- campos de auditoría del framework

## Reglas

1. `SiteKey + Locale + Path` es único.
2. Solo puede existir una ruta primaria por `ContentId + Locale`.
3. Una ruta debe pertenecer al mismo `SiteKey` y `Locale` que su `ContentItem`.
4. Las rutas públicas solo resuelven contenido `Published`.
5. Una ruta inactiva no puede resolver contenido público.
6. El path se normaliza con slash inicial, sin slash final salvo `/`, sin dobles slash y en minúscula.

## Compatibilidad

La migración crea una ruta primaria para el contenido existente usando:

```text
/{idioma}/{slug}
```

Ejemplos:

```text
/es/servicios
/en/services
```

Los endpoints públicos existentes por slug se mantienen temporalmente para no romper consumidores actuales.

La resolución canónica nueva es:

```text
GET /api/content/public/resolve?siteKey=main&locale=es-CR&path=/es/servicios
```

## Administración

Endpoints CMS:

```text
GET  /api/content/routes/content/{contentId}
POST /api/content/routes
PUT  /api/content/routes/{id}
```

Lectura utiliza `cms.view` y escritura utiliza `cms.edit`.

## Integridad de SiteKey

`content.sites.site_key` está protegido por un índice único parcial debido al soft-delete. PostgreSQL no permite referenciar un índice único parcial desde una FK, por lo que la relación `ContentRoute -> Site` se valida en la capa de aplicación.

La FK física `ContentRoute -> ContentItem` sí se mantiene con `ON DELETE CASCADE`.
