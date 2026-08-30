namespace SB.GestionPagos.Api.Seguridad;

/// <summary>
/// Nombres de las políticas de autorización del sistema.
/// </summary>
/// <remarks>
/// Los controladores escriben <c>[Authorize(Policy = PoliticasAutorizacion.SOLO_ADMINISTRADOR)]</c>
/// y no <c>[Authorize(Roles = "Administrador")]</c>. La diferencia importa: con roles sueltos,
/// el día que "escribir" pase a permitirlo también un rol Supervisor hay que buscar y editar
/// todos los atributos del proyecto; con políticas, se cambia la definición en un solo sitio
/// y todos los endpoints la heredan.
///
/// Dicho de otro modo: el rol dice QUIÉN es el usuario; la política dice QUÉ hace falta para
/// entrar. Los controladores solo deberían hablar del segundo.
/// </remarks>
public static class PoliticasAutorizacion
{
    /// <summary>
    /// Operaciones que modifican datos: alta, edición y baja de empleados y de entidades.
    /// </summary>
    public const string SOLO_ADMINISTRADOR = "SoloAdministrador";

    /// <summary>
    /// Consultas y reportes: accesible a los dos roles.
    /// </summary>
    /// <remarks>
    /// Exige un rol CONOCIDO y no solo estar autenticado. La diferencia se nota cuando
    /// mañana exista un tercer rol —"Auditor", pongamos—: con "basta estar autenticado",
    /// ese rol nuevo entraría a todo el sistema el día que se crea, sin que nadie lo haya
    /// decidido. Aquí no entra a nada hasta que se le agregue explícitamente.
    /// </remarks>
    public const string LECTURA = "Lectura";
}
