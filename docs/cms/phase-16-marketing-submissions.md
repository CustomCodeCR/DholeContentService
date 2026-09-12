# FASE 16 — Marketing submissions

FASE 16 agrega persistencia de formularios enviados sin convertir ContentService en CRM ni adelantar Leads, Consents o Campaigns.

## Persistencia

Tabla `content.marketing_submissions`:

- `Id`
- `FormId`
- `ContentId`
- `CampaignId`
- `SubmittedAtUtc`
- `Status`
- `SourceUrl`
- `ReferrerUrl`
- `UtmSource`
- `UtmMedium`
- `UtmCampaign`
- `UtmContent`
- `UtmTerm`
- `PayloadJson`
- `IpHash`
- `UserAgent`
- `CorrelationId`

El estado inicial es `Received`.

`FormId` tiene FK a `content.marketing_forms`. `ContentId` es opcional y referencia `content.content_items` con `SET NULL`. `CampaignId` se conserva como referencia opcional sin FK hasta FASE 21, cuando exista `content.campaigns`.

## Privacidad y validación

El endpoint nunca persiste la IP en claro. El valor de `RemoteIpAddress` se transforma mediante SHA-256 y solo se almacena como `IpHash`.

`PayloadJson` debe ser un objeto JSON y se valida contra los campos activos del formulario:

- se rechazan claves no declaradas;
- se rechazan campos obligatorios ausentes, nulos o vacíos;
- las claves se persisten usando el `FieldKey` canónico del formulario;
- no se escriben datos extra enviados por el cliente.

No se publica el payload completo a AuditLogs.

## API

### Envío público

`POST /api/content/forms/{formId}/submissions`

Es anónimo y solo acepta formularios con `Status = Active`.

La respuesta contiene únicamente:

- `SubmissionId`
- `SubmittedAtUtc`
- `SuccessMessage`

### Administración

Temporalmente usa `cms.view` hasta FASE 23:

- `GET /api/content/submissions`
- `GET /api/content/submissions/{id}`

La lista puede filtrarse por formulario, estado y rango de fechas.

## Fuera de alcance

FASE 16 no implementa:

- Leads de Mercadeo (FASE 17)
- Consentimientos (FASE 18)
- Reuniones (FASE 19)
- Campañas y FK real de CampaignId (FASE 21)
- UI de Submissions en DholeWeb (FASE 22)
- `cms.submissions.view` (FASE 23)
