# FASE 13 — Publicación programada

## Alcance

FASE 13 completa la publicación y despublicación programadas usando los campos existentes `ScheduledAtUtc` y `UnpublishAtUtc`.

## Programación

`POST /api/content/items/{id}/schedule` acepta:

```json
{
  "scheduledAtUtc": "2026-09-15T15:00:00Z",
  "unpublishAtUtc": "2026-10-15T15:00:00Z"
}
```

`UnpublishAtUtc` es opcional y, cuando se envía, debe ser posterior a `ScheduledAtUtc`.

Para preservar FASE 12, solo contenido `PendingReview` puede programarse. El mismo comando marca la revisión pendiente como `Approved` y cambia el contenido a `Scheduled` dentro de la misma unidad de trabajo.

## Worker

`ScheduledContentPublishingWorker` continúa ejecutándose con la configuración periódica existente y ahora:

1. busca contenido `Scheduled` con `ScheduledAtUtc <= now`;
2. publica ese contenido;
3. busca contenido `Published` con `UnpublishAtUtc <= now`;
4. lo despublica y limpia `UnpublishAtUtc`;
5. guarda los domain events mediante el Outbox existente;
6. invalida el caché actual de cada contenido afectado después de persistir.

El worker registra por separado las cantidades publicadas y despublicadas.

## Persistencia

No se agregan columnas nuevas. La migración de FASE 13 crea índices parciales para las dos consultas del worker:

- `ix_content_items_scheduled_due`
- `ix_content_items_unpublish_due`

Redis/caché público ampliado no se implementa aquí; corresponde a FASE 31.
