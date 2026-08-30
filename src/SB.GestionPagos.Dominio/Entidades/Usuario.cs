using SB.GestionPagos.Dominio.Enumeraciones;
using SB.GestionPagos.Dominio.Validaciones;

namespace SB.GestionPagos.Dominio.Entidades;

/// <summary>
/// Usuario que se autentica contra la API.
/// </summary>
/// <remarks>
/// El Dominio guarda un hash y nunca una contraseña en claro. Tampoco sabe con qué algoritmo
/// se produjo ese hash: BCrypt es un detalle reemplazable que vive en Infraestructura.
/// </remarks>
public sealed class Usuario
{
    public Usuario(string nombreUsuario, string hashContrasena, RolUsuario rol)
    {
        NombreUsuario = ValidacionDominio.TextoRequerido(nombreUsuario, nameof(NombreUsuario));
        HashContrasena = ValidacionDominio.TextoRequerido(hashContrasena, nameof(HashContrasena));
        Rol = rol;
    }

    public int Id { get; private set; }

    public string NombreUsuario { get; private set; }

    public string HashContrasena { get; private set; }

    public RolUsuario Rol { get; private set; }

    public void CambiarHashContrasena(string hashContrasena)
        => HashContrasena = ValidacionDominio.TextoRequerido(hashContrasena, nameof(HashContrasena));

    public void CambiarRol(RolUsuario nuevoRol) => Rol = nuevoRol;
}
