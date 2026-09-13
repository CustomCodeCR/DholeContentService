# FASE 31 — Caché Redis e invalidación automática

La API pública reutiliza la infraestructura Redis existente de `DholeContentService`.

## Recursos cacheados

- contenido publicado por slug (páginas/noticias)
- resolución de rutas públicas
- menús
- placements y sus items públicos
- collections y sus items públicos
- settings públicos

## Invalidación

Las claves públicas están versionadas por `siteKey`. Un cambio que afecta la vista pública rota la generación del sitio mediante `IContentCacheService.InvalidateSiteAsync`. Las claves de la generación anterior dejan de ser visibles inmediatamente y expiran por TTL, evitando `SCAN` o borrados por wildcard en Redis.

Los métodos legacy `RemoveContentAsync`, `RemoveMenuAsync` y `RemovePublicSettingsAsync` ahora invalidan la generación completa del sitio, por lo que los handlers existentes de publicación, edición, despublicación, archivo, borrado, navegación y settings refrescan todos los recursos públicos relacionados.

Los cambios de rutas, placements y collections invalidan explícitamente el sitio después de persistir.

El preview temporal de FASE 30 no usa esta caché pública.
