# CLAUDE.md — Contexto permanente del proyecto

> Coloca este archivo en la raíz del repositorio **antes** de la primera sesión de Claude Code.
> Claude Code lo lee automáticamente en cada sesión, así no tienes que repetir estas reglas
> en cada prompt.

---

## Contexto

Este repositorio es una **prueba técnica** para una posición de Desarrollador Full Stack
(contratista) con la Superintendencia de Bancos de la República Dominicana.

El candidato **debe poder explicar y defender cada línea de código en una entrevista técnica
posterior**. Se permite el uso de IA, pero la entrevista es el filtro que valida que el
conocimiento sea real.

Los documentos originales de la prueba están en `/docs`:
- `docs/Prueba-tecnica.pdf` — requisitos funcionales y de evaluación
- `docs/API-Especificaciones-Tecnicas.pdf` — normas de arquitectura y nomenclatura (obligatorias)
- `docs/Instrucciones.txt` — alcance adicional
- `docs/Maqueta.jpeg` — referencia visual obligatoria
- `docs/recursos/` — logo SB, ícono `home.svg`, `ListaEntidadesGubernamentales.xlsx`

---

## Protocolo de trabajo (obligatorio en cada respuesta)

Trabajas en modo **mentor**, no en modo autopiloto. En cada bloque de trabajo:

1. **Antes de escribir código**, explica en 5–10 líneas: qué vas a construir, qué decisión de
   diseño estás tomando y **qué alternativa descartaste y por qué**.
2. Escribe el código. Comenta solo lo que no sea evidente (el porqué, no el qué).
3. **Después de escribir**, entrega una sección `## Explicación` con:
   - Los conceptos técnicos que aparecen, definidos en lenguaje claro.
   - Cómo se conecta con los criterios de evaluación de la prueba.
   - **"Si te preguntan X, responde Y"**: 2–3 preguntas probables de entrevista sobre este
     bloque, con la respuesta que el candidato debería dar.
4. **Termina siempre haciéndome 2 preguntas de comprobación** sobre lo que acabas de escribir,
   y espera mi respuesta antes de seguir al siguiente bloque. Si respondo mal o a medias,
   corrígeme y explica de nuevo con otro ángulo antes de avanzar.

Nunca avances al siguiente bloque por tu cuenta. Un bloque a la vez.

---

## Reglas técnicas innegociables

Vienen del documento `API - ESPECIFICACIONES TECNICAS.pdf`. Violarlas cuesta puntos directos.

### Nomenclatura
- Clases, métodos, enums y propiedades: **PascalCase**
- Variables locales y parámetros: **camelCase**
- Constantes: **MAYÚSCULA_CON_GUION_BAJO**
- Interfaces: prefijo **`I`**
- **Prohibido usar abreviaturas** (`cantidadEmpleados`, no `cantEmp`)
- **Prohibidos los números mágicos** — todo literal numérico con significado de negocio va en
  una constante nombrada o en configuración
- Las cadenas de conexión van en `appsettings.json`, nunca en código

### Arquitectura
- **Onion Architecture** con las capas: Dominio, Aplicación, Servicios, Infraestructura
- Nombre de proyectos: `SB.GestionPagos.<Capa>`
- Las dependencias apuntan **siempre hacia adentro**. El Dominio no referencia a nadie.
- El proyecto API es solo el host: sin lógica de negocio en los controladores

### Stack
- Backend: .NET 8, C#, ASP.NET Core Web API, EF Core
- Frontend: React 18 + TypeScript + Vite
- Base de datos de empleados: SQL Server
- Base de datos de entidades gubernamentales: **archivo de texto plano dentro del proyecto**
  (así lo exige el documento de arquitectura)
- Autenticación: JWT Bearer con roles `Administrador` y `Usuario`
- Logging: Serilog (consola + archivo rotativo). Requisito literal del PDF:
  *"la aplicación debe loggear todo lo que pase"*
- Documentación: Swagger con soporte de Bearer token
- Pruebas: xUnit

### Dinero y cálculo
- Todo importe monetario es `decimal`. Nunca `double` ni `float`.
- Toda la lógica de cálculo de pago vive en el Dominio, no en servicios ni controladores.

### Identidad visual (de `Maqueta.jpeg` e `Instrucciones.txt`)
- Azul: `rgba(13, 48, 72, .9)`
- Gris: `rgb(237, 240, 247)`
- Ícono de inicio: `docs/recursos/home.svg`
- Logo: `docs/recursos/SUPERINTENDENCIA_DE_BANCOS.png`
- Layout: barra lateral azul fija con logo y navegación (Inicio / Consulta / Crear registro),
  área de contenido gris, tarjeta blanca con esquinas redondeadas y sombra suave

---

## Supuestos que ya están decididos (no los vuelvas a preguntar)

1. `EmpleadoPorHoras` **sí** captura `primerNombre`, aunque el PDF lo omite en su lista.
   Es una omisión de redacción del documento. Queda registrado en el README.
2. El cuarto tipo es **Empleado Asalariado por Comisión** (el PDF lo escribe mal en la pág. 4,
   pero la fórmula de la pág. 5 lo confirma).
3. Los nuevos tipos de empleado deben poder agregarse **sin modificar código existente**
   (Principio Abierto/Cerrado). Prohibido resolver el cálculo con `switch` sobre el tipo.
4. El módulo de entidades gubernamentales usa persistencia en archivo plano; el de empleados
   usa SQL Server. Ambos detrás de interfaces de repositorio definidas en el Dominio.

---

## Fuera de alcance

No implementes nada que no esté pedido: sin Docker Compose complejo, sin CI/CD, sin
microservicios, sin caché distribuida, sin refresh tokens rotativos. La prueba evalúa
**criterio**, no volumen. Todo lo que agregues, lo tendré que defender.

## Paquetes autorizados del proyecto

Usa exactamente estos. No agregues otros sin preguntarme primero.

### Backend (.NET 8)

SB.GestionPagos.Infraestructura:
- Microsoft.EntityFrameworkCore.SqlServer 8.0.*
- Microsoft.EntityFrameworkCore.Design 8.0.*
- Microsoft.Extensions.Configuration.Abstractions 8.0.*
- BCrypt.Net-Next 4.0.*
- System.IdentityModel.Tokens.Jwt 8.*

SB.GestionPagos.Api:
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.*
- Serilog.AspNetCore 8.0.*
- Serilog.Sinks.Console
- Serilog.Sinks.File
- Swashbuckle.AspNetCore 6.*

SB.GestionPagos.Aplicacion:
- FluentValidation 11.*
- FluentValidation.DependencyInjectionExtensions 11.*

SB.GestionPagos.Servicios:
- Microsoft.Extensions.Logging.Abstractions 8.0.*
  Es SOLO la interfaz ILogger<T>, no una implementación de logging. Serilog vive
  únicamente en el proyecto Api. Por eso se podría cambiar Serilog por NLog sin tocar
  un solo servicio: es inversión de dependencias aplicada al logging.

SB.GestionPagos.Pruebas:
- xunit
- xunit.runner.visualstudio
- Microsoft.NET.Test.Sdk
- FluentAssertions 6.*
- NSubstitute 5.*   (para simular repositorios; NO uses Moq)

El rate limiting NO requiere paquete: Microsoft.AspNetCore.RateLimiting está incluido en
.NET 8. Usa builder.Services.AddRateLimiter(...).

### Frontend

Crear con: npm create vite@latest frontend -- --template react-ts

Dependencias:
- react-router-dom 6.*
- axios
- react-hook-form
- zod
- @hookform/resolvers

Dev:
- eslint, prettier, @typescript-eslint/*

No agregues librerías de componentes (MUI, Ant, Chakra). El diseño debe replicar
Maqueta.jpeg con CSS propio o CSS Modules: eso demuestra criterio de maquetación, que es
un criterio evaluado.

### Entorno local

- SQL Server: contenedor Docker "sql-sb" en localhost,1433, usuario sa
- Cadena de conexión (va SOLO en appsettings.Development.json, nunca en código):
  Server=localhost,1433;Database=SbGestionPagos;User Id=sa;Password=SbPrueba2026!;TrustServerCertificate=True;
- El appsettings.json versionado NO debe contener la contraseña real. Usa un placeholder y
  documenta en el README cómo configurarla.

## Paquetes autorizados del proyecto

Usa exactamente estos. No agregues otros sin preguntarme primero.

### Backend (.NET 8)

SB.GestionPagos.Infraestructura:
- Microsoft.EntityFrameworkCore.SqlServer 8.0.*
- Microsoft.EntityFrameworkCore.Design 8.0.*
- Microsoft.Extensions.Configuration.Abstractions 8.0.*
- BCrypt.Net-Next 4.0.*
- System.IdentityModel.Tokens.Jwt 8.*

SB.GestionPagos.Api:
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.*
- Serilog.AspNetCore 8.0.*
- Serilog.Sinks.Console
- Serilog.Sinks.File
- Swashbuckle.AspNetCore 6.*

SB.GestionPagos.Aplicacion:
- FluentValidation 11.*
- FluentValidation.DependencyInjectionExtensions 11.*

SB.GestionPagos.Servicios:
- Microsoft.Extensions.Logging.Abstractions 8.0.*
  Es SOLO la interfaz ILogger<T>, no una implementación de logging. Serilog vive
  únicamente en el proyecto Api. Por eso se podría cambiar Serilog por NLog sin tocar
  un solo servicio: es inversión de dependencias aplicada al logging.

SB.GestionPagos.Pruebas:
- xunit
- xunit.runner.visualstudio
- Microsoft.NET.Test.Sdk
- FluentAssertions 6.*
- NSubstitute 5.*   (para simular repositorios; NO uses Moq)

El rate limiting NO requiere paquete: Microsoft.AspNetCore.RateLimiting está incluido en
.NET 8. Usa builder.Services.AddRateLimiter(...).

### Frontend

Crear con: npm create vite@latest frontend -- --template react-ts

Dependencias:
- react-router-dom 6.*
- axios
- react-hook-form
- zod
- @hookform/resolvers

Dev:
- eslint, prettier, @typescript-eslint/*

No agregues librerías de componentes (MUI, Ant, Chakra). El diseño debe replicar
Maqueta.jpeg con CSS propio o CSS Modules: eso demuestra criterio de maquetación, que es
un criterio evaluado.

### Entorno local

- SQL Server: contenedor Docker "sql-sb" en localhost,1433, usuario sa
- Cadena de conexión (va SOLO en appsettings.Development.json, nunca en código):
  Server=localhost,1433;Database=SbGestionPagos;User Id=sa;Password=SbPrueba2026!;TrustServerCertificate=True;
- El appsettings.json versionado NO debe contener la contraseña real. Usa un placeholder y
  documenta en el README cómo configurarla.
