namespace SB.GestionPagos.Aplicacion.Empleados.Dtos;

/// <summary>
/// Datos de alta de un Empleado Asalariado.
/// </summary>
/// <remarks>
/// No lleva <c>Id</c> ni <c>Estado</c>: el identificador lo asigna la base de datos y el
/// estado inicial lo impone el Dominio (todo empleado nace Activo). Un DTO de entrada solo
/// debe contener lo que el cliente tiene derecho a decidir.
/// </remarks>
public sealed record CrearEmpleadoAsalariadoDto(
    string PrimerNombre,
    string ApellidoPaterno,
    string NumeroSeguroSocial,
    string Departamento,
    decimal SalarioSemanal);
