using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.GestionPagos.Api.Seguridad;
using SB.GestionPagos.Aplicacion.Comunes;
using SB.GestionPagos.Aplicacion.Empleados;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;

namespace SB.GestionPagos.Api.Controladores;

/// <summary>
/// Alta y edición de UN tipo concreto de empleado. Cada tipo la hereda cerrando sus dos
/// parámetros de tipo y declarando su ruta.
/// </summary>
/// <typeparam name="TSolicitudCreacion">DTO de alta propio del tipo.</typeparam>
/// <typeparam name="TSolicitudActualizacion">DTO de edición propio del tipo.</typeparam>
/// <remarks>
/// El RNF-02 exige que agregar un tipo de empleado no obligue a modificar código existente, y
/// el canal HTTP es donde ese requisito estaba a punto de romperse: cuatro tipos por dos
/// operaciones son ocho acciones idénticas salvo por el DTO que reciben, y el quinto tipo
/// obligaría a editar un archivo ya escrito.
///
/// Con este controlador genérico, agregar un tipo es agregar UN archivo de doce líneas. Nada
/// de lo que ya existe cambia. Es la misma técnica que ya usa
/// <see cref="IEmpleadoServicio{TSolicitudCreacion, TSolicitudActualizacion}"/> en la capa
/// Aplicación, continuada hasta la frontera del sistema.
///
/// ALTERNATIVA DESCARTADA: un único controlador con las ocho acciones escritas a mano. Es más
/// fácil de leer de un vistazo, y por eso es lo más común, pero incumple el RNF-02 y repite
/// ocho veces la misma traducción de resultado a respuesta. También se descartó registrar un
/// controlador genérico con un <c>IApplicationFeatureProvider</c>: resuelve lo mismo con
/// bastante más maquinaria de la que este problema justifica.
///
/// Es abstracta, así que ASP.NET Core no la descubre: los endpoints los publican las cuatro
/// clases que la cierran.
/// </remarks>
public abstract class ControladorEmpleadosPorTipo<TSolicitudCreacion, TSolicitudActualizacion> : ControladorApi
    where TSolicitudCreacion : class
    where TSolicitudActualizacion : class
{
    private readonly IEmpleadoServicio<TSolicitudCreacion, TSolicitudActualizacion> _empleadoServicio;

    protected ControladorEmpleadosPorTipo(
        IEmpleadoServicio<TSolicitudCreacion, TSolicitudActualizacion> empleadoServicio)
    {
        _empleadoServicio = empleadoServicio;
    }

    /// <summary>
    /// Registra un empleado de este tipo y devuelve su pago semanal ya calculado (RF-01, RF-02).
    /// </summary>
    /// <response code="201">Empleado creado. La cabecera Location apunta al recurso.</response>
    /// <response code="400">Los datos enviados no superan las validaciones.</response>
    /// <response code="409">Ya existe un empleado con ese número de seguro social.</response>
    [HttpPost]
    [Authorize(Policy = PoliticasAutorizacion.SOLO_ADMINISTRADOR)]
    [ProducesResponseType(typeof(EmpleadoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmpleadoDto>> CrearAsync(
        TSolicitudCreacion solicitud,
        CancellationToken cancelacion)
    {
        Resultado<EmpleadoDto> resultado = await _empleadoServicio.CrearAsync(solicitud, cancelacion);

        if (!resultado.EsExitoso)
        {
            return ProblemaDesde(resultado);
        }

        // 201 con Location, no 200. El código dice "se creó algo nuevo" y la cabecera dice
        // dónde vive: el cliente puede consultarlo después sin adivinar la URL ni tener que
        // saber cómo se construye.
        return CreatedAtRoute(
            NombresDeRuta.OBTENER_EMPLEADO,
            new { identificador = resultado.Valor!.Id },
            resultado.Valor);
    }

    /// <summary>
    /// Actualiza un empleado de este tipo y recalcula su pago semanal (RF-05).
    /// </summary>
    /// <response code="200">Empleado actualizado, con el pago semanal recalculado.</response>
    /// <response code="404">No existe un empleado de este tipo con ese identificador.</response>
    [HttpPut("{identificador:int}")]
    [Authorize(Policy = PoliticasAutorizacion.SOLO_ADMINISTRADOR)]
    [ProducesResponseType(typeof(EmpleadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmpleadoDto>> ActualizarAsync(
        int identificador,
        TSolicitudActualizacion solicitud,
        CancellationToken cancelacion)
    {
        Resultado<EmpleadoDto> resultado =
            await _empleadoServicio.ActualizarAsync(identificador, solicitud, cancelacion);

        if (!resultado.EsExitoso)
        {
            return ProblemaDesde(resultado);
        }

        // Se devuelve el empleado actualizado en lugar de un 204 sin cuerpo: el pago semanal
        // lo recalcula el servidor, así que el cliente no puede saber el valor nuevo sin
        // pedirlo. Devolverlo aquí le ahorra una segunda petición.
        return Ok(resultado.Valor);
    }
}
