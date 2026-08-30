using Serilog.Context;

namespace SB.GestionPagos.Api.Middleware;

/// <summary>
/// Asigna a cada petición un identificador de correlación y lo adjunta a todo lo que se
/// registre mientras esa petición está viva.
/// </summary>
/// <remarks>
/// Sin esto, el archivo de registro es una lista plana de líneas de veinte peticiones
/// entrelazadas y no hay forma de saber cuáles pertenecen a la misma. Con el identificador,
/// diagnosticar un fallo es filtrar el archivo por un valor.
///
/// El identificador se ACEPTA si el cliente lo envía, y se genera si no. Aceptarlo es lo que
/// permite que el frontend —y mañana cualquier otro servicio— use el mismo valor en sus
/// propios registros: el rastro cruza la frontera entre procesos.
/// </remarks>
public sealed class MiddlewareCorrelacion
{
    /// <summary>Cabecera por la que entra y sale el identificador.</summary>
    public const string NOMBRE_CABECERA = "X-Id-Correlacion";

    /// <summary>Nombre de la propiedad con que aparece en los registros.</summary>
    public const string NOMBRE_PROPIEDAD_REGISTRO = "IdCorrelacion";

    /// <summary>Nombre con que viaja dentro del cuerpo de un ProblemDetails.</summary>
    public const string NOMBRE_PROPIEDAD_JSON = "idCorrelacion";

    /// <summary>
    /// Tope de longitud del identificador recibido. Existe porque el valor termina escrito
    /// en el archivo de registro: sin tope, un cliente podría inflarlo a un megabyte por
    /// petición y llenar el disco.
    /// </summary>
    private const int LONGITUD_MAXIMA = 64;

    private const char SEPARADOR_PERMITIDO = '-';

    /// <summary>Formato "N" de <see cref="Guid"/>: 32 dígitos hexadecimales sin guiones.</summary>
    private const string FORMATO_IDENTIFICADOR_GENERADO = "N";

    private readonly RequestDelegate _siguiente;

    public MiddlewareCorrelacion(RequestDelegate siguiente)
    {
        _siguiente = siguiente;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        string idCorrelacion = ObtenerOGenerar(contexto.Request);

        // TraceIdentifier es la propiedad que ASP.NET Core ya usa para identificar la
        // petición, y de donde ProblemDetails saca su "traceId". Sobrescribirla hace que el
        // identificador del cuerpo del error, el de la cabecera y el del registro sean el
        // mismo valor, en lugar de tres números distintos que el soporte tiene que cruzar.
        contexto.TraceIdentifier = idCorrelacion;

        // Se escribe antes de llamar a la siguiente pieza: en ese momento la respuesta aún
        // no ha empezado a enviarse y las cabeceras todavía se pueden tocar.
        contexto.Response.Headers[NOMBRE_CABECERA] = idCorrelacion;

        // PushProperty adjunta la propiedad a TODO evento registrado por debajo de esta
        // línea, incluidos los de la capa Servicios, que no sabe nada de HTTP ni de
        // correlación. Se apoya en Enrich.FromLogContext, activado en ConfiguracionRegistro.
        using (LogContext.PushProperty(NOMBRE_PROPIEDAD_REGISTRO, idCorrelacion))
        {
            await _siguiente(contexto);
        }
    }

    private static string ObtenerOGenerar(HttpRequest peticion)
    {
        string? identificadorRecibido = peticion.Headers[NOMBRE_CABECERA].FirstOrDefault();

        return EsAceptable(identificadorRecibido)
            ? identificadorRecibido!
            : Guid.NewGuid().ToString(FORMATO_IDENTIFICADOR_GENERADO);
    }

    /// <summary>
    /// Comprueba que el identificador recibido sea seguro para escribirlo en el registro.
    /// </summary>
    /// <remarks>
    /// No es paranoia: el valor viene del cliente y va a parar a un archivo de texto. Si se
    /// aceptara tal cual, alguien podría mandar un identificador con saltos de línea y
    /// fabricar entradas falsas en el registro —inyección de logs—, o meter caracteres de
    /// control que rompan la herramienta que después lo lee. Se admite solo lo que un
    /// identificador necesita: letras, dígitos y guiones.
    /// </remarks>
    private static bool EsAceptable(string? identificador)
    {
        if (string.IsNullOrWhiteSpace(identificador) || identificador.Length > LONGITUD_MAXIMA)
        {
            return false;
        }

        foreach (char caracter in identificador)
        {
            if (!char.IsAsciiLetterOrDigit(caracter) && caracter != SEPARADOR_PERMITIDO)
            {
                return false;
            }
        }

        return true;
    }
}
