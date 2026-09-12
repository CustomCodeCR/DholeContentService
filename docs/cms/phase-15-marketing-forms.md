# FASE 15 — Formularios de Mercadeo

## Alcance

FASE 15 agrega el modelo backend para definir formularios reutilizables de Mercadeo. No guarda respuestas enviadas por visitantes; esa responsabilidad corresponde a FASE 16.

## Persistencia

### `content.marketing_forms`

Guarda la definición principal del formulario:

- `Id`
- `SiteKey`
- `FormKey`
- `Name`
- `Purpose`
- `Status`
- `SuccessMessage`
- `NotificationTemplateKey`
- `SettingsJson`
- `Version`

`SiteKey + FormKey` es único entre registros no eliminados. `Version` inicia en 1 y aumenta al editar el formulario o modificar sus campos.

Propósitos iniciales:

- `contact` — contacto
- `quote-request` — solicitud de cotización
- `meeting` — agendar reunión
- `newsletter` — newsletter
- `campaign` — campaña

Estados iniciales: `Draft`, `Active`, `Inactive`.

### `content.marketing_form_fields`

Define los campos ordenados de cada formulario:

- `Id`
- `FormId`
- `FieldKey`
- `Label`
- `FieldType`
- `Placeholder`
- `IsRequired`
- `SortOrder`
- `ValidationJson`
- `OptionsJson`

`FormId + FieldKey` es único entre registros no eliminados. `ValidationJson` debe ser un objeto JSON y `OptionsJson` un arreglo JSON.

## API administrativa

Base: `/api/content/forms`

- `GET /` — listar y filtrar por sitio, propósito o estado.
- `GET /purposes` — catálogo de propósitos iniciales.
- `GET /{id}` — obtener formulario.
- `POST /` — crear formulario.
- `PUT /{id}` — editar formulario.
- `DELETE /{id}` — eliminar lógicamente formulario.
- `GET /{formId}/fields` — listar campos ordenados.
- `POST /{formId}/fields` — crear campo.
- `PUT /{formId}/fields/{fieldId}` — editar campo.
- `DELETE /{formId}/fields/{fieldId}` — eliminar lógicamente campo.

Hasta FASE 23 se conservan los scopes existentes: `cms.view` para lectura y `cms.edit` para escritura.

## Validaciones

- El sitio debe existir.
- `FormKey` y `FieldKey` se normalizan a minúsculas y solo admiten segmentos alfanuméricos separados por `.` o `-`.
- No se permite duplicar `FormKey` en el mismo sitio.
- No se permite duplicar `FieldKey` dentro del mismo formulario.
- `SortOrder` no puede ser negativo.
- `SettingsJson` y `ValidationJson` deben ser objetos JSON cuando se informan.
- `OptionsJson` debe ser un arreglo JSON cuando se informa.
- Las modificaciones de campos incrementan `Version` del formulario.

## Fuera de alcance

- guardar submissions o datos enviados por visitantes (FASE 16);
- crear leads (FASE 17);
- UI de Formularios en DholeWeb (FASE 22);
- scopes `cms.forms.view` y `cms.forms.edit` (FASE 23).
