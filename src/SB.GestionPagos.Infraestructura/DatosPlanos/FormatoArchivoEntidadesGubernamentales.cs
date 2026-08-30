using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace SB.GestionPagos.Infraestructura.DatosPlanos;

/// <summary>
/// Traduce entre una línea de texto del archivo plano y un <see cref="RegistroEntidadGubernamental"/>.
/// </summary>
/// <remarks>
/// El formato es JSONL: un objeto JSON por línea. Se eligió frente a un formato delimitado
/// (pipe o coma) por una razón concreta y verificable en estos mismos datos: dos entidades del
/// listado oficial traen comillas dobles en el nombre —el Jardín Botánico Nacional "Dr. Rafael
/// M. Moscoso" y el Museo Nacional de Historia Natural "Prof. Eugenio de Jesús Marcano"—. Un
/// formato delimitado obliga a escribir un escapador propio, y un escapador propio mal hecho
/// no falla ruidosamente: parte un registro en dos y corrompe el catálogo en silencio. Aquí el
/// escapado lo resuelve <c>System.Text.Json</c>, que viene en el framework base y no agrega
/// ningún paquete al proyecto.
///
/// Lo que se conserva del formato de línea: se puede abrir con cualquier editor, hacer
/// <c>grep</c>, y el diff en git de un alta es una sola línea. Y una línea corrupta no arrastra
/// al resto del archivo, cosa que sí pasaría con un único arreglo JSON envolviendo todo.
/// </remarks>
internal static class FormatoArchivoEntidadesGubernamentales
{
    /// <summary>Prefijo de las líneas de comentario, que se ignoran al leer.</summary>
    internal const string PREFIJO_COMENTARIO = "#";

    /// <summary>
    /// Fin de línea fijado a LF, sin depender del sistema operativo.
    /// </summary>
    /// <remarks>
    /// <c>Environment.NewLine</c> daría CRLF en Windows y LF en Linux. Como el archivo está
    /// versionado en git, eso haría que cada alta desde una máquina distinta reescribiera las
    /// 181 líneas y el diff dejara de mostrar qué cambió de verdad.
    /// </remarks>
    internal const string FIN_DE_LINEA = "\n";

    /// <summary>
    /// UTF-8 SIN marca de orden de bytes (BOM).
    /// </summary>
    /// <remarks>
    /// <c>Encoding.UTF8</c>, la propiedad estática, sí emite BOM al escribir. Esos tres bytes
    /// quedarían pegados al inicio de la primera línea y el analizador JSON fallaría con un
    /// error desconcertante sobre un carácter inesperado en la posición 0.
    /// </remarks>
    internal static readonly Encoding _codificacion = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// Codificador que deja los acentos como caracteres literales.
    /// </summary>
    /// <remarks>
    /// Por omisión, <c>System.Text.Json</c> escapa todo lo que no sea ASCII y escribiría
    /// "Educación", dejando el archivo ilegible para una persona. Habilitar todos los
    /// rangos Unicode devuelve la tilde literal sin bajar la guardia donde importa: el
    /// codificador sigue escapando los caracteres peligrosos en contextos HTML y JavaScript
    /// (&lt;, &gt;, &amp;, comilla simple), por eso NO se usa <c>UnsafeRelaxedJsonEscaping</c>.
    /// </remarks>
    private static readonly JsonSerializerOptions _opcionesJson = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        WriteIndented = false,
    };

    internal static string ALinea(RegistroEntidadGubernamental registro)
        => JsonSerializer.Serialize(registro, _opcionesJson);

    /// <summary>
    /// Indica si una línea aporta datos, o si es una línea en blanco o un comentario.
    /// </summary>
    internal static bool EsLineaDeDatos(string linea)
        => !string.IsNullOrWhiteSpace(linea)
            && !linea.TrimStart().StartsWith(PREFIJO_COMENTARIO, StringComparison.Ordinal);

    /// <summary>
    /// Convierte una línea de datos en un registro.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// La línea no es JSON válido o no contiene un objeto.
    /// </exception>
    internal static RegistroEntidadGubernamental DesdeLinea(string linea, int numeroLinea)
    {
        try
        {
            RegistroEntidadGubernamental? registro =
                JsonSerializer.Deserialize<RegistroEntidadGubernamental>(linea, _opcionesJson);

            // Deserializar el literal JSON "null" devuelve null sin lanzar excepción.
            return registro ?? throw new InvalidDataException(DescribirLineaInvalida(numeroLinea, "no contiene un objeto"));
        }
        catch (JsonException excepcion)
        {
            throw new InvalidDataException(
                DescribirLineaInvalida(numeroLinea, "no es JSON válido"),
                excepcion);
        }
    }

    internal static string DescribirLineaInvalida(int numeroLinea, string motivo)
        => FormattableString.Invariant(
            $"El archivo de entidades gubernamentales está corrupto: la línea {numeroLinea} {motivo}.");
}
