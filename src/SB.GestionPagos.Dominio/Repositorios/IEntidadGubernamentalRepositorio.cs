using SB.GestionPagos.Dominio.Entidades;

namespace SB.GestionPagos.Dominio.Repositorios;

/// <summary>
/// Acceso al listado de entidades gubernamentales.
/// </summary>
/// <remarks>
/// La interfaz es idéntica en forma a la de empleados aunque la implementación sea un archivo
/// de texto plano y no SQL Server: ese es justamente el punto de declararla en el Dominio.
/// </remarks>
public interface IEntidadGubernamentalRepositorio
{
    Task<IReadOnlyList<EntidadGubernamental>> ObtenerTodasAsync(CancellationToken cancelacion);

    Task<EntidadGubernamental?> ObtenerPorIdAsync(int identificador, CancellationToken cancelacion);

    /// <summary>
    /// Devuelve las entidades que satisfacen el filtro, ordenadas por nombre.
    /// </summary>
    /// <remarks>
    /// No pagina, a diferencia de la búsqueda de empleados: el catálogo tiene 181 registros y
    /// la implementación de archivo plano los tiene todos en memoria de todas formas. Paginar
    /// aquí sería maquinaria sin ganancia.
    /// </remarks>
    Task<IReadOnlyList<EntidadGubernamental>> BuscarAsync(
        FiltroBusquedaEntidadGubernamental filtro,
        CancellationToken cancelacion);

    Task AgregarAsync(EntidadGubernamental entidadGubernamental, CancellationToken cancelacion);

    Task ActualizarAsync(EntidadGubernamental entidadGubernamental, CancellationToken cancelacion);

    Task EliminarAsync(EntidadGubernamental entidadGubernamental, CancellationToken cancelacion);
}
