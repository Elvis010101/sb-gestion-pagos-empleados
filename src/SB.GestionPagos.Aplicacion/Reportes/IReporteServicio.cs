using SB.GestionPagos.Aplicacion.Comunes;
using SB.GestionPagos.Aplicacion.Reportes.Dtos;

namespace SB.GestionPagos.Aplicacion.Reportes;

/// <summary>
/// Generación del reporte semanal de pagos (RF-06).
/// </summary>
/// <remarks>
/// Es una interfaz aparte de <c>IEmpleadoServicio</c> aunque lea los mismos datos: son dos
/// razones de cambio distintas (Principio de Responsabilidad Única). El mantenimiento de
/// empleados cambia cuando cambia el CRUD; el reporte cambia cuando cambia lo que la nómina
/// necesita mostrar. Además permite que el rol Usuario acceda al reporte sin abrirle el CRUD.
/// </remarks>
public interface IReporteServicio
{
    Task<Resultado<ReporteSemanalDto>> GenerarReporteSemanalAsync(
        FiltroReporteSemanal filtro,
        CancellationToken cancelacion);
}
