namespace SB.GestionPagos.Aplicacion.Comunes;

/// <summary>
/// Resultado de un caso de uso que no devuelve datos (por ejemplo, eliminar).
/// </summary>
/// <remarks>
/// Los fallos ESPERABLES —"ese empleado no existe", "ese número de seguro social ya está
/// registrado"— no son excepciones: son una de las dos salidas normales de la operación.
/// Modelarlos como valor de retorno obliga a quien llama a contemplarlos, cosa que una
/// excepción no hace: un <c>catch</c> olvidado no se nota hasta que revienta en producción.
/// </remarks>
public sealed class Resultado : IResultado
{
    private Resultado(bool esExitoso, TipoErrorAplicacion tipoError, string mensaje)
    {
        EsExitoso = esExitoso;
        TipoError = tipoError;
        Mensaje = mensaje;
    }

    public bool EsExitoso { get; }

    public TipoErrorAplicacion TipoError { get; }

    public string Mensaje { get; }

    public static Resultado Exitoso() => new(true, TipoErrorAplicacion.Ninguno, string.Empty);

    public static Resultado NoEncontrado(string mensaje) => new(false, TipoErrorAplicacion.NoEncontrado, mensaje);

    public static Resultado Conflicto(string mensaje) => new(false, TipoErrorAplicacion.Conflicto, mensaje);

    public static Resultado ReglaDeNegocio(string mensaje) => new(false, TipoErrorAplicacion.ReglaDeNegocio, mensaje);

    public static Resultado CredencialesInvalidas(string mensaje)
        => new(false, TipoErrorAplicacion.CredencialesInvalidas, mensaje);
}

/// <summary>
/// Resultado de un caso de uso que, cuando sale bien, devuelve un valor de tipo
/// <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">Tipo del dato producido por la operación exitosa.</typeparam>
public sealed class Resultado<T> : IResultado
{
    private Resultado(bool esExitoso, T? valor, TipoErrorAplicacion tipoError, string mensaje)
    {
        EsExitoso = esExitoso;
        Valor = valor;
        TipoError = tipoError;
        Mensaje = mensaje;
    }

    public bool EsExitoso { get; }

    /// <summary>
    /// Valor producido por la operación. Solo tiene significado si
    /// <see cref="EsExitoso"/> es verdadero; en caso contrario es nulo.
    /// </summary>
    public T? Valor { get; }

    public TipoErrorAplicacion TipoError { get; }

    public string Mensaje { get; }

    public static Resultado<T> Exitoso(T valor)
        => new(true, valor, TipoErrorAplicacion.Ninguno, string.Empty);

    public static Resultado<T> NoEncontrado(string mensaje)
        => new(false, default, TipoErrorAplicacion.NoEncontrado, mensaje);

    public static Resultado<T> Conflicto(string mensaje)
        => new(false, default, TipoErrorAplicacion.Conflicto, mensaje);

    public static Resultado<T> ReglaDeNegocio(string mensaje)
        => new(false, default, TipoErrorAplicacion.ReglaDeNegocio, mensaje);

    public static Resultado<T> CredencialesInvalidas(string mensaje)
        => new(false, default, TipoErrorAplicacion.CredencialesInvalidas, mensaje);
}
