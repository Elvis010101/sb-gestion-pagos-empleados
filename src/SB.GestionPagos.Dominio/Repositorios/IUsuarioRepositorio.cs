using SB.GestionPagos.Dominio.Entidades;

namespace SB.GestionPagos.Dominio.Repositorios;

/// <summary>
/// Acceso a los usuarios que se autentican contra la API.
/// </summary>
public interface IUsuarioRepositorio
{
    Task<Usuario?> ObtenerPorIdAsync(int identificador, CancellationToken cancelacion);

    /// <summary>Búsqueda por nombre de usuario: es el punto de entrada del inicio de sesión.</summary>
    Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancelacion);

    Task<IReadOnlyList<Usuario>> ObtenerTodosAsync(CancellationToken cancelacion);

    Task AgregarAsync(Usuario usuario, CancellationToken cancelacion);

    Task ActualizarAsync(Usuario usuario, CancellationToken cancelacion);

    Task EliminarAsync(Usuario usuario, CancellationToken cancelacion);
}
