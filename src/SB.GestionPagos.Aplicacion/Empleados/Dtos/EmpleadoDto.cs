using SB.GestionPagos.Dominio.Enumeraciones;

namespace SB.GestionPagos.Aplicacion.Empleados.Dtos;

/// <summary>
/// Representación de lectura de un empleado, común a los cuatro tipos de contrato.
/// </summary>
/// <remarks>
/// Es un único DTO y no cuatro porque sus dos consumidores son únicos: una sola tabla de
/// consulta y un solo formulario de edición. Los campos propios de cada contrato viajan como
/// opcionales, y <see cref="TipoContrato"/> dice cuáles de ellos tienen valor: en el frontend
/// eso se modela como una unión discriminada de TypeScript.
///
/// El desglose del cálculo NO viaja aquí, sino en el reporte semanal. Adjuntarlo a cada fila
/// de un listado de 1.000 empleados triplicaría la respuesta para un dato que la pantalla de
/// consulta no muestra.
/// </remarks>
public sealed record EmpleadoDto
{
    public required int Id { get; init; }

    public required string PrimerNombre { get; init; }

    public required string ApellidoPaterno { get; init; }

    public required string NumeroSeguroSocial { get; init; }

    public required string Departamento { get; init; }

    public required EstadoEmpleado Estado { get; init; }

    /// <summary>
    /// Etiqueta del tipo de contrato, tomada del Dominio. Es lo que el frontend usa para
    /// saber qué campos opcionales debe mostrar.
    /// </summary>
    public required string TipoContrato { get; init; }

    /// <summary>
    /// Pago semanal ya calculado por el Dominio (RF-04).
    /// </summary>
    /// <remarks>
    /// Viaja calculado y no como fórmula porque el cálculo es responsabilidad del servidor:
    /// si el frontend lo repitiera, existirían dos versiones de la regla de negocio y
    /// tarde o temprano darían números distintos.
    /// </remarks>
    public required decimal PagoSemanalCalculado { get; init; }

    public required DateTime FechaCreacion { get; init; }

    /// <summary>Solo en Empleado Asalariado.</summary>
    public decimal? SalarioSemanal { get; init; }

    /// <summary>Solo en Empleado por Horas.</summary>
    public decimal? SueldoPorHora { get; init; }

    /// <summary>Solo en Empleado por Horas.</summary>
    public decimal? HorasTrabajadas { get; init; }

    /// <summary>En Empleado por Comisión y en Empleado Asalariado por Comisión.</summary>
    public decimal? VentasBrutas { get; init; }

    /// <summary>En Empleado por Comisión y en Empleado Asalariado por Comisión.</summary>
    public decimal? TarifaComision { get; init; }

    /// <summary>Solo en Empleado Asalariado por Comisión.</summary>
    public decimal? SalarioBase { get; init; }
}
