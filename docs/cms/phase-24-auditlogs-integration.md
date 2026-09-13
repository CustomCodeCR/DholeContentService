# FASE 24 — Integración con AuditLogs

FASE 24 completa la auditoría semántica del CMS usando la infraestructura existente de Dhole. No crea tablas nuevas ni una segunda integración HTTP.

## Flujo

1. Los comandos de Content generan `ContentAuditEvent` dentro de la misma unidad de trabajo que la mutación.
2. `ContentAuditService` serializa el contrato `Dhole.AuditLogs.Contracts.AuditEvents.RegisterAuditEventRequest` en Outbox con message type `audit.event.registered`.
3. El worker de Content publica el Outbox en Redis Stream `dhole.audit.events`.
4. DholeAuditLogsService consume el stream y persiste el evento mediante su Inbox.

El middleware HTTP continúa como capa de trazabilidad de requests; los eventos semánticos son la fuente con `BeforeJson`/`AfterJson` cuando la entidad está disponible.

## Acciones obligatorias

El CMS distingue explícitamente:

- `created`
- `updated`
- `published`
- `approved`
- `rejected`
- `deleted`
- `status_changed`
- `reordered`

Los cambios de `SortOrder` usan `reordered`. Los cambios de estado o activación usan `status_changed`, salvo acciones más específicas como `published`, `approved` o `rejected`.

## Contexto guardado

Cuando está disponible, cada evento conserva `UserId`, `UserName`, IP, User-Agent, correlation id, fecha, EntityType, EntityId y snapshots antes/después. El contexto HTTP lo aporta `AuditExecutionContextMiddleware` y el actor explícito de los comandos tiene prioridad para `UserId`.

Los submissions públicos no duplican `PayloadJson` de los formularios dentro de AuditLogs. Su snapshot contiene únicamente metadatos operativos (formulario, contenido/campaña, estado, origen/UTM, fecha y correlación). Los consentimientos guardan propósito, resultado, versión de política, origen y fecha.

## Cobertura incorporada

Además de los eventos ya existentes para contenido, SEO, media, taxonomías, menús, placements, collections, forms, leads, campaigns y redirects, FASE 24 agrega auditoría semántica a Sites, MarketingSubmissions, MarketingConsents, MeetingTypes y MeetingRequests. Las aprobaciones/rechazos de revisiones ahora conservan snapshots antes y después, incluidos los caminos indirectos de publicación y programación.

No hay migración de base de datos en esta fase. No se modifica DholeAuditLogsService porque el contrato y el stream existentes ya son compatibles. FASE 25 (IA) queda fuera de alcance.
