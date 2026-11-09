# Changelog

## [1.3.6] - 2026-11-08 (Dhole realtime coordinated hardening)

- PATCH 1.13.15 ecosystem release alignment with F2/F6 AOF read-only checks and F10 branch-preservation fix. Service candidate unchanged; realtime flags OFF and gates F6–F10 not formally approved.


## [1.3.5] - 2026-11-08 (F9 review candidate)

- PATCH SemVer global 1.13.14 para excepción F3 en revisión de F9. Mantiene gates formales, flags OFF y prohibición de despliegue hasta verificar staging y durabilidad.


## [1.3.4] - 2026-11-08 (candidate F3)

- PATCH de versión global 1.13.13 por validación WSS automatizada en Gateway. Sin cambios funcionales en Content ni despliegue.


## [1.3.3] - 2026-11-08 (F6/F9 staging-audit candidate)

- Sincroniza Dhole Realtime `rt-20261107-01` a versión global `1.13.12` tras incorporar auditoría real de persistencia en staging. PATCH 1.3.2 → 1.3.3; no modifica rutas, eventos, datos, contratos REST ni flags. No se fusiona hasta completar gates formales.


## [1.3.2] - 2026-11-08 (candidato F9, sin publicar)

- Sincroniza release global `rt-20261107-01` a `1.13.11` tras resolver los hallazgos de CI de Config, Reports y Gateway. Bump PATCH de DholeContentService `1.3.1` a `1.3.2`, sin modificar REST, eventos, precios ni datos; flags realtime OFF y PR F9 aún DRAFT.


## [1.3.1] - 2026-11-08 (candidato F9, sin publicación)

- Sincroniza metadatos de versión global del batch Dhole Realtime `rt-20261107-01` a `1.13.10`; actualización PATCH independiente de DholeContentService `1.3.0` → `1.3.1`. No cambia contratos REST, datos, lógica de negocio ni flags; requiere gates F0–F8 y CI de SHA final antes de merge.


- F6.13 (candidato): snapshots REST mínimos de menú y configuración (Id/IsActive e Id/IsPublic), autorización Content.View intacta; no se divulgan site keys, contenido inédito ni valores de ajustes.

## [1.3.0] - 2026-11-08 (candidato F6.13, NO LIBERADO)

- F6.13: eventos mínimos de cambios en contenidos, menús de navegación y configuraciones del sitio integrados en el mismo EF SaveChanges del Outbox existente.
- Evento independiente por recurso y stream dedicado; no transmite credenciales, datos privados, contenido de publicaciones, cuerpos ni URLs internas. REST autorizado es la única fuente de snapshots de datos.
- Requiere todos los flags de aprobación del release, apagados por defecto. Se añade CI de contrato y compilación; SemVer componente 1.2.1 → 1.3.0 y global candidato 1.13.0 sin despliegue.

## [1.2.1] - 2026-11-05

- Consolida SemVer por componente y despliegues automáticos a staging/producción por push.
- Sincroniza versión nativa y manifiesto del release global Dhole.

## [1.2.0] - 2026-11-05

- Incorporates universal Dhole SemVer policy with GitHub Actions checks.
- Synchronizes service assembly metadata with the ecosystem version.
- Adds permanent Codex instructions and coordinated release requirements.
