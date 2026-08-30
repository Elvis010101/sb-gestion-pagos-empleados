namespace SB.GestionPagos.Dominio.Enumeraciones;

/// <summary>
/// Rol de autorización del usuario. Es el valor que viajará como claim en el JWT.
/// </summary>
public enum RolUsuario
{
    /// <summary>CRUD completo de empleados y entidades, más la gestión de usuarios.</summary>
    Administrador = 1,

    /// <summary>Solo lectura de empleados, entidades y reportes.</summary>
    Usuario = 2
}
