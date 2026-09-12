# FASE 2 — Mejorar ContentItem

## Objetivo

Extender el `ContentItem` existente sin reemplazarlo y permitir slugs repetidos entre idiomas dentro del mismo sitio.

## Campos agregados

- `ParentContentId`
- `TranslationGroupId`
- `TemplateKey`
- `UnpublishAtUtc`
- `SitemapPriority`
- `SitemapChangeFrequency`

## Unicidad del slug

Antes:

```text
site + slug
```

Ahora:

```text
site + locale + slug
```

Esto permite, por ejemplo, que un mismo sitio tenga contenido localizado sin que un slug utilizado en español bloquee el equivalente de otro idioma.

## Persistencia

La migración `20260912112000_Phase2ContentItemLocalization`:

1. agrega las seis columnas a `content.content_items`;
2. agrega la relación opcional de `ParentContentId` hacia `content_items`;
3. reemplaza el índice único `site + slug` por `site + locale + slug`;
4. agrega índices para `ParentContentId` y `TranslationGroupId`;
5. limita `SitemapPriority` al rango de 0 a 1.

## Aplicación y API

Las operaciones de crear y actualizar contenido aceptan y devuelven los nuevos campos.

La validación de slug del repositorio y de los command handlers también incluye `Locale`, por lo que la regla de la aplicación coincide con la restricción de PostgreSQL.

## Pruebas

`Phase2ContentItemTests` valida:

- persistencia de los nuevos metadatos en el agregado;
- validación de `SitemapPriority`;
- existencia del índice único `SiteKey + Locale + Slug` en el modelo EF;
- eliminación de la restricción anterior `SiteKey + Slug` del modelo.
