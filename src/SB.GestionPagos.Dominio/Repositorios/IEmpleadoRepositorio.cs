using SB.GestionPagos.Dominio.Entidades;

namespace SB.GestionPagos.Dominio.Repositorios;

/// <summary>
/// Acceso a la colección de empleados.
/// </summary>
/// <remarks>
/// El contrato se declara aquí, en el Dominio, y se implementa en Infraestructura. Así la
/// flecha de dependencia apunta hacia adentro: Infraestructura conoce al Dominio, no al revés.
/// </remarks>
public interface IEmpleadoRepositorio
{
    Task<Empleado?> ObtenerPorIdAsync(int identificador, CancellationToken cancelacion);

    Task<IReadOnlyList<Empleado>> BuscarAsync(FiltroBusquedaEmpleado filtro, CancellationToken cancelacion);

    Task AgregarAsync(Empleado empleado, CancellationToken cancelacion);

    Task ActualizarAsync(Empleado empleado, CancellationToken cancelacion);

    Task EliminarAsync(Empleado empleado, CancellationToken cancelacion);
}
