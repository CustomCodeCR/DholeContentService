# FASE 19 — Agenda de reuniones

FASE 19 agrega la persistencia y administración de tipos y solicitudes de reunión dentro de DholeContentService.

## Tablas

- `content.meeting_types`
- `content.meeting_requests`

`meeting_types` configura sitio, nombre, slug, duración, buffer, modalidad, usuario/equipo asignado, settings y activación.

`meeting_requests` conserva la solicitud, referencias opcionales a Lead/Submission, ventana solicitada, zona horaria, asunto, mensaje, estado, asignación y datos de confirmación/proveedor externo.

## Estados

- `Requested`
- `PendingConfirmation`
- `Confirmed`
- `Rejected`
- `Cancelled`
- `Completed`

Las transiciones se validan en dominio. Una solicitud confirmada requiere una ventana confirmada válida.

## Integridad

- `SiteKey + Slug` es único para tipos activos/no eliminados.
- duración: 1..1440 minutos.
- buffer: 0..1440 minutos.
- una solicitud requiere LeadId o SubmissionId.
- la ventana solicitada y confirmada debe tener fin posterior al inicio.
- Lead y Submission se validan contra el mismo sitio del MeetingType.
- `AssignedUserId` no usa FK porque Auth es otro microservicio.

## API administrativa

Base: `/api/content/meetings`.

Tipos: listar, detalle, crear, actualizar y eliminar.

Solicitudes: listar, detalle, crear, pasar a pendiente de confirmación, confirmar, rechazar, cancelar y completar.

Lectura usa `cms.view` y escritura `cms.edit` hasta FASE 23.

## Fuera de alcance

FASE 19 no implementa envío de correos ni integración con Notifications. Los eventos Outbox específicos de reuniones corresponden a FASE 20. Tampoco implementa la UI de Agenda de DholeWeb (FASE 22) ni la API pública de reuniones (FASE 29).
