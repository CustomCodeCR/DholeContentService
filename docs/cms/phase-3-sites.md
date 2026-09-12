# FASE 3 — Sites

## Objetivo

Crear el catálogo de sitios del CMS sin mezclarlo con `SiteSetting`.

`Site` representa la identidad y configuración base de un sitio administrado por el CMS. `SiteSetting` continúa siendo un almacén de configuración key/value por `SiteKey`.

## Tabla

Se crea `content.sites` con:

- `Id`
- `SiteKey`
- `Name`
- `PrimaryDomain`
- `DefaultLocale`
- `TimeZone`
- `LogoMediaId`
- `FaviconMediaId`
- `DefaultOpenGraphMediaId`
- `Status`
- campos de auditoría del framework (`CreatedAtUtc`, `UpdatedAtUtc`, etc.)

## Reglas

- `SiteKey` es único entre registros activos.
- `PrimaryDomain` es único entre registros activos.
- `SiteKey` se normaliza a minúsculas.
- `PrimaryDomain` se guarda sin protocolo ni `/` final.
- los tres campos multimedia referencian `content.media_references` y usan `ON DELETE SET NULL`.

## Sitio principal

La migración crea el sitio inicial:

- `SiteKey`: `main`
- nombre: `Logística Castro Fallas`
- dominio: `logisticacastrofallas.com`
- locale: `es-CR`
- zona horaria: `America/Costa_Rica`
- estado: `Active`

## API administrativa

`/api/content/sites`

- `GET /` — listar sitios (`cms.view`)
- `GET /{siteKey}` — consultar sitio (`cms.view`)
- `POST /` — crear sitio (`cms.settings.edit`)
- `PUT /{siteKey}` — actualizar sitio (`cms.settings.edit`)

La implementación reutiliza CQRS, PostgreSQL y los scopes existentes del ContentService.
