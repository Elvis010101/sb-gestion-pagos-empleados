namespace SB.GestionPagos.Aplicacion.Reportes.Dtos;

/// <summary>
/// Fila del reporte semanal correspondiente a un empleado (RF-06).
/// </summary>
/// <param name="Id">Identificador del empleado.</param>
/// <param name="NombreCompleto">Nombre ya armado para mostrarse.</param>
/// <param name="Departamento">Departamento al que pertenece.</param>
/// <param name="TipoContrato">Etiqueta del tipo de contrato, provista por el Dominio.</param>
/// <param name="PagoSemanal">Total a pagar en la semana.</param>
/// <param name="DesglosePago">
/// Los conceptos que componen ese total. Es lo que satisface la exigencia del RF-06 de
/// "detallar los cálculos según el tipo de contrato".
/// </param>
/// <remarks>
/// Aquí el nombre viaja compuesto, y en <c>EmpleadoDto</c> viaja en campos separados. No es
/// una inconsistencia: el reporte es un documento terminado que se lee, mientras que el DTO
/// de empleado alimenta un formulario donde cada campo se edita por separado.
/// </remarks>
public sealed record LineaReporteEmpleadoDto(
    int Id,
    string NombreCompleto,
    string Departamento,
    string TipoContrato,
    decimal PagoSemanal,
    IReadOnlyList<LineaCalculoDto> DesglosePago);
