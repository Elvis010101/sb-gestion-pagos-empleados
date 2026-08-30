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

    /// <summary>
    /// Devuelve un tramo de empleados que cumplen el filtro (RF-03), junto al total.
    /// </summary>
    /// <remarks>
    /// Recibe el filtro y la paginación como parámetros —en lugar de devolver todo y dejar
    /// que quien llama recorte— para que la implementación pueda traducirlos a WHERE, OFFSET
    /// y FETCH. Es la firma la que hace posible el RNF-04: si el contrato devolviera la
    /// colección completa, ninguna implementación podría evitar traerla entera a memoria.
    /// </remarks>
    Task<PaginaDeRegistros<Empleado>> BuscarPaginaAsync(
        FiltroBusquedaEmpleado filtro,
        Paginacion paginacion,
        CancellationToken cancelacion);

    /// <summary>
    /// Devuelve todos los empleados que cumplen el filtro, sin paginar.
    /// </summary>
    /// <remarks>
    /// Existe como método aparte y no como "página de tamaño infinito" porque atiende un
    /// caso de uso distinto: el reporte semanal del RF-06 necesita la nómina completa para
    /// que su total sea el total. Tenerlo explícito impide que alguien lo invoque por
    /// descuido desde la pantalla de consulta.
    /// </remarks>
    Task<IReadOnlyList<Empleado>> ListarAsync(FiltroBusquedaEmpleado filtro, CancellationToken cancelacion);

    Task AgregarAsync(Empleado empleado, CancellationToken cancelacion);

    Task ActualizarAsync(Empleado empleado, CancellationToken cancelacion);

    Task EliminarAsync(Empleado empleado, CancellationToken cancelacion);
}
