# Changelog

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
