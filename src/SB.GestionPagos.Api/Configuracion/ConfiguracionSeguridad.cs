using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using SB.GestionPagos.Api.Seguridad;
using SB.GestionPagos.Dominio.Enumeraciones;
using SB.GestionPagos.Infraestructura.Seguridad;

namespace SB.GestionPagos.Api.Configuracion;

/// <summary>
/// Autenticación, autorización y límite de frecuencia del host.
/// </summary>
/// <remarks>
/// Vive en un archivo aparte y no dentro de <c>Program.cs</c> para que el arranque se lea de
/// un vistazo: qué se registra, en qué orden y nada más. Aquí no hay lógica de negocio, solo
/// configuración del canal HTTP, que es exactamente lo que le corresponde al proyecto Api.
/// </remarks>
internal static class ConfiguracionSeguridad
{
    internal static IServiceCollection AgregarSeguridad(
        this IServiceCollection servicios,
        IConfiguration configuracion)
    {
        AgregarAutenticacion(servicios, configuracion);
        AgregarAutorizacion(servicios);
        AgregarLimiteDePeticiones(servicios);

        return servicios;
    }

    /// <summary>
    /// Enseña al host a reconocer al portador de un token (AUTENTICACIÓN: quién eres).
    /// </summary>
    /// <remarks>
    /// Las reglas de validación no se escriben aquí: las construye <see cref="OpcionesJwt"/>,
    /// el mismo tipo que produce las credenciales con que se firma. Así el emisor y el
    /// validador no pueden discrepar en la clave, el algoritmo, el emisor ni la audiencia.
    ///
    /// Que el token viaje cifrado NO se resuelve aquí: un Bearer va en texto plano en la
    /// cabecera y quien lo intercepte puede reutilizarlo tal cual. Eso lo cubre HTTPS, que
    /// el host impone en el pipeline con <c>UseHttpsRedirection</c>.
    /// </remarks>
    private static void AgregarAutenticacion(IServiceCollection servicios, IConfiguration configuracion)
    {
        OpcionesJwt opcionesJwt = OpcionesJwt.LeerDe(configuracion);

        servicios
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opciones =>
            {
                opciones.TokenValidationParameters = opcionesJwt.ConstruirParametrosDeValidacion();

                // Sin esto, la biblioteca reescribe los claims cortos que emitimos ("sub",
                // "name", "role") a los URI largos de Microsoft en cuanto entran, y el
                // ClaimsPrincipal deja de parecerse al token que lo originó. Apagarlo hace
                // que lo que se ve en jwt.io sea literalmente lo que se lee en el servidor.
                opciones.MapInboundClaims = false;
            });
    }

    /// <summary>
    /// Declara qué hace falta para entrar a cada cosa (AUTORIZACIÓN: qué puedes hacer).
    /// </summary>
    private static void AgregarAutorizacion(IServiceCollection servicios)
    {
        servicios.AddAuthorization(opciones =>
        {
            // Denegar por omisión. Todo endpoint que no diga nada queda exigiendo un usuario
            // autenticado. Es la diferencia entre "olvidé poner [Authorize] y el endpoint
            // quedó abierto" y "olvidé poner [AllowAnonymous] y el endpoint quedó cerrado":
            // el primer olvido es una brecha, el segundo es un error visible en la primera
            // prueba. Un descuido tiene que costar disponibilidad, no seguridad.
            opciones.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            opciones.AddPolicy(
                PoliticasAutorizacion.SOLO_ADMINISTRADOR,
                politica => politica.RequireRole(nameof(RolUsuario.Administrador)));

            opciones.AddPolicy(
                PoliticasAutorizacion.LECTURA,
                politica => politica.RequireRole(
                    nameof(RolUsuario.Administrador),
                    nameof(RolUsuario.Usuario)));
        });
    }

    /// <summary>
    /// Freno de fuerza bruta sobre el inicio de sesión.
    /// </summary>
    /// <remarks>
    /// Ventana fija por dirección de origen: cada cliente tiene su propio cubo, así que el
    /// atacante no puede dejar fuera de servicio el login de los demás simplemente gastando
    /// el cupo global.
    ///
    /// No es una defensa completa —una botnet reparte los intentos entre miles de
    /// direcciones— y por eso no está sola: el costo de BCrypt encarece cada intento aunque
    /// venga de una dirección nueva, y el mensaje de error único impide averiguar qué
    /// nombres de usuario existen. Es la medida MÍNIMA razonable, no la única.
    /// </remarks>
    private static void AgregarLimiteDePeticiones(IServiceCollection servicios)
    {
        servicios.AddRateLimiter(opciones =>
        {
            // Por omisión, .NET responde 503 (servicio no disponible) al rechazar. 429 es lo
            // correcto: no es que el servidor esté caído, es que este cliente pidió de más.
            opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            opciones.AddPolicy(
                PoliticasLimiteDePeticiones.INICIO_SESION,
                contexto => RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ObtenerOrigenDeLaPeticion(contexto),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = PoliticasLimiteDePeticiones.INTENTOS_PERMITIDOS_POR_VENTANA,
                        Window = TimeSpan.FromMinutes(PoliticasLimiteDePeticiones.MINUTOS_DE_VENTANA),

                        // Cola de cero: el intento sobrante se RECHAZA, no se guarda para
                        // atenderlo después. Encolar peticiones de login solo acumularía
                        // trabajo pendiente a favor del atacante.
                        QueueLimit = 0
                    }));
        });
    }

    private static string ObtenerOrigenDeLaPeticion(HttpContext contexto)
        => contexto.Connection.RemoteIpAddress?.ToString()
           ?? PoliticasLimiteDePeticiones.ORIGEN_DESCONOCIDO;
}
