# FASE 5 — Page Builder

## Objetivo

Construir el Page Builder sobre el `BlocksJson` ya existente en `ContentItem`, sin crear una tabla por sección o componente.

## Formato canónico de bloque

```json
{
  "id": "identificador-unico",
  "type": "Hero",
  "isVisible": true,
  "data": {}
}
```

El documento completo es un arreglo JSON de bloques.

## Tipos soportados

- Hero
- RichText
- Image
- Video
- Gallery
- CTA
- ServicesGrid
- NewsGrid
- FAQ
- Testimonials
- Logos
- Stats
- Team
- Banner
- Form
- MeetingForm

No se aceptan tipos arbitrarios.

## Operaciones

El endpoint de operaciones soporta:

- `add`
- `edit`
- `delete`
- `duplicate`
- `move`
- `hide`
- `show`
- `visibility`

Cada operación crea una revisión del contenido antes de persistir el cambio y registra auditoría.

## API

- `GET /api/content/page-builder/block-types`
- `GET /api/content/page-builder/{contentId}`
- `POST /api/content/page-builder/{contentId}/operations`

Lectura requiere `cms.view`; escritura requiere `cms.pages.edit`.

## Seguridad

El Page Builder no permite bloques con JavaScript, Vue/componentes arbitrarios ni HTML inseguro. El validador rechaza propiedades de ejecución como `script`, `javascript`, `vue`, `component`, `rawHtml`, `unsafeHtml`, atributos/event handlers `on*`, y contenido con patrones como `<script`, `javascript:`, `data:text/html`, `v-html`, `iframe`, `object` o `embed`.

Los bloques solo pueden contener las propiedades superiores `id`, `type`, `isVisible` y `data`.

## Compatibilidad

La validación estricta del Page Builder se aplica a contenido de tipo `Page`. Los otros tipos de contenido conservan su comportamiento previo de `BlocksJson` para no introducir una ruptura fuera del alcance de FASE 5.

No se agrega ninguna migración ni tabla nueva en esta fase.
