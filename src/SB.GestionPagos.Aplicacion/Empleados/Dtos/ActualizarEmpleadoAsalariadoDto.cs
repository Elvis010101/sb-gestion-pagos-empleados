using SB.GestionPagos.Dominio.Enumeraciones;

namespace SB.GestionPagos.Aplicacion.Empleados.Dtos;

/// <summary>
/// Datos de edición de un Empleado Asalariado (RF-05).
/// </summary>
/// <remarks>
/// Es un tipo distinto del DTO de alta, y no el mismo reutilizado, por una asimetría real:
/// al crear no se elige el estado —el Dominio impone Activo— pero al editar sí, porque el
/// RF-03 filtra por estado y tiene que haber forma de cambiarlo. El identificador no viaja
/// en el cuerpo: va en la ruta, que es donde el protocolo HTTP identifica al recurso.
/// </remarks>
public sealed record ActualizarEmpleadoAsalariadoDto(
    string PrimerNombre,
    string ApellidoPaterno,
    string NumeroSeguroSocial,
    string Departamento,
    EstadoEmpleado Estado,
    decimal SalarioSemanal) : ISolicitudActualizacionEmpleado;
