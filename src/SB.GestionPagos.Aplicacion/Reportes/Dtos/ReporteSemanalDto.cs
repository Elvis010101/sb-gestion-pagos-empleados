namespace SB.GestionPagos.Aplicacion.Reportes.Dtos;

/// <summary>
/// Reporte semanal de pagos (RF-06).
/// </summary>
/// <param name="FechaGeneracionUtc">Instante en que se produjo el reporte.</param>
/// <param name="CantidadEmpleados">Cuántos empleados quedaron incluidos.</param>
/// <param name="TotalNominaSemanal">Suma de los pagos semanales de todos ellos.</param>
/// <param name="Empleados">El detalle, empleado por empleado.</param>
/// <remarks>
/// El total se calcula en el servidor y viaja resuelto: si el frontend sumara las filas,
/// habría dos lugares donde se decide cuánto se paga esta semana.
/// </remarks>
public sealed record ReporteSemanalDto(
    DateTime FechaGeneracionUtc,
    int CantidadEmpleados,
    decimal TotalNominaSemanal,
    IReadOnlyList<LineaReporteEmpleadoDto> Empleados);
