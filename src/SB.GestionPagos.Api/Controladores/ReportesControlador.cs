using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.GestionPagos.Api.Seguridad;
using SB.GestionPagos.Aplicacion.Comunes;
using SB.GestionPagos.Aplicacion.Reportes;
using SB.GestionPagos.Aplicacion.Reportes.Dtos;

namespace SB.GestionPagos.Api.Controladores;

/// <summary>
/// Reporte semanal de pagos (RF-06).
/// </summary>
/// <remarks>
/// Es un controlador aparte del de empleados aunque lea los mismos datos, por la misma razón
/// por la que <see cref="IReporteServicio"/> es una interfaz aparte: son dos razones de
/// cambio distintas. Además permite que el rol Usuario consulte la nómina sin darle acceso al
/// mantenimiento de empleados.
/// </remarks>
[Route("api/reportes")]
public sealed class ReportesControlador : ControladorApi
{
    private readonly IReporteServicio _reporteServicio;

    public ReportesControlador(IReporteServicio reporteServicio)
    {
        _reporteServicio = reporteServicio;
    }

    /// <summary>
    /// Genera el reporte de la nómina semanal, con el desglose del cálculo de cada empleado.
    /// </summary>
    /// <remarks>
    /// No pagina, y es deliberado: el total de un reporte tiene que ser el total. Una "página
    /// del reporte" daría una suma parcial presentada como si fuera la nómina completa.
    ///
    /// Por omisión excluye a los empleados inactivos, porque a un empleado dado de baja no se
    /// le paga la semana. Incluirlos es posible —una auditoría puede necesitarlo— pero hay
    /// que pedirlo explícitamente.
    /// </remarks>
    /// <response code="200">El reporte, con el total de la nómina y el detalle por empleado.</response>
    [HttpGet("nomina-semanal")]
    [Authorize(Policy = PoliticasAutorizacion.LECTURA)]
    [ProducesResponseType(typeof(ReporteSemanalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ReporteSemanalDto>> GenerarNominaSemanalAsync(
        [FromQuery] FiltroReporteSemanal filtro,
        CancellationToken cancelacion)
    {
        Resultado<ReporteSemanalDto> resultado =
            await _reporteServicio.GenerarReporteSemanalAsync(filtro, cancelacion);

        return resultado.EsExitoso ? Ok(resultado.Valor) : ProblemaDesde(resultado);
    }
}
