using SB.GestionPagos.Aplicacion.Comunes;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;

namespace SB.GestionPagos.Aplicacion.Empleados;

/// <summary>
/// Operaciones de empleados que son idénticas para los cuatro tipos de contrato.
/// </summary>
/// <remarks>
/// Buscar, consultar y eliminar no dependen del tipo: reciben o devuelven la forma unificada
/// <see cref="EmpleadoDto"/>. Por eso viven en una interfaz sin parámetros de tipo, que la
/// pantalla de consulta puede inyectar una sola vez.
/// </remarks>
public interface IEmpleadoServicio
{
    /// <summary>
    /// Devuelve la página de empleados que cumplen el filtro (RF-03).
    /// </summary>
    Task<Resultado<PaginaDto<EmpleadoDto>>> BuscarAsync(FiltroEmpleados filtro, CancellationToken cancelacion);

    Task<Resultado<EmpleadoDto>> ObtenerPorIdAsync(int identificador, CancellationToken cancelacion);

    Task<Resultado> EliminarAsync(int identificador, CancellationToken cancelacion);
}

/// <summary>
/// Alta y edición de UN tipo concreto de empleado.
/// </summary>
/// <typeparam name="TSolicitudCreacion">DTO de alta propio de ese tipo.</typeparam>
/// <typeparam name="TSolicitudActualizacion">DTO de edición propio de ese tipo.</typeparam>
/// <remarks>
/// El RNF-02 exige que agregar un tipo de empleado no obligue a modificar código existente.
/// Con una única interfaz que enumerara <c>CrearAsalariadoAsync</c>, <c>CrearPorHorasAsync</c>
/// y así, el quinto tipo obligaría a editarla —y con ella, a todas sus implementaciones—.
/// Al parametrizarla por los DTOs, un tipo nuevo solo agrega: su par de DTOs, su
/// implementación y su registro en el contenedor. Nada de lo ya escrito cambia.
///
/// Es el mismo recurso que usa .NET con <c>ILogger</c> e <c>ILogger&lt;T&gt;</c>: dos
/// interfaces del mismo nombre distinguidas por su número de parámetros de tipo.
/// </remarks>
public interface IEmpleadoServicio<TSolicitudCreacion, TSolicitudActualizacion>
    where TSolicitudCreacion : class
    where TSolicitudActualizacion : class
{
    Task<Resultado<EmpleadoDto>> CrearAsync(TSolicitudCreacion solicitud, CancellationToken cancelacion);

    Task<Resultado<EmpleadoDto>> ActualizarAsync(
        int identificador,
        TSolicitudActualizacion solicitud,
        CancellationToken cancelacion);
}
