# FASE 6 — Content Media

FASE 6 mejora la asociación de multimedia sin mover la responsabilidad de almacenamiento a `DholeContentService`.

## Responsabilidad

- `DholeStorageService` conserva los archivos reales.
- `content.media_references` conserva la referencia al archivo de Storage.
- `content.content_media` relaciona uno o más recursos multimedia con un `ContentItem`.

No se guardan bytes de imágenes, videos o documentos en PostgreSQL.

## Tabla `content.content_media`

Campos funcionales:

- `Id`
- `ContentId`
- `MediaReferenceId`
- `Role`
- `SortOrder`
- `AltTextOverride`
- `CaptionOverride`
- `FocalX`
- `FocalY`
- `SettingsJson`

La tabla también usa las columnas estándar de auditoría y soft-delete del servicio.

## Roles soportados

- `hero`
- `featured`
- `gallery`
- `thumbnail`
- `background`
- `video`
- `video.poster`
- `banner.desktop`
- `banner.mobile`
- `open-graph`

## Reglas

- El contenido debe existir y no estar eliminado.
- `MediaReference` debe existir y no estar eliminado.
- No se permite repetir `ContentId + MediaReferenceId + Role` mientras la relación esté activa.
- `SortOrder` debe ser mayor o igual a cero.
- `FocalX` y `FocalY`, cuando existan, deben estar entre `0` y `1`.
- `SettingsJson`, cuando exista, debe ser un objeto JSON válido.
- Los endpoints de actualización/eliminación comprueban que la asociación pertenezca al `ContentId` de la URL.

## API administrativa

Base:

`/api/content/items/{contentId}/media`

Operaciones:

- `GET /` — listar asociaciones, opcionalmente por `role`.
- `GET /roles` — obtener roles permitidos.
- `POST /` — asociar `MediaReference`.
- `PUT /{id}` — editar rol, orden, overrides, focal point y settings.
- `DELETE /{id}` — desvincular mediante soft-delete.

Lectura usa `cms.view`; escritura usa `cms.edit`.

## Fuera de alcance

FASE 6 no cambia la carga física de archivos, validación MIME, thumbnails, WebP/AVIF ni procesamiento de video. Eso corresponde a FASE 7 en `DholeStorageService`.
