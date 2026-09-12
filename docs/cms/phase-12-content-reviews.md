# FASE 12 — Aprobación de contenido

FASE 12 agrega trazabilidad explícita de revisión en `content.content_reviews`.

## Flujo

- `Draft` o `Rejected` -> `PendingReview` al enviar a revisión.
- Cada envío crea una `ContentRevision` inmutable y una review `Pending` vinculada a esa revisión.
- `PendingReview` -> `Published` al aprobar la review.
- `PendingReview` -> `Rejected` al rechazar la review.
- Un contenido rechazado puede editarse y reenviarse.
- `Scheduled` y `Archived` se mantienen; el worker de publicación programada pertenece a FASE 13.

## API administrativa

- `GET /api/content/reviews?contentId=&status=` — `cms.view`.
- `GET /api/content/reviews/{id}` — `cms.view`.
- `POST /api/content/reviews/{id}/approve` — `cms.publish`.
- `POST /api/content/reviews/{id}/reject` — `cms.publish`.
- `POST /api/content/items/{id}/review` conserva el flujo existente con `cms.edit`, pero ahora crea la review real.

Los scopes `cms.reviews.submit` y `cms.reviews.approve` no se crean todavía porque la especificación los reserva para FASE 23.

El endpoint de publicación existente registra/aprueba una review cuando es necesario para conservar compatibilidad con los clientes actuales y evitar publicaciones sin trazabilidad.

La interfaz completa de pendientes de aprobación en DholeWeb permanece reservada para FASE 22.
