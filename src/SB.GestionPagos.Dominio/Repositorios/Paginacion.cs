using SB.GestionPagos.Dominio.Validaciones;

namespace SB.GestionPagos.Dominio.Repositorios;

/// <summary>
/// Tramo de registros que se pide al repositorio en una consulta paginada.
/// </summary>
/// <remarks>
/// Vive en el Dominio porque es parte del contrato de <see cref="IEmpleadoRepositorio"/>,
/// y ese contrato se declara aquí. Valida sus propias invariantes: una página cero o un
/// tamaño negativo producirían un desplazamiento negativo en la consulta, y ese error
/// debe morir en el objeto, no en el motor de base de datos.
/// </remarks>
public sealed record Paginacion
{
    /// <summary>Las páginas se numeran desde 1, como las ve el usuario, no desde 0.</summary>
    public const int PAGINA_MINIMA = 1;

    public const int TAMANO_PAGINA_MINIMO = 1;

    /// <summary>
    /// Techo del tamaño de página. Sin él, un cliente podría pedir un millón de registros
    /// en una sola llamada y convertir la consulta paginada en una descarga completa.
    /// </summary>
    public const int TAMANO_PAGINA_MAXIMO = 100;

    /// <summary>
    /// Página más alta que puede pedirse sin que el desplazamiento desborde un <c>int</c>.
    /// </summary>
    /// <remarks>
    /// No es un número arbitrario: se deriva del propio cálculo de
    /// <see cref="RegistrosOmitidos"/>, que multiplica la página por el tamaño.
    /// </remarks>
    public const int PAGINA_MAXIMA = int.MaxValue / TAMANO_PAGINA_MAXIMO;

    public Paginacion(int pagina, int tamanoPagina)
    {
        Pagina = ValidacionDominio.EnRangoInclusivo(pagina, PAGINA_MINIMA, PAGINA_MAXIMA, nameof(Pagina));
        TamanoPagina = ValidacionDominio.EnRangoInclusivo(
            tamanoPagina,
            TAMANO_PAGINA_MINIMO,
            TAMANO_PAGINA_MAXIMO,
            nameof(TamanoPagina));
    }

    public int Pagina { get; }

    public int TamanoPagina { get; }

    /// <summary>
    /// Registros que la consulta debe saltar antes de empezar a leer.
    /// </summary>
    /// <remarks>
    /// El cálculo se hace una sola vez y aquí, para que ninguna implementación de
    /// repositorio tenga que recordar restarle uno a la página.
    /// </remarks>
    public int RegistrosOmitidos => (Pagina - PAGINA_MINIMA) * TamanoPagina;
}
