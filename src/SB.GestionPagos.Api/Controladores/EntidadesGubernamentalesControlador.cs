using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.GestionPagos.Api.Seguridad;
using SB.GestionPagos.Aplicacion.Comunes;
using SB.GestionPagos.Aplicacion.EntidadesGubernamentales;
using SB.GestionPagos.Aplicacion.EntidadesGubernamentales.Dtos;

namespace SB.GestionPagos.Api.Controladores;

/// <summary>
/// Mantenimiento del listado de entidades gubernamentales de la República Dominicana (RF-09).
/// </summary>
/// <remarks>
/// Este controlador es idéntico en forma al de empleados y, sin embargo, detrás no hay una
/// base de datos sino un archivo de texto plano. Que desde aquí no se note es la prueba de
/// que la Regla de Dependencia funciona: el controlador habla con una interfaz de la capa
/// Aplicación y no tiene forma de enterarse de dónde se guardan los datos.
/// </remarks>
[Route("api/entidades-gubernamentales")]
public sealed class EntidadesGubernamentalesControlador : ControladorApi
{
    private readonly IEntidadGubernamentalServicio _entidadGubernamentalServicio;

    public EntidadesGubernamentalesControlador(IEntidadGubernamentalServicio entidadGubernamentalServicio)
    {
        _entidadGubernamentalServicio = entidadGubernamentalServicio;
    }

    /// <summary>
    /// Devuelve las entidades que cumplen el filtro. Sin filtro, devuelve el catálogo completo.
    /// </summary>
    /// <remarks>
    /// No pagina, a diferencia de la consulta de empleados. El catálogo tiene 181 registros
    /// con un techo conocido, así que la lista entera cabe en una respuesta; la tabla de
    /// empleados no tiene techo. Paginar aquí sería agregar maquinaria sin ganancia.
    /// </remarks>
    /// <response code="200">Entidades que cumplen el filtro.</response>
    [HttpGet]
    [Authorize(Policy = PoliticasAutorizacion.LECTURA)]
    [ProducesResponseType(typeof(IReadOnlyList<EntidadGubernamentalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<EntidadGubernamentalDto>>> BuscarAsync(
        [FromQuery] FiltroEntidadesGubernamentales filtro,
        CancellationToken cancelacion)
    {
        // Un filtro sin criterios significa "no filtrar", y el servicio ya lo resuelve así.
        // Ramificar aquí entre "buscar" y "traer todo" metería una decisión en el controlador,
        // que es justo lo que no debe tener.
        Resultado<IReadOnlyList<EntidadGubernamentalDto>> resultado =
            await _entidadGubernamentalServicio.BuscarAsync(filtro, cancelacion);

        return resultado.EsExitoso ? Ok(resultado.Valor) : ProblemaDesde(resultado);
    }

    /// <summary>
    /// Devuelve una entidad gubernamental por su identificador.
    /// </summary>
    /// <response code="200">La entidad solicitada.</response>
    /// <response code="404">No existe una entidad con ese identificador.</response>
    [HttpGet("{identificador:int}", Name = NombresDeRuta.OBTENER_ENTIDAD_GUBERNAMENTAL)]
    [Authorize(Policy = PoliticasAutorizacion.LECTURA)]
    [ProducesResponseType(typeof(EntidadGubernamentalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EntidadGubernamentalDto>> ObtenerPorIdAsync(
        int identificador,
        CancellationToken cancelacion)
    {
        Resultado<EntidadGubernamentalDto> resultado =
            await _entidadGubernamentalServicio.ObtenerPorIdAsync(identificador, cancelacion);

        return resultado.EsExitoso ? Ok(resultado.Valor) : ProblemaDesde(resultado);
    }

    /// <summary>
    /// Agrega una entidad al catálogo.
    /// </summary>
    /// <response code="201">Entidad creada. La cabecera Location apunta al recurso.</response>
    /// <response code="400">Los datos enviados no superan las validaciones.</response>
    [HttpPost]
    [Authorize(Policy = PoliticasAutorizacion.SOLO_ADMINISTRADOR)]
    [ProducesResponseType(typeof(EntidadGubernamentalDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EntidadGubernamentalDto>> CrearAsync(
        CrearEntidadGubernamentalDto solicitud,
        CancellationToken cancelacion)
    {
        Resultado<EntidadGubernamentalDto> resultado =
            await _entidadGubernamentalServicio.CrearAsync(solicitud, cancelacion);

        if (!resultado.EsExitoso)
        {
            return ProblemaDesde(resultado);
        }

        return CreatedAtRoute(
            NombresDeRuta.OBTENER_ENTIDAD_GUBERNAMENTAL,
            new { identificador = resultado.Valor!.Id },
            resultado.Valor);
    }

    /// <summary>
    /// Actualiza una entidad del catálogo.
    /// </summary>
    /// <response code="200">La entidad actualizada.</response>
    /// <response code="404">No existe una entidad con ese identificador.</response>
    [HttpPut("{identificador:int}")]
    [Authorize(Policy = PoliticasAutorizacion.SOLO_ADMINISTRADOR)]
    [ProducesResponseType(typeof(EntidadGubernamentalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EntidadGubernamentalDto>> ActualizarAsync(
        int identificador,
        ActualizarEntidadGubernamentalDto solicitud,
        CancellationToken cancelacion)
    {
        Resultado<EntidadGubernamentalDto> resultado =
            await _entidadGubernamentalServicio.ActualizarAsync(identificador, solicitud, cancelacion);

        return resultado.EsExitoso ? Ok(resultado.Valor) : ProblemaDesde(resultado);
    }

    /// <summary>
    /// Elimina una entidad del catálogo.
    /// </summary>
    /// <remarks>
    /// Aquí la baja SÍ es física, a diferencia de la de empleados: esto es un catálogo, no un
    /// historial. Una entidad que se retira del listado no deja pagos detrás que haya que
    /// poder rastrear.
    /// </remarks>
    /// <response code="204">Entidad eliminada. Sin cuerpo: no hay nada que devolver.</response>
    /// <response code="404">No existe una entidad con ese identificador.</response>
    [HttpDelete("{identificador:int}")]
    [Authorize(Policy = PoliticasAutorizacion.SOLO_ADMINISTRADOR)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> EliminarAsync(int identificador, CancellationToken cancelacion)
    {
        Resultado resultado = await _entidadGubernamentalServicio.EliminarAsync(identificador, cancelacion);

        return resultado.EsExitoso ? NoContent() : ProblemaDesde(resultado);
    }
}
