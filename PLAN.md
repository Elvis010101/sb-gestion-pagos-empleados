# PLAN — Sistema de Gestión de Pagos de Empleados

> Análisis de la prueba técnica y plan de solución.
> Documentos fuente: `docs/Prueba tecnica-1.pdf`, `docs/API - ESPECIFICACIONES TECNICAS.pdf`,
> `docs/Instrucciones.txt`, `docs/Maqueta.jpeg`, `docs/recursos/`.

---

## 1. Requisitos

### 1.1 Requisitos funcionales

| # | Requisito | Fuente |
|---|---|---|
| RF-01 | CRUD de empleados (crear, consultar, actualizar, eliminar) | Prueba p.4 |
| RF-02 | Captura diferenciada por tipo: Asalariado, Por Horas, Por Comisión, Asalariado por Comisión | Prueba p.4 |
| RF-03 | Filtros de consulta por nombre, departamento y estado | Prueba p.4 |
| RF-04 | Cálculo automático del pago semanal según el tipo de empleado (4 fórmulas) | Prueba p.5 |
| RF-05 | Actualizar la información del empleado y recalcular el pago | Prueba p.5 |
| RF-06 | Reporte semanal con el pago de cada empleado, detallando el cálculo por tipo de contrato | Prueba p.5 |
| RF-07 | Gestión de usuarios con roles y autenticación JWT | Prueba p.4 |
| RF-08 | Permisos básicos: Administrador / Usuario | Prueba p.3–4 |
| RF-09 | Mantenimiento (CRUD) del listado de entidades gubernamentales de RD, persistido en archivo de texto plano dentro del proyecto | Espec. p.3 + Instrucciones |
| RF-10 | Todo el CRUD debe poder operarse desde la interfaz visual, no solo desde Swagger | Instrucciones, línea 5 |

### 1.2 Requisitos no funcionales

| # | Requisito | Fuente |
|---|---|---|
| RNF-01 | Usabilidad: interfaz intuitiva para usuarios no técnicos | Prueba p.5 |
| RNF-02 | Escalabilidad: permitir nuevos tipos de empleado y cálculos **sin modificar el código existente** | Prueba p.5 |
| RNF-03 | Mantenibilidad: diseño modular, cambios aislados por módulo | Prueba p.5 |
| RNF-04 | Rendimiento: procesar los cálculos de 1,000 empleados en menos de 2 segundos | Prueba p.6 |
| RNF-05 | Testabilidad: la aplicación debe poder probarse de forma controlada y reproducible | Prueba p.6 |
| RNF-06 | Onion Architecture de 4 capas; nombre de proyecto `[SB].[NombreProyecto].[Capa]` | Espec. p.3 |
| RNF-07 | 11 reglas de nomenclatura (PascalCase, camelCase, constantes en mayúscula, prefijo `I`, sin abreviaturas, sin números mágicos, cadenas de conexión en AppSettings) | Espec. p.4 |
| RNF-08 | Logging con Serilog: "la aplicación debe loggear todo lo que pase" | Prueba p.6, Espec. p.3 |
| RNF-09 | Documentación con Swagger, con soporte de Bearer token | Espec. p.3 |
| RNF-10 | Manejo de excepciones básico | Espec. p.3 |
| RNF-11 | Autenticación `Authorization: Bearer` (JWT) | Espec. p.3 |
| RNF-12 | Stack: .NET 8, C#, ASP.NET Core Web API, EF Core, React + TypeScript, SQL Server | Prueba p.6 |
| RNF-13 | Identidad visual: `Maqueta.jpeg`, azul `rgba(13,48,72,.9)`, gris `rgb(237,240,247)`, `home.svg`, logo SB | Prueba p.6, Instrucciones |
| RNF-14 | Entrega: repositorio Git, README con instrucciones, script `.sql` / `.bak` o migraciones EF | Prueba p.8 |

---

## 2. Criterios de evaluación → artefacto que los satisface

### 2.1 Lista A — Aplicación (PDF de la prueba, p.6)

| # | Criterio | Artefacto concreto |
|---|---|---|
| A1 | Estructura del código y buenas prácticas (OOD y SOLID) | Clase abstracta `Empleado` + 4 subclases con `CalcularPagoSemanal()` polimórfico (OCP, LSP); interfaces `IRepositorioEmpleado` e `IRepositorioEntidadGubernamental` declaradas en Dominio e implementadas en Infraestructura (DIP); un caso de uso por servicio (SRP) |
| A2 | Claridad de código y lógica implementada | Constantes nombradas `HORAS_SEMANALES_ESTANDAR = 40`, `FACTOR_HORA_EXTRA = 1.5m`, `PORCENTAJE_BONIFICACION_ASALARIADO_COMISION = 0.10m`; cero abreviaturas; documentación XML en el Dominio |
| A3 | Uso de arquitectura limpia | 6 proyectos: `Dominio`, `Aplicacion`, `Servicios`, `Infraestructura`, `Api`, `Pruebas`, con `Dominio` sin ninguna referencia de proyecto |
| A4 | Conexión frontend-backend limpia y funcional | Cliente `axios` único con interceptor de token y de errores; tipos TypeScript espejo de los DTOs; `ProblemDetails` como contrato de error |
| A5 | Buen diseño UI/UX básico | Layout de `Maqueta.jpeg` en CSS Modules (barra lateral azul fija, área gris, tarjeta blanca redondeada); `home.svg` en color `#d9480f` para el ítem activo; estados de carga, error y vacío |
| A6 | Seguridad mínima (auth, validaciones) | JWT Bearer con `[Authorize(Roles=…)]`, BCrypt para contraseñas, FluentValidation en Aplicación y Zod en el frontend, rate limiting nativo de .NET 8, CORS restringido al origen de Vite |
| A7 | Uso eficiente de base de datos (queries, relaciones) | EF Core con estrategia **TPH** para la jerarquía; tabla `Departamentos` relacionada por clave foránea; índices en `DepartamentoId` y `Estado`; `AsNoTracking()`, filtrado y paginación aplicados sobre `IQueryable` (nunca en memoria), proyección directa a DTO |
| A8 | La aplicación debe loggear todo lo que pase | Serilog con sinks de consola y archivo rotativo diario; `UseSerilogRequestLogging`; enriquecimiento con `CorrelationId` y usuario autenticado; logging explícito de cada operación de negocio; middleware global de excepciones. **Con enmascarado de `numeroSeguroSocial` y prohibición de loggear contraseñas y tokens** |
| A9 | Claridad en el README y pruebas incluidas | `README.md` con arranque paso a paso; `SB.GestionPagos.Pruebas` con mínimo 3 pruebas (una por fórmula, frontera de 40 horas, validación) |
| A10 | Uso de buenas prácticas en React y .NET | TypeScript en modo `strict`, `react-hook-form` + `zod`, rutas protegidas por rol, hooks propios; inyección de dependencias, `async/await` con `CancellationToken`, `.editorconfig` |

### 2.2 Lista B — Conceptualización (PDF de la prueba, p.7)

| # | Criterio | Artefacto concreto |
|---|---|---|
| B1 | Claridad técnica y profundidad en respuestas escritas | `RESPUESTAS-CONCEPTUALIZACION.md`, borrador de las 8 respuestas que se envían por correo (Instrucciones, línea 4) |
| B2 | Conexión y diseño del flujo frontend-backend | Sección con diagrama de flujo en el README + respuesta escrita |
| B3 | Nivel de reflexión arquitectónica y madurez técnica | Este `PLAN.md` + sección "Decisiones y alternativas descartadas" del README |

### 2.3 Criterio con cobertura débil

El criterio **A7** pide explícitamente *"queries y relaciones"*, pero el modelo natural de
esta prueba es prácticamente una sola tabla de empleados. Para que exista una relación real
que sustentar, `Departamento` se modela como **tabla catálogo con clave foránea** en lugar
de un campo de texto suelto. Esto además da soporte directo al filtro por departamento del
RF-03.

---

## 3. Inconsistencias entre documentos y su resolución

| # | Inconsistencia | Resolución adoptada |
|---|---|---|
| 1 | `primerNombre` no aparece en la captura de **Empleado por Horas** (p.4), pero sí en los otros tres tipos | Es una omisión de redacción. `PrimerNombre` vive en la clase base `Empleado` y aplica a los cuatro tipos |
| 2 | En la lista *"La solución debe permitir"* (p.4) el cuarto tipo aparece como *"por comisión y empleado por comisión"* — repite el tercero | El detalle de captura de la misma página y la fórmula 4 de p.5 confirman que es **Empleado Asalariado por Comisión** |
| 3 | El RNF-02 exige agregar tipos "sin modificar el código existente", lo que descarta resolver el cálculo con un `switch` | Polimorfismo: `CalcularPagoSemanal()` abstracto en la base, más un registro de tipos para la creación |
| 4 | **Base de datos contradictoria**: la prueba dice "SQL Server u Oracle" (p.6) y pide script `.sql` (p.8); las especificaciones dicen "archivo de texto plano en un directorio dentro del proyecto" (p.3) | Son dos módulos distintos: **empleados en SQL Server**, **entidades gubernamentales en archivo plano**. Ambos detrás de interfaces de repositorio declaradas en el Dominio |
| 5 | **Filtros sin campos que los sustenten**: el RF-03 exige filtrar por *departamento* y *estado*, pero ninguna de las cuatro listas de captura define esos campos. Tampoco existe `apellidoMaterno` | Se agregan `Departamento` (FK a tabla catálogo) y `Estado` (enum `Activo` / `Inactivo`) a la clase base `Empleado`, documentado como supuesto explícito en el README |
| 6 | Versión de .NET inconsistente dentro del mismo PDF: p.6 dice ".NET 8", p.8 dice ".NET 7/8" | .NET 8 (LTS), ya fijado en `global.json` (SDK 8.0.130) |
| 7 | *"Loggear todo lo que pase"* (A8) choca con *"seguridad mínima"* (A6): loggear literalmente todo escribiría contraseñas, tokens y números de seguro social en disco | Loggeo exhaustivo de eventos, con enmascarado de PII y exclusión total de secretos |
| 8 | Los roles nunca se definen: el PDF pide "permisos básicos (admin/usuario)" sin decir qué puede cada uno | Matriz explícita en el README: `Administrador` = CRUD completo + gestión de usuarios; `Usuario` = solo lectura de empleados, entidades y reportes |
| 9 | `rgba(237, 240, 247)` es sintaxis CSS inválida: `rgba()` requiere cuatro componentes. Aparece igual en el PDF y en `Instrucciones.txt` | Se interpreta como `rgb(237, 240, 247)`, que coincide con el gris de la maqueta |
| 10 | La maqueta solo tiene tres ítems de navegación (Inicio / Consulta / Crear registro) pero el alcance tiene cuatro módulos | "Consulta" y "Crear registro" se parametrizan por módulo; se añaden "Entidades" y "Reportes" respetando el mismo estilo visual |
| 11 | El formato del archivo plano no se especifica, y CSV es inviable: la columna `Sector` del Excel contiene comas (`"Industria, Comercio y MIPYMES"`) | Archivo **JSON** (sigue siendo texto plano), con escritura atómica (archivo temporal + `File.Move`) y bloqueo para concurrencia |
| 12 | **Confidencialidad vs. repositorio público**: p.8 pide subir a un repo Git público, pero el pie de página de ambos PDFs dice *"uso exclusivo de la SB, sólo distribuido por personal autorizado"* | Decisión del candidato: repositorio privado con acceso concedido al evaluador, o excluir `/docs` del repositorio público |
| 13 | La regla 7 de nomenclatura exige constantes en MAYÚSCULA; la convención de C# usa PascalCase para `const` | Gana el documento de SB, porque es lo que se puntúa. Se documenta en el README como decisión consciente |
| 14 | Nombres de archivo citados incorrectamente: el PDF dice `Maqueta.jpg` (real: `Maqueta.jpeg`) e `Instrucciones.txt` dice `ESPECIFICACIONES TECNICAS.PDF` (real: `API - ESPECIFICACIONES TECNICAS.pdf`) | El README usa los nombres reales de los archivos |

---

## 4. Plan de la solución

### 4.1 Proyectos y dirección de dependencias

```
SB.GestionPagos.Dominio          ->  (ninguna referencia)
SB.GestionPagos.Aplicacion       ->  Dominio
SB.GestionPagos.Servicios        ->  Aplicacion, Dominio
SB.GestionPagos.Infraestructura  ->  Aplicacion, Dominio
SB.GestionPagos.Api              ->  Servicios, Infraestructura   (solo host)
SB.GestionPagos.Pruebas          ->  Dominio, Aplicacion, Servicios
frontend/                        ->  Vite + React 18 + TypeScript
```

Toda flecha apunta hacia adentro. El Dominio no conoce EF Core, ni ASP.NET, ni JSON, ni la
existencia de una base de datos.

### 4.2 Qué va exactamente en cada capa

**Dominio** — el negocio puro. La clase abstracta `Empleado` y sus cuatro subclases, el
método abstracto `CalcularPagoSemanal()` que devuelve un `ResultadoPago` con el desglose,
las constantes de las fórmulas, `EntidadGubernamental`, `Usuario`, los enums, las
excepciones de dominio y las **interfaces de repositorio**. Compila sin ninguna dependencia
externa: esa es la prueba de que la capa está bien hecha.

**Aplicación** — el contrato de lo que el sistema sabe hacer, sin decidir cómo. DTOs de
entrada y salida, interfaces de casos de uso (`IEmpleadoServicio`, `IReporteServicio`,
`IEntidadGubernamentalServicio`), validadores de FluentValidation, mapeos y tipos de
resultado. No implementa nada que toque un recurso externo.

**Servicios** — la implementación de esos contratos. Orquesta el flujo: valida el DTO,
pide los datos al repositorio, deja que el Dominio calcule, mapea y devuelve la respuesta.
Depende únicamente de abstracciones.

**Infraestructura** — los detalles reemplazables. `GestionPagosDbContext`, configuraciones
de EF Core y TPH, migraciones, `RepositorioEmpleadoSql`,
`RepositorioEntidadGubernamentalArchivo`, hashing con BCrypt y emisión de JWT.

**Api** — solo host. Controladores delgados, registro de dependencias, Serilog, Swagger,
middleware de excepciones, CORS y rate limiting. Cero lógica de negocio.

### 4.3 Aplicación vs. Servicios

El documento de SB nombra ambas capas pero no las define. La lectura adoptada:

> **Aplicación es el guion; Servicios son los actores.**

**Aplicación declara *qué* puede hacer el sistema.** Es un proyecto de contratos:
`CrearEmpleadoDto`, `EmpleadoRespuestaDto`, `IEmpleadoServicio`, `CrearEmpleadoValidador`.
Es abstracta y estable: cambia solo cuando cambian los casos de uso.

**Servicios implementa *cómo* se coordinan.** `EmpleadoServicio : IEmpleadoServicio` valida,
consulta el repositorio, deja calcular al Dominio, mapea y devuelve. Es concreta y volátil:
cambia cada vez que cambia un detalle de orquestación.

Consecuencias prácticas que justifican el corte:

1. **La API depende solo de abstracciones.** El controlador inyecta `IEmpleadoServicio`
   (de Aplicación) y nunca ve la clase concreta.
2. **Las pruebas tienen una costura clara.** Se simula `IRepositorioEmpleado` con NSubstitute
   y se ejercita `EmpleadoServicio` sin base de datos: eso es exactamente el RNF-05.
3. **Regla mecánica para no equivocarse de capa:** si el tipo dice `interface` o termina en
   `Dto` / `Validador`, es Aplicación; si tiene un constructor con dependencias inyectadas,
   es Servicios.

Y la frontera con el Dominio: **la fórmula del pago nunca sale del Dominio.** Servicios
coordina; no calcula.

**Alternativa descartada:** la Clean Architecture canónica fusiona ambas en una sola capa
`Application`. Se descarta porque el documento de SB enumera literalmente cuatro capas y eso
se puntúa; además, separarlas hace visible la Regla de Dependencia, que es justo lo que se
está evaluando.

### 4.4 Secuencia de bloques de trabajo

| Bloque | Contenido |
|---|---|
| 0 | Análisis y plan (este documento) |
| 1 | Esqueleto: solución, 6 proyectos, referencias, `.editorconfig`, `.gitignore` |
| 2 | **Dominio** con TDD: jerarquía `Empleado`, las 4 fórmulas, constantes, frontera de 40 horas |
| 3 | **Aplicación**: DTOs, interfaces de casos de uso, validadores |
| 4 | **Infraestructura**: EF Core + TPH + migraciones; repositorio de entidades en archivo plano, con semilla generada desde el Excel |
| 5 | **Servicios**: empleados, entidades gubernamentales, reportes |
| 6 | **Seguridad**: usuarios, BCrypt, JWT, roles |
| 7 | **Api**: controladores, Serilog, middleware de excepciones, Swagger con Bearer, CORS, rate limiting |
| 8 | **Frontend**: layout de la maqueta, login, consulta con filtros, alta y edición, entidades, reporte |
| 9 | Cierre: script `.sql`, README, respuestas de conceptualización |

### 4.5 Decisiones de diseño registradas

**D-01 — `CalcularPagoSemanal()` devuelve el desglose, no un `decimal`.**
El RF-06 exige un reporte que detalle *los cálculos* según el tipo de contrato, no solo el
total. Se resuelve con un único método abstracto que devuelve un objeto de valor
`ResultadoPago`, compuesto por líneas `LineaCalculo(Concepto, Monto)`, donde el total es la
suma de las líneas.

```csharp
public sealed record LineaCalculo(string Concepto, decimal Monto);

public sealed record ResultadoPago(IReadOnlyList<LineaCalculo> Lineas)
{
    public decimal Total => Lineas.Sum(linea => linea.Monto);
}

public abstract ResultadoPago CalcularPagoSemanal();
```

*Alternativa descartada:* dos métodos abstractos separados (`CalcularPagoSemanal()` y
`ObtenerDesglose()`). Se descarta porque la fórmula quedaría escrita en dos lugares y podría
desincronizarse sin que nada falle de forma visible. Con un solo método, el total no puede
contradecir al desglose porque se deriva de él.

*Alternativa descartada:* construir el desglose en la capa Servicios. Requeriría ramificar
según el tipo de empleado — un `switch` encubierto que viola el RNF-02.

**D-02 — Los parámetros del cálculo son constantes del Dominio en esta entrega.**
`HORAS_SEMANALES_ESTANDAR`, `FACTOR_HORA_EXTRA` y
`PORCENTAJE_BONIFICACION_ASALARIADO_COMISION` se declaran como constantes en el Dominio.
Ningún requisito pide que sean configurables, y anticiparlo sería agregar alcance no
solicitado.

Si en el futuro debieran leerse de base de datos (por ejemplo, un factor de hora extra
distinto por departamento), la evolución **no** consiste en que el Dominio consulte el
repositorio, porque eso invertiría la Regla de Dependencia. Consiste en inyectar los
parámetros por método, con un objeto de valor definido en el propio Dominio:

```csharp
public sealed record ParametrosCalculoPago(
    int HorasSemanalesEstandar,
    decimal FactorHoraExtra,
    decimal PorcentajeBonificacionAsalariadoComision);

public abstract ResultadoPago CalcularPagoSemanal(ParametrosCalculoPago parametros);
```

Infraestructura lee los valores, Servicios arma el objeto **una sola vez** antes del bucle y
lo pasa a cada empleado. El Dominio nunca busca: siempre recibe. Así `CalcularPagoSemanal`
sigue siendo una función pura —comprobable sin base de datos ni simulacros (RNF-05)— y se
evita hacer 1,000 consultas de configuración, lo que rompería el RNF-04.

*Alternativa descartada:* inyectar un `IProveedorParametrosPago` en el constructor de
`Empleado`. EF Core es quien materializa las entidades al leerlas, de modo que el modelo de
negocio quedaría atado al contenedor de dependencias, y una operación aritmética pura
pasaría a necesitar un simulacro para probarse.

**D-03 — El número de seguro social identifica de forma única a un empleado.**
Confirmado como supuesto de negocio. Se cierra en dos niveles, porque uno solo no alcanza:

- **Índice único** sobre la columna en SQL Server (Bloque 5). Es la garantía real: dos
  peticiones simultáneas pueden pasar ambas por una comprobación previa antes de que
  cualquiera guarde, y solo el motor puede arbitrar esa carrera.
- **Comprobación previa en `EmpleadoServicioBase`**, que en el caso normal devuelve un 409
  con un mensaje entendible en lugar de dejar que estalle una excepción del motor.
- **Traducción de la violación del índice** a 409 en el manejador global de excepciones
  (Bloque 7), para que el caso raro de la carrera tampoco termine en un 500.

**D-04 — La nómina semanal excluye a los empleados inactivos por omisión.**
A un empleado dado de baja no se le paga la semana, así que incluirlo inflaría el total.
El filtro `FiltroReporteSemanal.IncluirInactivos` permite pedir la población completa para
auditoría, pero hay que pedirlo explícitamente: el valor por omisión de `bool` es el
comportamiento seguro. Se modeló como booleano y no como `EstadoEmpleado?` justamente por
eso — con el enum, "no enviar nada" habría significado "sin filtrar", es decir, la nómina
inflada por descuido.

Además, `ReporteSemanalDto` lleva en el encabezado la frase `PoblacionIncluida` ("Empleados
activos de todos los departamentos"). Un total de nómina sin decir de quiénes es no se puede
interpretar, y en cuanto el reporte se imprime o se pega en un correo, el contexto de la
pantalla que lo pidió se pierde y el número queda solo.

*Ambos supuestos van al README (Bloque 9).*

### 4.6 Entorno local verificado

- .NET SDK **8.0.130** (fijado en `global.json` con `rollForward: latestFeature`)
- Node **v22.23.2**, npm **10.9.8**
- Docker **29.7.2**; el contenedor `sql-sb` existe pero está **detenido**
  (`Exited (137)`, típicamente falta de memoria). Antes del Bloque 4 hay que ejecutar
  `docker start sql-sb` y verificar que tenga al menos 2 GB de memoria asignada.
- Datos de origen: `docs/recursos/ListaEntidadesGubernamentales.xlsx`, **181 entidades**
  con las columnas `Nombre`, `Categoría`, `Poder del Estado`, `Sector`.
