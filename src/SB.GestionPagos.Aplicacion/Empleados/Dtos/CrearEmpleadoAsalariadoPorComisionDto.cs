namespace SB.GestionPagos.Aplicacion.Empleados.Dtos;

/// <summary>
/// Datos de alta de un Empleado Asalariado por Comisión.
/// </summary>
/// <remarks>
/// La bonificación del 10 % sobre el salario base no se pide al cliente: es una constante del
/// Dominio y se aplica en el cálculo. Si viajara en el DTO, el cliente podría fijarla.
/// </remarks>
public sealed record CrearEmpleadoAsalariadoPorComisionDto(
    string PrimerNombre,
    string ApellidoPaterno,
    string NumeroSeguroSocial,
    string Departamento,
    decimal VentasBrutas,
    decimal TarifaComision,
    decimal SalarioBase);
