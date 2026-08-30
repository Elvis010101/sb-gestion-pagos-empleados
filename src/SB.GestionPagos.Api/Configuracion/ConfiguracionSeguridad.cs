using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using SB.GestionPagos.Api.Middleware;
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
    /// <summary>
    /// Categoría con la que aparecen en el registro los rechazos por límite de frecuencia.
    /// </summary>
    /// <remarks>
    /// Se crea a mano porque el rechazo ocurre dentro de una función de configuración, donde
    /// no hay una clase de la que <c>ILogger&lt;T&gt;</c> pueda tomar el nombre.
    /// </remarks>
    private const string CATEGORIA_REGISTRO_LIMITE = "SB.GestionPagos.Api.LimiteDePeticiones";

    private const string TITULO_DEMASIADAS_PETICIONES = "Demasiadas peticiones";

    private const string MENSAJE_DEMASIADAS_PETICIONES =
        "Se superó el número de peticiones permitidas. Espere unos segundos e inténtelo de nuevo.";

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
    /// Límite de frecuencia: uno general por dirección de origen y otro más estricto sobre el
    /// inicio de sesión.
    /// </summary>
    /// <remarks>
    /// .NET 8 ofrece cuatro algoritmos y aquí se usan dos, cada uno donde encaja:
    ///
    /// CUBO DE FICHAS para el límite general. Cada origen tiene un cubo que se rellena a
    /// ritmo constante y cada petición gasta una ficha. Tolera ráfagas —una pantalla que
    /// dispara seis peticiones al cargar— sin tolerar un ritmo sostenido alto, que es
    /// exactamente la forma que tiene el tráfico de una persona usando la aplicación.
    ///
    /// VENTANA FIJA para el inicio de sesión. Cinco intentos por minuto, sin margen de
    /// ráfaga, porque una ráfaga de intentos de contraseña es justo lo que hay que impedir; y
    /// porque un límite de ventana es el único que se le puede explicar a un usuario que se
    /// quedó fuera: "espere un minuto".
    ///
    /// Los dos límites se ACUMULAN sobre el login: la petición tiene que pasar el general y
    /// además el estricto. El general es por origen, así que un atacante tampoco puede dejar
    /// sin servicio a los demás usuarios gastando un cupo compartido.
    ///
    /// Ninguno de los dos es una defensa completa: una botnet reparte los intentos entre
    /// miles de direcciones. Por eso no están solos —el costo de BCrypt encarece cada intento
    /// aunque venga de una dirección nueva, y el mensaje de error único impide averiguar qué
    /// nombres de usuario existen—. Es la medida mínima razonable, no la única.
    /// </remarks>
    private static void AgregarLimiteDePeticiones(IServiceCollection servicios)
    {
        servicios.AddRateLimiter(opciones =>
        {
            // Por omisión, .NET responde 503 (servicio no disponible) al rechazar. 429 es lo
            // correcto: no es que el servidor esté caído, es que este cliente pidió de más.
            opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // El límite global se aplica a TODOS los endpoints, incluidos los que no declaran
            // ninguna política. Es lo contrario de ir marcando endpoints uno por uno: aquí
            // olvidarse de marcar uno no lo deja desprotegido.
            opciones.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                contexto => RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: ObtenerOrigenDeLaPeticion(contexto),
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = PoliticasLimiteDePeticiones.PETICIONES_EN_RAFAGA_POR_ORIGEN,
                        TokensPerPeriod = PoliticasLimiteDePeticiones.PETICIONES_REPUESTAS_POR_PERIODO,
                        ReplenishmentPeriod =
                            TimeSpan.FromSeconds(PoliticasLimiteDePeticiones.SEGUNDOS_DE_REPOSICION),

                        // El cubo se rellena solo, con un temporizador interno. Sin esto
                        // habría que reponerlo a mano desde algún sitio.
                        AutoReplenishment = true,

                        QueueLimit = 0
                    }));

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

            opciones.OnRejected = RechazarAsync;
        });
    }

    /// <summary>
    /// Qué se responde y qué se registra cuando una petición supera el límite.
    /// </summary>
    /// <remarks>
    /// Sin esto, .NET devuelve un 429 con el cuerpo vacío. Se personaliza por dos razones:
    /// para que el error tenga el mismo contrato ProblemDetails que todos los demás, y para
    /// que quede registrado. Un rechazo por frecuencia es justo el tipo de evento que
    /// alguien va a querer buscar en el archivo cuando sospeche de un ataque.
    /// </remarks>
    private static async ValueTask RechazarAsync(OnRejectedContext contexto, CancellationToken cancelacion)
    {
        // El propio limitador sabe cuánto falta para que haya cupo de nuevo. Devolverlo en
        // Retry-After convierte el rechazo en algo que un cliente puede manejar solo, en
        // lugar de dejarlo reintentando a ciegas y empeorando la congestión.
        if (contexto.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan esperaSugerida))
        {
            contexto.HttpContext.Response.Headers.RetryAfter =
                ((int)esperaSugerida.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
        }

        ILogger registrador = contexto.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(CATEGORIA_REGISTRO_LIMITE);

        registrador.LogWarning(
            "Petición rechazada por límite de frecuencia. Origen: {DireccionOrigen}. Ruta: {RutaSolicitada}.",
            ObtenerOrigenDeLaPeticion(contexto.HttpContext),
            contexto.HttpContext.Request.Path.Value);

        await RespuestaProblema.EscribirAsync(
            contexto.HttpContext,
            StatusCodes.Status429TooManyRequests,
            TITULO_DEMASIADAS_PETICIONES,
            MENSAJE_DEMASIADAS_PETICIONES);
    }

    private static string ObtenerOrigenDeLaPeticion(HttpContext contexto)
        => contexto.Connection.RemoteIpAddress?.ToString()
           ?? PoliticasLimiteDePeticiones.ORIGEN_DESCONOCIDO;
}
