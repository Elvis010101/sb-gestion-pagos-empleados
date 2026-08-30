using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace SB.GestionPagos.Api.Middleware;

/// <summary>
/// Escribe una respuesta de error con el formato ProblemDetails (RFC 7807) desde dentro del
/// canal de middlewares.
/// </summary>
/// <remarks>
/// Los controladores tienen el método <c>Problem(...)</c> de <c>ControllerBase</c>, pero un
/// middleware está por fuera de MVC y no lo hereda. Esta clase es el equivalente para esa
/// zona del canal, y existe para que los DOS sitios que responden errores sin pasar por un
/// controlador —el manejador de excepciones y el rechazo por límite de frecuencia— produzcan
/// exactamente el mismo contrato que produce un controlador. El frontend escribe un solo
/// manejador de errores, no tres.
///
/// El cuerpo no se arma aquí: se le pide a la misma fábrica que usa MVC. Así hay UN solo
/// lugar en todo el sistema que decide qué lleva un ProblemDetails, y un error nacido en el
/// canal es indistinguible de uno nacido en un controlador.
/// </remarks>
internal static class RespuestaProblema
{
    /// <summary>
    /// Tipo de contenido que la RFC 7807 define para estas respuestas.
    /// </summary>
    /// <remarks>
    /// No es <c>application/json</c> a secas: el sufijo <c>+json</c> le dice al cliente que
    /// el cuerpo se serializa como JSON pero que su esquema es el de un problema, y eso
    /// permite que un interceptor lo reconozca por la cabecera sin inspeccionar el cuerpo.
    /// </remarks>
    internal const string TIPO_CONTENIDO = "application/problem+json";

    private const string NOMBRE_PROPIEDAD_DETALLE_TECNICO = "detalleTecnico";

    internal static Task EscribirAsync(
        HttpContext contexto,
        int codigoEstado,
        string titulo,
        string detalle,
        string? detalleTecnico = null)
    {
        // Se resuelve por petición y no por inyección en el constructor porque quien llama es
        // un middleware —construido una sola vez al arrancar— y una función de configuración,
        // que ni siquiera es una clase.
        ProblemDetailsFactory fabrica = contexto.RequestServices.GetRequiredService<ProblemDetailsFactory>();

        ProblemDetails problema = fabrica.CreateProblemDetails(
            contexto,
            statusCode: codigoEstado,
            title: titulo,
            type: null,
            detail: detalle);

        // Solo llega con valor en Desarrollo. Es la única puerta por la que podría salir un
        // rastro de pila, y quien la abre es el manejador de excepciones tras comprobar el
        // entorno: aquí no se decide nada, solo se escribe lo que ya se decidió.
        if (detalleTecnico is not null)
        {
            problema.Extensions[NOMBRE_PROPIEDAD_DETALLE_TECNICO] = detalleTecnico;
        }

        contexto.Response.StatusCode = codigoEstado;

        return contexto.Response.WriteAsJsonAsync(problema, options: null, contentType: TIPO_CONTENIDO);
    }
}
