using SB.GestionPagos.Api.Middleware;

namespace SB.GestionPagos.Api.Configuracion;

/// <summary>
/// Política de CORS: qué orígenes de navegador pueden llamar a esta API.
/// </summary>
/// <remarks>
/// CORS no es una defensa del servidor —cualquier cliente que no sea un navegador, como
/// Postman o curl, lo ignora por completo—. Es una instrucción AL NAVEGADOR sobre qué
/// páginas pueden leer las respuestas de esta API. Sirve para que un sitio cualquiera no
/// pueda hacer peticiones a este backend desde el navegador de un usuario que ya tiene
/// sesión abierta. La autorización de verdad la sigue haciendo el token.
/// </remarks>
internal static class ConfiguracionCors
{
    internal const string POLITICA_FRONTEND = "PoliticaFrontend";

    private const string CLAVE_ORIGENES_PERMITIDOS = "Cors:OrigenesPermitidos";

    /// <summary>
    /// Origen del servidor de desarrollo de Vite. Es el valor de reserva si la configuración
    /// no trae ninguno; el real llega desde el appsettings de cada entorno.
    /// </summary>
    private const string ORIGEN_PREDETERMINADO = "http://localhost:5173";

    internal static IServiceCollection AgregarPoliticaDeCors(
        this IServiceCollection servicios,
        IConfiguration configuracion)
    {
        string[] origenesPermitidos =
            configuracion.GetSection(CLAVE_ORIGENES_PERMITIDOS).Get<string[]>() is { Length: > 0 } origenes
                ? origenes
                : [ORIGEN_PREDETERMINADO];

        servicios.AddCors(opciones => opciones.AddPolicy(
            POLITICA_FRONTEND,
            constructor => constructor

                // Orígenes nombrados, nunca AllowAnyOrigin. Con el comodín, cualquier página
                // de internet podría leer las respuestas de esta API desde el navegador de un
                // usuario autenticado.
                .WithOrigins(origenesPermitidos)
                .AllowAnyHeader()
                .AllowAnyMethod()

                // El frontend necesita poder LEER esta cabecera para mostrar el identificador
                // de correlación cuando algo falla. Por omisión, el navegador solo deja ver
                // un puñado de cabeceras estándar y oculta el resto aunque hayan llegado.
                .WithExposedHeaders(MiddlewareCorrelacion.NOMBRE_CABECERA)));

        // No se llama a AllowCredentials, y es deliberado: el token viaja en la cabecera
        // Authorization, que el frontend adjunta a mano. No usamos cookies de sesión, así que
        // no hay credenciales que el navegador deba mandar solo — y sin envío automático de
        // credenciales, la superficie de CSRF desaparece por diseño.
        return servicios;
    }
}
