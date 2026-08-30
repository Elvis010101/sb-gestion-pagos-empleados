using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SB.GestionPagos.Api.Middleware;

namespace SB.GestionPagos.Api.Errores;

/// <summary>
/// Construye TODOS los ProblemDetails que emite MVC, agregándoles el identificador de
/// correlación.
/// </summary>
/// <remarks>
/// Sin esta clase, el cuerpo de un error trae la propiedad <c>traceId</c> que ASP.NET Core
/// pone por omisión: el identificador de la actividad de trazado distribuido, que NO es el
/// mismo valor que viaja en la cabecera X-Id-Correlacion ni el que aparece en el archivo de
/// registro. El usuario reportaría un identificador que no existe en ningún log, que es peor
/// que no darle ninguno.
///
/// Es el punto de extensión que ASP.NET Core define para esto, y por eso se prefiere a
/// parchear cada controlador: cubre de una vez las tres fuentes de error de MVC —el
/// <c>Problem(...)</c> de los controladores, el <c>ValidationProblemDetails</c> del filtro de
/// validación y los errores que el propio framework genera, como un 415 por tipo de contenido
/// no soportado—. Las respuestas que se escriben fuera de MVC, en los middlewares, ya agregan
/// el mismo dato por su cuenta.
/// </remarks>
internal sealed class FabricaProblemDetails : ProblemDetailsFactory
{
    private readonly ApiBehaviorOptions _opciones;

    public FabricaProblemDetails(IOptions<ApiBehaviorOptions> opciones)
    {
        _opciones = opciones.Value;
    }

    public override ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null)
    {
        int codigoEstado = statusCode ?? StatusCodes.Status500InternalServerError;

        ProblemDetails problema = new()
        {
            Status = codigoEstado,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = instance
        };

        Completar(problema, httpContext, codigoEstado);

        return problema;
    }

    public override ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        ModelStateDictionary modelStateDictionary,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null)
    {
        ArgumentNullException.ThrowIfNull(modelStateDictionary);

        int codigoEstado = statusCode ?? StatusCodes.Status400BadRequest;

        ValidationProblemDetails problema = new(modelStateDictionary)
        {
            Status = codigoEstado,
            Type = type,
            Detail = detail,
            Instance = instance
        };

        // El título de un error de validación lo pone el framework ("One or more validation
        // errors occurred") y solo se sobrescribe si quien llama pidió otro. Asignarlo
        // siempre borraría ese texto.
        if (title is not null)
        {
            problema.Title = title;
        }

        Completar(problema, httpContext, codigoEstado);

        return problema;
    }

    /// <summary>
    /// Rellena lo que es común a cualquier problema: el enlace a la definición del código, la
    /// ruta que falló y el identificador de correlación.
    /// </summary>
    private void Completar(ProblemDetails problema, HttpContext contexto, int codigoEstado)
    {
        // ClientErrorMapping es la tabla que ASP.NET Core ya trae con el título y el enlace a
        // la RFC de cada código. Reutilizarla evita reescribir a mano "Not Found" y la URL de
        // cada estado, y mantiene la respuesta idéntica a la que produce el framework solo.
        if (_opciones.ClientErrorMapping.TryGetValue(codigoEstado, out ClientErrorData? datosDelCodigo))
        {
            problema.Title ??= datosDelCodigo.Title;
            problema.Type ??= datosDelCodigo.Link;
        }

        problema.Instance ??= contexto.Request.Path;

        // TraceIdentifier lo sobrescribió el middleware de correlación, así que este es el
        // mismo valor que la cabecera de la respuesta y el que prefija cada línea del registro.
        problema.Extensions[MiddlewareCorrelacion.NOMBRE_PROPIEDAD_JSON] = contexto.TraceIdentifier;
    }
}

/// <summary>
/// Registro del contrato de errores en el contenedor.
/// </summary>
internal static class ConfiguracionContratoDeErrores
{
    /// <summary>
    /// Definición del código 429, que no viene en la tabla de ASP.NET Core.
    /// </summary>
    /// <remarks>
    /// La tabla que trae el framework cubre los códigos de la RFC 9110. El 429 no está ahí:
    /// se define aparte, en la RFC 6585. Sin esta entrada, el rechazo por límite de
    /// frecuencia sería el único error de la API sin enlace a su definición.
    /// </remarks>
    private const string ENLACE_DEMASIADAS_PETICIONES = "https://tools.ietf.org/html/rfc6585#section-4";

    private const string TITULO_DEMASIADAS_PETICIONES = "Demasiadas peticiones";

    /// <remarks>
    /// Se usa <c>Replace</c> y no <c>AddSingleton</c> a propósito. MVC registra su fábrica con
    /// <c>TryAddSingleton</c>, de modo que el resultado dependería de si esta llamada va antes
    /// o después de <c>AddControllers</c>: antes, MVC no la pisaría; después, quedarían dos
    /// registros y ganaría el último por casualidad. <c>Replace</c> deja una sola y funciona
    /// en cualquier orden.
    /// </remarks>
    internal static IServiceCollection AgregarContratoDeErrores(this IServiceCollection servicios)
    {
        servicios.Replace(ServiceDescriptor.Singleton<ProblemDetailsFactory, FabricaProblemDetails>());

        servicios.Configure<ApiBehaviorOptions>(opciones =>
            opciones.ClientErrorMapping[StatusCodes.Status429TooManyRequests] = new ClientErrorData
            {
                Title = TITULO_DEMASIADAS_PETICIONES,
                Link = ENLACE_DEMASIADAS_PETICIONES
            });

        return servicios;
    }
}
