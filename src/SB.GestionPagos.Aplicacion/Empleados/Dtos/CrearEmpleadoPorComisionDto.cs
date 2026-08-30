namespace SB.GestionPagos.Aplicacion.Empleados.Dtos;

/// <summary>
/// Datos de alta de un Empleado por Comisión.
/// </summary>
/// <remarks>
/// <c>TarifaComision</c> viaja como fracción (0.10 = 10 %), igual que en el Dominio. La
/// conversión a porcentaje para mostrarlo es responsabilidad de la interfaz, no del contrato.
/// </remarks>
public sealed record CrearEmpleadoPorComisionDto(
    string PrimerNombre,
    string ApellidoPaterno,
    string NumeroSeguroSocial,
    string Departamento,
    decimal VentasBrutas,
    decimal TarifaComision);
