# FASE 8 — Sistema de banners y placements

FASE 8 implementa en `DholeContentService` los espacios administrables donde Mercadeo puede ubicar banners u otros contenidos.

## Persistencia

Se agregan:

- `content.placements`
- `content.placement_items`

`placements` define `SiteKey`, `Code`, `Name`, `AllowedTypesJson`, `MaxItems`, `SettingsJson` e `IsActive`.

`placement_items` relaciona un placement con un `ContentItem` y guarda orden, ventana de vigencia, settings e indicador de actividad.

## Reglas

- `SiteKey + Code` es único entre placements no eliminados.
- `Code` se normaliza a minúsculas y admite letras, números, `.`, `-` y `_`.
- `AllowedTypesJson` debe ser un arreglo JSON no vacío con valores válidos de `ContentType`.
- un contenido solo se puede asociar una vez al mismo placement mientras la asociación no esté eliminada.
- contenido y placement deben pertenecer al mismo sitio.
- el tipo del contenido debe estar permitido por `AllowedTypesJson`.
- `MaxItems` debe ser mayor a cero; funciona como configuración del límite de presentación y no bloquea contenido programado futuro.
- `ValidToUtc`, cuando existen ambas fechas, debe ser posterior a `ValidFromUtc`.
- `SettingsJson` debe ser un objeto JSON válido cuando se envía.

## API administrativa

Base: `/api/content/placements`

- `GET /?siteKey=main`
- `GET /{id}`
- `POST /`
- `PUT /{id}`
- `DELETE /{id}`
- `GET /{placementId}/items`
- `POST /{placementId}/items`
- `PUT /{placementId}/items/{itemId}`
- `DELETE /{placementId}/items/{itemId}`

Lectura usa `cms.view`. Escritura usa el scope ya existente `cms.banners.edit`.

## Fuera de alcance

FASE 8 no crea Collections, no cambia DholeWeb y no agrega todavía endpoints públicos de placements. La API pública se implementa en una fase posterior según la especificación del CMS.
