using SB.GestionPagos.Dominio.Enumeraciones;

namespace SB.GestionPagos.Aplicacion.Empleados.Dtos;

/// <summary>Datos de edición de un Empleado Asalariado por Comisión (RF-05).</summary>
public sealed record ActualizarEmpleadoAsalariadoPorComisionDto(
    string PrimerNombre,
    string ApellidoPaterno,
    string NumeroSeguroSocial,
    string Departamento,
    EstadoEmpleado Estado,
    decimal VentasBrutas,
    decimal TarifaComision,
    decimal SalarioBase);
