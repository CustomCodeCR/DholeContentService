# FASE 20 — Integración de reuniones con Notifications

## Alcance

DholeContentService no envía correos ni conoce SMTP. La creación y confirmación de reuniones escriben eventos de integración en el Outbox existente y el worker los publica en `dhole.notifications.events`.

## Eventos

- `content.meeting.requested`: se escribe en la misma unidad de trabajo que el nuevo `MeetingRequest` y contiene la información operativa necesaria para avisar a Mercadeo.
- `content.meeting.confirmed`: se escribe en la misma unidad de trabajo que la confirmación y contiene horario confirmado, enlace/proveedor y el email del cliente.

El email del cliente se resuelve primero desde `MarketingLead.Email`. Si no existe y la reunión está asociada a un submission, se usa únicamente un campo declarado en el formulario con `FieldType=email`; no se adivinan claves arbitrarias del payload.

Si no existe un email de cliente resoluble, la confirmación se rechaza para evitar registrar una confirmación que no pueda generar el correo exigido por FASE 20.

## Redis

El worker de Content enruta ambos eventos a `dhole.notifications.events`. DholeNotificationsService es responsable de convertirlos en mensajes Email y de la entrega/reintentos.

## Fuera de alcance

- SMTP o proveedores de correo dentro de ContentService.
- campañas de FASE 21.
- UI de reuniones de FASE 22.
- cambios de esquema: FASE 20 reutiliza Outbox y las tablas de reuniones existentes.
