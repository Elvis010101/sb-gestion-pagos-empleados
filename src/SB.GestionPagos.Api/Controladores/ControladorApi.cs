using Microsoft.AspNetCore.Mvc;
using SB.GestionPagos.Aplicacion.Comunes;

namespace SB.GestionPagos.Api.Controladores;

/// <summary>
/// Base de todos los controladores del sistema.
/// </summary>
/// <remarks>
/// Aporta lo único que todos comparten: la traducción de un <see cref="IResultado"/> fallido
/// al código de estado HTTP que le corresponde. Esa traducción es la frontera exacta entre
/// las dos capas: la capa Aplicación clasifica el fallo en términos de negocio y el proyecto
/// Api lo expresa en términos de protocolo. Ningún servicio nombra un número HTTP y ningún
/// controlador decide qué salió mal.
///
/// Es abstracta a propósito: ASP.NET Core no la descubre como controlador, así que no publica
/// endpoints propios.
/// </remarks>
// Sin [Produces("application/json"): ese atributo fuerza el tipo de contenido de TODAS las
// respuestas del controlador, incluidas las de error, y las de error deben anunciarse como
// application/problem+json, que es el tipo que la RFC 7807 define para ellas. El único
// formateador registrado es el de JSON, así que las respuestas exitosas siguen siendo JSON
// igualmente, y Swagger lo documenta a partir de los [ProducesResponseType].
[ApiController]
public abstract class ControladorApi : ControllerBase
{
    private const string TITULO_NO_ENCONTRADO = "Recurso no encontrado";
    private const string TITULO_CONFLICTO = "Conflicto con el estado actual";
    private const string TITULO_REGLA_DE_NEGOCIO = "Solicitud inválida";
    private const string TITULO_CREDENCIALES_INVALIDAS = "Credenciales inválidas";
    private const string TITULO_ERROR_INESPERADO = "Error interno del servidor";

    /// <summary>
    /// Convierte un resultado fallido en la respuesta de error que le corresponde.
    /// </summary>
    /// <remarks>
    /// Es un <c>switch</c> sobre un enum, y no contradice la prohibición de resolver el
    /// cálculo del pago con un <c>switch</c> sobre el tipo de empleado. Aquella regla existe
    /// porque los tipos de empleado CRECEN: cada tipo nuevo obligaría a editar el switch. Los
    /// tipos de error, en cambio, son cerrados por definición —el propio enum documenta que
    /// cada miembro existe porque se traduce a un código distinto—, y esta es la única
    /// función del sistema que los traduce. Poner cada código dentro de su servicio sería lo
    /// contrario a lo que se busca: metería HTTP en la capa que no debe conocerlo.
    /// </remarks>
    protected ActionResult ProblemaDesde(IResultado resultado)
    {
        (int codigoEstado, string titulo) = resultado.TipoError switch
        {
            TipoErrorAplicacion.NoEncontrado => (StatusCodes.Status404NotFound, TITULO_NO_ENCONTRADO),
            TipoErrorAplicacion.Conflicto => (StatusCodes.Status409Conflict, TITULO_CONFLICTO),
            TipoErrorAplicacion.ReglaDeNegocio => (StatusCodes.Status400BadRequest, TITULO_REGLA_DE_NEGOCIO),
            TipoErrorAplicacion.CredencialesInvalidas =>
                (StatusCodes.Status401Unauthorized, TITULO_CREDENCIALES_INVALIDAS),

            // Incluye TipoErrorAplicacion.Ninguno: llegar aquí con un resultado exitoso es un
            // error de programación en el controlador, no una respuesta legítima, y un 500 es
            // la forma honesta de decirlo.
            _ => (StatusCodes.Status500InternalServerError, TITULO_ERROR_INESPERADO)
        };

        // Problem() de ControllerBase produce un ProblemDetails con el mismo formato que
        // genera el framework por su cuenta ante un 404 o un 415. Construirlo a mano aquí
        // daría dos formatos de error distintos en la misma API.
        return Problem(title: titulo, detail: resultado.Mensaje, statusCode: codigoEstado);
    }
}
