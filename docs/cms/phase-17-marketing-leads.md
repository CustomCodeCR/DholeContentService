# FASE 17 — Leads básicos de Mercadeo

## Alcance

FASE 17 agrega `content.marketing_leads` como registro mínimo de captación de Mercadeo. No reemplaza ni duplica un CRM.

Campos funcionales:

- `SiteKey`
- `FirstName`
- `LastName`
- `Email`
- `Phone`
- `Company`
- `JobTitle`
- `Country`
- `Source`
- `Status`
- `OwnerUserId`
- `FirstTouchAtUtc`
- `LastTouchAtUtc`

El lead requiere al menos email o teléfono. Los emails se normalizan a minúsculas y son únicos por `SiteKey` cuando existen. `Status` inicia en `New` cuando no se especifica otro valor.

## API administrativa

Base: `/api/content/leads`

- `GET /` lista y filtra por sitio, estado, owner o source.
- `GET /{id}` obtiene un lead.
- `POST /` crea un lead.
- `PUT /{id}` actualiza los datos básicos del lead.
- `POST /{id}/touch` actualiza `LastTouchAtUtc` y opcionalmente `Source`.
- `DELETE /{id}` aplica soft-delete.

Hasta FASE 23 se mantienen `cms.view` para lectura y `cms.edit` para escritura. No se adelantan `cms.leads.view` ni `cms.leads.edit`.

## Integración futura con DholeCRMService

Crear, actualizar o eliminar un lead genera domain events. `ServiceDbContext` persiste esos eventos en el Outbox existente con los nombres:

- `content.marketing-lead.created`
- `content.marketing-lead.updated`
- `content.marketing-lead.deleted`

Un `DholeCRMService` puede consumir esos eventos cuando esté disponible. ContentService no hace llamadas HTTP directas al CRM y no implementa oportunidades, pipeline, scoring, actividades, notas ni automatizaciones comerciales.

## Fuera de alcance

- Consentimientos: FASE 18.
- Reuniones: FASE 19–20.
- Campañas: FASE 21.
- UI de Leads en DholeWeb: FASE 22.
- Scopes `cms.leads.*`: FASE 23.
