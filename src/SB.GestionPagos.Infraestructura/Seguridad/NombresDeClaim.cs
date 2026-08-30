namespace SB.GestionPagos.Infraestructura.Seguridad;

/// <summary>
/// Nombres de los claims que viajan dentro del JWT.
/// </summary>
/// <remarks>
/// Existen como constantes porque el mismo nombre se escribe al EMITIR el token y se lee al
/// VALIDARLO. Si uno de los dos lados dijera <c>"rol"</c> y el otro <c>"role"</c>, el token
/// sería perfectamente válido y aun así toda autorización por rol devolvería 403, sin
/// ningún error que apunte a la causa.
///
/// <c>sub</c>, <c>jti</c> y <c>name</c> son nombres registrados por el RFC 7519; se usan tal
/// cual en lugar de inventar equivalentes en español, porque cualquier herramienta que
/// inspeccione el token —jwt.io, un gateway, un proxy— los entiende sin traducción.
///
/// <c>role</c> no es un claim registrado, pero es la convención de OpenID Connect y evita
/// el nombre largo de Microsoft
/// (<c>http://schemas.microsoft.com/ws/2008/06/identity/claims/role</c>), que agregaría unos
/// 60 bytes a cada petición del sistema sin aportar nada.
/// </remarks>
public static class NombresDeClaim
{
    /// <summary>Sujeto del token: el identificador del usuario.</summary>
    public const string SUJETO = "sub";

    /// <summary>Nombre para mostrar del usuario autenticado.</summary>
    public const string NOMBRE_USUARIO = "name";

    /// <summary>Rol de autorización. Es el claim que leen las políticas de la Api.</summary>
    public const string ROL = "role";

    /// <summary>
    /// Identificador único de ESTE token concreto.
    /// </summary>
    /// <remarks>
    /// Hoy no se consulta en ningún lado. Se emite igual porque es la pieza sobre la que se
    /// construiría una lista de revocación: sin un identificador por token, revocar uno solo
    /// obligaría a invalidar todos los del usuario.
    /// </remarks>
    public const string IDENTIFICADOR_TOKEN = "jti";
}
