using System.Globalization;

namespace SB.GestionPagos.Infraestructura.DatosPlanos;

/// <summary>
/// Comparaciones de texto para la búsqueda en el catálogo: ignoran mayúsculas y acentos.
/// </summary>
/// <remarks>
/// El detalle importante es <c>IgnoreNonSpace</c>: descarta los diacríticos, de modo que
/// "educacion" encuentra "Educación". Sin eso, un usuario con un teclado sin tildes —o con
/// prisa— no encontraría más de la mitad del catálogo, porque los nombres oficiales llevan
/// acentos casi todos.
///
/// La alternativa habitual, <c>ToLower()</c> sobre ambos textos, resuelve las mayúsculas y
/// no toca los acentos; además crea dos cadenas nuevas por comparación y arrastra el problema
/// clásico del idioma turco, donde la 'I' no baja a 'i'. <c>CompareInfo</c> lo hace en un solo
/// paso, sin asignar memoria y con reglas de cotejo de verdad.
///
/// El resultado, además, es coherente con el otro repositorio: SQL Server aplica el mismo
/// criterio en <c>EmpleadoRepositorioSql</c>, porque la intercalación predeterminada de la
/// base (<c>..._CI_AS</c>) ya ignora mayúsculas. Que un usuario obtenga el mismo resultado
/// buscando empleados o entidades no es casualidad: se hizo coincidir a propósito.
/// </remarks>
internal static class ComparadorTextoCatalogo
{
    private const CompareOptions OPCIONES_COMPARACION = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    private static readonly CompareInfo _reglasDeCotejo = CultureInfo.InvariantCulture.CompareInfo;

    /// <summary>Coincidencia parcial: ¿aparece el texto buscado dentro del origen?</summary>
    internal static bool Contiene(string textoOrigen, string textoBuscado)
        => _reglasDeCotejo.IndexOf(textoOrigen, textoBuscado, OPCIONES_COMPARACION) >= 0;

    /// <summary>Coincidencia exacta, salvo por mayúsculas y acentos.</summary>
    internal static bool SonIguales(string primerTexto, string segundoTexto)
        => _reglasDeCotejo.Compare(primerTexto, segundoTexto, OPCIONES_COMPARACION) == 0;
}
