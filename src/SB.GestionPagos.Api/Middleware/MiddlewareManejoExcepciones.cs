using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SB.GestionPagos.Dominio.Excepciones;

namespace SB.GestionPagos.Api.Middleware;

/// <summary>
/// Red de seguridad del canal: convierte cualquier excepción no controlada en una respuesta
/// ProblemDetails y la deja registrada (RNF-10).
/// </summary>
/// <remarks>
/// Va PRIMERO en el canal, y no es una preferencia de estilo. Un middleware solo puede
/// atrapar lo que ocurre por debajo de él, porque el <c>try</c> envuelve la llamada a la
/// pieza siguiente. Puesto al final, no cubriría nada de lo que está encima —enrutamiento,
/// autenticación, límite de frecuencia, el propio MVC— y esas excepciones saldrían por
/// Kestrel como una página de error o un 500 vacío, sin registro y sin contrato.
///
/// Se escribe a mano en lugar de usar <c>IExceptionHandler</c> de .NET 8. Ambos caminos son
/// válidos y hacen lo mismo; se elige este porque el enunciado pide un middleware y porque
/// deja el orden del canal a la vista en <c>Program.cs</c>, que es justo lo que hay que
/// poder explicar.
/// </remarks>
public sealed class MiddlewareManejoExcepciones
{
    /// <summary>
    /// Código no estándar, introducido por nginx y adoptado como convención, para "el cliente
    /// cerró la conexión antes de que hubiera respuesta".
    /// </summary>
    /// <remarks>
    /// No es un 500: no falló nada del servidor. Distinguirlo importa porque, si se contara
    /// como error, cada usuario que cierra una pestaña a mitad de una consulta ensuciaría el
    /// panel de errores con un fallo que no existe.
    /// </remarks>
    private const int CODIGO_PETICION_ABANDONADA = 499;

    /// <summary>Violación de un índice único en SQL Server.</summary>
    private const int ERROR_SQL_INDICE_UNICO = 2601;

    /// <summary>Violación de una restricción UNIQUE o de clave primaria en SQL Server.</summary>
    private const int ERROR_SQL_CLAVE_DUPLICADA = 2627;

    private const string TITULO_REGLA_DE_NEGOCIO = "Regla de negocio incumplida";
    private const string TITULO_CONFLICTO = "Conflicto con un registro existente";
    private const string TITULO_ERROR_INTERNO = "Error interno del servidor";

    private const string MENSAJE_CONFLICTO =
        "Ya existe un registro con uno de los valores enviados que debe ser único.";

    /// <summary>
    /// Mensaje genérico del 500. Deliberadamente no dice NADA del fallo real.
    /// </summary>
    /// <remarks>
    /// Un mensaje de excepción del motor de base de datos revela nombres de tablas, de
    /// columnas y a veces fragmentos de la consulta: es reconocimiento gratuito para quien
    /// esté sondeando el sistema. El diagnóstico va al registro, donde lo ve el equipo; al
    /// cliente le llega el identificador de correlación, que es todo lo que necesita para
    /// que soporte encuentre el fallo exacto.
    /// </remarks>
    private const string MENSAJE_ERROR_INTERNO =
        "Ocurrió un error inesperado al procesar la solicitud. Comunique el identificador de "
        + "correlación al equipo de soporte.";

    private readonly RequestDelegate _siguiente;
    private readonly ILogger<MiddlewareManejoExcepciones> _registrador;
    private readonly IHostEnvironment _entorno;

    public MiddlewareManejoExcepciones(
        RequestDelegate siguiente,
        ILogger<MiddlewareManejoExcepciones> registrador,
        IHostEnvironment entorno)
    {
        _siguiente = siguiente;
        _registrador = registrador;
        _entorno = entorno;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _siguiente(contexto);
        }
        catch (OperationCanceledException) when (contexto.RequestAborted.IsCancellationRequested)
        {
            // El usuario cerró la pestaña o canceló la búsqueda. La cancelación viajó por el
            // CancellationToken hasta EF Core y volvió como excepción: es el mecanismo
            // funcionando, no un fallo. No se responde nada porque ya no hay a quién.
            _registrador.LogInformation(
                "Petición abandonada por el cliente. {Metodo} {Ruta}. Correlación: {IdCorrelacion}.",
                contexto.Request.Method,
                contexto.Request.Path.Value,
                contexto.TraceIdentifier);

            if (!contexto.Response.HasStarted)
            {
                contexto.Response.StatusCode = CODIGO_PETICION_ABANDONADA;
            }
        }
        catch (Exception excepcion)
        {
            await ManejarAsync(contexto, excepcion);
        }
    }

    private async Task ManejarAsync(HttpContext contexto, Exception excepcion)
    {
        (int codigoEstado, string titulo, string detalle) = Traducir(excepcion);

        Registrar(contexto, excepcion, codigoEstado);

        // Si la respuesta ya empezó a enviarse, las cabeceras y el código de estado ya
        // viajaron: escribir ahora produciría un cuerpo mezclado, mitad datos y mitad error.
        // Lo único correcto es cortar la conexión y dejar constancia.
        if (contexto.Response.HasStarted)
        {
            _registrador.LogWarning(
                "No se pudo devolver un ProblemDetails porque la respuesta ya había comenzado. "
                + "Correlación: {IdCorrelacion}.",
                contexto.TraceIdentifier);

            contexto.Abort();
            return;
        }

        await RespuestaProblema.EscribirAsync(
            contexto,
            codigoEstado,
            titulo,
            detalle,

            // El rastro de pila SOLO en Desarrollo. En Producción esta expresión es nula y el
            // cuerpo no contiene una línea de código interno.
            detalleTecnico: _entorno.IsDevelopment() ? excepcion.ToString() : null);
    }

    /// <summary>
    /// Decide el código de estado que corresponde a cada clase de fallo.
    /// </summary>
    /// <remarks>
    /// Los tres casos que se nombran aquí son los únicos que el sistema sabe interpretar;
    /// todo lo demás es un 500 por definición, porque si se supiera qué es, se habría
    /// devuelto un resultado y no lanzado una excepción.
    /// </remarks>
    private static (int CodigoEstado, string Titulo, string Detalle) Traducir(Exception excepcion) => excepcion switch
    {
        // El Dominio rechazó el dato. Es culpa del cliente, no del servidor: 400. El mensaje
        // se puede mostrar tal cual porque estas excepciones se redactan para un humano y no
        // contienen nada interno.
        ExcepcionDominio excepcionDominio =>
            (StatusCodes.Status400BadRequest, TITULO_REGLA_DE_NEGOCIO, excepcionDominio.Message),

        // Dos peticiones simultáneas pasaron la comprobación previa de unicidad antes de que
        // ninguna guardara, y el índice único de SQL Server arbitró la carrera. El caso normal
        // ya lo resuelve el servicio con un 409 legible; esto solo cubre esa ventana de
        // milisegundos para que no termine en un 500.
        DbUpdateException excepcionActualizacion when EsViolacionDeUnicidad(excepcionActualizacion) =>
            (StatusCodes.Status409Conflict, TITULO_CONFLICTO, MENSAJE_CONFLICTO),

        _ => (StatusCodes.Status500InternalServerError, TITULO_ERROR_INTERNO, MENSAJE_ERROR_INTERNO)
    };

    /// <remarks>
    /// Que el proyecto Api conozca dos números de error de SQL Server es la concesión
    /// consciente de este diseño: es conocimiento de un motor concreto viviendo en el host.
    /// Se acepta porque traducir excepciones a códigos HTTP es exactamente el trabajo de esta
    /// capa y porque el alcance es de dos constantes. Si el sistema creciera, lo correcto
    /// sería que <c>EmpleadoRepositorioSql</c> atrapara esto y lanzara una excepción propia
    /// del proyecto, dejando aquí un caso más sin nombrar a SQL Server.
    /// </remarks>
    private static bool EsViolacionDeUnicidad(DbUpdateException excepcion)
        => excepcion.InnerException is SqlException excepcionSql
           && excepcionSql.Number is ERROR_SQL_INDICE_UNICO or ERROR_SQL_CLAVE_DUPLICADA;

    /// <summary>
    /// Registra el fallo con la severidad que le corresponde.
    /// </summary>
    /// <remarks>
    /// La distinción no es cosmética: si un dato mal formado se registrara como Error, el
    /// panel de errores se llenaría de equivocaciones de usuarios y el fallo real quedaría
    /// escondido entre ellas. Error significa "hay que ir a mirar".
    /// </remarks>
    private void Registrar(HttpContext contexto, Exception excepcion, int codigoEstado)
    {
        const string PLANTILLA =
            "Petición fallida. {Metodo} {Ruta} respondió {CodigoEstado}. Correlación: {IdCorrelacion}.";

        object[] argumentos =
        [
            contexto.Request.Method,
            contexto.Request.Path.Value ?? string.Empty,
            codigoEstado,
            contexto.TraceIdentifier
        ];

        if (codigoEstado >= StatusCodes.Status500InternalServerError)
        {
            _registrador.LogError(excepcion, PLANTILLA, argumentos);
            return;
        }

        _registrador.LogWarning(excepcion, PLANTILLA, argumentos);
    }
}
