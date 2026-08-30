using SB.GestionPagos.Dominio.Enumeraciones;

namespace SB.GestionPagos.Aplicacion.Empleados.Dtos;

/// <summary>
/// Parte común a los DTOs de edición de los cuatro tipos de empleado.
/// </summary>
/// <remarks>
/// Los datos personales y el estado se editan igual en todos los tipos; solo cambian los
/// datos del contrato. Al declarar esa parte común como contrato, la capa Servicios puede
/// aplicarla UNA sola vez en su clase base, y deja de ser posible que uno de los cuatro
/// servicios se olvide de llamar a <c>CambiarEstado</c>.
///
/// No declara comportamiento: es un contrato de forma. Los <c>record</c> posicionales que la
/// implementan la satisfacen con las propiedades que ya generan.
/// </remarks>
public interface ISolicitudActualizacionEmpleado
{
    string PrimerNombre { get; }

    string ApellidoPaterno { get; }

    string NumeroSeguroSocial { get; }

    string Departamento { get; }

    EstadoEmpleado Estado { get; }
}
