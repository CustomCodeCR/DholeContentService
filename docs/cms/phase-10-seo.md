# FASE 10 — SEO

## Alcance

FASE 10 completa el SEO usando los campos que ya existen en `ContentItem`; no agrega tablas ni requiere migración.

Se mantienen `SeoTitle`, `SeoDescription`, `SeoKeywords`, `CanonicalUrl`, `Robots`, `OpenGraphMediaId` y `StructuredDataJson`.

## Validación

- `CanonicalUrl`, cuando existe, debe ser una URL absoluta HTTP/HTTPS.
- `Robots` se normaliza y usa `index,follow` por defecto.
- `StructuredDataJson` debe ser un objeto o arreglo JSON válido.
- `OpenGraphMediaId`, cuando se modifica por el endpoint SEO dedicado, debe apuntar a una referencia multimedia existente.

## API administrativa

- `GET /api/content/seo/{contentId}/preview` — `cms.view`.
- `PUT /api/content/seo/{contentId}` — `cms.seo.edit`.

El preview resuelve fallbacks de título, descripción, canonical e imagen y devuelve representaciones para Google, Facebook, LinkedIn y Twitter Cards.

## Sitemap y robots

Se exponen de forma anónima:

- `/sitemap.xml?siteKey=main`
- `/robots.txt?siteKey=main`
- `/api/content/public/sitemap.xml?siteKey=main`
- `/api/content/public/robots.txt?siteKey=main`

El sitemap incluye únicamente rutas primarias activas asociadas a contenido `Published`, no eliminado, no vencido y sin `noindex`.

## OpenGraph, Twitter Cards y JSON-LD

El contrato de preview entrega los metadatos efectivos para OpenGraph y Twitter Cards. `StructuredDataJson` queda disponible como JSON-LD validado para el renderer público.

## DholeWeb

El módulo Mercadeo completa el formulario SEO con Robots y JSON-LD y muestra previews de Google, Facebook y LinkedIn.

## Fuera de alcance

FASE 10 no crea redirects; corresponden a FASE 11. La consolidación final de la API pública y el renderer de FennecWeb corresponden a fases posteriores.
