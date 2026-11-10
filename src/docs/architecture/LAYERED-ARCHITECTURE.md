# JPDFs — arquitectura por capas

## Objetivo

Organizar JPDFs como un **monolito modular por capas**, manteniendo la aplicación WPF y el motor PDF como componentes separados. La meta es que la interfaz no contenga reglas de negocio, que los casos de uso no dependan de WPF y que el acceso a archivos, red, OCR y APIs externas quede detrás de adaptadores.

Esta guía describe el estado comprobado del repositorio y define la estructura objetivo. No afirma que la separación en proyectos ya esté terminada.

## Estado actual

La solución contiene un proyecto principal WPF (`JPDFs.csproj`), un motor PDF independiente (`engine/JPDFs.Engine`), proyectos de pruebas, herramientas, empaquetado y dependencias de terceros.

En el proyecto WPF ya existen directorios con responsabilidades reconocibles:

| Directorio actual | Responsabilidad predominante | Capa destino |
| --- | --- | --- |
| `Shell/`, `Controls/`, `Themes/`, `App.xaml` | Ventanas, controles, interacción y composición visual WPF | Presentation |
| `Features/` | Coordinadores y flujos de funcionalidades; algunos dependen del host WPF | Application (tras desacoplar los hosts) |
| `Models/` | Modelos de vista y tipos compartidos; responsabilidad mixta | Domain o Presentation, según su uso |
| `Services/` | Servicios de aplicación, lógica de documentos y adaptadores de plataforma mezclados | Application o Infrastructure, según su responsabilidad |
| `engine/JPDFs.Engine/` | Lectura, escritura, renderizado y procesamiento PDF | Motor de dominio / componente especializado |
| `third_party/` | Código externo incorporado | Dependencias externas |
| `JPDFs.Tests/`, `engine/JPDFs.Engine.Tests/`, `tests/` | Pruebas unitarias, integración y smoke tests | Verification |
| `Packaging/`, `PreviewHandler/`, `build/`, `tools/` | Instalador, integración con Windows, compilación y utilidades | Delivery / adaptadores de plataforma |

**Situación importante:** aunque los directorios ya sugieren capas, el proyecto principal sigue siendo un único proyecto WPF y contiene dependencias cruzadas. En particular, `Services/` no es una capa pura y parte de `Features/` conoce interfaces del shell. Por eso no basta con cambiar nombres de carpetas para obtener una arquitectura por capas real.

## Estructura objetivo

La migración debe avanzar hacia esta organización lógica:

```text
JPDFs/
├── README.md
├── Build-JPDFs-Installer.ps1
├── Compilar-J-PDFs.cmd
└── src/
    ├── JPDFs.sln
    ├── global.json
    ├── src/
    │   ├── JPDFs.Presentation/
    │   │   ├── App.xaml
    │   │   ├── Shell/
    │   │   ├── Controls/
    │   │   ├── Themes/
    │   │   └── ViewModels/
    │   ├── JPDFs.Application/
    │   │   ├── Abstractions/
    │   │   ├── Documents/
    │   │   ├── Search/
    │   │   ├── Ocr/
    │   │   ├── Signing/
    │   │   └── Workflows/
    │   ├── JPDFs.Domain/
    │   │   ├── Documents/
    │   │   ├── Pages/
    │   │   ├── Annotations/
    │   │   ├── Security/
    │   │   └── Shared/
    │   └── JPDFs.Infrastructure/
    │       ├── Files/
    │       ├── Pdf/
    │       ├── Ocr/
    │       ├── Windows/
    │       ├── Persistence/
    │       └── External/
    ├── engine/
    │   └── JPDFs.Engine/
    ├── tests/
    │   ├── JPDFs.Domain.Tests/
    │   ├── JPDFs.Application.Tests/
    │   ├── JPDFs.Infrastructure.Tests/
    │   └── JPDFs.Presentation.Tests/
    ├── third_party/
    ├── Packaging/
    ├── PreviewHandler/
    ├── tools/
    ├── build/
    └── docs/
```

Los nombres son una estructura objetivo, no rutas que deban crearse vacías ni proyectos que ya existan.

## Regla de dependencias

Las dependencias deben apuntar hacia el centro:

```text
Presentation ───────► Application ───────► Domain
      │                    ▲
      └────────────────────┤
                           │
Infrastructure ────────────┘

Application define contratos (interfaces).
Infrastructure implementa esos contratos.
Presentation compone e inicia las implementaciones.
Domain no referencia WPF, archivos, red ni infraestructura.
JPDFs.Engine se consume mediante adaptadores definidos en Infrastructure.
```

Reglas obligatorias:

1. **Domain** contiene entidades, objetos de valor, reglas e invariantes. No referencia WPF ni paquetes de infraestructura.
2. **Application** contiene casos de uso, coordinación y contratos. No referencia controles, ventanas ni tipos WPF.
3. **Infrastructure** implementa contratos: sistema de archivos, configuración, OCR nativo, descargas, certificados, impresión e integración con Windows.
4. **Presentation** contiene XAML, ventanas, controles, ViewModels y composición de dependencias. No implementa reglas de negocio.
5. Los proyectos de pruebas deben probar cada capa sin necesitar iniciar toda la aplicación cuando no sea necesario.
6. Las dependencias de terceros permanecen aisladas y no se copian dentro de Domain o Application.

## Criterios para clasificar archivos

Antes de mover un archivo, revisar su contenido y sus referencias; el nombre por sí solo no determina su capa.

- Si representa estado exclusivo de una pantalla o implementa `INotifyPropertyChanged`, pertenece a Presentation.
- Si coordina una acción del usuario y define el resultado del flujo, pertenece a Application.
- Si expresa una regla o estado válido del documento sin depender de tecnología, pertenece a Domain.
- Si accede a disco, registro, red, procesos, APIs nativas, OCR o bibliotecas externas, pertenece a Infrastructure.
- Si una clase mezcla responsabilidades, primero se separa por comportamiento y después se mueve.
- Los tipos compartidos no deben convertirse automáticamente en una carpeta común; deben tener un propietario claro.

## Plan de migración seguro

La migración se realizará en etapas pequeñas. Cada etapa debe compilar y ejecutar sus pruebas antes de continuar.

### Etapa 1 — Inventario y línea base

- Registrar todos los proyectos, referencias, namespaces y scripts que usan rutas físicas.
- Ejecutar restore, build y pruebas en Windows.
- Identificar dependencias WPF dentro de `Features/`, `Models/` y `Services/`.
- No mover archivos durante el inventario.

### Etapa 2 — Separar Presentation

- Mantener `Shell/`, `Controls/`, `Themes/` y XAML en Presentation.
- Extraer ViewModels de los controles y code-behind cuando sea viable.
- Dejar el arranque y el contenedor de composición en Presentation.

### Etapa 3 — Extraer Application

- Convertir los flujos de `Features/` en casos de uso independientes del shell.
- Sustituir referencias directas a `IViewerHost`, `IOcrHost` y otros hosts por contratos pequeños.
- Mantener los adaptadores WPF en Presentation.

### Etapa 4 — Aislar Domain

- Separar reglas puras y tipos de negocio de los modelos de pantalla.
- Evitar que Domain dependa de WPF, del sistema de archivos o del motor concreto.
- Mantener `JPDFs.Engine` como componente especializado y conectarlo mediante adaptadores.

### Etapa 5 — Extraer Infrastructure

- Agrupar implementaciones de los contratos: archivos, configuración, OCR, certificados, descargas, impresión y Windows.
- Mover los servicios mixtos solo después de dividir sus responsabilidades.
- Registrar implementaciones en el punto de composición de Presentation.

### Etapa 6 — Verificación y limpieza

- Actualizar solución, referencias, workflows, scripts de publicación, instalador, documentación y rutas de pruebas.
- Ejecutar build Release, pruebas del motor, pruebas de aplicación y smoke tests WPF.
- Comprobar que el paquete instalado y el portable siguen incluyendo los mismos recursos y dependencias.

## Restricciones de compatibilidad

- No cambiar namespaces públicos, nombres de ensamblado, identificadores de paquetes ni contratos de CLI sin una migración explícita.
- No modificar el motor PDF ni sus algoritmos como parte de un simple movimiento de carpetas.
- No cambiar el contenido de recursos, traducciones, licencias o archivos de terceros.
- No mover scripts de entrada de la raíz mientras los comandos documentados y automatizaciones dependan de ellos.
- Mantener rutas de salida y nombres de instalador que esperan los workflows actuales.
- Cada movimiento debe actualizar todas las referencias y verificarse con una compilación real en Windows.

## Definición de terminado

La arquitectura se considerará aplicada cuando:

- La solución tenga proyectos separados para Domain, Application, Infrastructure y Presentation.
- Las referencias entre proyectos respeten la regla de dependencias.
- Domain y Application puedan compilar y probarse sin WPF.
- Presentation sea el único proyecto que compone la aplicación WPF.
- Los adaptadores de infraestructura implementen contratos definidos hacia el interior.
- Build, pruebas, publicación, instalador y smoke tests funcionen desde un checkout limpio.
