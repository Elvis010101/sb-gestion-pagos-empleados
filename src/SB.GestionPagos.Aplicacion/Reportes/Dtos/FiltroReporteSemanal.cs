using SB.GestionPagos.Dominio.Enumeraciones;

namespace SB.GestionPagos.Aplicacion.Reportes.Dtos;

/// <summary>
/// Criterios opcionales para acotar el reporte semanal.
/// </summary>
/// <remarks>
/// Deliberadamente NO tiene paginación, a diferencia de <c>FiltroEmpleados</c>. El reporte es
/// un documento cuyo total debe ser el total: una "página del reporte" daría una suma parcial
/// presentada como si fuera la nómina completa. Es también el escenario exacto que mide el
/// RNF-04 —1.000 empleados calculados en menos de 2 segundos—, y por eso este es el único
/// camino del sistema que trae la colección entera.
/// </remarks>
public sealed record FiltroReporteSemanal
{
    public string? Departamento { get; init; }

    public EstadoEmpleado? Estado { get; init; }
}
