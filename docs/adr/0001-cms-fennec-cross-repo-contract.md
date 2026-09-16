# ADR-0001 — Contrato cross-repo Dhole CMS → Fennec realtime

- Estado: Accepted
- Fecha: 2026-09-16
- Batch: `feature/cms-fennec-realtime`
- Owner: `CustomCodeCR/DholeContentService`

## Contexto

El ecosistema ya tiene un CMS headless, almacenamiento físico, notificaciones internas, analítica, API pública, Redis y Outbox. El objetivo de este ADR es congelar los límites entre repositorios antes de completar los gaps del batch CMS/Fennec realtime, evitando un segundo CMS, un segundo cache, un segundo sistema de eventos o dependencias circulares.

Este ADR define contratos y ownership. No implementa todavía el manifest ni el hub público; esas capacidades se agregan en las fases específicas del plan.

## Decisión

### Ownership por repositorio

| Repositorio | Responsabilidad canónica | No debe asumir |
|---|---|---|
| `DholeWeb` | Backoffice CMS, edición, preview autorizado y operaciones administrativas vía API | Persistir la fuente de verdad pública ni emitir eventos públicos directamente |
| `DholeContentService` | Fuente de verdad CMS, publicación, revisiones, API pública, cache público, Outbox y contrato realtime público | Almacenar blobs físicos ni delegar el estado canónico a SignalR |
| `DholeStorageService` | Blobs/media físicos y providers Local/S3/Azure/MinIO | Ser la fuente de verdad editorial del media CMS |
| `DholeNotificationsService` | Notificaciones internas/autenticadas del equipo editorial | Servir como hub público de Fennec |
| `DholeReportsService` | Analítica y reportes de contenido | Escribir/controlar estado CMS |
| `FennecWeb` | Renderer público SSR/CSR y consumidor de REST + invalidaciones realtime | Persistir contenido canónico o depender de SignalR para poder renderizar |

### Flujo canónico

```text
DholeWeb
  -> DholeContentService Admin API (JWT + scopes)
  -> transacción de publicación
       -> estado/revisión pública
       -> Outbox
  -> commit DB
  -> invalidación del Redis public cache existente
  -> estado público resoluble por REST
  -> ContentChanged por ContentHub
  -> FennecWeb recibe invalidación
  -> FennecWeb vuelve a consultar REST
```

`REST` es siempre el estado canónico. `SignalR` solamente invalida/notifica cambios.

Drafts, autosaves, comentarios de revisión y previews autorizados nunca generan un evento público ni se exponen por la API pública.

## Dependencias permitidas

```text
DholeWeb -> DholeContentService (Admin API)
DholeWeb -> DholeStorageService (Media API cuando corresponda)
DholeWeb <-> DholeNotificationsService (notificaciones internas)
DholeWeb -> DholeReportsService (lectura de reportes)

DholeContentService -> DholeStorageService (referencias/operaciones de media existentes)
DholeContentService -> Redis / Outbox / Redis Streams existentes
DholeContentService -> Notifications/Reports mediante las integraciones/eventos existentes, sin acoplar sus modelos internos

FennecWeb -> DholeContentService (Public REST API)
FennecWeb -> DholeContentService (ContentHub público)
```

No se permite que `DholeContentService`, `Storage`, `Notifications` o `Reports` dependan de `DholeWeb` o `FennecWeb`. Tampoco se permite que Notifications o Reports se conviertan en dependencias necesarias para resolver una lectura pública de contenido.

## Contratos canónicos

Los contratos públicos son propiedad de `DholeContentService`.

- Los DTO .NET que correspondan viven en `Dhole.Content.Contracts` o en la capa pública equivalente ya usada por el servicio.
- DholeWeb y FennecWeb pueden mantener tipos TypeScript de transporte que reflejen el JSON público/admin, pero no redefinen semántica ni ownership.
- No se crea una segunda librería de dominio compartida entre frontend y backend.
- Una futura generación OpenAPI puede reemplazar tipos manuales sin cambiar este ownership.

### Public site manifest

Contrato previsto para la fase de API pública/manifest:

```json
{
  "siteKey": "main",
  "version": 145,
  "lastChangedAtUtc": "2026-09-16T18:00:00Z"
}
```

Semántica:

- `siteKey`: identificador público estable del sitio.
- `version`: versión pública monotónica por sitio; solo cambia cuando cambia una representación pública.
- `lastChangedAtUtc`: instante UTC del último cambio público efectivo.
- Draft, autosave, revisión no publicada y preview no incrementan `version`.
- Publish/unpublish y cambios públicos de menú/settings/redirect/media sí deben incrementar `version` cuando alteran la representación pública.

Endpoint canónico a implementar si no aparece uno equivalente antes de Fase 5:

```http
GET /api/public/sites/{siteKey}/manifest
```

Los endpoints públicos ya existentes bajo `/api/public/*` continúan siendo canónicos. Los aliases legacy se mantienen solo por compatibilidad y no justifican crear endpoints duplicados.

### Evento público `ContentChanged`

Owner: `DholeContentService`.

Nombre de evento SignalR:

```text
ContentChanged
```

Payload canónico:

```json
{
  "eventId": "3f16ba44-20bf-42ea-a49c-8b74695ce59c",
  "siteKey": "main",
  "entityType": "Page",
  "entityId": "bdb724b6-3d8c-4e2f-bdd4-9f67bd7adb82",
  "route": "/servicios",
  "revision": 146,
  "changedAtUtc": "2026-09-16T18:01:00Z"
}
```

Campos:

- `eventId`: UUID estable del evento; Fennec lo usa para deduplicación.
- `siteKey`: sitio público afectado.
- `entityType`: uno de `Page`, `Article`, `News`, `Menu`, `SiteSettings`, `Redirect`, `Media`.
- `entityId`: identificador de la entidad pública afectada.
- `route`: ruta pública afectada cuando aplica; `null` cuando no existe una ruta única.
- `revision`: versión pública monotónica del sitio después del cambio. Debe poder compararse con `manifest.version`.
- `changedAtUtc`: timestamp UTC del cambio público efectivo.

`Article` es el nombre semántico público; en el dominio actual puede mapear al `ContentType.Post` existente. No se crea una entidad Article paralela por este contrato.

El evento no transporta HTML, bloques completos, contenido de página, JWT, datos privados del editor, secretos, drafts, autosaves ni comentarios de revisión.

### Hub público

Al no existir un hub público equivalente durante el inventario, el contrato reservado para la fase SignalR es:

```text
Path: /hubs/content
Hub: ContentHub
Event: ContentChanged
Site group: site:{siteKey}
```

El hub será read-only desde la perspectiva administrativa. El cliente podrá identificarse/suscribirse a un sitio usando únicamente `siteKey` público; no habrá métodos públicos para publicar, editar, aprobar, programar o ejecutar operaciones administrativas.

`NotificationsHub` permanece separado, autenticado y orientado a grupos de usuario internos.

## Publicación confiable y orden

Se reutiliza el Outbox existente; no se crea otro bus ni otra tabla de eventos.

La secuencia obligatoria para un cambio público es:

1. Dentro de la transacción, persistir la nueva versión/estado publicado y el mensaje Outbox.
2. Commit de base de datos.
3. Invalidar la generación correspondiente del Redis public cache existente.
4. Confirmar que el estado nuevo puede resolverse por la API pública.
5. Emitir `ContentChanged` al grupo `site:{siteKey}`.
6. Entregar los eventos internos de Notifications/Reports que correspondan usando la infraestructura existente.
7. Marcar como procesado según la semántica del Outbox/dispatcher existente.

La implementación concreta de Fase 6 debe adaptar/extender el pipeline actual sin reemplazar `IIntegrationEventOutboxWriter`, `OutboxProcessor`, Redis Streams ni `IContentCacheService`.

No se permite emitir `ContentChanged` antes de que la invalidación de cache sea efectiva y el nuevo estado sea resoluble por REST.

## Reconciliación y tolerancia a fallos

FennecWeb debe funcionar completamente por REST con SignalR apagado.

En cliente:

1. Existe una sola conexión SignalR por app/browser session.
2. Se habilita reconexión automática.
3. Cada `eventId` se procesa de forma idempotente.
4. Un evento con `revision` menor o igual a la versión pública ya aplicada se puede ignorar.
5. Al reconectar, Fennec consulta el manifest.
6. Si `manifest.version` difiere de la versión conocida, Fennec refetch de globals/menú/ruta actual según corresponda.
7. Un fallo de SignalR nunca impide SSR, navegación o lecturas REST.

## Seguridad

- Admin API: JWT + scopes existentes, validados en backend.
- Public API: read-only y solo contenido público/publicado.
- ContentHub: payload mínimo, sin datos administrativos o sensibles.
- CORS del hub: solo origins públicos esperados por ambiente; nunca `AllowAnyOrigin` con credentials.
- Preview de draft: canal HTTP autorizado separado del Public API y del ContentHub.

## Compatibilidad con infraestructura existente

Este ADR congela semántica, no exige reemplazos:

- API pública: extender `/api/public/*` existente.
- Cache: reutilizar `IContentCacheService` y su versionado/invalidation por sitio.
- Outbox: reutilizar la infraestructura Outbox y Redis Streams existente.
- Eventos de dominio: reutilizar `ContentItemPublishedDomainEvent`, `ContentItemUnpublishedDomainEvent`, `NavigationMenuChangedDomainEvent`, `SiteSettingChangedDomainEvent` y equivalentes.
- Notifications: conservar `NotificationsHub` autenticado.
- Reports: consumir eventos/datos sin convertirse en source of truth.

## Consecuencias

### Positivas

- Un único owner por responsabilidad.
- REST y SignalR no compiten como fuentes de verdad.
- Fennec tolera pérdida temporal de eventos.
- No hay dependencia circular entre repositorios.
- Las fases 5, 6 y 8 tienen un contrato estable para manifest, evento y reconciliación.

### Restricciones

- Cambiar el shape de `PublicSiteManifest` o `ContentChanged` después de implementarlos requiere compatibilidad/versionado explícito.
- Un cambio público no se considera propagado hasta que cache y REST estén consistentes antes del broadcast.
- El contrato no autoriza a publicar drafts ni autosaves por realtime.

## Definition of Done de esta ADR

- Ownership cross-repo explícito y único.
- REST público definido como estado canónico.
- SignalR definido exclusivamente como invalidación.
- Manifest y `ContentChanged` tienen payload/semántica congelados para las fases posteriores.
- Outbox y Redis existentes se reutilizan.
- NotificationsHub interno queda separado del ContentHub público.
- No se introducen dependencias circulares.
