# DholeContentService

CMS headless de Grupo Castro Fallas para Dhole. Sustituye PublishCore y concentra la administración editorial del sitio público sin duplicar Auth, Storage, Gateway, Audit Logs, Redis ni Notifications.

## Capacidades

- Noticias, artículos, páginas, anuncios, banners, videos y contenido reutilizable.
- Editor por bloques mediante `BlocksJson` para texto, encabezados, imágenes, galerías, videos, botones, CTA, columnas, tablas, embeds y bloques personalizados.
- Borrador, revisión, programación, publicación, despublicación y archivado.
- Versionado completo y restauración de revisiones.
- SEO por contenido: title, description, keywords, canonical, robots, OpenGraph y JSON-LD.
- Categorías y etiquetas.
- Menús jerárquicos.
- Configuración global del sitio.
- Biblioteca multimedia respaldada por `DholeStorageService`; Content solo conserva referencias.
- API pública headless para FennecWeb y API administrativa para DholeWeb.
- PostgreSQL, Redis, JWT, Outbox/Inbox y estructura de proyectos estándar Dhole.
- Worker para publicaciones programadas.
- Permisos: `cms.view`, `cms.create`, `cms.edit`, `cms.delete`, `cms.publish`, `cms.media.upload`, `cms.media.delete`, `cms.pages.edit`, `cms.news.edit`, `cms.banners.edit`, `cms.seo.edit`, `cms.settings.edit`.

## Rutas principales

- `GET /api/v1/public/content/{slug}`
- `GET /api/v1/public/content?type=News`
- `GET /api/v1/public/pages/{slug}`
- `GET /api/v1/public/menus/{location}`
- `GET /api/v1/public/settings`
- `GET /api/v1/content`
- `POST /api/v1/content`
- `PUT /api/v1/content/{id}`
- `POST /api/v1/content/{id}/publish`
- `POST /api/v1/content/{id}/schedule`
- `GET /api/v1/content/{id}/revisions`
- `POST /api/v1/content/{id}/revisions/{revisionId}/restore`
- `POST /api/v1/media`
- CRUD de categorías, etiquetas, menús y settings.

Las imágenes y videos se cargan a `DholeStorageService` usando `sourceService=DholeContentService`.
