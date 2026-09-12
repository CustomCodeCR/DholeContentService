# FASE 23 — Permisos

ContentService incorpora los 14 scopes granulares de Mercadeo sin eliminar los scopes CMS existentes.

Los endpoints que reciben un scope específico de FASE 23 aceptan **scope específico OR scope legacy**. Ejemplos: formularios aceptan `cms.forms.view` o `cms.view`; edición de leads acepta `cms.leads.edit` o `cms.edit`; aprobación acepta `cms.reviews.approve` o `cms.publish`.

Esto permite introducir roles con permisos granulares sin romper usuarios y roles existentes. Las lecturas para las que FASE 23 no define un scope específico continúan usando `cms.view`.

No hay cambios de base de datos ni migraciones en esta fase.
