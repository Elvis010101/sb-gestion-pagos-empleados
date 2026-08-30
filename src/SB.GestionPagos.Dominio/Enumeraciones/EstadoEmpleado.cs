namespace SB.GestionPagos.Dominio.Enumeraciones;

/// <summary>
/// Situación laboral del empleado. Sustenta el filtro por estado del RF-03.
/// </summary>
/// <remarks>
/// Los valores se asignan de forma explícita: lo que se persiste es el número, así que
/// reordenar o insertar miembros no debe cambiar el significado de los datos ya guardados.
/// </remarks>
public enum EstadoEmpleado
{
    Activo = 1,
    Inactivo = 2
}
