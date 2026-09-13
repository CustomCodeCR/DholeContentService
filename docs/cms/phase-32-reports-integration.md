# FASE 32 — Integración con DholeReportsService

`DholeContentService` no implementa un motor de reportes. Su responsabilidad en esta fase es emitir eventos de analítica mediante el outbox existente y Redis Streams.

## Eventos

- `content.analytics.page-viewed`
- `content.analytics.form-submitted`
- `content.analytics.interaction-clicked`
- `content.meeting.requested` (evento existente, replicado a Notifications y Reports)

Todos los eventos de analítica se publican en `dhole.reports.events`. Para reuniones el Outbox soporta múltiples destinos, por lo que el mismo evento continúa llegando a `dhole.notifications.events` y también llega a Reports.

## API pública de tracking

- `POST /api/public/analytics/page-views`
- `POST /api/public/analytics/clicks`

Los payloads incluyen `siteKey`, contenido/campaña cuando aplica, URL/ruta y dimensiones UTM. El API no calcula métricas ni agrega información; esa responsabilidad pertenece exclusivamente a `DholeReportsService`.
