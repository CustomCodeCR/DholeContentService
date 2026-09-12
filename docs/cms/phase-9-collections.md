# FASE 9 — Collections

## Objetivo

Crear un modelo genérico para información repetitiva del CMS sin introducir una tabla distinta para cada tipo de contenido.

Las Collections pueden representar, entre otros:

- preguntas frecuentes;
- testimonios;
- clientes;
- logos;
- aliados;
- beneficios;
- equipo;
- certificaciones;
- estadísticas.

## Persistencia

Se agregan las tablas `content.collections` y `content.collection_items`.

Una Collection pertenece a un `SiteKey` y se identifica por un `Code` único dentro del sitio. Sus elementos almacenan la información específica en `DataJson`, que debe ser un objeto JSON. De esta forma el modelo no depende de la estructura particular de FAQ, testimonios, logos u otros conjuntos repetitivos.

## API administrativa

Base: `/api/content/collections`.

- `GET /?siteKey=main`
- `GET /{id}`
- `POST /`
- `PUT /{id}`
- `DELETE /{id}`
- `GET /{collectionId}/items`
- `POST /{collectionId}/items`
- `PUT /{collectionId}/items/{itemId}`
- `DELETE /{collectionId}/items/{itemId}`

Lectura usa `cms.view`. Escritura usa temporalmente `cms.edit`; el scope específico `cms.collections.edit` pertenece a la FASE 23 y no se adelanta en esta fase.

## Validaciones

- `SiteKey + Code` es único entre registros no eliminados.
- `Code` se normaliza a minúsculas y admite letras, números, `.`, `-` y `_`.
- `SettingsJson`, cuando existe, debe ser un objeto JSON.
- `DataJson` es obligatorio y debe ser un objeto JSON.
- `SortOrder` no puede ser negativo.
- Los elementos se listan por `SortOrder` y fecha de creación.

## Alcance

FASE 9 no crea tablas específicas por tipo de colección y no implementa la API pública de Collections. La API pública se reserva para la FASE 29.
