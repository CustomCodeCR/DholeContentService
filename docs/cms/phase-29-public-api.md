# FASE 29 — API administrativa y pública

## Rutas canónicas

- Administración: `/api/cms/*`
- Pública: `/api/public/*`

Los endpoints administrativos existentes bajo `/api/content/*` se mantienen temporalmente por compatibilidad. `MapCmsAdminAliases` replica únicamente endpoints que ya tienen metadata de autorización y excluye explícitamente cualquier endpoint `AllowAnonymous`, por lo que una ruta pública nunca se convierte en una ruta `/api/cms/*`.

La API pública se mantiene anónima y solo usa consultas de contenido publicado. Se conserva `/api/content/public/*` como alias legacy para no romper FennecWeb durante la migración.

## Recursos públicos

- páginas publicadas
- resolución de rutas publicadas
- noticias publicadas
- menús activos
- placements activos; sus items solo se devuelven cuando están vigentes y apuntan a contenido publicado
- collections activas e items activos
- settings públicos
- formularios activos, campos y envío de submissions
- tipos de reunión activos y creación pública de solicitudes sin aceptar IDs internos de lead/submission
- `sitemap.xml`
- `robots.txt`

Nunca se publican listados administrativos de solicitudes de reunión, submissions, leads, consentimientos, borradores, contenido pendiente de revisión, archivado, eliminado o no publicado.
