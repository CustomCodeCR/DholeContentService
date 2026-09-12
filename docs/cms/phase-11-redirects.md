# FASE 11 — Redirects

## Objetivo

Agregar redirects administrables por sitio para conservar URLs anteriores cuando Mercadeo cambia una ruta pública.

## Persistencia

Se agrega `content.redirects` con los campos funcionales `Id`, `SiteKey`, `SourcePath`, `TargetUrl`, `StatusCode`, `IsActive`, `ValidFromUtc` y `ValidToUtc`, además de las columnas estándar de auditoría y soft-delete.

Solo se permiten códigos HTTP `301` y `302`. `SiteKey + SourcePath` es único entre registros no eliminados. La ruta de origen se normaliza con `/` inicial, sin `/` final salvo raíz y en minúsculas. El destino puede ser una ruta relativa o una URL HTTP/HTTPS.

## API

Administración bajo `/api/content/redirects`:

- `GET /?siteKey=main` — `cms.view`.
- `POST /` — `cms.edit`.
- `PUT /{id}` — `cms.edit`.
- `DELETE /{id}` — `cms.edit`.

Resolución pública:

- `GET /api/content/public/redirect?siteKey=main&path=/ruta-antigua`.

La resolución solo devuelve redirects activos y dentro de su ventana de vigencia.

## Cambio automático de URL

`PUT /api/content/routes/{id}` acepta `CreatePermanentRedirect`. Cuando la ruta cambia y esta opción es `true`, la actualización de `content_routes` y el redirect `301` desde la ruta anterior hacia la nueva se guardan en la misma unidad de trabajo. Si ya existe un redirect para la ruta anterior, se actualiza hacia el nuevo destino.

DholeWeb ofrece esta opción al editar la dirección pública de contenido existente.

## Scopes

FASE 11 no adelanta `cms.redirects.edit`; ese scope pertenece a FASE 23. Hasta entonces la escritura reutiliza `cms.edit`.

FASE 11 no implementa aprobación de contenido de FASE 12.
