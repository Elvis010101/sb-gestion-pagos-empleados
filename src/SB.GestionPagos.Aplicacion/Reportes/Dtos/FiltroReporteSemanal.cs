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
    /// <summary>Departamento a reportar. Nulo significa todos.</summary>
    public string? Departamento { get; init; }

    /// <summary>
    /// Si el reporte debe incluir a los empleados dados de baja.
    /// </summary>
    /// <remarks>
    /// El valor por omisión es <c>false</c>, y es una regla de negocio, no una comodidad: a
    /// un empleado inactivo no se le paga la semana, así que incluirlo inflaría la nómina.
    /// Se expone como opción explícita porque una auditoría sí puede necesitar ver a todos;
    /// pedirlo tiene que ser una decisión consciente de quien consulta, nunca el estado por
    /// omisión.
    ///
    /// Es un booleano y no un <c>EstadoEmpleado?</c> a propósito: con el enum, "no enviar
    /// nada" significaría "sin filtrar", es decir, la nómina inflada por descuido. Aquí el
    /// valor por omisión de <c>bool</c> es justamente el comportamiento seguro.
    /// </remarks>
    public bool IncluirInactivos { get; init; }
}
