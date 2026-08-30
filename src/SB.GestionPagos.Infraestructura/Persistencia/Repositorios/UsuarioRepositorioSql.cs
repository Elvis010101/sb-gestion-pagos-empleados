using Microsoft.EntityFrameworkCore;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Dominio.Repositorios;

namespace SB.GestionPagos.Infraestructura.Persistencia.Repositorios;

/// <summary>
/// Implementación de <see cref="IUsuarioRepositorio"/> sobre SQL Server con EF Core.
/// </summary>
internal sealed class UsuarioRepositorioSql : IUsuarioRepositorio
{
    private readonly GestionPagosDbContext _contexto;

    public UsuarioRepositorioSql(GestionPagosDbContext contexto)
    {
        _contexto = contexto;
    }

    /// <summary>
    /// Trae un usuario por identificador, CON seguimiento: es el camino de la edición.
    /// </summary>
    public Task<Usuario?> ObtenerPorIdAsync(int identificador, CancellationToken cancelacion)
        => _contexto.Usuarios
            .FirstOrDefaultAsync(usuario => usuario.Id == identificador, cancelacion);

    /// <summary>
    /// Trae un usuario por su nombre, SIN seguimiento.
    /// </summary>
    /// <remarks>
    /// Es la consulta del inicio de sesión, que solo lee: compara el hash y emite un token,
    /// sin modificar nada. Se apoya en el índice único de <c>NombreUsuario</c>, así que es
    /// una búsqueda directa y no un recorrido de la tabla, algo que importa porque es el
    /// único endpoint que se puede invocar sin estar autenticado.
    /// </remarks>
    public Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancelacion)
    {
        string nombreNormalizado = nombreUsuario.Trim();

        return _contexto.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(usuario => usuario.NombreUsuario == nombreNormalizado, cancelacion);
    }

    public async Task<IReadOnlyList<Usuario>> ObtenerTodosAsync(CancellationToken cancelacion)
        => await _contexto.Usuarios
            .AsNoTracking()
            .OrderBy(usuario => usuario.NombreUsuario)
            .ToListAsync(cancelacion);

    public async Task AgregarAsync(Usuario usuario, CancellationToken cancelacion)
    {
        _contexto.Usuarios.Add(usuario);

        await _contexto.SaveChangesAsync(cancelacion);
    }

    public async Task ActualizarAsync(Usuario usuario, CancellationToken cancelacion)
    {
        if (_contexto.Entry(usuario).State == EntityState.Detached)
        {
            _contexto.Usuarios.Update(usuario);
        }

        await _contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// Borra la fila del usuario.
    /// </summary>
    /// <remarks>
    /// Aquí sí hay borrado físico, a diferencia de los empleados. La razón es que un usuario
    /// es una credencial de acceso, no un sujeto de negocio: no hay pagos que rastrear hasta
    /// él. Dejarlo "inactivo" solo dejaría una credencial más en la tabla.
    /// </remarks>
    public async Task EliminarAsync(Usuario usuario, CancellationToken cancelacion)
    {
        _contexto.Usuarios.Remove(usuario);

        await _contexto.SaveChangesAsync(cancelacion);
    }
}
