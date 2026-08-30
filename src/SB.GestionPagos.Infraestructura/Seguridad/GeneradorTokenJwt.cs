using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SB.GestionPagos.Aplicacion.Seguridad;
using SB.GestionPagos.Dominio.Entidades;

namespace SB.GestionPagos.Infraestructura.Seguridad;

/// <summary>
/// Implementación de <see cref="IGeneradorTokenJwt"/> con firma HMAC-SHA256.
/// </summary>
/// <remarks>
/// Es <c>internal</c>: el proyecto Api inyecta la interfaz y nunca nombra esta clase.
///
/// Nada de lo que se escribe aquí es secreto. El contenido de un JWT viaja codificado en
/// Base64Url, no cifrado: cualquiera que intercepte el token puede leerlo. Por eso van el
/// identificador, el nombre y el rol —datos que el propio usuario ya conoce— y nunca el
/// hash de la contraseña, el número de seguro social ni ningún otro dato personal.
/// </remarks>
internal sealed class GeneradorTokenJwt : IGeneradorTokenJwt
{
    private readonly OpcionesJwt _opciones;

    // El manejador no guarda estado entre llamadas: se reutiliza en lugar de construir uno
    // en cada inicio de sesión.
    private readonly JwtSecurityTokenHandler _manejadorDeTokens = new();

    public GeneradorTokenJwt(OpcionesJwt opciones)
    {
        _opciones = opciones;
    }

    public TokenGenerado Generar(Usuario usuario)
    {
        // Un solo instante para las dos fechas. Leer DateTime.UtcNow dos veces daría valores
        // distintos, y la vigencia real no sería exactamente la configurada.
        DateTime instanteDeEmisionUtc = DateTime.UtcNow;
        DateTime instanteDeExpiracionUtc = instanteDeEmisionUtc.AddMinutes(_opciones.MinutosDeVigencia);

        Claim[] claims =
        [
            new Claim(NombresDeClaim.SUJETO, usuario.Id.ToString(CultureInfo.InvariantCulture)),
            new Claim(NombresDeClaim.NOMBRE_USUARIO, usuario.NombreUsuario),

            // El VALOR del claim es el nombre del miembro del enum ("Administrador"), no su
            // número. Las políticas de la Api comparan contra `nameof(RolUsuario.Administrador)`,
            // así que un renombre del enum rompe la compilación en vez de romper los permisos
            // en silencio.
            new Claim(NombresDeClaim.ROL, usuario.Rol.ToString()),

            new Claim(NombresDeClaim.IDENTIFICADOR_TOKEN, Guid.NewGuid().ToString())
        ];

        JwtSecurityToken token = new(
            issuer: _opciones.Emisor,
            audience: _opciones.Audiencia,
            claims: claims,
            notBefore: instanteDeEmisionUtc,
            expires: instanteDeExpiracionUtc,
            signingCredentials: _opciones.ConstruirCredencialesDeFirma());

        // La fecha de expiración se devuelve aparte para que la capa Aplicación no tenga que
        // abrir el token para leerla: ese es justamente el detalle que la abstracción oculta.
        return new TokenGenerado(_manejadorDeTokens.WriteToken(token), instanteDeExpiracionUtc);
    }
}
