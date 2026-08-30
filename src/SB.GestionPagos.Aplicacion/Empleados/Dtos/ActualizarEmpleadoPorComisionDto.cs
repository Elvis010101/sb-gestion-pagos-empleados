using SB.GestionPagos.Dominio.Enumeraciones;

namespace SB.GestionPagos.Aplicacion.Empleados.Dtos;

/// <summary>Datos de edición de un Empleado por Comisión (RF-05).</summary>
public sealed record ActualizarEmpleadoPorComisionDto(
    string PrimerNombre,
    string ApellidoPaterno,
    string NumeroSeguroSocial,
    string Departamento,
    EstadoEmpleado Estado,
    decimal VentasBrutas,
    decimal TarifaComision);
