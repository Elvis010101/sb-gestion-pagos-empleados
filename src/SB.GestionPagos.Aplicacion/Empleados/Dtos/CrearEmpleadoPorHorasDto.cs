namespace SB.GestionPagos.Aplicacion.Empleados.Dtos;

/// <summary>
/// Datos de alta de un Empleado por Horas.
/// </summary>
/// <remarks>
/// Incluye <c>PrimerNombre</c> aunque la p. 4 del PDF lo omite en la captura de este tipo:
/// es una omisión de redacción del documento, registrada como supuesto en el README.
/// </remarks>
public sealed record CrearEmpleadoPorHorasDto(
    string PrimerNombre,
    string ApellidoPaterno,
    string NumeroSeguroSocial,
    string Departamento,
    decimal SueldoPorHora,
    decimal HorasTrabajadas);
