# FASE 18 — Consentimientos de Mercadeo

FASE 18 agrega evidencia explícita de consentimiento sin convertir DholeContentService en un sistema legal, CRM o plataforma de automatización.

## Modelo

`content.marketing_consents` conserva:

- `LeadId` y/o `SubmissionId`;
- `Purpose`;
- `Granted`;
- `PolicyVersion`;
- `Source`;
- `CapturedAtUtc`.

Propósitos iniciales:

- `privacy` — privacidad;
- `contact` — contacto;
- `newsletter` — newsletter;
- `commercial-communications` — comunicaciones comerciales.

Cada registro es evidencia histórica. La API no expone `PUT` ni `DELETE`; un cambio de decisión se registra como una nueva captura para el mismo propósito.

## Integridad

Al crear un consentimiento se exige al menos `LeadId` o `SubmissionId`. Las referencias apuntan a `marketing_leads` y `marketing_submissions` con `RESTRICT` para evitar borrar físicamente evidencia asociada. Si se suministran ambas referencias desde administración, deben pertenecer al mismo sitio.

`PolicyVersion` y `Source` son obligatorios. `CapturedAtUtc` puede ser histórico pero nunca futuro.

## Captura desde formularios

`SubmitMarketingFormRequest` acepta una colección opcional `Consents`. Cada elemento contiene propósito, decisión, versión de política y fuente opcional. Si no se indica fuente se usa `form`.

Submission y consentimientos se persisten dentro del mismo `SaveChanges`. Se rechazan propósitos duplicados dentro de un mismo envío.

## API administrativa

Base: `/api/content/consents`

- `GET /` — consulta por lead, submission, propósito, decisión y rango de captura (`cms.view`).
- `GET /purposes` — catálogo de propósitos (`cms.view`).
- `GET /{id}` — detalle (`cms.view`).
- `POST /` — captura administrativa (`cms.edit`).

FASE 23 no define scopes específicos de consentimientos, por lo que esta fase conserva los scopes CMS genéricos.

## Fuera de alcance

- UI de DholeWeb;
- reuniones de FASE 19;
- Notifications de FASE 20;
- campañas de FASE 21;
- automatizaciones comerciales o lógica de CRM.
