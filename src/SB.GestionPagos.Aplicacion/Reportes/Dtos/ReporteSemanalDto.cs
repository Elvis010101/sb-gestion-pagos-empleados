namespace SB.GestionPagos.Aplicacion.Reportes.Dtos;

/// <summary>
/// Reporte semanal de pagos (RF-06).
/// </summary>
/// <param name="FechaGeneracionUtc">Instante en que se produjo el reporte.</param>
/// <param name="PoblacionIncluida">
/// Frase lista para el encabezado que describe a quiénes cubre el reporte, por ejemplo
/// "Empleados activos de todos los departamentos".
/// </param>
/// <param name="Departamento">Departamento reportado. Nulo significa todos.</param>
/// <param name="IncluyeInactivos">Si se contabilizó a los empleados dados de baja.</param>
/// <param name="CantidadEmpleados">Cuántos empleados quedaron incluidos.</param>
/// <param name="TotalNominaSemanal">Suma de los pagos semanales de todos ellos.</param>
/// <param name="Empleados">El detalle, empleado por empleado.</param>
/// <remarks>
/// El total se calcula en el servidor y viaja resuelto: si el frontend sumara las filas,
/// habría dos lugares donde se decide cuánto se paga esta semana.
///
/// La descripción de la población viaja PEGADA al total, y no queda solo en la pantalla que
/// lanzó la consulta: un total de nómina sin decir de quiénes es no se puede interpretar, y
/// en cuanto el reporte se imprime, se exporta o se pega en un correo, el contexto de la
/// pantalla se pierde y el número queda solo.
/// </remarks>
public sealed record ReporteSemanalDto(
    DateTime FechaGeneracionUtc,
    string PoblacionIncluida,
    string? Departamento,
    bool IncluyeInactivos,
    int CantidadEmpleados,
    decimal TotalNominaSemanal,
    IReadOnlyList<LineaReporteEmpleadoDto> Empleados);
