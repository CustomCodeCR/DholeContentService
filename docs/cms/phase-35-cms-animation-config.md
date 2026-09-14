# FASE 35 — Configuración de animaciones desde el CMS

Los bloques de Page Builder pueden incluir una propiedad superior opcional `animation` compatible con el Motion System de Fennec (FASE 34).

## Contrato

`animation` admite `preset`, `duration`, `delay`, `easing`, `stagger`, `trigger`, `once` y `distance`.

Presets: `none`, `fade`, `fade-up`, `fade-down`, `fade-left`, `fade-right`, `slide-up`, `slide-left`, `slide-right`, `zoom-in`, `zoom-out`, `scale`, `blur-in`.

El backend valida estrictamente presets, easing, trigger y rangos numéricos. Si `animation` existe con campos omitidos, completa los defaults centrales de FASE 34. Si la propiedad no existe, sigue siendo válida para compatibilidad y el consumidor debe interpretar `none`.

La ejecución visual de la animación y `IntersectionObserver` no pertenecen a esta fase.
