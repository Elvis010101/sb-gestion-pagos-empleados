using SB.GestionPagos.Api.Middleware;
using Serilog;
using Serilog.Events;

namespace SB.GestionPagos.Api.Configuracion;

/// <summary>
/// Configuración de Serilog: consola, archivo rotativo diario y registro de cada petición
/// (RNF-08, "la aplicación debe loggear todo lo que pase").
/// </summary>
/// <remarks>
/// Este es el único archivo del sistema que nombra a Serilog además del .csproj. Las capas
/// Servicios y Aplicación registran contra <c>ILogger&lt;T&gt;</c> de
/// Microsoft.Extensions.Logging, que es solo la interfaz: sustituir Serilog por NLog sería
/// reescribir esta clase y nada más.
///
/// La configuración va en código y no en el appsettings a propósito. En el appsettings queda
/// lo que cambia entre máquinas —el directorio de salida— y en código lo que es una decisión
/// del proyecto: qué se registra, con qué severidad y con qué formato. Así una entrada mal
/// escrita en un JSON no puede apagar el registro en silencio.
/// </remarks>
internal static class ConfiguracionRegistro
{
    private const string CLAVE_DIRECTORIO = "Registro:Directorio";
    private const string DIRECTORIO_PREDETERMINADO = "Registros";

    /// <summary>
    /// Patrón del nombre del archivo. Serilog inserta la fecha donde termina el nombre,
    /// justo antes de la extensión: <c>sb-gestion-pagos-20260830.log</c>.
    /// </summary>
    private const string PATRON_NOMBRE_ARCHIVO = "sb-gestion-pagos-.log";

    /// <summary>
    /// Cuántos archivos diarios se conservan antes de que Serilog borre el más viejo.
    /// </summary>
    /// <remarks>
    /// Un registro que crece sin techo termina llenando el disco del servidor, y un disco
    /// lleno tumba la aplicación entera: el registro pasaría de ser la herramienta de
    /// diagnóstico a ser la causa de la caída. Un mes cubre de sobra el plazo en que alguien
    /// investiga un incidente.
    /// </remarks>
    private const int ARCHIVOS_CONSERVADOS = 30;

    private const int MEGABYTES_POR_ARCHIVO = 50;
    private const int BYTES_POR_MEGABYTE = 1024 * 1024;

    /// <remarks>
    /// El nombre de la propiedad de correlación no se escribe a mano: se concatena desde la
    /// constante del middleware que la publica. Si allí se renombrara, esta plantilla dejaría
    /// de compilar en lugar de imprimir un hueco vacío en cada línea del archivo.
    /// </remarks>
    private const string PLANTILLA_SALIDA =
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{"
        + MiddlewareCorrelacion.NOMBRE_PROPIEDAD_REGISTRO
        + "}] {Message:lj}{NewLine}{Exception}";

    /// <remarks>
    /// Los nombres entre llaves —RequestMethod, RequestPath, StatusCode, Elapsed— los fija
    /// Serilog.AspNetCore: son las propiedades que su middleware calcula. El texto que los
    /// une sí es nuestro. Los cuatro datos que pide el enunciado (método, ruta, código y
    /// duración) están aquí; el quinto, el identificador de correlación, lo agrega el
    /// contexto de registro y aparece en el prefijo de cada línea.
    /// </remarks>
    private const string PLANTILLA_PETICION =
        "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0000} ms";

    private const string NOMBRE_PROPIEDAD_USUARIO = "Usuario";
    private const string NOMBRE_PROPIEDAD_ORIGEN = "DireccionOrigen";
    private const string USUARIO_ANONIMO = "anonimo";
    private const string ORIGEN_DESCONOCIDO = "origen-desconocido";

    /// <summary>
    /// Registrador mínimo que funciona ANTES de que exista el contenedor de dependencias.
    /// </summary>
    /// <remarks>
    /// Sin él, todo lo que ocurre durante el arranque no se registra en ningún sitio. Y ahí
    /// pasan cosas que importan: <c>AgregarInfraestructura</c> lanza una excepción si falta
    /// la cadena de conexión, y <c>OpcionesJwt</c> otra si falta la clave de firma. Con el
    /// registrador de arranque, ese fallo queda escrito; sin él, el proceso muere en
    /// silencio y el operador solo ve que la aplicación no levantó.
    /// </remarks>
    internal static Serilog.ILogger CrearRegistradorDeArranque()
        => new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: PLANTILLA_SALIDA)
            .CreateBootstrapLogger();

    /// <summary>
    /// Configuración definitiva, ya con acceso a la configuración de la aplicación.
    /// </summary>
    internal static void Configurar(
        HostBuilderContext contexto,
        IServiceProvider servicios,
        LoggerConfiguration registro)
    {
        string directorio = contexto.Configuration[CLAVE_DIRECTORIO] ?? DIRECTORIO_PREDETERMINADO;

        registro
            .MinimumLevel.Information()

            // ASP.NET Core registra por su cuenta el inicio y el fin de cada petición en
            // nivel Information. Como ya emitimos nuestra propia línea de resumen —más
            // completa y con el identificador de correlación—, dejarlo activo escribiría
            // tres líneas por petición diciendo casi lo mismo. Un registro con ruido es un
            // registro que nadie lee.
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)

            // EF Core escribe cada consulta SQL en Information. Es valioso al depurar y
            // asfixiante en producción: mil empleados en un reporte son mil líneas. Se sube
            // el umbral a Warning y se puede bajar puntualmente cuando haga falta.
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)

            // Habilita LogContext.PushProperty, que es lo que hace que el identificador de
            // correlación aparezca en eventos de capas que no saben que existe HTTP.
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: PLANTILLA_SALIDA)
            .WriteTo.File(
                path: Path.Combine(directorio, PATRON_NOMBRE_ARCHIVO),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: ARCHIVOS_CONSERVADOS,
                fileSizeLimitBytes: (long)MEGABYTES_POR_ARCHIVO * BYTES_POR_MEGABYTE,
                rollOnFileSizeLimit: true,
                outputTemplate: PLANTILLA_SALIDA);
    }

    /// <summary>
    /// Inserta el middleware que resume cada petición en una sola línea.
    /// </summary>
    /// <remarks>
    /// Se usa el de Serilog en lugar de escribir uno propio con un cronómetro porque ya mide
    /// exactamente lo que hace falta y, sobre todo, porque conoce el momento correcto: emite
    /// la línea cuando la respuesta terminó de enviarse, no cuando el controlador devolvió.
    /// </remarks>
    internal static IApplicationBuilder UsarRegistroDePeticiones(this IApplicationBuilder aplicacion)
        => aplicacion.UseSerilogRequestLogging(opciones =>
        {
            opciones.MessageTemplate = PLANTILLA_PETICION;
            opciones.GetLevel = DeterminarNivel;
            opciones.EnrichDiagnosticContext = Enriquecer;
        });

    /// <summary>
    /// Severidad de la línea de resumen según cómo terminó la petición.
    /// </summary>
    private static LogEventLevel DeterminarNivel(HttpContext contexto, double duracion, Exception? excepcion)
    {
        if (excepcion is not null || contexto.Response.StatusCode >= StatusCodes.Status500InternalServerError)
        {
            return LogEventLevel.Error;
        }

        if (contexto.Response.StatusCode >= StatusCodes.Status400BadRequest)
        {
            // 401, 404 y 429 no son fallos del servidor, pero tampoco son tráfico normal:
            // en Warning se pueden buscar sin leer el archivo entero.
            return LogEventLevel.Warning;
        }

        // La sonda de salud se consulta cada pocos segundos y siempre dice lo mismo. En
        // Information llenaría el archivo con miles de líneas idénticas y enterraría lo que
        // de verdad pasó. "Loggear todo lo que pase" no significa registrar el latido con la
        // misma importancia que una nómina: sigue registrada, en un nivel que no estorba.
        return EsSondaDeSalud(contexto) ? LogEventLevel.Debug : LogEventLevel.Information;
    }

    /// <summary>
    /// Agrega a la línea de resumen los datos que la plantilla no trae.
    /// </summary>
    /// <remarks>
    /// Esto es una LISTA BLANCA, y ahí está la garantía de que no se filtran secretos: se
    /// nombra uno por uno lo que se quiere registrar, en lugar de volcar la petición y
    /// después tachar lo sensible. Con una lista negra, el día que aparezca una cabecera
    /// nueva con un token, se registraría hasta que alguien se acuerde de excluirla. Aquí,
    /// para que un secreto acabe en el archivo, alguien tiene que escribirlo a propósito.
    ///
    /// No se registran: el cuerpo de la petición (donde viaja la contraseña del inicio de
    /// sesión), la cabecera Authorization (donde viaja el token) ni la cadena de consulta.
    /// </remarks>
    private static void Enriquecer(IDiagnosticContext contextoDiagnostico, HttpContext contexto)
    {
        contextoDiagnostico.Set(
            NOMBRE_PROPIEDAD_USUARIO,
            contexto.User.Identity?.IsAuthenticated == true
                ? contexto.User.Identity.Name
                : USUARIO_ANONIMO);

        contextoDiagnostico.Set(
            NOMBRE_PROPIEDAD_ORIGEN,
            contexto.Connection.RemoteIpAddress?.ToString() ?? ORIGEN_DESCONOCIDO);
    }

    private static bool EsSondaDeSalud(HttpContext contexto)
        => contexto.Request.Path.StartsWithSegments(RutasDelHost.SALUD);
}
