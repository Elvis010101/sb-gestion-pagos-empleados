using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace SB.GestionPagos.Infraestructura.Seguridad;

/// <summary>
/// Parámetros con los que se firma y se valida el token de acceso, leídos de la sección
/// <c>Jwt</c> del appsettings.
/// </summary>
/// <remarks>
/// Un único tipo produce las credenciales de FIRMA (las usa el emisor, en esta capa) y los
/// parámetros de VALIDACIÓN (los usa el middleware de la Api). Es deliberado: emisor y
/// validador tienen que coincidir en clave, algoritmo, emisor y audiencia, y si cada lado
/// leyera la configuración por su cuenta podrían desincronizarse. El síntoma sería un 401
/// en todas las peticiones sin ningún mensaje que explique por qué.
///
/// La clave de firma NO se expone como propiedad pública: es un secreto, y una propiedad
/// pública es exactamente lo que un serializador recorre cuando alguien vuelca el objeto a
/// un log de diagnóstico.
/// </remarks>
public sealed class OpcionesJwt
{
    /// <summary>Sección del appsettings donde viven estos valores.</summary>
    public const string CLAVE_SECCION = "Jwt";

    /// <summary>
    /// Valor que lleva la clave de firma en el <c>appsettings.json</c> versionado.
    /// </summary>
    /// <remarks>
    /// Se compara contra él de forma explícita. Una comprobación de longitud no bastaría:
    /// un marcador de texto suficientemente largo la pasaría, y la aplicación arrancaría
    /// firmando tokens con una clave que está publicada en el repositorio, es decir, con
    /// una clave que cualquiera puede usar para fabricarse un token de administrador.
    /// </remarks>
    public const string CLAVE_FIRMA_SIN_CONFIGURAR = "__CONFIGURAR_FUERA_DEL_REPOSITORIO__";

    private const string CLAVE_EMISOR = "Emisor";

    private const string CLAVE_AUDIENCIA = "Audiencia";

    private const string CLAVE_CLAVE_DE_FIRMA = "ClaveDeFirma";

    private const string CLAVE_MINUTOS_DE_VIGENCIA = "MinutosDeVigencia";

    /// <summary>
    /// Longitud mínima de la clave de firma, en caracteres.
    /// </summary>
    /// <remarks>
    /// HMAC-SHA256 trabaja con bloques de 256 bits. Una clave más corta que su propia
    /// salida no aporta más resistencia y la biblioteca la rechaza; 32 caracteres ASCII son
    /// esos 256 bits.
    /// </remarks>
    private const int LONGITUD_MINIMA_CLAVE_FIRMA = 32;

    private const int MINUTOS_DE_VIGENCIA_MINIMOS = 1;

    /// <summary>
    /// Techo de vigencia: ocho horas, una jornada.
    /// </summary>
    /// <remarks>
    /// Un JWT no se puede invalidar antes de que expire, así que su vigencia es el tiempo
    /// máximo que un token robado sigue sirviendo. El tope está en el código y no en la
    /// configuración a propósito: es una decisión de seguridad del sistema, no un ajuste de
    /// despliegue que alguien pueda subir a un año sin darse cuenta.
    /// </remarks>
    private const int MINUTOS_DE_VIGENCIA_MAXIMOS = 480;

    private readonly string _claveDeFirma;

    private OpcionesJwt(string emisor, string audiencia, string claveDeFirma, int minutosDeVigencia)
    {
        Emisor = emisor;
        Audiencia = audiencia;
        MinutosDeVigencia = minutosDeVigencia;
        _claveDeFirma = claveDeFirma;
    }

    /// <summary>Quién emite el token. Se escribe en el claim <c>iss</c> y se valida al recibirlo.</summary>
    public string Emisor { get; }

    /// <summary>Para quién es el token. Se escribe en el claim <c>aud</c> y se valida al recibirlo.</summary>
    public string Audiencia { get; }

    /// <summary>Minutos que el token sigue siendo válido desde su emisión.</summary>
    public int MinutosDeVigencia { get; }

    /// <summary>
    /// Lee y comprueba la sección <c>Jwt</c> de la configuración.
    /// </summary>
    /// <remarks>
    /// Falla al arrancar y no en el primer inicio de sesión, igual que la cadena de conexión.
    /// Una aplicación mal configurada que levanta bien y revienta cuando entra el primer
    /// usuario es mucho más difícil de diagnosticar que una que se niega a levantar diciendo
    /// exactamente qué le falta.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Si falta un valor o es inaceptable.</exception>
    public static OpcionesJwt LeerDe(IConfiguration configuracion)
    {
        IConfigurationSection seccion = configuracion.GetSection(CLAVE_SECCION);

        string emisor = LeerTextoObligatorio(seccion, CLAVE_EMISOR);
        string audiencia = LeerTextoObligatorio(seccion, CLAVE_AUDIENCIA);
        string claveDeFirma = LeerTextoObligatorio(seccion, CLAVE_CLAVE_DE_FIRMA);

        if (claveDeFirma == CLAVE_FIRMA_SIN_CONFIGURAR)
        {
            throw new InvalidOperationException(
                $"La clave de firma del JWT sigue con el valor de marcador '{CLAVE_FIRMA_SIN_CONFIGURAR}'. " +
                "Configure un valor propio en appsettings.Development.json o en una variable de entorno.");
        }

        if (claveDeFirma.Length < LONGITUD_MINIMA_CLAVE_FIRMA)
        {
            throw new InvalidOperationException(
                $"La clave de firma del JWT debe tener al menos {LONGITUD_MINIMA_CLAVE_FIRMA} caracteres " +
                "para poder firmar con HMAC-SHA256.");
        }

        if (!int.TryParse(seccion[CLAVE_MINUTOS_DE_VIGENCIA], out int minutosDeVigencia) ||
            minutosDeVigencia < MINUTOS_DE_VIGENCIA_MINIMOS ||
            minutosDeVigencia > MINUTOS_DE_VIGENCIA_MAXIMOS)
        {
            throw new InvalidOperationException(
                $"'{CLAVE_SECCION}:{CLAVE_MINUTOS_DE_VIGENCIA}' debe ser un número entero entre " +
                $"{MINUTOS_DE_VIGENCIA_MINIMOS} y {MINUTOS_DE_VIGENCIA_MAXIMOS}.");
        }

        return new OpcionesJwt(emisor, audiencia, claveDeFirma, minutosDeVigencia);
    }

    /// <summary>
    /// Credenciales con las que el emisor firma el token.
    /// </summary>
    /// <remarks>
    /// Firma SIMÉTRICA (HMAC-SHA256): la misma clave firma y verifica. Es lo correcto aquí
    /// porque quien emite y quien valida son el mismo servicio. La alternativa —firma
    /// asimétrica RS256, con clave privada para firmar y pública para verificar— resuelve un
    /// problema que este sistema no tiene: que varios servicios de terceros validen tokens
    /// sin poder fabricarlos.
    /// </remarks>
    public SigningCredentials ConstruirCredencialesDeFirma()
        => new(ConstruirClaveSimetrica(), SecurityAlgorithms.HmacSha256);

    /// <summary>
    /// Reglas que el middleware de la Api aplica a cada token que llega.
    /// </summary>
    public TokenValidationParameters ConstruirParametrosDeValidacion()
        => new()
        {
            // Firma: que el token venga de nosotros y no lo haya tocado nadie por el camino.
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = ConstruirClaveSimetrica(),

            // Lista blanca de algoritmos. Sin esto, el validador acepta cualquier algoritmo
            // que el propio token declare en su cabecera, y esa es la puerta de los ataques
            // de confusión de algoritmo: el atacante cambia `alg` por uno que le convenga y
            // vuelve a firmar. El algoritmo lo decide el servidor, no el token.
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },

            // Procedencia y destinatario.
            ValidateIssuer = true,
            ValidIssuer = Emisor,
            ValidateAudience = true,
            ValidAudience = Audiencia,

            // Vigencia. RequireExpirationTime rechaza además un token SIN fecha de
            // vencimiento: sin esa comprobación, "no tiene exp" pasaría como "no ha vencido".
            ValidateLifetime = true,
            RequireExpirationTime = true,

            // Por omisión la biblioteca regala cinco minutos de tolerancia de reloj. Emisor y
            // validador son el mismo proceso, así que no hay desfase que compensar, y dejarlo
            // haría que un token de 60 minutos valiera 65.
            ClockSkew = TimeSpan.Zero,

            // De qué claim salen User.Identity.Name y User.IsInRole(...). Hay que decirlo
            // porque emitimos los nombres cortos y no los URI largos de Microsoft.
            NameClaimType = NombresDeClaim.NOMBRE_USUARIO,
            RoleClaimType = NombresDeClaim.ROL
        };

    private SymmetricSecurityKey ConstruirClaveSimetrica()
        => new(Encoding.UTF8.GetBytes(_claveDeFirma));

    private static string LeerTextoObligatorio(IConfigurationSection seccion, string clave)
    {
        string? valor = seccion[clave];

        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new InvalidOperationException(
                $"Falta el valor de configuración '{CLAVE_SECCION}:{clave}'. " +
                "Revise la sección Jwt del appsettings.");
        }

        return valor;
    }
}
