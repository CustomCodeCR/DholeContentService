# FASE 21 — Campañas

FASE 21 incorpora `content.campaigns` sin adelantar la UI de Mercadeo de FASE 22 ni los scopes `cms.campaigns.*` de FASE 23.

## Modelo

La entidad conserva exactamente los campos definidos para la fase: `Id`, `SiteKey`, `Name`, `Slug`, `Status`, `StartsAtUtc`, `EndsAtUtc`, `LandingContentId`, `UtmSource`, `UtmMedium`, `UtmCampaign`, `GoalType` y `SettingsJson`, además de la auditoría/soft-delete estándar del servicio.

`SiteKey + Slug` es único entre campañas no eliminadas. La ventana temporal permite extremos nulos y, si ambos existen, `EndsAtUtc` debe ser posterior a `StartsAtUtc`. `SettingsJson` debe ser un objeto JSON.

`LandingContentId` referencia una landing page (`ContentType.Page`) del mismo sitio. El contenido de esa landing puede usar los bloques existentes del Page Builder para banners y formularios sin crear tablas específicas de campaña.

## Atribución

`marketing_submissions.CampaignId`, reservado desde FASE 16, queda relacionado con `content.campaigns`. Al recibir un submission con campaña, ContentService valida que la campaña exista y pertenezca al mismo sitio que el formulario. La FK se crea `NOT VALID` para no invalidar datos históricos previos a la existencia de `content.campaigns`, pero sí protege las nuevas escrituras.

A partir de esa atribución, el formulario se conoce por `FormId` y las reuniones que conservan `SubmissionId` mantienen el vínculo con la campaña. No se agregan columnas `CampaignId` artificiales a formularios, leads o reuniones porque la especificación no las define.

## API administrativa

Base: `/api/content/campaigns`.

- `GET /?siteKey=&status=` — `cms.view`
- `GET /{id}` — `cms.view`
- `POST /` — `cms.edit`
- `PUT /{id}` — `cms.edit`
- `DELETE /{id}` — `cms.edit`

Los scopes específicos `cms.campaigns.view` y `cms.campaigns.edit` quedan para FASE 23.
