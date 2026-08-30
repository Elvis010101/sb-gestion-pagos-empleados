using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.GestionPagos.Api.Seguridad;
using SB.GestionPagos.Aplicacion.Comunes;
using SB.GestionPagos.Aplicacion.Empleados;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;

namespace SB.GestionPagos.Api.Controladores;

/// <summary>
/// Consulta y baja de empleados: las operaciones que son iguales para los cuatro tipos de
/// contrato (RF-01, RF-03).
/// </summary>
/// <remarks>
/// El alta y la edición NO están aquí, porque dependen del tipo: viven en los cuatro
/// controladores que heredan de
/// <see cref="ControladorEmpleadosPorTipo{TSolicitudCreacion, TSolicitudActualizacion}"/>.
/// El corte es el mismo que ya hacía la capa Aplicación con sus dos interfaces, y no una
/// invención del canal HTTP.
/// </remarks>
[Route("api/empleados")]
public sealed class EmpleadosControlador : ControladorApi
{
    private readonly IEmpleadoServicio _empleadoServicio;

    public EmpleadosControlador(IEmpleadoServicio empleadoServicio)
    {
        _empleadoServicio = empleadoServicio;
    }

    /// <summary>
    /// Devuelve la página de empleados que cumplen el filtro (RF-03).
    /// </summary>
    /// <remarks>
    /// Filtra por nombre, departamento y estado, y siempre pagina. Nunca devuelve la tabla
    /// completa: una consulta sin límite es una consulta que deja de responder el día que la
    /// tabla crece.
    /// </remarks>
    /// <response code="200">Página de empleados, con el total de registros que cumplen el filtro.</response>
    /// <response code="400">Los parámetros de paginación están fuera de rango.</response>
    [HttpGet]
    [Authorize(Policy = PoliticasAutorizacion.LECTURA)]
    [ProducesResponseType(typeof(PaginaDto<EmpleadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginaDto<EmpleadoDto>>> BuscarAsync(
        // [FromQuery] es obligatorio: con [ApiController], un parámetro de tipo complejo se
        // asume que viene en el cuerpo, y un GET no lleva cuerpo. Sin este atributo el
        // enlazado fallaría en tiempo de ejecución, no de compilación.
        [FromQuery] FiltroEmpleados filtro,
        CancellationToken cancelacion)
    {
        Resultado<PaginaDto<EmpleadoDto>> resultado = await _empleadoServicio.BuscarAsync(filtro, cancelacion);

        return resultado.EsExitoso ? Ok(resultado.Valor) : ProblemaDesde(resultado);
    }

    /// <summary>
    /// Devuelve un empleado por su identificador, con su pago semanal calculado.
    /// </summary>
    /// <response code="200">El empleado solicitado.</response>
    /// <response code="404">No existe un empleado con ese identificador.</response>
    [HttpGet("{identificador:int}", Name = NombresDeRuta.OBTENER_EMPLEADO)]
    [Authorize(Policy = PoliticasAutorizacion.LECTURA)]
    [ProducesResponseType(typeof(EmpleadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmpleadoDto>> ObtenerPorIdAsync(
        int identificador,
        CancellationToken cancelacion)
    {
        Resultado<EmpleadoDto> resultado = await _empleadoServicio.ObtenerPorIdAsync(identificador, cancelacion);

        return resultado.EsExitoso ? Ok(resultado.Valor) : ProblemaDesde(resultado);
    }

    /// <summary>
    /// Da de baja a un empleado. Es una baja lógica: la fila no se borra.
    /// </summary>
    /// <remarks>
    /// El empleado queda inactivo y deja de contar en la nómina semanal, pero su historial
    /// sigue existiendo. Borrar la fila haría irrastreables los pagos que ya se le hicieron.
    /// </remarks>
    /// <response code="204">Empleado dado de baja. Sin cuerpo: no hay nada que devolver.</response>
    /// <response code="404">No existe un empleado con ese identificador.</response>
    [HttpDelete("{identificador:int}")]
    [Authorize(Policy = PoliticasAutorizacion.SOLO_ADMINISTRADOR)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> EliminarAsync(int identificador, CancellationToken cancelacion)
    {
        Resultado resultado = await _empleadoServicio.EliminarAsync(identificador, cancelacion);

        // 204 y no 200 con un cuerpo de confirmación: el recurso ya no está disponible, así
        // que no hay representación que devolver. Repetir la llamada da otra vez 204, porque
        // el servicio es idempotente: un doble clic no produce un error.
        return resultado.EsExitoso ? NoContent() : ProblemaDesde(resultado);
    }
}
