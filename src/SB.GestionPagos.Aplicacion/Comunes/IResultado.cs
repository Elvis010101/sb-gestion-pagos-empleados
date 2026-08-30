namespace SB.GestionPagos.Aplicacion.Comunes;

/// <summary>
/// Parte común de <see cref="Resultado"/> y <see cref="Resultado{T}"/>.
/// </summary>
/// <remarks>
/// Existe para que la capa Api escriba UNA sola traducción de resultado a código HTTP,
/// válida tanto para las operaciones que devuelven un valor como para las que no.
/// Es una interfaz y no una clase base porque los dos tipos declaran métodos de fábrica
/// estáticos con los mismos nombres: con herencia, <c>Resultado&lt;T&gt;.NoEncontrado(...)</c>
/// resolvería al método del padre y devolvería el tipo equivocado.
/// </remarks>
public interface IResultado
{
    bool EsExitoso { get; }

    TipoErrorAplicacion TipoError { get; }

    /// <summary>Mensaje apto para mostrarse al usuario. Vacío cuando la operación fue exitosa.</summary>
    string Mensaje { get; }
}
